using LeaveManagementApi.DTOs;
using LeaveManagementApi.Models;
using LeaveManagementApi.Repositories;

namespace LeaveManagementApi.Services;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public LeaveTypeService(ILeaveTypeRepository leaveTypeRepository)
    {
        _leaveTypeRepository = leaveTypeRepository;
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetAllAsync()
    {
        var leaveTypes = await _leaveTypeRepository.GetAllAsync();
        return leaveTypes.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetActiveAsync()
    {
        var leaveTypes = await _leaveTypeRepository.GetActiveAsync();
        return leaveTypes.Select(MapToDto);
    }

    public async Task<LeaveTypeDto?> GetByIdAsync(int id)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
        return leaveType != null ? MapToDto(leaveType) : null;
    }

    public async Task<ApiResponse<LeaveTypeDto>> CreateAsync(CreateLeaveTypeDto dto)
    {
        // Check if name already exists
        var existingLeaveType = await _leaveTypeRepository.GetByNameAsync(dto.Name);
        if (existingLeaveType != null)
        {
            return ApiResponse<LeaveTypeDto>.FailureResponse(
                "A leave type with this name already exists.");
        }

        var leaveType = new LeaveType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDays = dto.DefaultDays,
            IsActive = true
        };

        var createdLeaveType = await _leaveTypeRepository.CreateAsync(leaveType);
        return ApiResponse<LeaveTypeDto>.SuccessResponse(
            MapToDto(createdLeaveType), 
            "Leave type created successfully.");
    }

    public async Task<ApiResponse<LeaveTypeDto>> UpdateAsync(int id, UpdateLeaveTypeDto dto)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
        if (leaveType == null)
        {
            return ApiResponse<LeaveTypeDto>.FailureResponse("Leave type not found.");
        }

        // Check if name is being changed and if new name already exists
        if (!string.Equals(leaveType.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingLeaveType = await _leaveTypeRepository.GetByNameAsync(dto.Name);
            if (existingLeaveType != null)
            {
                return ApiResponse<LeaveTypeDto>.FailureResponse(
                    "A leave type with this name already exists.");
            }
        }

        leaveType.Name = dto.Name;
        leaveType.Description = dto.Description;
        leaveType.DefaultDays = dto.DefaultDays;
        leaveType.IsActive = dto.IsActive;

        var updatedLeaveType = await _leaveTypeRepository.UpdateAsync(leaveType);
        return ApiResponse<LeaveTypeDto>.SuccessResponse(
            MapToDto(updatedLeaveType), 
            "Leave type updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var exists = await _leaveTypeRepository.ExistsAsync(id);
        if (!exists)
        {
            return ApiResponse<bool>.FailureResponse("Leave type not found.");
        }

        var result = await _leaveTypeRepository.DeleteAsync(id);
        return result 
            ? ApiResponse<bool>.SuccessResponse(true, "Leave type deleted successfully.")
            : ApiResponse<bool>.FailureResponse("Failed to delete leave type.");
    }

    private static LeaveTypeDto MapToDto(LeaveType leaveType)
    {
        return new LeaveTypeDto
        {
            Id = leaveType.Id,
            Name = leaveType.Name,
            Description = leaveType.Description,
            DefaultDays = leaveType.DefaultDays,
            IsActive = leaveType.IsActive
        };
    }
}
