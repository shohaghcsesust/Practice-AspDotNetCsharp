using System.ComponentModel.DataAnnotations;

namespace LeaveManagementApi.DTOs;

// ==================== Authentication DTOs ====================

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public int? ManagerId { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}

// ==================== Leave Balance DTOs ====================

public class LeaveBalanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
}

public class AdjustBalanceDto
{
    [Required]
    [Range(0, 365)]
    public decimal TotalDays { get; set; }
}

// ==================== Audit Log DTOs ====================

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

// ==================== Employee DTOs ====================

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }
}

public class UpdateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

// ==================== LeaveType DTOs ====================

public class LeaveTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefaultDays { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLeaveTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 365)]
    public int DefaultDays { get; set; }
}

public class UpdateLeaveTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 365)]
    public int DefaultDays { get; set; }

    public bool IsActive { get; set; }
}

// ==================== LeaveRequest DTOs ====================

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ApproverComments { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLeaveRequestDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class UpdateLeaveRequestDto
{
    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class ApproveRejectLeaveDto
{
    [Required]
    public int ApprovedById { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

// ==================== Common DTOs ====================

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// ==================== Public Holiday DTOs ====================

public class PublicHolidayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePublicHolidayDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsRecurring { get; set; }
}

public class UpdatePublicHolidayDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }
}

// ==================== Notification DTOs ====================

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

// ==================== Carry Forward DTOs ====================

public class LeaveCarryForwardDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int FromYear { get; set; }
    public int ToYear { get; set; }
    public decimal CarriedDays { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
}

public class ProcessCarryForwardDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public int FromYear { get; set; }

    [Required]
    public int ToYear { get; set; }

    [Required]
    [Range(0, 365)]
    public decimal MaxCarryForwardDays { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

// ==================== Attachment DTOs ====================

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
}

// ==================== Report DTOs ====================

public class DashboardStatsDto
{
    public int TotalEmployees { get; set; }
    public int TotalLeaveRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int EmployeesOnLeaveToday { get; set; }
    public int OnLeaveToday { get; set; }
    public int ApprovedToday { get; set; }
    public int TotalLeavesTaken { get; set; }
    public decimal TotalLeaveDaysAvailable { get; set; }
    public int ActiveLeaveTypes { get; set; }
    public int UpcomingHolidays { get; set; }
    public decimal AverageLeaveUtilization { get; set; }
}

public class LeaveReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? LeaveTypeId { get; set; }
    public int? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? Status { get; set; }
}

public class LeaveReportDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MonthlyLeaveStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int TotalDays { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
}

public class DepartmentLeaveStatsDto
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int TotalRequests { get; set; }
    public int TotalLeavesTaken { get; set; }
    public int TotalDaysTaken { get; set; }
    public int TotalDays { get; set; }
    public decimal AverageDaysPerEmployee { get; set; }
}

public class LeaveTypeStatsDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int TotalDays { get; set; }
    public double AverageDays { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class EmployeeBalanceReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<LeaveBalanceDto> Balances { get; set; } = new();
}

public class EmployeeLeaveBalanceReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<LeaveBalanceItemDto> Balances { get; set; } = new();
}

public class LeaveBalanceItemDto
{
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
}

// ==================== Calendar DTOs ====================

public class CalendarEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "leave", "holiday"
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Color { get; set; }
    public string? EmployeeName { get; set; }
    public string? LeaveType { get; set; }
    public string? Status { get; set; }
}

public class CalendarFilterDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public string? Department { get; set; }
    public bool IncludeHolidays { get; set; } = true;
}

// ==================== Approval Workflow DTOs ====================

public class ApprovalStepDto
{
    public int Id { get; set; }
    public int LeaveRequestId { get; set; }
    public int ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime? ActionDate { get; set; }
}

public class ProcessApprovalDto
{
    [Required]
    public int StepId { get; set; }

    [Required]
    public bool Approved { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

// ==================== Advanced Search DTOs ====================

public class LeaveRequestSearchDto
{
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public int? MinDays { get; set; }
    public int? MaxDays { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ==================== Batch Operations DTOs ====================

public class BatchApproveDto
{
    [Required]
    public List<int> LeaveRequestIds { get; set; } = new();

    [Required]
    public int ApprovedById { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class BatchRejectDto
{
    [Required]
    public List<int> LeaveRequestIds { get; set; } = new();

    [Required]
    public int RejectedById { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class BatchResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BatchResultItemDto> Results { get; set; } = new();
}

public class BatchResultItemDto
{
    public int Id { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
