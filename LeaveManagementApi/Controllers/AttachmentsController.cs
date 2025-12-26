using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet("leave-request/{leaveRequestId}")]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetAttachments(int leaveRequestId)
    {
        var attachments = await _attachmentService.GetAttachmentsAsync(leaveRequestId);
        return Ok(attachments);
    }

    [HttpPost("leave-request/{leaveRequestId}")]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> UploadAttachment(int leaveRequestId, IFormFile file)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _attachmentService.UploadAttachmentAsync(leaveRequestId, userId, file);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var result = await _attachmentService.DownloadAttachmentAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return File(result.Value.FileData, result.Value.ContentType, result.Value.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAttachment(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _attachmentService.DeleteAttachmentAsync(id, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
