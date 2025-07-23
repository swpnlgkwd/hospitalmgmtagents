using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the AutoReplaceShiftsForLeaveTool.
    /// Automatically finds and assigns replacement staff for all shifts impacted by a leave.
    /// </summary>
    public class AutoReplaceShiftsForLeaveToolHandler : IToolHandler
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly ILogger<AutoReplaceShiftsForLeaveToolHandler> _logger;

        public AutoReplaceShiftsForLeaveToolHandler(
            ILeaveRequestRepository repository,
            ILogger<AutoReplaceShiftsForLeaveToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => AutoReplaceShiftsForLeaveTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            // Extract and validate staffId
            if (!root.TryGetProperty("staffId", out var staffIdProp) || staffIdProp.GetInt32() <= 0)
            {
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Invalid or missing staffId. It must be a positive integer."
                }));
            }

            int staffId = staffIdProp.GetInt32();

            // Extract and validate fromDate
            if (!root.TryGetProperty("fromDate", out var fromDateProp) ||
                !DateOnly.TryParse(fromDateProp.GetString(), out var fromDate))
            {
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Invalid or missing fromDate. Expected format: YYYY-MM-DD."
                }));
            }

            // Extract and validate toDate (optional, fallback to fromDate)
            DateOnly toDate = fromDate;
            if (root.TryGetProperty("toDate", out var toDateProp) &&
                DateOnly.TryParse(toDateProp.GetString(), out var parsedToDate))
            {
                toDate = parsedToDate;
            }

            // Ensure fromDate <= toDate
            if (toDate < fromDate)
            {
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "toDate cannot be earlier than fromDate."
                }));
            }

            var request = new GetImpactedShiftsByLeaveRequest
            {
                StaffId = staffId,
                FromDate = fromDate.ToDateTime(TimeOnly.MinValue),
                ToDate = toDate.ToDateTime(TimeOnly.MaxValue)
            };

            var result = await _repository.AutoReplaceShiftsForLeaveAsync(request);

            var resultJson = JsonSerializer.Serialize(new
            {
                success = true,
                message = result
            });

            _logger.LogInformation("Auto replace shifts tool result: {Result}", resultJson);
            return new ToolOutput(call.Id, resultJson);
        }
    }
}
