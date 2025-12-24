using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using LeaveManagementApi.Repositories;

namespace LeaveManagementApi.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        IEmployeeRepository employeeRepository,
        ILeaveTypeRepository leaveTypeRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync()
    {
        var leaveRequests = await _leaveRequestRepository.GetAllAsync();
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var leaveRequests = await _leaveRequestRepository.GetByEmployeeIdAsync(employeeId);
        return leaveRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingAsync()
    {
        var leaveRequests = await _leaveRequestRepository.GetPendingAsync();
        return leaveRequests.Select(MapToDto);
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        return leaveRequest != null ? MapToDto(leaveRequest) : null;
    }

    public async Task<ApiResponse<LeaveRequestDto>> CreateAsync(CreateLeaveRequestDto dto)
    {
        // Validate employee exists
        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Employee not found.");
        }

        // Validate leave type exists
        var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId);
        if (leaveType == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Leave type not found.");
        }

        // Validate dates
        if (dto.EndDate < dto.StartDate)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "End date must be greater than or equal to start date.");
        }

        if (dto.StartDate.Date < DateTime.Today)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "Start date cannot be in the past.");
        }

        // Check for overlapping requests
        var hasOverlapping = await _leaveRequestRepository.HasOverlappingRequestAsync(
            dto.EmployeeId, dto.StartDate, dto.EndDate);
        if (hasOverlapping)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "There is already a leave request for the selected dates.");
        }

        var totalDays = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending
        };

        var createdRequest = await _leaveRequestRepository.CreateAsync(leaveRequest);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(createdRequest.Id);
        
        return ApiResponse<LeaveRequestDto>.SuccessResponse(
            MapToDto(result!), 
            "Leave request created successfully.");
    }

    public async Task<ApiResponse<LeaveRequestDto>> UpdateAsync(int id, UpdateLeaveRequestDto dto)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Leave request not found.");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "Only pending leave requests can be updated.");
        }

        // Validate leave type exists
        var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId);
        if (leaveType == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Leave type not found.");
        }

        // Validate dates
        if (dto.EndDate < dto.StartDate)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "End date must be greater than or equal to start date.");
        }

        // Check for overlapping requests (excluding current request)
        var hasOverlapping = await _leaveRequestRepository.HasOverlappingRequestAsync(
            leaveRequest.EmployeeId, dto.StartDate, dto.EndDate, id);
        if (hasOverlapping)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "There is already a leave request for the selected dates.");
        }

        var totalDays = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;

        leaveRequest.LeaveTypeId = dto.LeaveTypeId;
        leaveRequest.StartDate = dto.StartDate;
        leaveRequest.EndDate = dto.EndDate;
        leaveRequest.TotalDays = totalDays;
        leaveRequest.Reason = dto.Reason;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);
        
        return ApiResponse<LeaveRequestDto>.SuccessResponse(
            MapToDto(result!), 
            "Leave request updated successfully.");
    }

    public async Task<ApiResponse<LeaveRequestDto>> ApproveAsync(int id, ApproveRejectLeaveDto dto)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Leave request not found.");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "Only pending leave requests can be approved.");
        }

        // Validate approver exists
        var approver = await _employeeRepository.GetByIdAsync(dto.ApprovedById);
        if (approver == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Approver not found.");
        }

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedById = dto.ApprovedById;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = dto.Comments;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);
        
        return ApiResponse<LeaveRequestDto>.SuccessResponse(
            MapToDto(result!), 
            "Leave request approved successfully.");
    }

    public async Task<ApiResponse<LeaveRequestDto>> RejectAsync(int id, ApproveRejectLeaveDto dto)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Leave request not found.");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "Only pending leave requests can be rejected.");
        }

        // Validate approver exists
        var approver = await _employeeRepository.GetByIdAsync(dto.ApprovedById);
        if (approver == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Approver not found.");
        }

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedById = dto.ApprovedById;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = dto.Comments;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);
        
        return ApiResponse<LeaveRequestDto>.SuccessResponse(
            MapToDto(result!), 
            "Leave request rejected.");
    }

    public async Task<ApiResponse<LeaveRequestDto>> CancelAsync(int id)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse("Leave request not found.");
        }

        if (leaveRequest.Status == LeaveStatus.Cancelled)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "Leave request is already cancelled.");
        }

        leaveRequest.Status = LeaveStatus.Cancelled;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);
        
        return ApiResponse<LeaveRequestDto>.SuccessResponse(
            MapToDto(result!), 
            "Leave request cancelled successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var exists = await _leaveRequestRepository.ExistsAsync(id);
        if (!exists)
        {
            return ApiResponse<bool>.FailureResponse("Leave request not found.");
        }

        var result = await _leaveRequestRepository.DeleteAsync(id);
        return result 
            ? ApiResponse<bool>.SuccessResponse(true, "Leave request deleted successfully.")
            : ApiResponse<bool>.FailureResponse("Failed to delete leave request.");
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leaveRequest)
    {
        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = leaveRequest.Employee != null 
                ? $"{leaveRequest.Employee.FirstName} {leaveRequest.Employee.LastName}" 
                : string.Empty,
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = leaveRequest.LeaveType?.Name ?? string.Empty,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            TotalDays = leaveRequest.TotalDays,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status.ToString(),
            ApproverComments = leaveRequest.ApproverComments,
            ApprovedByName = leaveRequest.ApprovedBy != null 
                ? $"{leaveRequest.ApprovedBy.FirstName} {leaveRequest.ApprovedBy.LastName}" 
                : null,
            ApprovedAt = leaveRequest.ApprovedAt,
            CreatedAt = leaveRequest.CreatedAt
        };
    }
}
