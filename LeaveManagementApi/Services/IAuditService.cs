using LeaveManagementApi.Models;

namespace LeaveManagementApi.Services;

public interface IAuditService
{
    Task LogAsync(int? userId, string userEmail, AuditAction action, string entityType, string? entityId = null, object? oldValues = null, object? newValues = null);
}
