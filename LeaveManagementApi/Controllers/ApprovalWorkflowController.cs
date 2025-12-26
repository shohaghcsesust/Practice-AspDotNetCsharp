using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalWorkflowController : ControllerBase
{
    private readonly IApprovalWorkflowService _workflowService;

    public ApprovalWorkflowController(IApprovalWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("leave-request/{leaveRequestId}")]
    public async Task<ActionResult<IEnumerable<ApprovalStepDto>>> GetApprovalSteps(int leaveRequestId)
    {
        var steps = await _workflowService.GetApprovalStepsAsync(leaveRequestId);
        return Ok(steps);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<ApprovalStepDto>>> GetPendingApprovals()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var steps = await _workflowService.GetPendingApprovalsForUserAsync(userId);
        return Ok(steps);
    }

    [HttpPost("process")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<ApiResponse<ApprovalStepDto>>> ProcessApproval(ProcessApprovalDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _workflowService.ProcessApprovalStepAsync(userId, dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("initiate/{leaveRequestId}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<bool>>> InitiateWorkflow(int leaveRequestId)
    {
        var result = await _workflowService.InitiateWorkflowAsync(leaveRequestId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
