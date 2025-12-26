using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface IPublicHolidayService
{
    Task<IEnumerable<PublicHolidayDto>> GetHolidaysAsync(int year);
    Task<PublicHolidayDto?> GetHolidayByIdAsync(int id);
    Task<ApiResponse<PublicHolidayDto>> CreateHolidayAsync(CreatePublicHolidayDto dto);
    Task<ApiResponse<PublicHolidayDto>> UpdateHolidayAsync(int id, UpdatePublicHolidayDto dto);
    Task<ApiResponse<bool>> DeleteHolidayAsync(int id);
    Task<int> GetWorkingDaysAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsHolidayAsync(DateTime date);
}

public class PublicHolidayService : IPublicHolidayService
{
    private readonly LeaveDbContext _context;

    public PublicHolidayService(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PublicHolidayDto>> GetHolidaysAsync(int year)
    {
        var query = _context.PublicHolidays
            .Where(h => h.IsActive && (h.Date.Year == year || h.IsRecurring));

        return await query
            .OrderBy(h => h.Date)
            .Select(h => new PublicHolidayDto
            {
                Id = h.Id,
                Name = h.Name,
                Date = h.Date,
                Description = h.Description,
                IsRecurring = h.IsRecurring,
                IsActive = h.IsActive
            })
            .ToListAsync();
    }

    public async Task<PublicHolidayDto?> GetHolidayByIdAsync(int id)
    {
        var holiday = await _context.PublicHolidays.FindAsync(id);
        if (holiday == null) return null;

        return new PublicHolidayDto
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Date = holiday.Date,
            Description = holiday.Description,
            IsRecurring = holiday.IsRecurring,
            IsActive = holiday.IsActive
        };
    }

    public async Task<ApiResponse<PublicHolidayDto>> CreateHolidayAsync(CreatePublicHolidayDto dto)
    {
        var holiday = new PublicHoliday
        {
            Name = dto.Name,
            Date = dto.Date,
            Description = dto.Description,
            IsRecurring = dto.IsRecurring,
            IsActive = true
        };

        _context.PublicHolidays.Add(holiday);
        await _context.SaveChangesAsync();

        return ApiResponse<PublicHolidayDto>.SuccessResponse(new PublicHolidayDto
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Date = holiday.Date,
            Description = holiday.Description,
            IsRecurring = holiday.IsRecurring,
            IsActive = holiday.IsActive
        }, "Public holiday created successfully");
    }

    public async Task<ApiResponse<PublicHolidayDto>> UpdateHolidayAsync(int id, UpdatePublicHolidayDto dto)
    {
        var holiday = await _context.PublicHolidays.FindAsync(id);
        if (holiday == null)
        {
            return ApiResponse<PublicHolidayDto>.FailureResponse("Public holiday not found");
        }

        holiday.Name = dto.Name;
        holiday.Date = dto.Date;
        holiday.Description = dto.Description;
        holiday.IsRecurring = dto.IsRecurring;
        holiday.IsActive = dto.IsActive;
        holiday.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<PublicHolidayDto>.SuccessResponse(new PublicHolidayDto
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Date = holiday.Date,
            Description = holiday.Description,
            IsRecurring = holiday.IsRecurring,
            IsActive = holiday.IsActive
        }, "Public holiday updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteHolidayAsync(int id)
    {
        var holiday = await _context.PublicHolidays.FindAsync(id);
        if (holiday == null)
        {
            return ApiResponse<bool>.FailureResponse("Public holiday not found");
        }

        _context.PublicHolidays.Remove(holiday);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Public holiday deleted successfully");
    }

    public async Task<int> GetWorkingDaysAsync(DateTime startDate, DateTime endDate)
    {
        var holidays = await _context.PublicHolidays
            .Where(h => h.IsActive && h.Date >= startDate && h.Date <= endDate)
            .Select(h => h.Date.Date)
            .ToListAsync();

        int workingDays = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Skip holidays
            if (holidays.Contains(date.Date))
                continue;

            workingDays++;
        }

        return workingDays;
    }

    public async Task<bool> IsHolidayAsync(DateTime date)
    {
        return await _context.PublicHolidays
            .AnyAsync(h => h.IsActive && h.Date.Date == date.Date);
    }
}
