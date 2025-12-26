using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using LeaveManagementApi.Repositories;

namespace LeaveManagementApi.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly ILogger<LeaveRequestService> _logger;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        IEmployeeRepository employeeRepository,
        ILeaveTypeRepository leaveTypeRepository,
        ILeaveBalanceService leaveBalanceService,
        IEmailService emailService,
        IAuditService auditService,
        ILogger<LeaveRequestService> logger)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveBalanceService = leaveBalanceService;
        _emailService = emailService;
        _auditService = auditService;
        _logger = logger;
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

        var totalDays = CalculateBusinessDays(dto.StartDate, dto.EndDate);

        // Check leave balance
        var hasSufficientBalance = await _leaveBalanceService.HasSufficientBalanceAsync(
            dto.EmployeeId, dto.LeaveTypeId, totalDays);
        if (!hasSufficientBalance)
        {
            return ApiResponse<LeaveRequestDto>.FailureResponse(
                "Insufficient leave balance for this leave type.");
        }

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

        // Audit log
        await _auditService.LogAsync(employee.Id, employee.Email, AuditAction.Create, "LeaveRequest", 
            createdRequest.Id.ToString(), null, new { leaveRequest.LeaveTypeId, leaveRequest.StartDate, leaveRequest.EndDate });

        // Send email notification to manager
        if (employee.ManagerId.HasValue)
        {
            var manager = await _employeeRepository.GetByIdAsync(employee.ManagerId.Value);
            if (manager != null)
            {
                await _emailService.SendLeaveRequestNotificationAsync(
                    manager.Email,
                    $"{employee.FirstName} {employee.LastName}",
                    dto.StartDate,
                    dto.EndDate,
                    leaveType.Name);
            }
        }
        
        return ApiResponse<LeaveRequestDto>.SuccessResponse(
            MapToDto(result!), 
            "Leave request created successfully.");
    }

    private static int CalculateBusinessDays(DateTime start, DateTime end)
    {
        int days = 0;
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                days++;
        }
        return days > 0 ? days : 1; // At least 1 day
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

        var oldStatus = leaveRequest.Status;
        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedById = dto.ApprovedById;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = dto.Comments;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        // Deduct leave balance
        await _leaveBalanceService.DeductBalanceAsync(
            leaveRequest.EmployeeId, 
            leaveRequest.LeaveTypeId, 
            leaveRequest.TotalDays);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);

        // Audit log
        await _auditService.LogAsync(approver.Id, approver.Email, AuditAction.Approve, "LeaveRequest",
            id.ToString(), new { Status = oldStatus.ToString() }, new { Status = leaveRequest.Status.ToString() });

        // Send email notification to employee
        if (result?.Employee != null)
        {
            await _emailService.SendLeaveApprovalNotificationAsync(
                result.Employee.Email,
                $"{result.Employee.FirstName} {result.Employee.LastName}",
                result.StartDate,
                result.EndDate,
                true,
                dto.Comments);
        }
        
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

        var oldStatus = leaveRequest.Status;
        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedById = dto.ApprovedById;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = dto.Comments;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);

        // Audit log
        await _auditService.LogAsync(approver.Id, approver.Email, AuditAction.Reject, "LeaveRequest",
            id.ToString(), new { Status = oldStatus.ToString() }, new { Status = leaveRequest.Status.ToString() });

        // Send email notification to employee
        if (result?.Employee != null)
        {
            await _emailService.SendLeaveApprovalNotificationAsync(
                result.Employee.Email,
                $"{result.Employee.FirstName} {result.Employee.LastName}",
                result.StartDate,
                result.EndDate,
                false,
                dto.Comments);
        }
        
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

        var wasApproved = leaveRequest.Status == LeaveStatus.Approved;
        leaveRequest.Status = LeaveStatus.Cancelled;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        // If the leave was approved, restore the balance
        if (wasApproved)
        {
            await _leaveBalanceService.RestoreBalanceAsync(
                leaveRequest.EmployeeId,
                leaveRequest.LeaveTypeId,
                leaveRequest.TotalDays);
        }
        
        // Reload with navigation properties
        var result = await _leaveRequestRepository.GetByIdAsync(id);

        // Audit log
        if (result?.Employee != null)
        {
            await _auditService.LogAsync(result.EmployeeId, result.Employee.Email, AuditAction.Update, "LeaveRequest",
                id.ToString(), new { Status = wasApproved ? "Approved" : "Pending" }, new { Status = "Cancelled" });
        }
        
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

    public async Task<PaginatedResult<LeaveRequestDto>> SearchAsync(LeaveRequestSearchDto searchDto)
    {
        var allRequests = await _leaveRequestRepository.GetAllAsync();
        var query = allRequests.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchDto.EmployeeName))
        {
            var searchTerm = searchDto.EmployeeName.ToLower();
            query = query.Where(lr => 
                lr.Employee != null &&
                ($"{lr.Employee.FirstName} {lr.Employee.LastName}").ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(searchDto.Department))
        {
            query = query.Where(lr => lr.Employee != null && lr.Employee.Department == searchDto.Department);
        }

        if (searchDto.LeaveTypeId.HasValue)
        {
            query = query.Where(lr => lr.LeaveTypeId == searchDto.LeaveTypeId.Value);
        }

        if (!string.IsNullOrEmpty(searchDto.Status))
        {
            if (Enum.TryParse<LeaveStatus>(searchDto.Status, true, out var status))
            {
                query = query.Where(lr => lr.Status == status);
            }
        }

        if (searchDto.StartDateFrom.HasValue)
        {
            query = query.Where(lr => lr.StartDate >= searchDto.StartDateFrom.Value);
        }

        if (searchDto.StartDateTo.HasValue)
        {
            query = query.Where(lr => lr.StartDate <= searchDto.StartDateTo.Value);
        }

        if (searchDto.EndDateFrom.HasValue)
        {
            query = query.Where(lr => lr.EndDate >= searchDto.EndDateFrom.Value);
        }

        if (searchDto.EndDateTo.HasValue)
        {
            query = query.Where(lr => lr.EndDate <= searchDto.EndDateTo.Value);
        }

        if (searchDto.MinDays.HasValue)
        {
            query = query.Where(lr => lr.TotalDays >= searchDto.MinDays.Value);
        }

        if (searchDto.MaxDays.HasValue)
        {
            query = query.Where(lr => lr.TotalDays <= searchDto.MaxDays.Value);
        }

        // Apply sorting
        query = searchDto.SortBy?.ToLower() switch
        {
            "startdate" => searchDto.SortDescending ? query.OrderByDescending(lr => lr.StartDate) : query.OrderBy(lr => lr.StartDate),
            "enddate" => searchDto.SortDescending ? query.OrderByDescending(lr => lr.EndDate) : query.OrderBy(lr => lr.EndDate),
            "totaldays" => searchDto.SortDescending ? query.OrderByDescending(lr => lr.TotalDays) : query.OrderBy(lr => lr.TotalDays),
            "status" => searchDto.SortDescending ? query.OrderByDescending(lr => lr.Status) : query.OrderBy(lr => lr.Status),
            _ => query.OrderByDescending(lr => lr.CreatedAt)
        };

        var totalCount = query.Count();
        var page = Math.Max(1, searchDto.Page);
        var pageSize = Math.Max(1, Math.Min(100, searchDto.PageSize));

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PaginatedResult<LeaveRequestDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<BatchResultDto> BatchApproveAsync(BatchApproveDto dto)
    {
        var result = new BatchResultDto
        {
            TotalProcessed = dto.LeaveRequestIds.Count,
            Results = new List<BatchResultItemDto>()
        };

        foreach (var requestId in dto.LeaveRequestIds)
        {
            try
            {
                var approveResult = await ApproveAsync(requestId, new ApproveRejectLeaveDto
                {
                    ApprovedById = dto.ApprovedById,
                    Comments = dto.Comments
                });

                result.Results.Add(new BatchResultItemDto
                {
                    Id = requestId,
                    Success = approveResult.Success,
                    Message = approveResult.Message
                });

                if (approveResult.Success)
                    result.SuccessCount++;
                else
                    result.FailureCount++;
            }
            catch (Exception ex)
            {
                result.Results.Add(new BatchResultItemDto
                {
                    Id = requestId,
                    Success = false,
                    Message = ex.Message
                });
                result.FailureCount++;
            }
        }

        return result;
    }

    public async Task<BatchResultDto> BatchRejectAsync(BatchRejectDto dto)
    {
        var result = new BatchResultDto
        {
            TotalProcessed = dto.LeaveRequestIds.Count,
            Results = new List<BatchResultItemDto>()
        };

        foreach (var requestId in dto.LeaveRequestIds)
        {
            try
            {
                var rejectResult = await RejectAsync(requestId, new ApproveRejectLeaveDto
                {
                    ApprovedById = dto.RejectedById,
                    Comments = dto.Comments
                });

                result.Results.Add(new BatchResultItemDto
                {
                    Id = requestId,
                    Success = rejectResult.Success,
                    Message = rejectResult.Message
                });

                if (rejectResult.Success)
                    result.SuccessCount++;
                else
                    result.FailureCount++;
            }
            catch (Exception ex)
            {
                result.Results.Add(new BatchResultItemDto
                {
                    Id = requestId,
                    Success = false,
                    Message = ex.Message
                });
                result.FailureCount++;
            }
        }

        return result;
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
