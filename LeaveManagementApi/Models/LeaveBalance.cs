using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagementApi.Models;

public class LeaveBalance
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public int Year { get; set; }

    /// <summary>
    /// Total days allocated for this leave type
    /// </summary>
    public decimal TotalDays { get; set; }

    /// <summary>
    /// Days already used
    /// </summary>
    public decimal UsedDays { get; set; }

    /// <summary>
    /// Remaining days (calculated property)
    /// </summary>
    [NotMapped]
    public decimal RemainingDays => TotalDays - UsedDays;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("EmployeeId")]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("LeaveTypeId")]
    public virtual LeaveType? LeaveType { get; set; }
}
