# Leave Management API - Design Overview

## Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Authentication & Authorization](#authentication--authorization)
5. [Design Patterns](#design-patterns)
6. [Database Design](#database-design)
7. [API Endpoints](#api-endpoints)
8. [Data Flow](#data-flow)
9. [Technology Stack](#technology-stack)
10. [Configuration](#configuration)
11. [Getting Started](#getting-started)

---

## Project Overview

The **Leave Management API** is a RESTful web service built with ASP.NET Core that allows organizations to manage employee leave requests. The system supports:

- **JWT Authentication & Authorization** with role-based access control
- **Role-based access** (Admin, Manager, Employee)
- Employee management (CRUD operations)
- Leave type configuration (Annual, Sick, Casual, etc.)
- Leave request submission, approval, and rejection
- **Leave balance tracking** with automatic deduction/restoration
- **Email notifications** for leave requests and approvals
- **Audit logging** for all important actions

---

## Architecture

The project follows a **Clean/Layered Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│                        (Controllers)                         │
│  ┌───────────┬───────────┬───────────┬───────────┬────────┐ │
│  │ AuthCtrl  │Employees  │LeaveTypes │LeaveReqs  │AdminCtrl│ │
│  │           │  Ctrl     │   Ctrl    │   Ctrl    │         │ │
│  └─────┬─────┴─────┬─────┴─────┬─────┴─────┬─────┴────┬────┘ │
└────────┼───────────┼───────────┼───────────┼──────────┼──────┘
         │           │           │           │          │
         ▼           ▼           ▼           ▼          ▼
┌─────────────────────────────────────────────────────────────┐
│                      Business Layer                          │
│                        (Services)                            │
│  ┌─────────┬─────────┬─────────┬─────────┬─────────┬──────┐ │
│  │AuthSvc  │JwtSvc   │EmailSvc │AuditSvc │LeaveBal │Others│ │
│  │         │         │         │         │  Svc    │      │ │
│  └────┬────┴────┬────┴────┬────┴────┬────┴────┬────┴──────┘ │
└───────┼─────────┼─────────┼─────────┼─────────┼─────────────┘
        │         │         │         │         │
        ▼         ▼         ▼         ▼         ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Access Layer                       │
│                       (Repositories)                         │
│  ┌─────────────────┬─────────────────┬─────────────────┐    │
│  │ EmployeeRepo    │ LeaveTypeRepo   │ LeaveRequestRepo│    │
│  └────────┬────────┴────────┬────────┴────────┬────────┘    │
└───────────┼─────────────────┼─────────────────┼─────────────┘
            │                 │                 │
            ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      Database Layer                          │
│                   (Entity Framework Core)                    │
│              ┌─────────────────────────┐                    │
│              │     LeaveDbContext      │                    │
│              └───────────┬─────────────┘                    │
│                          ▼                                   │
│              ┌─────────────────────────┐                    │
│              │   SQL Server / InMemory │                    │
│              └─────────────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
LeaveManagementApi/
│
├── Controllers/                    # API Endpoints (Presentation Layer)
│   ├── AuthController.cs           # Authentication endpoints (login, register, refresh)
│   ├── AdminController.cs          # Admin-only endpoints (users, audit logs, balance adjustment)
│   ├── EmployeesController.cs      # Employee CRUD endpoints
│   ├── LeaveTypesController.cs     # Leave Type CRUD endpoints
│   ├── LeaveRequestsController.cs  # Leave Request management endpoints
│   └── LeaveBalanceController.cs   # Leave balance viewing endpoints
│
├── Services/                       # Business Logic Layer
│   ├── IAuthService.cs             # Authentication service interface
│   ├── AuthService.cs              # Authentication service implementation
│   ├── IJwtService.cs              # JWT token service interface
│   ├── JwtService.cs               # JWT token generation/validation
│   ├── IEmailService.cs            # Email service interface
│   ├── EmailService.cs             # Email notification service
│   ├── IAuditService.cs            # Audit logging interface
│   ├── AuditService.cs             # Audit logging implementation
│   ├── ILeaveBalanceService.cs     # Leave balance service interface
│   ├── LeaveBalanceService.cs      # Leave balance tracking
│   ├── IEmployeeService.cs         # Employee service interface
│   ├── EmployeeService.cs          # Employee service implementation
│   ├── ILeaveTypeService.cs        # Leave Type service interface
│   ├── LeaveTypeService.cs         # Leave Type service implementation
│   ├── ILeaveRequestService.cs     # Leave Request service interface
│   └── LeaveRequestService.cs      # Leave Request service implementation
│
├── Repositories/                   # Data Access Layer
│   ├── IEmployeeRepository.cs      # Employee repository interface
│   ├── EmployeeRepository.cs       # Employee repository implementation
│   ├── ILeaveTypeRepository.cs     # Leave Type repository interface
│   ├── LeaveTypeRepository.cs      # Leave Type repository implementation
│   ├── ILeaveRequestRepository.cs  # Leave Request repository interface
│   └── LeaveRequestRepository.cs   # Leave Request repository implementation
│
├── Models/                         # Domain Entities
│   ├── Employee.cs                 # Employee entity (with Role, PasswordHash, ManagerId)
│   ├── LeaveType.cs                # Leave Type entity
│   ├── LeaveRequest.cs             # Leave Request entity + LeaveStatus enum
│   ├── LeaveBalance.cs             # Leave Balance entity (tracks used/remaining days)
│   ├── Role.cs                     # Role enum (Employee, Manager, Admin)
│   ├── AuditLog.cs                 # Audit Log entity + AuditAction enum
│   └── RefreshToken.cs             # Refresh Token entity for JWT
│
├── DTOs/                           # Data Transfer Objects
│   └── DTOs.cs                     # All DTOs (Auth, Employee, LeaveType, LeaveRequest, etc.)
│
├── Configuration/                  # Configuration Classes
│   ├── JwtSettings.cs              # JWT configuration settings
│   └── EmailSettings.cs            # Email/SMTP configuration settings
│
├── Data/                           # Database Configuration
│   ├── LeaveDbContext.cs           # EF Core DbContext
│   └── DbInitializer.cs            # Seed data (users with roles, leave balances)
│
├── Program.cs                      # Application entry point, DI & JWT configuration
├── appsettings.json                # Application configuration (JWT, Email settings)
├── appsettings.Development.json    # Development-specific configuration
└── LeaveManagementApi.csproj       # Project file with dependencies
```

---

## Authentication & Authorization

### JWT Authentication Flow

```
┌─────────┐     POST /api/auth/login      ┌─────────────────┐
│  Client │──────────────────────────────▶│  AuthController │
└─────────┘  { email, password }          └────────┬────────┘
                                                   │
                                                   ▼
                                          ┌─────────────────┐
                                          │   AuthService   │
                                          │  - Verify pwd   │
                                          │  - Generate JWT │
                                          └────────┬────────┘
                                                   │
                                                   ▼
┌─────────┐     { accessToken, refreshToken }     │
│  Client │◀──────────────────────────────────────┘
└─────────┘

┌─────────┐     GET /api/employees        ┌─────────────────┐
│  Client │──────────────────────────────▶│    JWT Auth     │
└─────────┘  Authorization: Bearer <token>│   Middleware    │
                                          └────────┬────────┘
                                                   │ Valid?
                                                   ▼
                                          ┌─────────────────┐
                                          │ EmployeesCtrl   │
                                          │ [Authorize]     │
                                          └─────────────────┘
```

### Role-Based Access Control

| Role | Permissions |
|------|-------------|
| **Employee** | View own leave requests, create requests, view balances, view leave types |
| **Manager** | All Employee permissions + approve/reject team leave requests, view pending requests |
| **Admin** | Full access: manage users, roles, leave types, view audit logs, adjust balances |

### Authorization Attributes

```csharp
[Authorize]                          // Any authenticated user
[Authorize(Roles = "Admin")]         // Admin only
[Authorize(Roles = "Manager,Admin")] // Manager or Admin
```

---

## Design Patterns

### 1. Repository Pattern
Abstracts data access logic from business logic.

```csharp
// Interface defines contract
public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    // ...
}

// Implementation handles data access
public class EmployeeRepository : IEmployeeRepository
{
    private readonly LeaveDbContext _context;
    
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.ToListAsync();
    }
}
```

### 2. Service Layer Pattern
Encapsulates business logic and validation.

```csharp
public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repository;
    
    public async Task<ApiResponse<LeaveRequestDto>> CreateAsync(CreateLeaveRequestDto dto)
    {
        // Business validation
        if (dto.EndDate < dto.StartDate)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Invalid dates");
        
        // Check for overlapping requests
        var hasOverlapping = await _repository.HasOverlappingRequestAsync(...);
        if (hasOverlapping)
            return ApiResponse<LeaveRequestDto>.FailureResponse("Overlapping request exists");
        
        // Create if valid
        var request = await _repository.CreateAsync(...);
        return ApiResponse<LeaveRequestDto>.SuccessResponse(MapToDto(request));
    }
}
```

### 3. Dependency Injection (DI)
All dependencies are injected via constructor injection.

```csharp
// Registration in Program.cs
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Usage in Controller
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;  // Injected by DI container
    }
}
```

### 4. DTO Pattern (Data Transfer Objects)
Separates API contracts from domain models.

```csharp
// Domain Model (internal)
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public DateTime CreatedAt { get; set; }  // Not exposed to API
}

// DTO (external contract)
public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string FullName => $"{FirstName} {LastName}";  // Computed property
}
```

---

## Database Design

### Entity Relationship Diagram

```
┌─────────────────────────┐       ┌──────────────────┐
│       Employee          │       │    LeaveType     │
├─────────────────────────┤       ├──────────────────┤
│ Id (PK)                 │       │ Id (PK)          │
│ FirstName               │       │ Name             │
│ LastName                │       │ Description      │
│ Email (Unique)          │       │ DefaultDays      │
│ PasswordHash            │       │ IsActive         │
│ Role (Enum)             │       │ CreatedAt        │
│ ManagerId (FK, nullable)│──┐    │ UpdatedAt        │
│ Department              │  │    └────────┬─────────┘
│ Position                │  │             │
│ HireDate                │◀─┘ (self-ref)  │
│ IsActive                │                │
│ CreatedAt               │                │
│ UpdatedAt               │                │
└───────────┬─────────────┘                │
            │                              │
            │    ┌─────────────────────────┘
            │    │
            ▼    ▼
