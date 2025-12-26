using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Services;

public interface ICarryForwardService
{
    Task<IEnumerable<LeaveCarryForwardDto>> GetEmployeeCarryForwardsAsync(int employeeId);
    Task<ApiResponse<LeaveCarryForwardDto>> ProcessCarryForwardAsync(ProcessCarryForwardDto dto);
    Task<ApiResponse<int>> ProcessYearEndCarryForwardAsync(int fromYear, int toYear);
    Task ExpireCarryForwardsAsync();
}

public class CarryForwardService : ICarryForwardService
{
    private readonly LeaveDbContext _context;

    public CarryForwardService(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveCarryForwardDto>> GetEmployeeCarryForwardsAsync(int employeeId)
    {
        return await _context.LeaveCarryForwards
            .Where(cf => cf.EmployeeId == employeeId && !cf.IsExpired)
            .Include(cf => cf.LeaveType)
            .OrderByDescending(cf => cf.ToYear)
            .Select(cf => new LeaveCarryForwardDto
            {
                Id = cf.Id,
                EmployeeId = cf.EmployeeId,
                LeaveTypeId = cf.LeaveTypeId,
                LeaveTypeName = cf.LeaveType.Name,
                FromYear = cf.FromYear,
                ToYear = cf.ToYear,
                CarriedDays = cf.CarriedDays,
                MaxCarryForwardDays = cf.MaxCarryForwardDays,
                ExpiryDate = cf.ExpiryDate,
                IsExpired = cf.IsExpired
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<LeaveCarryForwardDto>> ProcessCarryForwardAsync(ProcessCarryForwardDto dto)
    {
        // Check if leave type allows carry forward
        var leaveType = await _context.LeaveTypes.FindAsync(dto.LeaveTypeId);
        if (leaveType == null)
        {
            return ApiResponse<LeaveCarryForwardDto>.FailureResponse("Leave type not found");
        }

        // Get employee's remaining balance for the year
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => 
                lb.EmployeeId == dto.EmployeeId && 
                lb.LeaveTypeId == dto.LeaveTypeId && 
                lb.Year == dto.FromYear);

        if (balance == null)
        {
            return ApiResponse<LeaveCarryForwardDto>.FailureResponse("No balance found for the specified year");
        }

        var remainingDays = balance.TotalDays - balance.UsedDays;
        var carryDays = Math.Min(remainingDays, dto.MaxCarryForwardDays);

        if (carryDays <= 0)
        {
            return ApiResponse<LeaveCarryForwardDto>.FailureResponse("No days available to carry forward");
        }

        // Check if carry forward already exists
        var existing = await _context.LeaveCarryForwards
            .FirstOrDefaultAsync(cf => 
                cf.EmployeeId == dto.EmployeeId && 
                cf.LeaveTypeId == dto.LeaveTypeId && 
                cf.FromYear == dto.FromYear &&
                cf.ToYear == dto.ToYear);

        if (existing != null)
        {
            return ApiResponse<LeaveCarryForwardDto>.FailureResponse("Carry forward already processed for this period");
        }

        var carryForward = new LeaveCarryForward
        {
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            FromYear = dto.FromYear,
            ToYear = dto.ToYear,
            CarriedDays = carryDays,
            MaxCarryForwardDays = dto.MaxCarryForwardDays,
            ExpiryDate = dto.ExpiryDate
        };

        _context.LeaveCarryForwards.Add(carryForward);

        // Add to next year's balance
        var nextYearBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => 
                lb.EmployeeId == dto.EmployeeId && 
                lb.LeaveTypeId == dto.LeaveTypeId && 
                lb.Year == dto.ToYear);

        if (nextYearBalance != null)
        {
            nextYearBalance.TotalDays += carryDays;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<LeaveCarryForwardDto>.SuccessResponse(new LeaveCarryForwardDto
        {
            Id = carryForward.Id,
            EmployeeId = carryForward.EmployeeId,
            LeaveTypeId = carryForward.LeaveTypeId,
            LeaveTypeName = leaveType.Name,
            FromYear = carryForward.FromYear,
            ToYear = carryForward.ToYear,
            CarriedDays = carryForward.CarriedDays,
            MaxCarryForwardDays = carryForward.MaxCarryForwardDays,
            ExpiryDate = carryForward.ExpiryDate,
            IsExpired = carryForward.IsExpired
        }, $"Successfully carried forward {carryDays} days");
    }

    public async Task<ApiResponse<int>> ProcessYearEndCarryForwardAsync(int fromYear, int toYear)
    {
        var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
        var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
        var processedCount = 0;

        foreach (var employee in employees)
        {
            foreach (var leaveType in leaveTypes)
            {
                var result = await ProcessCarryForwardAsync(new ProcessCarryForwardDto
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = leaveType.Id,
                    FromYear = fromYear,
                    ToYear = toYear,
                    MaxCarryForwardDays = Math.Min(5, leaveType.DefaultDays / 2), // Default: 5 days or half
                    ExpiryDate = new DateTime(toYear, 3, 31) // Expire end of Q1
                });

                if (result.Success)
                {
                    processedCount++;
                }
            }
        }

        return ApiResponse<int>.SuccessResponse(processedCount, $"Processed {processedCount} carry forwards");
    }

    public async Task ExpireCarryForwardsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var expiredCarryForwards = await _context.LeaveCarryForwards
            .Where(cf => !cf.IsExpired && cf.ExpiryDate.HasValue && cf.ExpiryDate.Value.Date <= today)
            .ToListAsync();

        foreach (var cf in expiredCarryForwards)
        {
            cf.IsExpired = true;

            // Reduce from current year balance
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => 
                    lb.EmployeeId == cf.EmployeeId && 
                    lb.LeaveTypeId == cf.LeaveTypeId && 
                    lb.Year == cf.ToYear);

            if (balance != null && balance.TotalDays >= cf.CarriedDays)
            {
                balance.TotalDays -= cf.CarriedDays;
            }
        }

        await _context.SaveChangesAsync();
    }
}
