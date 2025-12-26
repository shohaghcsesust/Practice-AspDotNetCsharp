using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using LeaveManagementApi.Configuration;

namespace LeaveManagementApi.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendLeaveRequestNotificationAsync(string toEmail, string employeeName, DateTime startDate, DateTime endDate, string leaveType)
    {
        var subject = $"New Leave Request from {employeeName}";
        var body = $@"
            <html>
            <body>
                <h2>New Leave Request</h2>
                <p><strong>Employee:</strong> {employeeName}</p>
                <p><strong>Leave Type:</strong> {leaveType}</p>
                <p><strong>Start Date:</strong> {startDate:yyyy-MM-dd}</p>
                <p><strong>End Date:</strong> {endDate:yyyy-MM-dd}</p>
                <p>Please review and approve/reject this request in the Leave Management System.</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendLeaveApprovalNotificationAsync(string toEmail, string employeeName, DateTime startDate, DateTime endDate, bool isApproved, string? comments)
    {
        var status = isApproved ? "Approved" : "Rejected";
        var subject = $"Leave Request {status}";
        var body = $@"
            <html>
            <body>
                <h2>Leave Request {status}</h2>
                <p>Dear {employeeName},</p>
                <p>Your leave request from <strong>{startDate:yyyy-MM-dd}</strong> to <strong>{endDate:yyyy-MM-dd}</strong> has been <strong>{status.ToLower()}</strong>.</p>
                {(string.IsNullOrEmpty(comments) ? "" : $"<p><strong>Comments:</strong> {comments}</p>")}
                <p>Thank you.</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrEmpty(_settings.SmtpServer))
        {
            _logger.LogWarning("SMTP server not configured. Email to {Email} with subject '{Subject}' was not sent.", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw - email failure shouldn't break the main operation
        }
    }
}
