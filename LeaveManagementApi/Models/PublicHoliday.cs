using System.ComponentModel.DataAnnotations;

namespace LeaveManagementApi.Models;

public class PublicHoliday
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsRecurring { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
