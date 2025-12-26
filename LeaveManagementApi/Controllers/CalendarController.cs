using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetCalendarEvents([FromQuery] CalendarFilterDto filter)
    {
        var events = await _calendarService.GetCalendarEventsAsync(filter);
        return Ok(events);
    }

    [HttpGet("team")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetTeamCalendar(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var events = await _calendarService.GetTeamCalendarAsync(userId, startDate, endDate);
        return Ok(events);
    }
}
