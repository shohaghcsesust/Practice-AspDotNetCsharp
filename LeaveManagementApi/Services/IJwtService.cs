using LeaveManagementApi.Models;

namespace LeaveManagementApi.Services;

public interface IJwtService
{
    string GenerateAccessToken(Employee employee);
    RefreshToken GenerateRefreshToken(int employeeId);
    int? ValidateAccessToken(string token);
}
