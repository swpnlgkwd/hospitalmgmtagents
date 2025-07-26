using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools.Shift;
using HospitalStaffMgmtApis.Agents.Tools.Shifts;
using HospitalStaffMgmtApis.Data.Models.Shift;
using HospitalStaffMgmtApis.Data.Models.Staff;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers.ShiftHandlers
{
    /// <summary>
    /// Handler for the AssignStaffToShift tool.
    /// Assigns a staff member to a shift using shiftId or shiftDate + shiftType, optionally replacing an existing staff.
    /// </summary>
    public class AssignShiftToStaffToolHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<AssignShiftToStaffToolHandler> _logger;

        public AssignShiftToStaffToolHandler(
            IStaffRepository repository,
            ILogger<AssignShiftToStaffToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => AssignStaffToShiftTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            try
            {
                var request = new AssignShiftRequest
                {
                    FromStaffId = root.TryGetProperty("fromStaffId", out var fromProp) ? fromProp.GetInt32() : (int?)null,
                    ToStaffId = root.GetProperty("toStaffId").GetInt32(),
                    ShiftId = root.TryGetProperty("shiftId", out var shiftIdProp) ? shiftIdProp.GetInt32() : (int?)null,
                    ShiftDate = root.TryGetProperty("shiftDate", out var dateProp) ? dateProp.GetString() ?? string.Empty : string.Empty,
                    ShiftType = root.TryGetProperty("shiftType", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty
                };

                if (!request.ShiftId.HasValue && (string.IsNullOrWhiteSpace(request.ShiftDate) || string.IsNullOrWhiteSpace(request.ShiftType)))
                {
                    return ErrorResponse(call.Id, "Either 'shiftId' or both 'shiftDate' and 'shiftType' must be provided.");
                }

                var result = await _repository.AssignStaffToShift(request);

                var outputJson = JsonSerializer.Serialize(new
                {
                    success = true,
                    message = result
                });

                _logger.LogInformation("AssignStaffToShift completed successfully: {Result}", outputJson);
                return new ToolOutput(call.Id, outputJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling assignStaffToShift tool call");
                return ErrorResponse(call.Id, "Internal error while assigning shift.");
            }
        }

        private ToolOutput ErrorResponse(string callId, string message)
        {
            var errorJson = JsonSerializer.Serialize(new
            {
                success = false,
                message
            });

            return new ToolOutput(callId, errorJson);
        }
    }

}
