---
marp: true
theme: default
paginate: true
backgroundColor: #fff
style: |
  section {
    font-family: 'Segoe UI', Arial, sans-serif;
  }
  h1 {
    color: #0078d4;
  }
  h2 {
    color: #106ebe;
  }
  code {
    background-color: #f4f4f4;
  }
---

# Leave Management API

### ASP.NET Core Web API Project

![bg right:40% 80%](https://upload.wikimedia.org/wikipedia/commons/e/ee/.NET_Core_Logo.svg)

**Built with .NET 10 & Entity Framework Core**


### Prepared by:
Md. Ahsan Kabir
PSE, BJIT Limited

---

# 📋 Agenda

1. Project Overview
2. Architecture & Design Patterns
3. Database Design
4. API Endpoints
5. Key Features
6. Code Walkthrough
7. Demo
8. Future Enhancements

---

# 🎯 Project Overview

### What is Leave Management API?

A **RESTful Web Service** that enables organizations to:

- ✅ Manage employee information
- ✅ Configure leave types (Annual, Sick, Casual, etc.)
- ✅ Submit and track leave requests
- ✅ Approve or reject leave applications

---

# 🏗️ Architecture

### 3-Layer Clean Architecture

```
┌─────────────────────────────────────┐
│    Presentation Layer (Controllers) │
└──────────────────┬──────────────────┘
                   ▼
┌─────────────────────────────────────┐
│    Business Layer (Services)        │
└──────────────────┬──────────────────┘
                   ▼
┌─────────────────────────────────────┐
│    Data Access Layer (Repositories) │
└──────────────────┬──────────────────┘
                   ▼
┌─────────────────────────────────────┐
│    Database (Entity Framework Core) │
└─────────────────────────────────────┘
```

---

# 📁 Project Structure

```
LeaveManagementApi/
├── Controllers/          # API Endpoints
│   ├── EmployeesController.cs
│   ├── LeaveTypesController.cs
│   └── LeaveRequestsController.cs
├── Services/             # Business Logic
├── Repositories/         # Data Access
├── Models/               # Domain Entities
├── DTOs/                 # Data Transfer Objects
├── Data/                 # EF Core DbContext
└── Program.cs            # Entry Point
```

---

# 🎨 Design Patterns Used

| Pattern | Purpose |
|---------|---------|
| **Repository** | Abstracts data access from business logic |
| **Service Layer** | Encapsulates business rules & validation |
| **Dependency Injection** | Loose coupling & testability |
| **DTO Pattern** | Separates API contracts from domain models |

---

# 💉 Dependency Injection

### Registration in Program.cs

```csharp
// Repositories (Data Access)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

// Services (Business Logic)
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
```

---

# 🗄️ Database Design

### Entity Relationship Diagram

```
┌──────────────┐         ┌──────────────┐
│   Employee   │         │  LeaveType   │
├──────────────┤         ├──────────────┤
│ Id (PK)      │         │ Id (PK)      │
│ FirstName    │         │ Name         │
│ LastName     │         │ DefaultDays  │
│ Email        │         │ IsActive     │
│ Department   │         └──────┬───────┘
└──────┬───────┘                │
       │                        │
       └────────┬───────────────┘
                ▼
       ┌──────────────────┐
       │  LeaveRequest    │
       ├──────────────────┤
       │ EmployeeId (FK)  │
       │ LeaveTypeId (FK) │
       │ StartDate        │
       │ EndDate          │
       │ Status           │
       └──────────────────┘
```

---

# 📊 Leave Status Flow

```
    ┌─────────┐
    │ PENDING │
    └────┬────┘
         │
    ┌────┴────┐
    ▼         ▼
┌────────┐  ┌────────┐
│APPROVED│  │REJECTED│
└────────┘  └────────┘
    │
    ▼
┌─────────┐
│CANCELLED│ (Employee can cancel approved leave)
└─────────┘
```

---

# 🔌 API Endpoints - Employees

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/employees` | Get all employees |
| `GET` | `/api/employees/{id}` | Get by ID |
| `POST` | `/api/employees` | Create employee |
| `PUT` | `/api/employees/{id}` | Update employee |
| `DELETE` | `/api/employees/{id}` | Delete employee |

---

# 🔌 API Endpoints - Leave Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/leaverequests` | Get all requests |
| `GET` | `/api/leaverequests/pending` | Get pending only |
| `POST` | `/api/leaverequests` | Create request |
| `POST` | `/api/leaverequests/{id}/approve` | Approve |
| `POST` | `/api/leaverequests/{id}/reject` | Reject |
| `POST` | `/api/leaverequests/{id}/cancel` | Cancel |

---

# ✨ Key Features

### Business Validations

- ✅ **Email uniqueness** - No duplicate employee emails
- ✅ **Date validation** - End date must be ≥ Start date
- ✅ **No past dates** - Can't request leave for past dates
- ✅ **Overlap detection** - Prevents double-booking leaves
- ✅ **Status constraints** - Only pending requests can be approved/rejected

---

# 💻 Code Example - Controller

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Create(
    [FromBody] CreateLeaveRequestDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _leaveRequestService.CreateAsync(dto);
    
    if (!result.Success)
        return BadRequest(result);

    return CreatedAtAction(nameof(GetById), 
        new { id = result.Data!.Id }, result);
}
```

---

# 💻 Code Example - Service

```csharp
public async Task<ApiResponse<LeaveRequestDto>> CreateAsync(
    CreateLeaveRequestDto dto)
{
    // Validate employee exists
    var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
    if (employee == null)
        return ApiResponse<LeaveRequestDto>.FailureResponse("Employee not found.");

    // Check for overlapping requests
    var hasOverlapping = await _leaveRequestRepository
        .HasOverlappingRequestAsync(dto.EmployeeId, dto.StartDate, dto.EndDate);
    if (hasOverlapping)
        return ApiResponse<LeaveRequestDto>.FailureResponse("Overlapping request exists.");

    // Create leave request...
}
```

---

# 🛠️ Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | ASP.NET Core (.NET 10) |
| Language | C# 12 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server / In-Memory |
| API Docs | OpenAPI 3.0 + Scalar UI |
| IDE | Visual Studio / VS Code |

---

# 🚀 How to Run

### Prerequisites
- .NET 10 SDK installed

### Commands

```bash
# Navigate to project folder
cd LeaveManagementApi

# Restore packages
dotnet restore

# Run the application
dotnet run

# Access API Documentation
# Open: http://localhost:5000/scalar/v1
```

---

# 📸 Demo

### API Documentation (Scalar UI)

Access at: `http://localhost:5000/scalar/v1`

**Features:**
- Interactive API testing
- Request/Response examples
- Schema documentation
- Code generation for multiple languages

---

# 🔮 Future Enhancements

| Feature | Priority |
|---------|----------|
| JWT Authentication | High |
| Role-based Authorization | High |
| Leave Balance Tracking | Medium |
| Email Notifications | Medium |
| Audit Logging | Low |
| Unit Tests | High |
| Docker Support | Medium |

---

# 📈 Benefits of This Architecture

- ✅ **Maintainability** - Easy to update individual layers
- ✅ **Testability** - Mockable interfaces for unit testing
- ✅ **Scalability** - Can swap implementations easily
- ✅ **Separation of Concerns** - Clear responsibilities
- ✅ **Reusability** - Services can be reused across controllers

---

# 🙏 Thank You!

### Questions?

---

**Project Repository:** `LeaveManagementApi`

**API Documentation:** `http://localhost:5000/scalar/v1`

**Contact:** Md. Ahsan Kabir | ahsan.kabir@bjitgroup.com

---

# 📚 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [REST API Best Practices](https://restfulapi.net/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
