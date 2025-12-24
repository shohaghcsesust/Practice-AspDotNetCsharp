using LeaveManagementApi.Data;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Repositories;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly LeaveDbContext _context;

    public LeaveTypeRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveType>> GetAllAsync()
    {
        return await _context.LeaveTypes
            .OrderBy(lt => lt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveType>> GetActiveAsync()
    {
        return await _context.LeaveTypes
            .Where(lt => lt.IsActive)
            .OrderBy(lt => lt.Name)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetByIdAsync(int id)
    {
        return await _context.LeaveTypes.FindAsync(id);
    }

    public async Task<LeaveType?> GetByNameAsync(string name)
    {
        return await _context.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.Name.ToLower() == name.ToLower());
    }

    public async Task<LeaveType> CreateAsync(LeaveType leaveType)
    {
        leaveType.CreatedAt = DateTime.UtcNow;
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();
        return leaveType;
    }

    public async Task<LeaveType> UpdateAsync(LeaveType leaveType)
    {
        leaveType.UpdatedAt = DateTime.UtcNow;
        _context.LeaveTypes.Update(leaveType);
        await _context.SaveChangesAsync();
        return leaveType;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);
        if (leaveType == null)
            return false;

        _context.LeaveTypes.Remove(leaveType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.LeaveTypes.AnyAsync(lt => lt.Id == id);
    }
}
