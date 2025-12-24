using LeaveManagementApi.Data;
using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly LeaveDbContext _context;

    public LeaveRequestRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ApprovedBy)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ApprovedBy)
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ApprovedBy)
            .Where(lr => lr.Status == status)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
    {
        return await GetByStatusAsync(LeaveStatus.Pending);
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ApprovedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest)
    {
        leaveRequest.CreatedAt = DateTime.UtcNow;
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest)
    {
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
            return false;

        _context.LeaveRequests.Remove(leaveRequest);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.LeaveRequests.AnyAsync(lr => lr.Id == id);
    }

    public async Task<bool> HasOverlappingRequestAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeId = null)
    {
        var query = _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId)
            .Where(lr => lr.Status != LeaveStatus.Rejected && lr.Status != LeaveStatus.Cancelled)
            .Where(lr => lr.StartDate <= endDate && lr.EndDate >= startDate);

        if (excludeId.HasValue)
        {
            query = query.Where(lr => lr.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
