using Microsoft.EntityFrameworkCore;
using LeaveManagementApi.Data;
using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;

namespace LeaveManagementApi.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly LeaveDbContext _context;
    private readonly ILogger<LeaveBalanceService> _logger;

    public LeaveBalanceService(LeaveDbContext context, ILogger<LeaveBalanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetEmployeeBalancesAsync(int employeeId, int? year = null)
    {
        var currentYear = year ?? DateTime.UtcNow.Year;

        return await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == currentYear)
            .Select(lb => new LeaveBalanceDto
            {
                Id = lb.Id,
                EmployeeId = lb.EmployeeId,
                LeaveTypeId = lb.LeaveTypeId,
                LeaveTypeName = lb.LeaveType!.Name,
                Year = lb.Year,
                TotalDays = lb.TotalDays,
                UsedDays = lb.UsedDays,
                RemainingDays = lb.TotalDays - lb.UsedDays
            })
            .ToListAsync();
    }

    public async Task InitializeEmployeeBalancesAsync(int employeeId)
    {
        var currentYear = DateTime.UtcNow.Year;
        var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();

        foreach (var leaveType in leaveTypes)
        {
            var exists = await _context.LeaveBalances
                .AnyAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveType.Id && lb.Year == currentYear);

            if (!exists)
            {
                _context.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveType.Id,
                    Year = currentYear,
                    TotalDays = leaveType.DefaultDays,
                    UsedDays = 0
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Initialized leave balances for employee {EmployeeId} for year {Year}", employeeId, currentYear);
    }

    public async Task<bool> HasSufficientBalanceAsync(int employeeId, int leaveTypeId, decimal daysRequested)
    {
        var currentYear = DateTime.UtcNow.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveTypeId && lb.Year == currentYear);

        if (balance == null)
            return false;

        return balance.RemainingDays >= daysRequested;
    }

    public async Task DeductBalanceAsync(int employeeId, int leaveTypeId, decimal days)
    {
        var currentYear = DateTime.UtcNow.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveTypeId && lb.Year == currentYear);

        if (balance != null)
        {
            balance.UsedDays += days;
            balance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deducted {Days} days from employee {EmployeeId} leave type {LeaveTypeId}", days, employeeId, leaveTypeId);
        }
    }

    public async Task RestoreBalanceAsync(int employeeId, int leaveTypeId, decimal days)
    {
        var currentYear = DateTime.UtcNow.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveTypeId && lb.Year == currentYear);

        if (balance != null)
        {
            balance.UsedDays = Math.Max(0, balance.UsedDays - days);
            balance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Restored {Days} days to employee {EmployeeId} leave type {LeaveTypeId}", days, employeeId, leaveTypeId);
        }
    }

    public async Task<LeaveBalanceDto?> AdjustBalanceAsync(int employeeId, int leaveTypeId, decimal totalDays)
    {
        var currentYear = DateTime.UtcNow.Year;
        var balance = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveTypeId && lb.Year == currentYear);

        if (balance == null)
            return null;

        balance.TotalDays = totalDays;
        balance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new LeaveBalanceDto
        {
            Id = balance.Id,
            EmployeeId = balance.EmployeeId,
            LeaveTypeId = balance.LeaveTypeId,
            LeaveTypeName = balance.LeaveType!.Name,
            Year = balance.Year,
            TotalDays = balance.TotalDays,
            UsedDays = balance.UsedDays,
            RemainingDays = balance.RemainingDays
        };
    }
}
