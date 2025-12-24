using LeaveManagementApi.Models;

namespace LeaveManagementApi.Repositories;

public interface ILeaveRequestRepository
{
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status);
    Task<IEnumerable<LeaveRequest>> GetPendingAsync();
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> HasOverlappingRequestAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeId = null);
}
