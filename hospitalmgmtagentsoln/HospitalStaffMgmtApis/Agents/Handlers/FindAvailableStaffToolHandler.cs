using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.FunctionTools;
using HospitalStaffMgmtApis.Data.Model.HospitalStaffMgmtApis.Models.Requests.HospitalStaffMgmtApis.Models.Requests;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the FindAvailableStaff tool.
    /// Fetches available staff based on filters like department, role, date or date range, and shift type.
    /// </summary>
    public class FindAvailableStaffToolHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<FindAvailableStaffToolHandler> _logger;

        public FindAvailableStaffToolHandler(IStaffRepository repository, ILogger<FindAvailableStaffToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => FindAvailableStaffTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            var request = new FindAvailableStaffRequest
            {
                DepartmentId = root.TryGetProperty("departmentId", out var deptProp) && deptProp.ValueKind == JsonValueKind.Number
                    ? deptProp.GetInt32()
                    : (int?)null,

                RoleId = root.TryGetProperty("roleId", out var roleProp) && roleProp.ValueKind == JsonValueKind.Number
                    ? roleProp.GetInt32()
                    : (int?)null,

                Date = root.TryGetProperty("date", out var dateProp) &&
                       DateTime.TryParse(dateProp.GetString(), out var date)
                    ? date
                    : (DateTime?)null,

                FromDate = root.TryGetProperty("fromDate", out var fromDateProp) &&
                           DateTime.TryParse(fromDateProp.GetString(), out var fromDate)
                    ? fromDate
                    : (DateTime?)null,

                ToDate = root.TryGetProperty("toDate", out var toDateProp) &&
                         DateTime.TryParse(toDateProp.GetString(), out var toDate)
                    ? toDate
                    : (DateTime?)null,

                ShiftType = root.TryGetProperty("shiftType", out var shiftTypeProp)
                    ? shiftTypeProp.GetString()
                    : null,

                IncludeFatigueCheck = root.TryGetProperty("includeFatigueCheck", out var fatigueProp) &&
                                      fatigueProp.ValueKind == JsonValueKind.True
                    ? true
                    : fatigueProp.ValueKind == JsonValueKind.False
                        ? false
                        : true // default true
            };

            // ❌ If toDate is before fromDate → invalid
            if (request.FromDate.HasValue && request.ToDate.HasValue && request.ToDate < request.FromDate)
            {
                return ErrorOutput(call.Id, "toDate cannot be earlier than fromDate.");
            }

            // ✅ Validate shiftType if present
            var validShiftTypes = new[] { "Morning", "Evening", "Night" };
            if (!string.IsNullOrWhiteSpace(request.ShiftType) &&
                !validShiftTypes.Contains(request.ShiftType, StringComparer.OrdinalIgnoreCase))
            {
                return ErrorOutput(call.Id, $"Invalid shift type '{request.ShiftType}'. Allowed values: Morning, Evening, Night.");
            }

            _logger.LogInformation("Fetching available staff with request: {@Request}", request);

            var result = await _repository.FindAvailableStaffAsync(request);

            var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("Available staff data: {Result}", resultJson);

            return new ToolOutput(call.Id, resultJson);
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
