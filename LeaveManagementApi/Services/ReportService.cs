using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface IReportService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<LeaveReportDto>> GetLeaveReportAsync(LeaveReportFilterDto filter);
    Task<IEnumerable<EmployeeLeaveBalanceReportDto>> GetLeaveBalanceReportAsync(int year);
    Task<IEnumerable<MonthlyLeaveStatsDto>> GetMonthlyLeaveStatsAsync(int year);
    Task<IEnumerable<DepartmentLeaveStatsDto>> GetDepartmentLeaveStatsAsync(int year);
    Task<IEnumerable<LeaveTypeStatsDto>> GetLeaveTypeStatsAsync(int year);
}

public class ReportService : IReportService
{
    private readonly LeaveDbContext _context;

    public ReportService(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var currentYear = today.Year;

        return new DashboardStatsDto
        {
            TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
            PendingRequests = await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveStatus.Pending),
            ApprovedToday = await _context.LeaveRequests.CountAsync(lr => 
                lr.Status == LeaveStatus.Approved && 
                lr.ApprovedAt.HasValue && 
                lr.ApprovedAt.Value.Date == today),
            OnLeaveToday = await _context.LeaveRequests.CountAsync(lr =>
                lr.Status == LeaveStatus.Approved &&
                lr.StartDate <= today &&
                lr.EndDate >= today),
            TotalLeavesTaken = await _context.LeaveRequests
                .Where(lr => lr.Status == LeaveStatus.Approved && lr.StartDate.Year == currentYear)
                .SumAsync(lr => lr.TotalDays),
            TotalLeaveDaysAvailable = await _context.LeaveBalances
                .Where(lb => lb.Year == currentYear)
                .SumAsync(lb => lb.TotalDays),
            ActiveLeaveTypes = await _context.LeaveTypes.CountAsync(lt => lt.IsActive),
            UpcomingHolidays = await _context.PublicHolidays
                .CountAsync(h => h.IsActive && h.Date >= today && h.Date <= today.AddMonths(3))
        };
    }

    public async Task<IEnumerable<LeaveReportDto>> GetLeaveReportAsync(LeaveReportFilterDto filter)
    {
        var query = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(lr => lr.StartDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(lr => lr.EndDate <= filter.EndDate.Value);

        if (filter.EmployeeId.HasValue)
            query = query.Where(lr => lr.EmployeeId == filter.EmployeeId.Value);

        if (filter.LeaveTypeId.HasValue)
            query = query.Where(lr => lr.LeaveTypeId == filter.LeaveTypeId.Value);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(lr => lr.Status.ToString() == filter.Status);

        if (!string.IsNullOrEmpty(filter.Department))
            query = query.Where(lr => lr.Employee.Department == filter.Department);

        return await query
            .OrderByDescending(lr => lr.CreatedAt)
            .Select(lr => new LeaveReportDto
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeName = $"{lr.Employee.FirstName} {lr.Employee.LastName}",
                Department = lr.Employee.Department,
                LeaveTypeName = lr.LeaveType.Name,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Status = lr.Status.ToString(),
                Reason = lr.Reason,
                CreatedAt = lr.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeLeaveBalanceReportDto>> GetLeaveBalanceReportAsync(int year)
    {
        return await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => new EmployeeLeaveBalanceReportDto
            {
                EmployeeId = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}",
                Department = e.Department,
                Balances = _context.LeaveBalances
                    .Where(lb => lb.EmployeeId == e.Id && lb.Year == year)
                    .Include(lb => lb.LeaveType)
                    .Select(lb => new LeaveBalanceItemDto
                    {
                        LeaveTypeName = lb.LeaveType.Name,
                        TotalDays = lb.TotalDays,
                        UsedDays = lb.UsedDays,
                        RemainingDays = lb.TotalDays - lb.UsedDays
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<MonthlyLeaveStatsDto>> GetMonthlyLeaveStatsAsync(int year)
    {
        var monthlyStats = await _context.LeaveRequests
            .Where(lr => lr.Status == LeaveStatus.Approved && lr.StartDate.Year == year)
            .GroupBy(lr => lr.StartDate.Month)
            .Select(g => new MonthlyLeaveStatsDto
            {
                Month = g.Key,
                MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                TotalRequests = g.Count(),
                TotalDays = g.Sum(lr => lr.TotalDays)
            })
            .OrderBy(s => s.Month)
            .ToListAsync();

        // Fill in missing months with zeros
        var allMonths = Enumerable.Range(1, 12)
            .Select(m => monthlyStats.FirstOrDefault(ms => ms.Month == m) ?? new MonthlyLeaveStatsDto
            {
                Month = m,
                MonthName = new DateTime(year, m, 1).ToString("MMMM"),
                TotalRequests = 0,
                TotalDays = 0
            })
            .ToList();

        return allMonths;
    }

    public async Task<IEnumerable<DepartmentLeaveStatsDto>> GetDepartmentLeaveStatsAsync(int year)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.Status == LeaveStatus.Approved && lr.StartDate.Year == year)
            .Include(lr => lr.Employee)
            .GroupBy(lr => lr.Employee.Department)
            .Select(g => new DepartmentLeaveStatsDto
            {
                Department = g.Key ?? "Unassigned",
                TotalRequests = g.Count(),
                TotalDays = g.Sum(lr => lr.TotalDays),
                EmployeeCount = g.Select(lr => lr.EmployeeId).Distinct().Count()
            })
            .OrderByDescending(s => s.TotalDays)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveTypeStatsDto>> GetLeaveTypeStatsAsync(int year)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.Status == LeaveStatus.Approved && lr.StartDate.Year == year)
            .Include(lr => lr.LeaveType)
            .GroupBy(lr => new { lr.LeaveTypeId, lr.LeaveType.Name })
            .Select(g => new LeaveTypeStatsDto
            {
                LeaveTypeId = g.Key.LeaveTypeId,
                LeaveTypeName = g.Key.Name,
                TotalRequests = g.Count(),
                TotalDays = g.Sum(lr => lr.TotalDays),
                AverageDays = Math.Round(g.Average(lr => (double)lr.TotalDays), 1)
            })
            .OrderByDescending(s => s.TotalDays)
            .ToListAsync();
    }
}
