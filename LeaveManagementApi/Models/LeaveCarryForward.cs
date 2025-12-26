using System.ComponentModel.DataAnnotations;

namespace LeaveManagementApi.Models;

public class LeaveCarryForward
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public int FromYear { get; set; }

    public int ToYear { get; set; }

    public decimal CarriedDays { get; set; }

    public decimal MaxCarryForwardDays { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsExpired { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
