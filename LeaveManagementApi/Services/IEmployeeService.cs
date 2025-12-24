using LeaveManagementApi.DTOs;

namespace LeaveManagementApi.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeDto dto);
    Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