┌────────────────────────────┐    ┌─────────────────────────┐
│       LeaveRequest         │    │      LeaveBalance       │
├────────────────────────────┤    ├─────────────────────────┤
│ Id (PK)                    │    │ Id (PK)                 │
│ EmployeeId (FK)            │───▶│ EmployeeId (FK)         │◀──Employee
│ LeaveTypeId (FK)           │───▶│ LeaveTypeId (FK)        │◀──LeaveType
│ StartDate                  │    │ Year                    │
│ EndDate                    │    │ TotalDays               │
│ TotalDays                  │    │ UsedDays                │
│ Reason                     │    │ RemainingDays (computed)│
│ Status (Enum)              │    │ CreatedAt               │
│ ApproverComments           │    │ UpdatedAt               │
│ ApprovedById (FK)          │───▶└─────────────────────────┘
│ ApprovedAt                 │
│ CreatedAt                  │
│ UpdatedAt                  │    ┌─────────────────────────┐
└────────────────────────────┘    │       AuditLog          │
                                  ├─────────────────────────┤
┌─────────────────────────┐       │ Id (PK)                 │
│      RefreshToken       │       │ UserId                  │
├─────────────────────────┤       │ Action (Enum)           │
│ Id (PK)                 │       │ EntityType              │
│ Token                   │       │ EntityId                │
│ EmployeeId (FK)         │──────▶│ Details                 │
│ Expires                 │       │ IpAddress               │
│ Created                 │       │ CreatedAt               │
│ Revoked                 │       └─────────────────────────┘
│ ReplacedByToken         │
└─────────────────────────┘
```

### Enums

```csharp
public enum LeaveStatus
{
    Pending = 0,    // Awaiting approval
    Approved = 1,   // Approved by manager
    Rejected = 2,   // Rejected by manager
    Cancelled = 3   // Cancelled by employee
}

