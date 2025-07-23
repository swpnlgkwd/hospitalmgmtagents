using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools.Leave;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using HospitalStaffMgmtApis.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers.LeaveRequestHandlers
{
    /// <summary>
    /// Handler for the ViewPendingLeaveRequestTool.
    /// Retrieves pending leave requests with optional filters for staff name, date range, or department.
    /// </summary>
    public class ViewPendingLeaveRequestToolHandler : IToolHandler
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly ILogger<ViewPendingLeaveRequestToolHandler> _logger;

        public ViewPendingLeaveRequestToolHandler(
            ILeaveRequestRepository repository,
            ILogger<ViewPendingLeaveRequestToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Name of the tool as defined in FunctionToolDefinition.
        /// </summary>
        public string ToolName => ViewPendingLeaveRequestsTool.GetTool().Name;

        /// <summary>
        /// Handles the request by parsing input JSON and returning filtered pending leave requests.
        /// </summary>
        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            try
            {
                var pendingRequest = new PendingLeaveRequest();

                if (root.TryGetProperty("staffName", out var staffNameProp))
                {
                    pendingRequest.StaffName = staffNameProp.GetString();
                }

                if (root.TryGetProperty("departmentName", out var deptProp))
                {
                    pendingRequest.DepartmentName = deptProp.GetString();
                }


                if (root.TryGetProperty("fromDate", out var fromDateProp))
                {
                    pendingRequest.FromDate = fromDateProp.GetString();
                }

                if (root.TryGetProperty("toDate", out var toDateProp))
                {
                    pendingRequest.ToDate = toDateProp.GetString();
                }

                var result = await _repository.FetchPendingLeaveRequestsAsync(pendingRequest);

                var resultJson = JsonSerializer.Serialize(result);
                _logger.LogInformation("ViewPendingLeaveRequestTool result: {Result}", resultJson);

                return new ToolOutput(call.Id, resultJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ViewPendingLeaveRequestToolHandler");
                return CreateError(call.Id, "An internal error occurred while retrieving leave requests.");
            }
        }

        /// <summary>
        /// Creates a ToolOutput with error details.
        /// </summary>
        private static ToolOutput CreateError(string callId, string message)
        {
            var errorJson = JsonSerializer.Serialize(new { success = false, message });
            return new ToolOutput(callId, errorJson);
        }
    }
}
