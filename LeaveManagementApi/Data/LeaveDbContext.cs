using LeaveManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Data;

public class LeaveDbContext : DbContext
{
    public LeaveDbContext(DbContextOptions<LeaveDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<LeaveType> LeaveTypes { get; set; } = null!;
    public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
    public DbSet<LeaveBalance> LeaveBalances { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PublicHoliday> PublicHolidays { get; set; } = null!;
    public DbSet<LeaveAttachment> LeaveAttachments { get; set; } = null!;
    public DbSet<ApprovalStep> ApprovalSteps { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<LeaveCarryForward> LeaveCarryForwards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            
            entity.HasOne(e => e.Manager)
                  .WithMany()
                  .HasForeignKey(e => e.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveType configuration
        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasOne(lr => lr.Employee)
                  .WithMany(e => e.LeaveRequests)
                  .HasForeignKey(lr => lr.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.LeaveType)
                  .WithMany(lt => lt.LeaveRequests)
                  .HasForeignKey(lr => lr.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(lr => lr.ApprovedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveBalance configuration
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.Year }).IsUnique();
            
            entity.HasOne(lb => lb.Employee)
                  .WithMany(e => e.LeaveBalances)
                  .HasForeignKey(lb => lb.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lb => lb.LeaveType)
                  .WithMany()
                  .HasForeignKey(lb => lb.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(rt => rt.Employee)
                  .WithMany(e => e.RefreshTokens)
                  .HasForeignKey(rt => rt.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PublicHoliday configuration
        modelBuilder.Entity<PublicHoliday>(entity =>
        {
            entity.HasIndex(e => new { e.Date, e.Name }).IsUnique();
        });

        // LeaveAttachment configuration
        modelBuilder.Entity<LeaveAttachment>(entity =>
        {
            entity.HasOne(la => la.LeaveRequest)
                  .WithMany()
                  .HasForeignKey(la => la.LeaveRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(la => la.UploadedBy)
                  .WithMany()
                  .HasForeignKey(la => la.UploadedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ApprovalStep configuration
        modelBuilder.Entity<ApprovalStep>(entity =>
        {
            entity.HasOne(a => a.LeaveRequest)
                  .WithMany()
                  .HasForeignKey(a => a.LeaveRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Approver)
                  .WithMany()
                  .HasForeignKey(a => a.ApproverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // LeaveCarryForward configuration
        modelBuilder.Entity<LeaveCarryForward>(entity =>
        {
            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.FromYear, e.ToYear }).IsUnique();

            entity.HasOne(cf => cf.Employee)
                  .WithMany()
                  .HasForeignKey(cf => cf.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cf => cf.LeaveType)
                  .WithMany()
                  .HasForeignKey(cf => cf.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