public enum Role
{
    Employee = 0,   // Regular employee
    Manager = 1,    // Can approve/reject team requests
    Admin = 2       // Full system access
}

public enum AuditAction
{
    Create = 0,
    Update = 1,
    Delete = 2,
    Login = 3,
    Logout = 4,
    Approve = 5,
    Reject = 6
}
```

---

## API Endpoints

### Authentication API

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register new user | No |
| `POST` | `/api/auth/login` | Login and get tokens | No |
| `POST` | `/api/auth/refresh` | Refresh access token | No |
| `POST` | `/api/auth/logout` | Logout and revoke token | Yes |

### Admin API

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/admin/users` | Get all users | Admin |
| `GET` | `/api/admin/users/{id}` | Get user by ID | Admin |
| `PUT` | `/api/admin/users/{id}/role` | Update user role | Admin |
| `GET` | `/api/admin/audit-logs` | Get all audit logs | Admin |
| `POST` | `/api/admin/leave-balance/adjust` | Adjust employee balance | Admin |

### Employees API

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/employees` | Get all employees | Yes |
| `GET` | `/api/employees/{id}` | Get employee by ID | Yes |
| `POST` | `/api/employees` | Create new employee | Admin |
| `PUT` | `/api/employees/{id}` | Update employee | Admin |
| `DELETE` | `/api/employees/{id}` | Delete employee | Admin |

### Leave Types API

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/leavetypes` | Get all leave types | Yes |
| `GET` | `/api/leavetypes/active` | Get active leave types only | Yes |
| `GET` | `/api/leavetypes/{id}` | Get leave type by ID | Yes |
| `POST` | `/api/leavetypes` | Create new leave type | Admin |
| `PUT` | `/api/leavetypes/{id}` | Update leave type | Admin |
| `DELETE` | `/api/leavetypes/{id}` | Delete leave type | Admin |

