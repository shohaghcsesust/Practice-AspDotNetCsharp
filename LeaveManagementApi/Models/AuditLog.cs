using System.ComponentModel.DataAnnotations;

namespace LeaveManagementApi.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [MaxLength(150)]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public AuditAction Action { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EntityId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    Approve,
    Reject
}
