using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using LeaveManagementApi.Services;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly LeaveDbContext _context;
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        LeaveDbContext context, 
        ILeaveBalanceService leaveBalanceService,
        IAuditService auditService,
        ILogger<AdminController> logger)
    {
        _context = context;
        _leaveBalanceService = leaveBalanceService;
        _auditService = auditService;
        _logger = logger;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetCurrentUserEmail() => User.FindFirstValue(ClaimTypes.Email) ?? "";

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetUsers()
    {
        var employees = await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Department = e.Department,
                Position = e.Position,
                Role = e.Role.ToString(),
                ManagerId = e.ManagerId,
                HireDate = e.HireDate,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(employees);
    }

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    [HttpPut("users/{id}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateUserRole(int id, [FromBody] string role)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        if (!Enum.TryParse<Role>(role, true, out var newRole))
            return BadRequest(new { message = "Invalid role. Valid values: Employee, Manager, Admin" });

        var oldRole = employee.Role;
        employee.Role = newRole;
        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(GetCurrentUserId(), GetCurrentUserEmail(), AuditAction.Update, "Employee", 
            id.ToString(), new { Role = oldRole.ToString() }, new { Role = newRole.ToString() });

        return NoContent();
    }

    /// <summary>
    /// Get audit logs (Admin only)
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (!string.IsNullOrEmpty(action) && Enum.TryParse<AuditAction>(action, true, out var auditAction))
            query = query.Where(a => a.Action == auditAction);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Action = a.Action.ToString(),
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return Ok(new PaginatedResult<AuditLogDto>
        {
            Items = logs,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Adjust employee leave balance (Admin only)
    /// </summary>
    [HttpPut("users/{employeeId}/balance/{leaveTypeId}")]
    [ProducesResponseType(typeof(LeaveBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveBalanceDto>> AdjustBalance(int employeeId, int leaveTypeId, [FromBody] AdjustBalanceDto dto)
    {
        var result = await _leaveBalanceService.AdjustBalanceAsync(employeeId, leaveTypeId, dto.TotalDays);

        if (result == null)
            return NotFound(new { message = "Leave balance not found." });

        await _auditService.LogAsync(GetCurrentUserId(), GetCurrentUserEmail(), AuditAction.Update, "LeaveBalance",
            $"{employeeId}-{leaveTypeId}", null, new { TotalDays = dto.TotalDays });

        return Ok(result);
    }

    /// <summary>
    /// Initialize leave balances for an employee (Admin only)
    /// </summary>
    [HttpPost("users/{employeeId}/initialize-balances")]
    [ProducesResponseType(typeof(IEnumerable<LeaveBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> InitializeBalances(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
            return NotFound(new { message = "Employee not found." });

        await _leaveBalanceService.InitializeEmployeeBalancesAsync(employeeId);
        var balances = await _leaveBalanceService.GetEmployeeBalancesAsync(employeeId);

        return Ok(balances);
    }
}
