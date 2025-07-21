using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the ApplyForLeaveTool.
    /// Submits a leave request for a staff member and validates overlap or conflicts.
    /// </summary>
    public class ApplyForLeaveToolHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<ApplyForLeaveToolHandler> _logger;

        public ApplyForLeaveToolHandler(
            IStaffRepository repository,
            ILogger<ApplyForLeaveToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => ApplyForLeaveTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            try
            {
                // Validate and parse staffId
                if (!root.TryGetProperty("staffId", out var staffIdProp) || staffIdProp.GetInt32() <= 0)
                {
                    return CreateError(call.Id, "Invalid or missing staffId. It must be a positive integer.");
                }

                // Validate and parse leaveStart
                if (!root.TryGetProperty("leaveStart", out var startProp) ||
                    !DateTime.TryParse(startProp.GetString(), out var leaveStart))
                {
                    return CreateError(call.Id, "Invalid or missing leaveStart. Expected format: YYYY-MM-DD.");
                }

                // Validate and parse leaveEnd
                if (!root.TryGetProperty("leaveEnd", out var endProp) ||
                    !DateTime.TryParse(endProp.GetString(), out var leaveEnd))
                {
                    return CreateError(call.Id, "Invalid or missing leaveEnd. Expected format: YYYY-MM-DD.");
                }

                // Optional leave type
                string leaveType = root.TryGetProperty("leaveType", out var leaveTypeProp)
                    ? leaveTypeProp.GetString() ?? "Unspecified"
                    : "Unspecified";

                if (leaveEnd < leaveStart)
                {
                    return CreateError(call.Id, "leaveEnd must be on or after leaveStart.");
                }

                var leaveRequest = new ApplyForLeaveRequest
                {
                    StaffId = staffIdProp.GetInt32(),
                    LeaveStart =  leaveStart,
                    LeaveEnd = leaveEnd,
                    LeaveType = leaveType
                };

                bool success = await _repository.ApplyForLeaveAsync(leaveRequest);

                var result = new
                {
                    success,
                    message = success
                        ? "Leave request submitted successfully."
                        : "Leave request overlaps with an existing leave or failed due to unknown error."
                };

                var resultJson = JsonSerializer.Serialize(result);
                _logger.LogInformation("ApplyForLeaveTool result: {Result}", resultJson);

                return new ToolOutput(call.Id, resultJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in ApplyForLeaveToolHandler.");
                return CreateError(call.Id, "An internal error occurred while processing the leave request.");
            }
        }

        private static ToolOutput CreateError(string callId, string message)
        {
            var errorJson = JsonSerializer.Serialize(new { success = false, message });
            return new ToolOutput(callId, errorJson);
        }
    }
}
