using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface IApprovalWorkflowService
{
    Task<IEnumerable<ApprovalStepDto>> GetApprovalStepsAsync(int leaveRequestId);
    Task<ApiResponse<ApprovalStepDto>> ProcessApprovalStepAsync(int approverId, ProcessApprovalDto dto);
    Task<ApiResponse<bool>> InitiateWorkflowAsync(int leaveRequestId);
    Task<IEnumerable<ApprovalStepDto>> GetPendingApprovalsForUserAsync(int approverId);
}

public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly LeaveDbContext _context;
    private readonly INotificationService _notificationService;

    public ApprovalWorkflowService(LeaveDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<ApprovalStepDto>> GetApprovalStepsAsync(int leaveRequestId)
    {
        return await _context.ApprovalSteps
            .Where(s => s.LeaveRequestId == leaveRequestId)
            .Include(s => s.Approver)
            .OrderBy(s => s.StepOrder)
            .Select(s => new ApprovalStepDto
            {
                Id = s.Id,
                LeaveRequestId = s.LeaveRequestId,
                ApproverId = s.ApproverId,
                ApproverName = $"{s.Approver.FirstName} {s.Approver.LastName}",
                StepOrder = s.StepOrder,
                Status = s.Status.ToString(),
                Comments = s.Comments,
                ActionDate = s.ActionDate
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<ApprovalStepDto>> ProcessApprovalStepAsync(int approverId, ProcessApprovalDto dto)
    {
        var step = await _context.ApprovalSteps
            .Include(s => s.LeaveRequest)
                .ThenInclude(lr => lr.Employee)
            .Include(s => s.Approver)
            .FirstOrDefaultAsync(s => s.Id == dto.StepId);

        if (step == null)
        {
            return ApiResponse<ApprovalStepDto>.FailureResponse("Approval step not found");
        }

        if (step.ApproverId != approverId)
        {
            return ApiResponse<ApprovalStepDto>.FailureResponse("You are not authorized to process this approval");
        }

        if (step.Status != ApprovalStatus.Pending)
        {
            return ApiResponse<ApprovalStepDto>.FailureResponse("This step has already been processed");
        }

        // Check if previous steps are approved
        var previousSteps = await _context.ApprovalSteps
            .Where(s => s.LeaveRequestId == step.LeaveRequestId && s.StepOrder < step.StepOrder)
            .ToListAsync();

        if (previousSteps.Any(s => s.Status != ApprovalStatus.Approved))
        {
            return ApiResponse<ApprovalStepDto>.FailureResponse("Previous approval steps must be completed first");
        }

        step.Status = dto.Approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        step.Comments = dto.Comments;
        step.ActionDate = DateTime.UtcNow;

        // If rejected, reject the entire request
        if (!dto.Approved)
        {
            step.LeaveRequest.Status = LeaveStatus.Rejected;
            step.LeaveRequest.ApprovedById = approverId;
            step.LeaveRequest.ApprovedAt = DateTime.UtcNow;
            step.LeaveRequest.ApproverComments = dto.Comments;

            // Cancel remaining steps
            var remainingSteps = await _context.ApprovalSteps
                .Where(s => s.LeaveRequestId == step.LeaveRequestId && s.StepOrder > step.StepOrder)
                .ToListAsync();

            foreach (var remaining in remainingSteps)
            {
                remaining.Status = ApprovalStatus.Skipped;
            }

            // Notify employee
            await _notificationService.SendLeaveRequestNotificationAsync(
                step.LeaveRequest.EmployeeId,
                step.LeaveRequestId,
                "rejected"
            );
        }
        else
        {
            // Check if this was the last step
            var remainingSteps = await _context.ApprovalSteps
                .Where(s => s.LeaveRequestId == step.LeaveRequestId && s.StepOrder > step.StepOrder)
                .ToListAsync();

            if (!remainingSteps.Any())
            {
                // All steps approved - approve the request
                step.LeaveRequest.Status = LeaveStatus.Approved;
                step.LeaveRequest.ApprovedById = approverId;
                step.LeaveRequest.ApprovedAt = DateTime.UtcNow;
                step.LeaveRequest.ApproverComments = "Approved through multi-level workflow";

                // Update leave balance
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(lb =>
                        lb.EmployeeId == step.LeaveRequest.EmployeeId &&
                        lb.LeaveTypeId == step.LeaveRequest.LeaveTypeId &&
                        lb.Year == step.LeaveRequest.StartDate.Year);

                if (balance != null)
                {
                    balance.UsedDays += step.LeaveRequest.TotalDays;
                }

                // Notify employee
                await _notificationService.SendLeaveRequestNotificationAsync(
                    step.LeaveRequest.EmployeeId,
                    step.LeaveRequestId,
                    "approved"
                );
            }
            else
            {
                // Notify next approver
                var nextStep = remainingSteps.OrderBy(s => s.StepOrder).First();
                await _notificationService.CreateNotificationAsync(new Models.Notification
                {
                    UserId = nextStep.ApproverId,
                    Title = "Pending Approval",
                    Message = $"Leave request from {step.LeaveRequest.Employee.FirstName} {step.LeaveRequest.Employee.LastName} requires your approval",
                    Type = "approval",
                    Link = $"/approvals/{step.LeaveRequestId}"
                });
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ApprovalStepDto>.SuccessResponse(new ApprovalStepDto
        {
            Id = step.Id,
            LeaveRequestId = step.LeaveRequestId,
            ApproverId = step.ApproverId,
            ApproverName = $"{step.Approver.FirstName} {step.Approver.LastName}",
            StepOrder = step.StepOrder,
            Status = step.Status.ToString(),
            Comments = step.Comments,
            ActionDate = step.ActionDate
        }, dto.Approved ? "Approval step approved" : "Request rejected");
    }

    public async Task<ApiResponse<bool>> InitiateWorkflowAsync(int leaveRequestId)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

        if (leaveRequest == null)
        {
            return ApiResponse<bool>.FailureResponse("Leave request not found");
        }

        // Simple workflow: Employee's manager, then their manager (if exists)
        var approvers = new List<int>();
        
        if (leaveRequest.Employee.ManagerId.HasValue)
        {
            approvers.Add(leaveRequest.Employee.ManagerId.Value);

            // Get manager's manager for escalation
            var manager = await _context.Employees.FindAsync(leaveRequest.Employee.ManagerId.Value);
            if (manager?.ManagerId.HasValue == true)
            {
                approvers.Add(manager.ManagerId.Value);
            }
        }

        if (!approvers.Any())
        {
            // No managers - auto-approve or require admin approval
            var admin = await _context.Employees.FirstOrDefaultAsync(e => e.Role == Role.Admin);
            if (admin != null)
            {
                approvers.Add(admin.Id);
            }
        }

        // Create approval steps
        for (int i = 0; i < approvers.Count; i++)
        {
            _context.ApprovalSteps.Add(new ApprovalStep
            {
                LeaveRequestId = leaveRequestId,
                ApproverId = approvers[i],
                StepOrder = i + 1,
                Status = ApprovalStatus.Pending
            });
        }

        await _context.SaveChangesAsync();

        // Notify first approver
        if (approvers.Any())
        {
            await _notificationService.CreateNotificationAsync(new Models.Notification
            {
                UserId = approvers[0],
                Title = "New Leave Request",
                Message = $"Leave request from {leaveRequest.Employee.FirstName} {leaveRequest.Employee.LastName} requires your approval",
                Type = "approval",
                Link = $"/approvals/{leaveRequestId}"
            });
        }

        return ApiResponse<bool>.SuccessResponse(true, $"Workflow initiated with {approvers.Count} approval step(s)");
    }

    public async Task<IEnumerable<ApprovalStepDto>> GetPendingApprovalsForUserAsync(int approverId)
    {
        return await _context.ApprovalSteps
            .Where(s => s.ApproverId == approverId && s.Status == ApprovalStatus.Pending)
            .Include(s => s.LeaveRequest)
                .ThenInclude(lr => lr.Employee)
            .Include(s => s.Approver)
            .Select(s => new ApprovalStepDto
            {
                Id = s.Id,
                LeaveRequestId = s.LeaveRequestId,
                ApproverId = s.ApproverId,
                ApproverName = $"{s.Approver.FirstName} {s.Approver.LastName}",
                StepOrder = s.StepOrder,
                Status = s.Status.ToString(),
                Comments = s.Comments,
                ActionDate = s.ActionDate
            })
            .ToListAsync();
    }
}
