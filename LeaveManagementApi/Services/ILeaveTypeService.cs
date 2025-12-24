using LeaveManagementApi.DTOs;

namespace LeaveManagementApi.Services;

public interface ILeaveTypeService
{
    Task<IEnumerable<LeaveTypeDto>> GetAllAsync();
    Task<IEnumerable<LeaveTypeDto>> GetActiveAsync();
    Task<LeaveTypeDto?> GetByIdAsync(int id);
    Task<ApiResponse<LeaveTypeDto>> CreateAsync(CreateLeaveTypeDto dto);
    Task<ApiResponse<LeaveTypeDto>> UpdateAsync(int id, UpdateLeaveTypeDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
