using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    /// <summary>
    /// Get all leave requests (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAll()
    {
        var leaveRequests = await _leaveRequestService.GetAllAsync();
        return Ok(leaveRequests);
    }

    /// <summary>
    /// Get pending leave requests (Manager/Admin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPending()
    {
        var leaveRequests = await _leaveRequestService.GetPendingAsync();
        return Ok(leaveRequests);
    }

    /// <summary>
    /// Get leave requests by employee ID
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByEmployeeId(int employeeId)
    {
        var leaveRequests = await _leaveRequestService.GetByEmployeeIdAsync(employeeId);
        return Ok(leaveRequests);
    }

    /// <summary>
    /// Get leave request by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveRequestDto>> GetById(int id)
    {
        var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound(new { message = "Leave request not found" });
        }
        return Ok(leaveRequest);
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Create([FromBody] CreateLeaveRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _leaveRequestService.CreateAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing leave request (only pending requests)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Update(int id, [FromBody] UpdateLeaveRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _leaveRequestService.UpdateAsync(id, dto);
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
    /// Approve a leave request (Manager/Admin only)
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Approve(int id, [FromBody] ApproveRejectLeaveDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _leaveRequestService.ApproveAsync(id, dto);
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
    /// Reject a leave request (Manager/Admin only)
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Reject(int id, [FromBody] ApproveRejectLeaveDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _leaveRequestService.RejectAsync(id, dto);
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
    /// Cancel a leave request
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Cancel(int id)
    {
        var result = await _leaveRequestService.CancelAsync(id);
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
    /// Delete a leave request (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _leaveRequestService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Advanced search for leave requests (Admin/Manager only)
    /// </summary>
    [HttpPost("search")]
    [Authorize(Roles = "Manager,Admin,HR")]
    [ProducesResponseType(typeof(PaginatedResult<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<LeaveRequestDto>>> Search([FromBody] LeaveRequestSearchDto searchDto)
    {
        var result = await _leaveRequestService.SearchAsync(searchDto);
        return Ok(result);
    }

    /// <summary>
    /// Batch approve multiple leave requests (Manager/Admin only)
    /// </summary>
    [HttpPost("batch-approve")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(BatchResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchResultDto>> BatchApprove([FromBody] BatchApproveDto dto)
    {
        var result = await _leaveRequestService.BatchApproveAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Batch reject multiple leave requests (Manager/Admin only)
    /// </summary>
    [HttpPost("batch-reject")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(BatchResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchResultDto>> BatchReject([FromBody] BatchRejectDto dto)
    {
        var result = await _leaveRequestService.BatchRejectAsync(dto);
        return Ok(result);
    }
}
