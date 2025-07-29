using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Handlers;
using HospitalSchedulingApp.Agent.Tools.Staff;
using HospitalSchedulingApp.Dtos.Staff.Requests;
using HospitalSchedulingApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class SearchAvailableStaffToolHandler : IToolHandler
{
    private readonly IStaffService _staffService;
    private readonly ILogger<SearchAvailableStaffToolHandler> _logger;

    public SearchAvailableStaffToolHandler(
        IStaffService staffService,
        ILogger<SearchAvailableStaffToolHandler> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }

    public string ToolName => SearchAvailableStaffTool.GetTool().Name;

    public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("startDate", out var startDateProp) ||
                !root.TryGetProperty("endDate", out var endDateProp))
            {
                return CreateError(call.Id, "startDate and endDate are required.");
            }

            var filterDto = new AvailableStaffFilterDto
            {
                StartDate = DateOnly.Parse(startDateProp.GetString()!),
                EndDate = DateOnly.Parse(endDateProp.GetString()!),
                ShiftType = root.TryGetProperty("shiftType", out var shiftTypeProp)
                    ? shiftTypeProp.GetString()?.Trim()
                    : null,
                Department = root.TryGetProperty("department", out var deptProp)
                    ? deptProp.GetString()
                    : null
            };

            var result = await _staffService.SearchAvailableStaffAsync(filterDto);

            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("searchAvailableStaff: No available staff found for given filter.");
                return CreateError(call.Id, "No available staff found for the given criteria.");
            }

            var output = new
            {
                success = true,
                availableStaff = result.Select(s => new
                {
                    staffId = s.StaffId,
                    staffName = s.StaffName,
                    roleId = s.RoleId,
                    roleName = s.RoleName,
                    departmentId = s.StaffDepartmentId,
                    departmentName = s.StaffDepartmentName,
                    isActive = s.IsActive
                })
            };

            return new ToolOutput(call.Id, JsonSerializer.Serialize(output));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in searchAvailableStaff tool handler.");
            return CreateError(call.Id, "An error occurred while processing the request.");
        }
    }

    private ToolOutput CreateError(string callId, string message)
    {
        var errorJson = JsonSerializer.Serialize(new
        {
            success = false,
            error = message
        });

        return new ToolOutput(callId, errorJson);
    }
}
