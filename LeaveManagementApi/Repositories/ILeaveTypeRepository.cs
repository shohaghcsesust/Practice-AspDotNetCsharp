using LeaveManagementApi.Models;

namespace LeaveManagementApi.Repositories;

public interface ILeaveTypeRepository
{
    Task<IEnumerable<LeaveType>> GetAllAsync();
    Task<IEnumerable<LeaveType>> GetActiveAsync();
    Task<LeaveType?> GetByIdAsync(int id);
    Task<LeaveType?> GetByNameAsync(string name);
    Task<LeaveType> CreateAsync(LeaveType leaveType);
    Task<LeaveType> UpdateAsync(LeaveType leaveType);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
