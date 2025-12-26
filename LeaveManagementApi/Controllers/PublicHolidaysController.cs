using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PublicHolidaysController : ControllerBase
{
    private readonly IPublicHolidayService _holidayService;

    public PublicHolidaysController(IPublicHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PublicHolidayDto>>> GetHolidays([FromQuery] int? year)
    {
        var holidays = await _holidayService.GetHolidaysAsync(year ?? DateTime.Now.Year);
        return Ok(holidays);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PublicHolidayDto>> GetHoliday(int id)
    {
        var holiday = await _holidayService.GetHolidayByIdAsync(id);
        if (holiday == null)
        {
            return NotFound();
        }
        return Ok(holiday);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<PublicHolidayDto>>> CreateHoliday(CreatePublicHolidayDto dto)
    {
        var result = await _holidayService.CreateHolidayAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetHoliday), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<PublicHolidayDto>>> UpdateHoliday(int id, UpdatePublicHolidayDto dto)
    {
        var result = await _holidayService.UpdateHolidayAsync(id, dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteHoliday(int id)
    {
        var result = await _holidayService.DeleteHolidayAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("check/{date}")]
    public async Task<ActionResult<bool>> IsHoliday(DateTime date)
    {
        var isHoliday = await _holidayService.IsHolidayAsync(date);
        return Ok(isHoliday);
    }

    [HttpGet("working-days")]
    public async Task<ActionResult<int>> GetWorkingDays([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var workingDays = await _holidayService.GetWorkingDaysAsync(startDate, endDate);
        return Ok(new { StartDate = startDate, EndDate = endDate, WorkingDays = workingDays });
    }
}
