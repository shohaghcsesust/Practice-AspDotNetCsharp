using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface IAttachmentService
{
    Task<IEnumerable<AttachmentDto>> GetAttachmentsAsync(int leaveRequestId);
    Task<ApiResponse<AttachmentDto>> UploadAttachmentAsync(int leaveRequestId, int uploadedById, IFormFile file);
    Task<ApiResponse<bool>> DeleteAttachmentAsync(int attachmentId, int userId);
    Task<(byte[] FileData, string ContentType, string FileName)?> DownloadAttachmentAsync(int attachmentId);
}

public class AttachmentService : IAttachmentService
{
    private readonly LeaveDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public AttachmentService(LeaveDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IEnumerable<AttachmentDto>> GetAttachmentsAsync(int leaveRequestId)
    {
        return await _context.LeaveAttachments
            .Where(a => a.LeaveRequestId == leaveRequestId)
            .Include(a => a.UploadedBy)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt,
                UploadedByName = $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}"
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<AttachmentDto>> UploadAttachmentAsync(int leaveRequestId, int uploadedById, IFormFile file)
    {
        // Validate leave request exists
        var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            return ApiResponse<AttachmentDto>.FailureResponse("Leave request not found");
        }

        // Validate file size
        if (file.Length > MaxFileSize)
        {
            return ApiResponse<AttachmentDto>.FailureResponse("File size exceeds 5MB limit");
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return ApiResponse<AttachmentDto>.FailureResponse($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }

        // Create uploads directory if not exists
        var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "attachments");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save to database
        var attachment = new LeaveAttachment
        {
            LeaveRequestId = leaveRequestId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath,
            UploadedById = uploadedById,
            UploadedAt = DateTime.UtcNow
        };

        _context.LeaveAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        var uploader = await _context.Employees.FindAsync(uploadedById);

        return ApiResponse<AttachmentDto>.SuccessResponse(new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt,
            UploadedByName = uploader != null ? $"{uploader.FirstName} {uploader.LastName}" : "Unknown"
        }, "File uploaded successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAttachmentAsync(int attachmentId, int userId)
    {
        var attachment = await _context.LeaveAttachments
            .Include(a => a.LeaveRequest)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            return ApiResponse<bool>.FailureResponse("Attachment not found");
        }

        // Only allow deletion by uploader or admin
        var user = await _context.Employees.FindAsync(userId);
        if (attachment.UploadedById != userId && user?.Role != Role.Admin)
        {
            return ApiResponse<bool>.FailureResponse("Not authorized to delete this attachment");
        }

        // Delete physical file
        if (File.Exists(attachment.FilePath))
        {
            File.Delete(attachment.FilePath);
        }

        _context.LeaveAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Attachment deleted successfully");
    }

    public async Task<(byte[] FileData, string ContentType, string FileName)?> DownloadAttachmentAsync(int attachmentId)
    {
        var attachment = await _context.LeaveAttachments.FindAsync(attachmentId);
        if (attachment == null || !File.Exists(attachment.FilePath))
        {
            return null;
        }

        var fileData = await File.ReadAllBytesAsync(attachment.FilePath);
        return (fileData, attachment.ContentType, attachment.FileName);
    }
}
