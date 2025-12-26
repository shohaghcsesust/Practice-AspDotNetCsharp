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
    }
}
