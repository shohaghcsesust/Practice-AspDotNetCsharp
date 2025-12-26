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
3. Authentication & Authorization
4. Database Design
5. API Endpoints
6. Key Features
7. Code Walkthrough
8. Demo

---

# 🎯 Project Overview

### What is Leave Management API?

A **RESTful Web Service** that enables organizations to:

- ✅ **JWT Authentication** - Secure login with access & refresh tokens
- ✅ **Role-based Authorization** - Admin, Manager, Employee roles
- ✅ **Leave Balance Tracking** - Track used/remaining days per type
- ✅ **Email Notifications** - Automated alerts on request status
- ✅ **Audit Logging** - Track all system actions
- ✅ Submit and track leave requests

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
│   ├── AuthController.cs        # Login, Register, Refresh
│   ├── AdminController.cs       # User & Audit management
│   ├── EmployeesController.cs
│   ├── LeaveRequestsController.cs
│   ├── LeaveBalanceController.cs # Balance tracking
│   └── LeaveTypesController.cs
├── Services/             # Auth, JWT, Email, Audit, Balance
├── Configuration/        # JwtSettings, EmailSettings
├── Models/               # Employee, Role, LeaveBalance, AuditLog
├── Data/                 # EF Core DbContext + Seeder
└── Program.cs            # Entry Point + JWT Config
```

---

# 🎨 Design Patterns Used

| Pattern | Purpose |
|---------|---------|
| **Repository** | Abstracts data access from business logic |
| **Service Layer** | Encapsulates business rules & validation |
| **Dependency Injection** | Loose coupling & testability |
| **DTO Pattern** | Separates API contracts from domain models |
| **JWT Bearer Auth** | Stateless authentication with tokens |

---

# 💉 Dependency Injection

### Registration in Program.cs

```csharp
// Authentication & Security Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();

// Business Logic Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
```

---

# 🔐 Authentication & Authorization

### JWT Token Flow

```
┌─────────┐  POST /api/auth/login   ┌─────────────┐
│ Client  │────────────────────────▶│ AuthService │
└─────────┘  { email, password }    └──────┬──────┘
     ▲                                     │
     │     { accessToken, refreshToken }   │
     └─────────────────────────────────────┘
```

### Role-Based Access Control

| Role | Permissions |
|------|-------------|
| **Employee** | View own requests, create leave, view balances |
| **Manager** | + Approve/Reject team requests |
| **Admin** | Full access: users, audit logs, balance adjust |

---

# 🗄️ Database Design

### Entity Relationship Diagram

```
┌─────────────────┐       ┌──────────────┐
│    Employee     │       │  LeaveType   │
├─────────────────┤       ├──────────────┤
│ Id, Email       │       │ Id, Name     │
│ PasswordHash    │       │ DefaultDays  │
│ Role (Enum)     │       └──────┬───────┘
│ ManagerId (FK)  │──┐           │
└───────┬─────────┘  │     ┌─────┴─────┐
        │            │     │           │
        ▼            │     ▼           ▼
┌───────────────┐    │  ┌─────────────────┐
│ LeaveRequest  │    │  │  LeaveBalance   │
├───────────────┤    │  ├─────────────────┤
│ EmployeeId    │    │  │ EmployeeId      │
│ LeaveTypeId   │    │  │ LeaveTypeId     │
│ Status (Enum) │    │  │ TotalDays       │
│ ApprovedById  │────┘  │ UsedDays        │
└───────────────┘       └─────────────────┘
```

**+ AuditLog, RefreshToken entities**

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

# 🔌 API Endpoints - Authentication

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Register new user | ❌ |
| `POST` | `/api/auth/login` | Login, get tokens | ❌ |
| `POST` | `/api/auth/refresh` | Refresh access token | ❌ |
| `POST` | `/api/auth/logout` | Logout, revoke token | ✅ |

---

# 🔌 API Endpoints - Admin

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/admin/users` | Get all users | Admin |
| `PUT` | `/api/admin/users/{id}/role` | Update role | Admin |
| `GET` | `/api/admin/audit-logs` | View audit logs | Admin |
| `POST` | `/api/admin/leave-balance/adjust` | Adjust balance | Admin |

---

# 🔌 API Endpoints - Leave Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/leaverequests` | Get all requests | ✅ |
| `GET` | `/api/leaverequests/pending` | Get pending only | Mgr/Admin |
| `POST` | `/api/leaverequests` | Create (checks balance) | ✅ |
| `POST` | `/api/leaverequests/{id}/approve` | Approve | Mgr/Admin |
| `POST` | `/api/leaverequests/{id}/reject` | Reject | Mgr/Admin |

---

# 🔌 API Endpoints - Leave Balance

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/leavebalance/my` | Get my balances | ✅ |
| `GET` | `/api/leavebalance/employee/{id}` | Get employee's balance | Mgr/Admin |

---

# ✨ Key Features

### Security & Business Validations

- 🔐 **JWT Authentication** - Access + Refresh tokens
- 👥 **Role-based Access** - Admin, Manager, Employee
- 📊 **Balance Tracking** - Auto-deduct on approval
- 📧 **Email Notifications** - Request/approval alerts
- 📝 **Audit Logging** - Track all actions
- ✅ **Overlap detection** - Prevents double-booking
- ✅ **Balance check** - Can't exceed available days

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
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt |
| Database | SQL Server / In-Memory |
| API Docs | OpenAPI 3.0 + Scalar UI |

---

# 🚀 How to Run

### Prerequisites
- .NET 10 SDK installed

### Commands

```bash
dotnet restore && dotnet run
# Open: http://localhost:5000/scalar/v1
```

### Test Credentials

| Email | Role | Password |
|-------|------|----------|
| `admin@company.com` | Admin | `Password123!` |
| `sarah.williams@company.com` | Manager | `Password123!` |
| `john.doe@company.com` | Employee | `Password123!` |

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

# ✅ Implemented Features

| Feature | Status |
|---------|--------|
| JWT Authentication | ✅ Done |
| Role-based Authorization | ✅ Done |
| Leave Balance Tracking | ✅ Done |
| Email Notifications | ✅ Done |
| Audit Logging | ✅ Done |

### Next Steps
- 🧪 Unit & Integration Tests
- 🐳 Docker Support
- 📱 Vue.js Frontend

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
