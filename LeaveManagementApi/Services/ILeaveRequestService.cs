using LeaveManagementApi.DTOs;

namespace LeaveManagementApi.Services;

public interface ILeaveRequestService
{
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync();
    Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequestDto>> GetPendingAsync();
    Task<LeaveRequestDto?> GetByIdAsync(int id);
    Task<ApiResponse<LeaveRequestDto>> CreateAsync(CreateLeaveRequestDto dto);
    Task<ApiResponse<LeaveRequestDto>> UpdateAsync(int id, UpdateLeaveRequestDto dto);
    Task<ApiResponse<LeaveRequestDto>> ApproveAsync(int id, ApproveRejectLeaveDto dto);
    Task<ApiResponse<LeaveRequestDto>> RejectAsync(int id, ApproveRejectLeaveDto dto);
    Task<ApiResponse<LeaveRequestDto>> CancelAsync(int id);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
