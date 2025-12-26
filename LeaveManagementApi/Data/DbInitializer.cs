using LeaveManagementApi.Models;

namespace LeaveManagementApi.Data;

public static class DbInitializer
{
    public static void Initialize(LeaveDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Employees.Any())
        {
            return; // DB has been seeded
        }

        // Seed Leave Types
        var leaveTypes = new LeaveType[]
        {
            new LeaveType
            {
                Name = "Annual Leave",
                Description = "Yearly vacation leave for rest and relaxation",
                DefaultDays = 20,
                IsActive = true
            },
            new LeaveType
            {
                Name = "Sick Leave",
                Description = "Leave for illness or medical appointments",
                DefaultDays = 10,
                IsActive = true
            },
            new LeaveType
            {
                Name = "Casual Leave",
                Description = "Leave for personal matters and emergencies",
                DefaultDays = 5,
                IsActive = true
            },
            new LeaveType
            {
                Name = "Maternity Leave",
                Description = "Leave for expecting mothers",
                DefaultDays = 90,
                IsActive = true
            },
            new LeaveType
            {
                Name = "Paternity Leave",
                Description = "Leave for new fathers",
                DefaultDays = 10,
                IsActive = true
            }
        };

        context.LeaveTypes.AddRange(leaveTypes);
        context.SaveChanges();

        // Default password for all seeded users: "Password123!"
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        // Seed Employees with Roles
        var employees = new Employee[]
        {
            // Admin user
            new Employee
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@company.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Admin,
                Department = "Administration",
                Position = "System Administrator",
                HireDate = new DateTime(2015, 1, 1),
                IsActive = true
            },
            // Manager
            new Employee
            {
                FirstName = "Sarah",
                LastName = "Williams",
                Email = "sarah.williams@company.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Manager,
                Department = "Engineering",
                Position = "Team Lead",
                HireDate = new DateTime(2017, 11, 10),
                IsActive = true
            },
            // HR Manager
            new Employee
            {
                FirstName = "Mike",
                LastName = "Johnson",
                Email = "mike.johnson@company.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Manager,
                Department = "HR",
                Position = "HR Manager",
                HireDate = new DateTime(2018, 3, 20),
                IsActive = true
            },
            // Employee reporting to Sarah
            new Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Employee,
                ManagerId = 2, // Sarah
                Department = "Engineering",
                Position = "Software Developer",
                HireDate = new DateTime(2020, 1, 15),
                IsActive = true
            },
            // Employee reporting to Sarah
            new Employee
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Employee,
                ManagerId = 2, // Sarah
                Department = "Engineering",
                Position = "Senior Developer",
                HireDate = new DateTime(2019, 6, 1),
                IsActive = true
            }
        };

        context.Employees.AddRange(employees);
        context.SaveChanges();

        // Initialize Leave Balances for all employees
        var currentYear = DateTime.UtcNow.Year;
        var savedLeaveTypes = context.LeaveTypes.ToList();
        var savedEmployees = context.Employees.ToList();

        foreach (var employee in savedEmployees)
        {
            foreach (var leaveType in savedLeaveTypes)
            {
                context.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = leaveType.Id,
                    Year = currentYear,
                    TotalDays = leaveType.DefaultDays,
                    UsedDays = 0
                });
            }
        }
        context.SaveChanges();

        // Seed some Leave Requests
        var leaveRequests = new LeaveRequest[]
        {
            new LeaveRequest
            {
                EmployeeId = 4, // John
                LeaveTypeId = 1, // Annual Leave
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today.AddDays(12),
                TotalDays = 5,
                Reason = "Family vacation",
                Status = LeaveStatus.Pending
            },
            new LeaveRequest
            {
                EmployeeId = 5, // Jane
                LeaveTypeId = 2, // Sick Leave
                StartDate = DateTime.Today.AddDays(-2),
                EndDate = DateTime.Today.AddDays(-1),
                TotalDays = 2,
                Reason = "Not feeling well",
                Status = LeaveStatus.Approved,
                ApprovedById = 2, // Sarah (manager)
                ApprovedAt = DateTime.Today.AddDays(-3),
                ApproverComments = "Get well soon!"
            }
        };

        context.LeaveRequests.AddRange(leaveRequests);
        context.SaveChanges();

        // Update Jane's leave balance to reflect approved leave
        var janeBalance = context.LeaveBalances
            .FirstOrDefault(lb => lb.EmployeeId == 5 && lb.LeaveTypeId == 2 && lb.Year == currentYear);
        if (janeBalance != null)
        {
            janeBalance.UsedDays = 2;
            context.SaveChanges();
        }
    }
}
