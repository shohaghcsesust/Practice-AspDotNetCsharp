using LeaveManagementApi.DTOs;
using LeaveManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportsController(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _reportService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpPost("leaves")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<LeaveReportDto>>> GetLeaveReport(LeaveReportFilterDto filter)
    {
        var report = await _reportService.GetLeaveReportAsync(filter);
        return Ok(report);
    }

    [HttpGet("monthly")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<MonthlyLeaveStatsDto>>> GetMonthlyStats([FromQuery] int year)
    {
        var stats = await _reportService.GetMonthlyLeaveStatsAsync(year);
        return Ok(stats);
    }

    [HttpGet("department")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<DepartmentLeaveStatsDto>>> GetDepartmentStats([FromQuery] int year)
    {
        var stats = await _reportService.GetDepartmentLeaveStatsAsync(year);
        return Ok(stats);
    }

    [HttpGet("leave-types")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<LeaveTypeStatsDto>>> GetLeaveTypeStats([FromQuery] int year)
    {
        var stats = await _reportService.GetLeaveTypeStatsAsync(year);
        return Ok(stats);
    }

    [HttpGet("balances")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<EmployeeBalanceReportDto>>> GetBalanceReport([FromQuery] int year)
    {
        var report = await _reportService.GetLeaveBalanceReportAsync(year);
        return Ok(report);
    }

    // Export endpoints
    [HttpPost("export/leaves")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> ExportLeaveRequests(LeaveReportFilterDto filter)
    {
        var csvData = await _exportService.ExportLeaveRequestsToCsvAsync(filter);
        return File(csvData, "text/csv", $"leave-requests-{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet("export/balances")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> ExportBalances([FromQuery] int year)
    {
        var csvData = await _exportService.ExportLeaveBalancesToCsvAsync(year);
        return File(csvData, "text/csv", $"leave-balances-{year}.csv");
    }

    [HttpGet("export/employees")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ExportEmployees()
    {
        var csvData = await _exportService.ExportEmployeesToCsvAsync();
        return File(csvData, "text/csv", $"employees-{DateTime.Now:yyyyMMdd}.csv");
    }
}
