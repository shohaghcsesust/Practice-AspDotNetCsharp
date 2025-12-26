using System.Text.Json;
using LeaveManagementApi.Data;
using LeaveManagementApi.Models;

namespace LeaveManagementApi.Services;

public class AuditService : IAuditService
{
    private readonly LeaveDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(LeaveDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(int? userId, string userEmail, AuditAction action, string entityType, string? entityId = null, object? oldValues = null, object? newValues = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = false }) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = false }) : null,
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {Action} on {EntityType} by {UserEmail}", action, entityType, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for {Action} on {EntityType}", action, entityType);
            // Don't throw - audit failure shouldn't break the main operation
        }
    }
}
