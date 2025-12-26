using Microsoft.EntityFrameworkCore;
using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;

namespace LeaveManagementApi.Services;

public class AuthService : IAuthService
{
    private readonly LeaveDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        LeaveDbContext context,
        IJwtService jwtService,
        ILeaveBalanceService leaveBalanceService,
        IAuditService auditService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _leaveBalanceService = leaveBalanceService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Employees.AnyAsync(e => e.Email == request.Email))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            return null;
        }

        // Validate manager if specified
        if (request.ManagerId.HasValue)
        {
            var manager = await _context.Employees.FindAsync(request.ManagerId.Value);
            if (manager == null || (manager.Role != Role.Manager && manager.Role != Role.Admin))
            {
                _logger.LogWarning("Registration failed: Invalid manager ID {ManagerId}", request.ManagerId);
                return null;
            }
        }

        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.Employee,
            ManagerId = request.ManagerId,
            Department = request.Department ?? string.Empty,
            Position = request.Position ?? string.Empty,
            HireDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Initialize leave balances
        await _leaveBalanceService.InitializeEmployeeBalancesAsync(employee.Id);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(employee);
        var refreshToken = _jwtService.GenerateRefreshToken(employee.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(employee.Id, employee.Email, AuditAction.Create, "Employee", employee.Id.ToString());

        _logger.LogInformation("User registered successfully: {Email}", employee.Email);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(employee)
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == request.Email && e.IsActive);

        if (employee == null || !BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
        {
            _logger.LogWarning("Login failed for email: {Email}", request.Email);
            return null;
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(employee);
        var refreshToken = _jwtService.GenerateRefreshToken(employee.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(employee.Id, employee.Email, AuditAction.Login, "Employee", employee.Id.ToString());

        _logger.LogInformation("User logged in successfully: {Email}", employee.Email);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(employee)
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.Employee)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

        if (token == null || token.Employee == null || !token.Employee.IsActive)
        {
            _logger.LogWarning("Refresh token validation failed");
            return null;
        }

        // Revoke old token
        token.IsRevoked = true;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(token.Employee);
        var newRefreshToken = _jwtService.GenerateRefreshToken(token.EmployeeId);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Token refreshed for user: {Email}", token.Employee.Email);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(token.Employee)
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (token == null)
            return false;

        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for employee: {EmployeeId}", token.EmployeeId);
        return true;
    }

    private static UserDto MapToUserDto(Employee employee)
    {
        return new UserDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Role = employee.Role.ToString(),
            ManagerId = employee.ManagerId,
            Department = employee.Department,
            Position = employee.Position
        };
    }
}
