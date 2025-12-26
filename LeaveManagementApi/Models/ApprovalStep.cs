using System.ComponentModel.DataAnnotations;

namespace LeaveManagementApi.Models;

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Skipped
}

public class ApprovalStep
{
    public int Id { get; set; }

    public int LeaveRequestId { get; set; }

    public int ApproverId { get; set; }

    public int StepOrder { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    [MaxLength(500)]
    public string? Comments { get; set; }

    public DateTime? ActionDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public LeaveRequest LeaveRequest { get; set; } = null!;
    public Employee Approver { get; set; } = null!;
}
