using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Tools.Staff;
using HospitalSchedulingApp.Services.Interfaces;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Handlers.Staff
{
    public class ResolveStaffInfoByNameToolHandler : IToolHandler
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<ResolveStaffInfoByNameToolHandler> _logger;

        public ResolveStaffInfoByNameToolHandler(
            IStaffService staffService,
            ILogger<ResolveStaffInfoByNameToolHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public string ToolName => ResolveStaffInfoByNameTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            string inputName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()?.Trim() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(inputName))
            {
                _logger.LogWarning("resolveStaffInfoByName: Name was not provided.");
                return CreateError(call.Id, "Staff name is required.");
            }

            if (inputName.Length < 2)
            {
                _logger.LogWarning("resolveStaffInfoByName: Name '{Input}' is too short.", inputName);
                return CreateError(call.Id, "Staff name must be at least 2 characters long.");
            }

            var staffMatches = await _staffService.FetchActiveStaffByNamePatternAsync(inputName);

            if (staffMatches == null || staffMatches.Count == 0)
            {
                _logger.LogInformation("resolveStaffInfoByName: No match found for '{Name}'", inputName);
                return CreateError(call.Id, $"No staff found matching: {inputName}");
            }

            var result = new
            {
                success = true,
                matches = staffMatches.Select(s => new
                {
                    staffId = s.StaffId,
                    staffName = s.StaffName,
                    roleId = s.RoleId,
                    roleName = s.RoleName,
                    departmentId = s.StaffDepartmentId,
                    departmentName = s.StaffDepartmentName
                })
            };

            _logger.LogInformation("resolveStaffInfoByName: Found {Count} match(es) for '{Input}'", staffMatches.Count, inputName);
            return new ToolOutput(call.Id, JsonSerializer.Serialize(result));
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
}
