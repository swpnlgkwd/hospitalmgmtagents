
using Azure.AI.Agents.Persistent;
using global::HospitalStaffMgmtApis.Agents.Handlers;
using global::HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest;
using global::HospitalStaffMgmtApis.Data.Repository.Interfaces;
using HospitalStaffMgmtApis.Agents.Tools.Leave.HospitalStaffMgmtApis.Agents.Tools.Leave;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools.Leave.HospitalStaffMgmtApis.Agents.Handlers.LeaveRequestHandlers
{
    public class ApproveOrRejectLeaveRequestToolHandler : IToolHandler
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly ILogger<ApproveOrRejectLeaveRequestToolHandler> _logger;

        public ApproveOrRejectLeaveRequestToolHandler(
            ILeaveRequestRepository repository,
            ILogger<ApproveOrRejectLeaveRequestToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => ApproveOrRejectLeaveRequestTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            try
            {
                var leaveActionRequest = new LeaveActionRequest();

                if (root.TryGetProperty("leaveRequestId", out var leaveIdProp) && leaveIdProp.TryGetInt32(out int leaveId))
                    leaveActionRequest.LeaveRequestId = leaveId;

                if (root.TryGetProperty("staffId", out var staffIdProp) && staffIdProp.TryGetInt32(out int staffId))
                    leaveActionRequest.StaffId = staffId;

                if (root.TryGetProperty("staffName", out var staffNameProp))
                    leaveActionRequest.StaffName = staffNameProp.GetString();

                if (root.TryGetProperty("leaveStartDate", out var startProp))
                    leaveActionRequest.LeaveStartDate = startProp.GetString();

                if (root.TryGetProperty("leaveEndDate", out var endProp))
                    leaveActionRequest.LeaveEndDate = endProp.GetString();

                if (root.TryGetProperty("approvalStatus", out var statusProp))
                    leaveActionRequest.ApprovalStatus = statusProp.GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(leaveActionRequest.ApprovalStatus))
                {
                    return CreateError(call.Id, "approvalStatus is required.");
                }

                if (leaveActionRequest.LeaveRequestId == null)
                {
                    if (string.IsNullOrWhiteSpace(leaveActionRequest.StaffName) ||
                        string.IsNullOrWhiteSpace(leaveActionRequest.LeaveStartDate) ||
                        string.IsNullOrWhiteSpace(leaveActionRequest.LeaveEndDate))
                    {
                        return CreateError(call.Id, "Either leaveRequestId must be provided, or staffName, leaveStartDate, and leaveEndDate are required.");
                    }
                }

                var resultMessage = await _repository.ApproveOrRejectLeaveRequestAsync(leaveActionRequest);

                var resultJson = JsonSerializer.Serialize(new { success = true, message = resultMessage });
                _logger.LogInformation("Leave approval handled: {Result}", resultJson);

                return new ToolOutput(call.Id, resultJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ApproveOrRejectLeaveRequestToolHandler");
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

