using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LeaveManagementApi.Services;

public interface IExportService
{
    Task<byte[]> ExportLeaveRequestsToCsvAsync(LeaveReportFilterDto filter);
    Task<byte[]> ExportLeaveBalancesToCsvAsync(int year);
    Task<byte[]> ExportEmployeesToCsvAsync();
}

public class ExportService : IExportService
{
    private readonly LeaveDbContext _context;
    private readonly IReportService _reportService;

    public ExportService(LeaveDbContext context, IReportService reportService)
    {
        _context = context;
        _reportService = reportService;
    }

    public async Task<byte[]> ExportLeaveRequestsToCsvAsync(LeaveReportFilterDto filter)
    {
        var data = await _reportService.GetLeaveReportAsync(filter);
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("ID,Employee Name,Department,Leave Type,Start Date,End Date,Total Days,Status,Reason,Created At");

        // Data
        foreach (var item in data)
        {
            sb.AppendLine($"{item.Id},{EscapeCsv(item.EmployeeName)},{EscapeCsv(item.Department)},{EscapeCsv(item.LeaveTypeName)},{item.StartDate:yyyy-MM-dd},{item.EndDate:yyyy-MM-dd},{item.TotalDays},{item.Status},{EscapeCsv(item.Reason)},{item.CreatedAt:yyyy-MM-dd HH:mm}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportLeaveBalancesToCsvAsync(int year)
    {
        var data = await _reportService.GetLeaveBalanceReportAsync(year);
        var sb = new StringBuilder();

        // Get all leave types for header
        var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).Select(lt => lt.Name).ToListAsync();
        
        // Header
        sb.Append("Employee ID,Employee Name,Department");
        foreach (var lt in leaveTypes)
        {
            sb.Append($",{lt} Total,{lt} Used,{lt} Remaining");
        }
        sb.AppendLine();

        // Data
        foreach (var employee in data)
        {
            sb.Append($"{employee.EmployeeId},{EscapeCsv(employee.EmployeeName)},{EscapeCsv(employee.Department)}");
            
            foreach (var lt in leaveTypes)
            {
                var balance = employee.Balances.FirstOrDefault(b => b.LeaveTypeName == lt);
                if (balance != null)
                {
                    sb.Append($",{balance.TotalDays},{balance.UsedDays},{balance.RemainingDays}");
                }
                else
                {
                    sb.Append(",0,0,0");
                }
            }
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportEmployeesToCsvAsync()
    {
        var employees = await _context.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

        var sb = new StringBuilder();

        // Header
        sb.AppendLine("ID,First Name,Last Name,Email,Department,Position,Role,Hire Date,Manager ID");

        // Data
        foreach (var emp in employees)
        {
            sb.AppendLine($"{emp.Id},{EscapeCsv(emp.FirstName)},{EscapeCsv(emp.LastName)},{emp.Email},{EscapeCsv(emp.Department)},{EscapeCsv(emp.Position)},{emp.Role},{emp.HireDate:yyyy-MM-dd},{emp.ManagerId?.ToString() ?? ""}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
