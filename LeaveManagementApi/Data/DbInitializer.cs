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

        // Seed Employees
        var employees = new Employee[]
        {
            new Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                Department = "Engineering",
                Position = "Software Developer",
                HireDate = new DateTime(2020, 1, 15),
                IsActive = true
            },
            new Employee
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                Department = "Engineering",
                Position = "Senior Developer",
                HireDate = new DateTime(2019, 6, 1),
                IsActive = true
            },
            new Employee
            {
                FirstName = "Mike",
                LastName = "Johnson",
                Email = "mike.johnson@company.com",
                Department = "HR",
                Position = "HR Manager",
                HireDate = new DateTime(2018, 3, 20),
                IsActive = true
            },
            new Employee
            {
                FirstName = "Sarah",
                LastName = "Williams",
                Email = "sarah.williams@company.com",
                Department = "Engineering",
                Position = "Team Lead",
                HireDate = new DateTime(2017, 11, 10),
                IsActive = true
            }
        };

        context.Employees.AddRange(employees);
        context.SaveChanges();

        // Seed some Leave Requests
        var leaveRequests = new LeaveRequest[]
        {
            new LeaveRequest
            {
                EmployeeId = 1,
                LeaveTypeId = 1,
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today.AddDays(12),
                TotalDays = 5,
                Reason = "Family vacation",
                Status = LeaveStatus.Pending
            },
            new LeaveRequest
            {
                EmployeeId = 2,
                LeaveTypeId = 2,
                StartDate = DateTime.Today.AddDays(-2),
                EndDate = DateTime.Today.AddDays(-1),
                TotalDays = 2,
                Reason = "Not feeling well",
                Status = LeaveStatus.Approved,
                ApprovedById = 4,
                ApprovedAt = DateTime.Today.AddDays(-3),
                ApproverComments = "Get well soon!"
            }
        };

        context.LeaveRequests.AddRange(leaveRequests);
        context.SaveChanges();
    }
}
