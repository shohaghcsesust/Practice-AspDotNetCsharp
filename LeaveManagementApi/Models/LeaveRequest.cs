using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagementApi.Models;

public class LeaveRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Total number of days requested
    /// </summary>
    public int TotalDays { get; set; }

    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    [MaxLength(500)]
    public string? ApproverComments { get; set; }

    public int? ApprovedById { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("EmployeeId")]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("LeaveTypeId")]
    public virtual LeaveType? LeaveType { get; set; }

    [ForeignKey("ApprovedById")]
    public virtual Employee? ApprovedBy { get; set; }
}

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}
