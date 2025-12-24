using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using LeaveManagementApi.Repositories;

namespace LeaveManagementApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee != null ? MapToDto(employee) : null;
    }

    public async Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        // Check if email already exists
        var existingEmployee = await _employeeRepository.GetByEmailAsync(dto.Email);
        if (existingEmployee != null)
        {
            return ApiResponse<EmployeeDto>.FailureResponse(
                "An employee with this email already exists.");
        }

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Department = dto.Department,
            Position = dto.Position,
            HireDate = dto.HireDate,
            IsActive = true
        };

        var createdEmployee = await _employeeRepository.CreateAsync(employee);
        return ApiResponse<EmployeeDto>.SuccessResponse(
            MapToDto(createdEmployee), 
            "Employee created successfully.");
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            return ApiResponse<EmployeeDto>.FailureResponse("Employee not found.");
        }

        // Check if email is being changed and if new email already exists
        if (!string.Equals(employee.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingEmployee = await _employeeRepository.GetByEmailAsync(dto.Email);
            if (existingEmployee != null)
            {
                return ApiResponse<EmployeeDto>.FailureResponse(
                    "An employee with this email already exists.");
            }
        }

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Department = dto.Department;
        employee.Position = dto.Position;
        employee.IsActive = dto.IsActive;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee);
        return ApiResponse<EmployeeDto>.SuccessResponse(
            MapToDto(updatedEmployee), 
            "Employee updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var exists = await _employeeRepository.ExistsAsync(id);
        if (!exists)
        {
            return ApiResponse<bool>.FailureResponse("Employee not found.");
        }

        var result = await _employeeRepository.DeleteAsync(id);
        return result 
            ? ApiResponse<bool>.SuccessResponse(true, "Employee deleted successfully.")
            : ApiResponse<bool>.FailureResponse("Failed to delete employee.");
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Department = employee.Department,
            Position = employee.Position,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive
        };
    }
}
