using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Hubs;
using LeaveManagementApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(int userId, string title, string message, string type = "info", string? link = null);
    Task CreateNotificationAsync(Notification notification);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteNotificationAsync(int notificationId, int userId);
    Task SendLeaveRequestNotificationAsync(LeaveRequest leaveRequest, string action);
    Task SendLeaveRequestNotificationAsync(int employeeId, int leaveRequestId, string action);
}

public class NotificationService : INotificationService
{
    private readonly LeaveDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(LeaveDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<NotificationDto> CreateNotificationAsync(int userId, string title, string message, string type = "info", string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var dto = new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        // Send real-time notification via SignalR
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", dto);

        return dto;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationRead", notificationId);
            return true;
        }
        return notification != null;
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"user_{userId}").SendAsync("AllNotificationsRead");
    }

    public async Task DeleteNotificationAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var dto = new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        await _hubContext.Clients.Group($"user_{notification.UserId}").SendAsync("ReceiveNotification", dto);
    }

    public async Task SendLeaveRequestNotificationAsync(int employeeId, int leaveRequestId, string action)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);
        
        if (leaveRequest != null)
        {
            await SendLeaveRequestNotificationAsync(leaveRequest, action);
        }
    }

    public async Task SendLeaveRequestNotificationAsync(LeaveRequest leaveRequest, string action)
    {
        var employee = await _context.Employees.FindAsync(leaveRequest.EmployeeId);
        if (employee == null) return;

        switch (action.ToLower())
        {
            case "created":
                // Notify manager
                if (employee.ManagerId.HasValue)
                {
                    await CreateNotificationAsync(
                        employee.ManagerId.Value,
                        "New Leave Request",
                        $"{employee.FirstName} {employee.LastName} has submitted a leave request.",
                        "info",
                        $"/pending-approvals"
                    );
                }
                break;

            case "approved":
                await CreateNotificationAsync(
                    leaveRequest.EmployeeId,
                    "Leave Request Approved",
                    $"Your leave request from {leaveRequest.StartDate:MMM dd} to {leaveRequest.EndDate:MMM dd} has been approved.",
                    "success",
                    $"/leave-requests"
                );
                break;

            case "rejected":
                await CreateNotificationAsync(
                    leaveRequest.EmployeeId,
                    "Leave Request Rejected",
                    $"Your leave request from {leaveRequest.StartDate:MMM dd} to {leaveRequest.EndDate:MMM dd} has been rejected.",
                    "error",
                    $"/leave-requests"
                );
                break;

            case "cancelled":
                if (employee.ManagerId.HasValue)
                {
                    await CreateNotificationAsync(
                        employee.ManagerId.Value,
                        "Leave Request Cancelled",
                        $"{employee.FirstName} {employee.LastName} has cancelled their leave request.",
                        "warning",
                        $"/pending-approvals"
                    );
                }
                break;
        }
    }
}