### Leave Requests API

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/leaverequests` | Get all leave requests | Yes |
| `GET` | `/api/leaverequests/pending` | Get pending requests | Yes |
| `GET` | `/api/leaverequests/employee/{employeeId}` | Get requests by employee | Yes |
| `GET` | `/api/leaverequests/{id}` | Get request by ID | Yes |
| `POST` | `/api/leaverequests` | Create new request (checks balance) | Yes |
| `PUT` | `/api/leaverequests/{id}` | Update pending request | Yes |
| `POST` | `/api/leaverequests/{id}/approve` | Approve request | Manager, Admin |
| `POST` | `/api/leaverequests/{id}/reject` | Reject request | Manager, Admin |
| `POST` | `/api/leaverequests/{id}/cancel` | Cancel request | Yes |
| `DELETE` | `/api/leaverequests/{id}` | Delete request | Admin |

### Leave Balance API

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/leavebalance/my` | Get current user's balances | Yes |
| `GET` | `/api/leavebalance/employee/{employeeId}` | Get employee's balances | Yes |
| `GET` | `/api/leavebalance/employee/{employeeId}/year/{year}` | Get balances by year | Yes |

---

## Data Flow

### Example: Creating a Leave Request

```
┌─────────┐     ┌────────────────────┐     ┌─────────────────────┐
│  Client │────▶│ LeaveRequestsCtrl  │────▶│ LeaveRequestService │
└─────────┘     └────────────────────┘     └──────────┬──────────┘
                                                      │
    ┌─────────────────────────────────────────────────┘
    │
    ▼
┌───────────────────────────────────────────────────────────────┐
│                    Validation Steps                           │
├───────────────────────────────────────────────────────────────┤
│ 1. Validate Employee exists (IEmployeeRepository)             │
│ 2. Validate LeaveType exists (ILeaveTypeRepository)           │
│ 3. Validate dates (EndDate >= StartDate)                      │
│ 4. Check for overlapping requests (ILeaveRequestRepository)   │
└───────────────────────────────────────────────────────────────┘
    │
    ▼ (If all validations pass)
┌───────────────────────────────────────────────────────────────┐
│              ILeaveRequestRepository.CreateAsync()            │
└───────────────────────────────────────────────────────────────┘
    │
    ▼
┌───────────────────────────────────────────────────────────────┐
│                  LeaveDbContext.SaveChanges()                 │
└───────────────────────────────────────────────────────────────┘
    │
    ▼
┌───────────────────────────────────────────────────────────────┐
│               Return ApiResponse<LeaveRequestDto>             │
└───────────────────────────────────────────────────────────────┘
```

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | ASP.NET Core (.NET 10) |
| **Language** | C# 12 |
| **ORM** | Entity Framework Core 10 |
| **Database** | SQL Server / In-Memory (configurable) |
| **Authentication** | JWT Bearer Tokens |
| **Password Hashing** | BCrypt |
| **API Documentation** | OpenAPI 3.0 + Scalar UI |
| **Dependency Injection** | Built-in ASP.NET Core DI |

