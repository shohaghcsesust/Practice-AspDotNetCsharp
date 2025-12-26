using LeaveManagementApi.DTOs;

namespace LeaveManagementApi.Services;

public interface ILeaveBalanceService
{
    Task<IEnumerable<LeaveBalanceDto>> GetEmployeeBalancesAsync(int employeeId, int? year = null);
    Task InitializeEmployeeBalancesAsync(int employeeId);
    Task<bool> HasSufficientBalanceAsync(int employeeId, int leaveTypeId, decimal daysRequested);
    Task DeductBalanceAsync(int employeeId, int leaveTypeId, decimal days);
    Task RestoreBalanceAsync(int employeeId, int leaveTypeId, decimal days);
    Task<LeaveBalanceDto?> AdjustBalanceAsync(int employeeId, int leaveTypeId, decimal totalDays);
}
