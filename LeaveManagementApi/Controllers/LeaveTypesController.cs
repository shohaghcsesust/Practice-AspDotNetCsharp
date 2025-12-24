using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LeaveTypesController : ControllerBase
{
    private readonly ILeaveTypeService _leaveTypeService;

    public LeaveTypesController(ILeaveTypeService leaveTypeService)
    {
        _leaveTypeService = leaveTypeService;
    }

    /// <summary>
    /// Get all leave types
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetAll()
    {
        var leaveTypes = await _leaveTypeService.GetAllAsync();
        return Ok(leaveTypes);
    }

    /// <summary>
    /// Get active leave types only
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<LeaveTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetActive()
    {
        var leaveTypes = await _leaveTypeService.GetActiveAsync();
        return Ok(leaveTypes);
    }

    /// <summary>
    /// Get leave type by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeaveTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveTypeDto>> GetById(int id)
    {
        var leaveType = await _leaveTypeService.GetByIdAsync(id);
        if (leaveType == null)
        {
            return NotFound(new { message = "Leave type not found" });
        }
        return Ok(leaveType);
    }

    /// <summary>
    /// Create a new leave type
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LeaveTypeDto>>> Create([FromBody] CreateLeaveTypeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _leaveTypeService.CreateAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing leave type
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaveTypeDto>>> Update(int id, [FromBody] UpdateLeaveTypeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _leaveTypeService.UpdateAsync(id, dto);
        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a leave type
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _leaveTypeService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