---

## Configuration

### Database Configuration (Program.cs)

```csharp
// Option 1: In-Memory Database (Development/Demo)
builder.Services.AddDbContext<LeaveDbContext>(options =>
    options.UseInMemoryDatabase("LeaveManagementDb"));

// Option 2: SQL Server (Production)
builder.Services.AddDbContext<LeaveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveManagementDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "LeaveManagementApi",
    "Audience": "LeaveManagementApi",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@company.com",
    "SenderName": "Leave Management System",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

### JWT Configuration (Program.cs)

```csharp
// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 / VS Code
- SQL Server (optional, for production)

### Run the Application

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Access API Documentation
# Open browser: http://localhost:5000/scalar/v1
```

### Default Test Credentials

The application seeds the following test users on startup:

| Email | Role | Password |
|-------|------|----------|
| `admin@company.com` | Admin | `Password123!` |
| `sarah.williams@company.com` | Manager | `Password123!` |
| `mike.johnson@company.com` | Manager | `Password123!` |
| `john.doe@company.com` | Employee | `Password123!` |
| `jane.smith@company.com` | Employee | `Password123!` |

### Test API Endpoints

```bash
# 1. Login to get JWT token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@company.com",
    "password": "Password123!"
  }'

# Response: { "accessToken": "eyJ...", "refreshToken": "..." }

# 2. Get all employees (with token)
curl http://localhost:5000/api/employees \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# 3. Create a leave request (with token)
curl -X POST http://localhost:5000/api/leaverequests \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "employeeId": 1,
    "leaveTypeId": 1,
    "startDate": "2025-01-15",
    "endDate": "2025-01-17",
    "reason": "Vacation"
  }'

# 4. Get my leave balances
curl http://localhost:5000/api/leavebalance/my \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# 5. Approve a leave request (Manager/Admin only)
curl -X POST http://localhost:5000/api/leaverequests/1/approve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{ "comments": "Approved. Enjoy your vacation!" }'
```

---

## Future Enhancements

- [x] JWT Authentication & Authorization ✅
- [x] Role-based access (Admin, Manager, Employee) ✅
- [x] Leave balance tracking ✅
- [x] Email notifications ✅
- [x] Audit logging ✅
- [ ] Pagination for large datasets
- [ ] Unit & Integration tests
- [ ] Rate limiting
- [ ] Swagger/OpenAPI authentication UI
- [ ] Password reset functionality
- [ ] Two-factor authentication (2FA)

---

*Document Version: 2.0*  
*Last Updated: January 2025*
