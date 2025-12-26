using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface ICalendarService
{
    Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(CalendarFilterDto filter);
    Task<IEnumerable<CalendarEventDto>> GetTeamCalendarAsync(int managerId, DateTime startDate, DateTime endDate);
}

public class CalendarService : ICalendarService
{
    private readonly LeaveDbContext _context;
    private readonly IPublicHolidayService _holidayService;

    public CalendarService(LeaveDbContext context, IPublicHolidayService holidayService)
    {
        _context = context;
        _holidayService = holidayService;
    }

    public async Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(CalendarFilterDto filter)
    {
        var events = new List<CalendarEventDto>();

        // Get approved leave requests
        var leavesQuery = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.Status == LeaveStatus.Approved)
            .Where(lr => lr.StartDate <= filter.EndDate && lr.EndDate >= filter.StartDate);

        if (filter.EmployeeId.HasValue)
        {
            leavesQuery = leavesQuery.Where(lr => lr.EmployeeId == filter.EmployeeId.Value);
        }

        if (!string.IsNullOrEmpty(filter.Department))
        {
            leavesQuery = leavesQuery.Where(lr => lr.Employee.Department == filter.Department);
        }

        var leaves = await leavesQuery.ToListAsync();

        events.AddRange(leaves.Select(lr => new CalendarEventDto
        {
            Id = lr.Id,
            Title = $"{lr.Employee.FirstName} {lr.Employee.LastName} - {lr.LeaveType.Name}",
            Type = "leave",
            Start = lr.StartDate,
            End = lr.EndDate,
            Color = GetLeaveTypeColor(lr.LeaveType.Name),
            EmployeeName = $"{lr.Employee.FirstName} {lr.Employee.LastName}",
            LeaveType = lr.LeaveType.Name,
            Status = lr.Status.ToString()
        }));

        // Get public holidays
        if (filter.IncludeHolidays)
        {
            var holidays = await _holidayService.GetHolidaysAsync(filter.StartDate.Year);
            var filteredHolidays = holidays.Where(h => h.Date >= filter.StartDate && h.Date <= filter.EndDate);

            events.AddRange(filteredHolidays.Select(h => new CalendarEventDto
            {
                Id = h.Id * -1, // Negative to differentiate from leave requests
                Title = h.Name,
                Type = "holiday",
                Start = h.Date,
                End = h.Date,
                Color = "#EF4444" // Red for holidays
            }));
        }

        return events.OrderBy(e => e.Start);
    }

    public async Task<IEnumerable<CalendarEventDto>> GetTeamCalendarAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        // Get team members
        var teamMemberIds = await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .Select(e => e.Id)
            .ToListAsync();

        // Include manager themselves
        teamMemberIds.Add(managerId);

        return await GetCalendarEventsAsync(new CalendarFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            IncludeHolidays = true
        });
    }

    private static string GetLeaveTypeColor(string leaveType)
    {
        return leaveType.ToLower() switch
        {
            "annual leave" or "vacation" => "#3B82F6", // Blue
            "sick leave" => "#F97316", // Orange
            "personal leave" => "#8B5CF6", // Purple
            "maternity leave" => "#EC4899", // Pink
            "paternity leave" => "#06B6D4", // Cyan
            "unpaid leave" => "#6B7280", // Gray
            _ => "#10B981" // Green default
        };
    }
}
