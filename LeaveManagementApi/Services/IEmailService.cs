namespace LeaveManagementApi.Services;

public interface IEmailService
{
    Task SendLeaveRequestNotificationAsync(string toEmail, string employeeName, DateTime startDate, DateTime endDate, string leaveType);
    Task SendLeaveApprovalNotificationAsync(string toEmail, string employeeName, DateTime startDate, DateTime endDate, bool isApproved, string? comments);
    Task SendEmailAsync(string toEmail, string subject, string body);
}
