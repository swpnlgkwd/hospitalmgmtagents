using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools.HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using HospitalStaffMgmtApis.Models.Requests;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers.ShiftHandlers
{
    /// <summary>
    /// Handles the execution of the UncoverShiftTool, responsible for retrieving uncovered (unassigned) shifts
    /// based on provided filters such as date range, department, role, or shift type.
    /// </summary>
    public class UncoverShiftToolHandler : IToolHandler
    {
        private readonly IShiftRepository _repository;
        private readonly ILogger<UncoverShiftToolHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UncoverShiftToolHandler"/> class.
        /// </summary>
        /// <param name="repository">The staff repository for accessing uncovered shifts.</param>
        /// <param name="logger">Logger instance for logging tool execution.</param>
        public UncoverShiftToolHandler(IShiftRepository repository, ILogger<UncoverShiftToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Gets the name of the tool this handler supports.
        /// </summary>
        public string ToolName => UncoverShiftsTool.GetTool().Name;

        /// <summary>
        /// Handles the tool execution for fetching uncovered shifts.
        /// Validates input parameters, applies defaults, and invokes repository method to retrieve results.
        /// </summary>
        /// <param name="call">The required function call from the agent.</param>
        /// <param name="root">The root JSON element containing input parameters.</param>
        /// <returns>Tool output containing uncovered shift data in JSON format.</returns>
        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            var request = new GetUncoveredShiftsRequest();

            // Parse FromDate (required)
            if (root.TryGetProperty("fromDate", out var fromDateProp) &&
                DateTime.TryParse(fromDateProp.GetString(), out var parsedFromDate))
            {
                request.FromDate = parsedFromDate;
            }
            else
            {
                request.FromDate = DateTime.Today; // default if not provided or invalid
            }

            // Parse ToDate (optional)
            if (root.TryGetProperty("toDate", out var toDateProp) &&
                DateTime.TryParse(toDateProp.GetString(), out var parsedToDate))
            {
                request.ToDate = parsedToDate;
            }

            // Parse DepartmentId (optional)
            if (root.TryGetProperty("departmentId", out var deptProp) &&
                deptProp.ValueKind == JsonValueKind.Number &&
                deptProp.TryGetInt32(out var deptId))
            {
                request.DepartmentId = deptId;
            }

            // Parse Role (optional)
            if (root.TryGetProperty("role", out var roleProp) &&
                roleProp.ValueKind == JsonValueKind.String)
            {
                request.Role = roleProp.GetString();
            }

            // Parse ShiftType (optional)
            if (root.TryGetProperty("shiftType", out var shiftTypeProp) &&
                shiftTypeProp.ValueKind == JsonValueKind.String)
            {
                request.ShiftType = shiftTypeProp.GetString();
            }
            

            // Call the repository with parsed request
            var uncoveredShifts = await _repository.GetUncoveredShiftsAsync(request);

            var resultJson = JsonSerializer.Serialize(uncoveredShifts, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("Shift schedule data retrieved: {Result}", resultJson);

            return new ToolOutput(call.Id, resultJson);
        }

        /// <summary>
        /// Returns a ToolOutput with a formatted error message.
        /// </summary>
        /// <param name="callId">Tool call ID for correlation.</param>
        /// <param name="message">Error message to return.</param>
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
