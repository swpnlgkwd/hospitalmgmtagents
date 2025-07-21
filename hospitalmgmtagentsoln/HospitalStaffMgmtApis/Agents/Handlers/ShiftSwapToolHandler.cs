using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.FunctionTools;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Repository;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the ShiftSwapTool. Handles requests to swap shifts between two staff members.
    /// </summary>
    public class ShiftSwapToolHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<ShiftSwapToolHandler> _logger;

        public ShiftSwapToolHandler(IStaffRepository repository, ILogger<ShiftSwapToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => ShiftSwapTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            try
            {
                // Safely extract parameters from JSON input
                if (!root.TryGetProperty("staffId1", out var staffId1Prop) ||
                    !root.TryGetProperty("shiftDate1", out var shiftDate1Prop) ||
                    !root.TryGetProperty("shiftTypeId1", out var shiftTypeId1Prop) ||
                    !root.TryGetProperty("staffId2", out var staffId2Prop) ||
                    !root.TryGetProperty("shiftDate2", out var shiftDate2Prop) ||
                    !root.TryGetProperty("shiftTypeId2", out var shiftTypeId2Prop))
                {
                    _logger.LogWarning("ShiftSwapTool: Missing one or more required properties.");
                    return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "Missing required shift swap input fields."
                    }));
                }

                var request = new SwapShiftRequest
                {
                    StaffId1 = staffId1Prop.GetInt32(),
                    ShiftDate1 = shiftDate1Prop.GetDateTime(),
                    ShiftTypeId1 = shiftTypeId1Prop.GetInt32(),
                    StaffId2 = staffId2Prop.GetInt32(),
                    ShiftDate2 = shiftDate2Prop.GetDateTime(),
                    ShiftTypeId2 = shiftTypeId2Prop.GetInt32()
                };

                var result = await _repository.SwapShiftsAsync(request);

                if (!result)
                {
                    _logger.LogInformation("ShiftSwapTool: Swap failed - {Reason}");
                    return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "ShiftSwapTool: Swap failed - {Reason}"
                    }));
                }

                _logger.LogInformation("ShiftSwapTool: Swap successful ");

                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = true,
                    message = $"Shifts successfully swapped ."
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShiftSwapTool: Exception occurred during shift swap handling.");
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "Unexpected error while processing shift swap request."
                }));
            }
        }
    }
}
