using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Tools.Shift;
using HospitalSchedulingApp.Dtos.Shift.Requests;
using HospitalSchedulingApp.Services.Interfaces;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Handlers.Shift
{
    public class FilterPlannedShiftsToolHandler : IToolHandler
    {
        private readonly IPlannedShiftService _plannedShiftService;
        private readonly ILogger<FilterPlannedShiftsToolHandler> _logger;

        public FilterPlannedShiftsToolHandler(IPlannedShiftService plannedShiftService, ILogger<FilterPlannedShiftsToolHandler> logger)
        {
            _plannedShiftService = plannedShiftService;
            _logger = logger;
        }

        public string ToolName => FilterShiftScheduleTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            var filter = new ShiftFilterDto
            {
                StaffName = root.TryGetProperty("staffName", out var staffNameProp) ? staffNameProp.GetString() : null,
                DepartmentName = root.TryGetProperty("departmentName", out var deptNameProp) ? deptNameProp.GetString() : null,
                ShiftTypeName = root.TryGetProperty("shiftTypeName", out var shiftTypeProp) ? shiftTypeProp.GetString() : null,
                ShiftStatusName = root.TryGetProperty("shiftStatusName", out var statusProp) ? statusProp.GetString() : null,
                FromDate = root.TryGetProperty("fromDate", out var fromProp) && DateTime.TryParse(fromProp.GetString(), out var fromDate)
                            ? fromDate : null,
                ToDate = root.TryGetProperty("toDate", out var toProp) && DateTime.TryParse(toProp.GetString(), out var toDate)
                            ? toDate : null
            };

            _logger.LogInformation("Filtering shifts with: {@Filter}", filter);

            var results = await _plannedShiftService.FetchFilteredPlannedShiftsAsync(filter);

            var json = JsonSerializer.Serialize(results, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return new ToolOutput(call.Id, json);
        }

        private ToolOutput ErrorOutput(string callId, string message)
        {
            var errorJson = JsonSerializer.Serialize(new
            {
                success = false,
                error = message
            });

            _logger.LogWarning("Validation failed: {Error}", message);
            return new ToolOutput(callId, errorJson);
        }
    }
}
