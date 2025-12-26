using System.ComponentModel.DataAnnotations;

namespace LeaveManagementApi.Models;

public class LeaveAttachment
{
    public int Id { get; set; }

    public int LeaveRequestId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int UploadedById { get; set; }

    // Navigation properties
    public LeaveRequest LeaveRequest { get; set; } = null!;
    public Employee UploadedBy { get; set; } = null!;
}
