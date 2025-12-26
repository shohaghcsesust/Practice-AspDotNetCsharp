using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveBalanceController : ControllerBase
{
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly ILogger<LeaveBalanceController> _logger;

    public LeaveBalanceController(ILeaveBalanceService leaveBalanceService, ILogger<LeaveBalanceController> logger)
    {
        _leaveBalanceService = leaveBalanceService;
        _logger = logger;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get current user's leave balances
    /// </summary>
    [HttpGet("my-balances")]
    [ProducesResponseType(typeof(IEnumerable<LeaveBalanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetMyBalances([FromQuery] int? year = null)
    {
        var userId = GetCurrentUserId();
        var balances = await _leaveBalanceService.GetEmployeeBalancesAsync(userId, year);
        return Ok(balances);
    }

    /// <summary>
    /// Get leave balances for a specific employee (Manager/Admin only)
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<LeaveBalanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetEmployeeBalances(int employeeId, [FromQuery] int? year = null)
    {
        var balances = await _leaveBalanceService.GetEmployeeBalancesAsync(employeeId, year);
        return Ok(balances);
    }
}
