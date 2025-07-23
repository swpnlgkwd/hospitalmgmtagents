using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Agents.Tools.Shift;
using HospitalStaffMgmtApis.Data.Models.Staff;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using HospitalStaffMgmtApis.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers.ShiftHandlers
{
    /// <summary>
    /// Handles the execution of the BackToBackWeeklyShiftTool, 
    /// responsible for retrieving staff with back-to-back (fatigued) shifts.
    /// </summary>
    public class ViewBacktoBackWeeklyShiftHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<ViewBacktoBackWeeklyShiftHandler> _logger;

        public ViewBacktoBackWeeklyShiftHandler(IStaffRepository repository, ILogger<ViewBacktoBackWeeklyShiftHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => BackToBackWeeklyShiftTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            var request = new FatiqueStaffRequest
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                StaffName = string.Empty
            };

            if (root.TryGetProperty("fromDate", out var fromDateProp) &&
                DateTime.TryParse(fromDateProp.GetString(), out var parsedFromDate))
            {
                request.StartDate = parsedFromDate;
            }

            if (root.TryGetProperty("toDate", out var toDateProp) &&
                DateTime.TryParse(toDateProp.GetString(), out var parsedToDate))
            {
                request.EndDate = parsedToDate;
            }

            if (root.TryGetProperty("staffName", out var staffNameProp))
            {
                request.StaffName = staffNameProp.GetString() ?? string.Empty;
            }

            try
            {
                var fatiguedStaff = await _repository.GetFatiguedStaffAsync(request);

                var resultJson = JsonSerializer.Serialize(fatiguedStaff, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Fatigued staff fetched: {Result}", resultJson);

                return new ToolOutput(call.Id, resultJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fatigued staff shifts.");
                return ErrorOutput(call.Id, "Failed to retrieve data due to an internal error.");
            }
        }

        private ToolOutput ErrorOutput(string callId, string message)
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
