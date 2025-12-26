using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarryForwardController : ControllerBase
{
    private readonly ICarryForwardService _carryForwardService;

    public CarryForwardController(ICarryForwardService carryForwardService)
    {
        _carryForwardService = carryForwardService;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<LeaveCarryForwardDto>>> GetEmployeeCarryForwards(int employeeId)
    {
        var carryForwards = await _carryForwardService.GetEmployeeCarryForwardsAsync(employeeId);
        return Ok(carryForwards);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<LeaveCarryForwardDto>>> GetMyCarryForwards()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var carryForwards = await _carryForwardService.GetEmployeeCarryForwardsAsync(userId);
        return Ok(carryForwards);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<LeaveCarryForwardDto>>> ProcessCarryForward(ProcessCarryForwardDto dto)
    {
        var result = await _carryForwardService.ProcessCarryForwardAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("year-end")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> ProcessYearEndCarryForward([FromQuery] int fromYear, [FromQuery] int toYear)
    {
        var result = await _carryForwardService.ProcessYearEndCarryForwardAsync(fromYear, toYear);
        return Ok(result);
    }

    [HttpPost("expire")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ExpireCarryForwards()
    {
        await _carryForwardService.ExpireCarryForwardsAsync();
        return Ok(new { Message = "Expired carry forwards processed successfully" });
    }
}
