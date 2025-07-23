using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Repository;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the GetShiftSchedule tool.
    /// Fetches shift schedules based on staff ID, date range, and shift type.
    /// </summary>
    //public class FindCoverageToolHandler : IToolHandler
    //{
    //    private readonly IStaffRepository _repository;
    //    private readonly ILogger<GetShiftScheduleToolHandler> _logger;

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="GetShiftScheduleToolHandler"/> class.
    //    /// </summary>
    //    /// <param name="repository">The staff repository for accessing shift schedules.</param>
    //    /// <param name="logger">Logger instance for logging shift schedule activities.</param>
    //    public FindCoverageToolHandler(IStaffRepository repository, ILogger<GetShiftScheduleToolHandler> logger)
    //    {
    //        _repository = repository;
    //        _logger = logger;
    //    }

    //    /// <summary>
    //    /// Gets the name of the tool this handler supports.
    //    /// </summary>
    //    public string ToolName => GetShiftScheduleTool.GetTool().Name;

    //    /// <summary>
    //    /// Handles the execution of the GetShiftSchedule tool.
    //    /// Validates and parses input parameters, applies fallbacks, and retrieves shift schedules.
    //    /// </summary>
    //    public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
    //    {
    //        var request = new Coverage
    //        {

    //            FromDate = root.TryGetProperty("fromDate", out var fromDateProp) &&
    //                       DateTime.TryParse(fromDateProp.GetString(), out var fromDate)
    //                ? fromDate,                    

    //            ToDate = root.TryGetProperty("toDate", out var toDateProp) &&
    //                     DateTime.TryParse(toDateProp.GetString(), out var toDate)
    //                ? toDate,

    //            ShiftType = root.TryGetProperty("shiftType", out var shiftTypeProp)
    //                ? shiftTypeProp.GetString()
    //                : null,


    //            Department = root.TryGetProperty("department", out var departmentProp)
    //                ? shiftTypeProp.GetString()
    //                : null

    //        };

    //        //// ✅ staffId is mandatory
    //        //if (!request.StaffId.HasValue)
    //        //{
    //        //    return ErrorOutput(call.Id, "staffId is required to fetch shift schedule.");
    //        //}

    //        // ✅ Default fromDate to today if not provided
    //        request.FromDate ??= DateOnly.FromDateTime(DateTime.Today);

    //        // ❌ If toDate is before fromDate → invalid
    //        if (request.ToDate.HasValue && request.ToDate < request.FromDate)
    //        {
    //            return ErrorOutput(call.Id, "toDate cannot be earlier than fromDate.");
    //        }

    //        // ✅ Validate shiftType if present
    //        var validShiftTypes = new[] { "Morning", "Evening", "Night" };
    //        if (!string.IsNullOrWhiteSpace(request.ShiftType) &&
    //            !validShiftTypes.Contains(request.ShiftType, StringComparer.OrdinalIgnoreCase))
    //        {
    //            return ErrorOutput(call.Id, $"Invalid shift type '{request.ShiftType}'. Allowed values: Morning, Evening, Night.");
    //        }

    //        _logger.LogInformation("Fetching shift schedule with parameters: {@Request}", request);

    //        var shiftResults = await _repository.GetShiftScheduleAsync(request);

    //        var resultJson = JsonSerializer.Serialize(shiftResults, new JsonSerializerOptions
    //        {
    //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //        });

    //        _logger.LogInformation("Shift schedule data retrieved: {Result}", resultJson);

    //        return new ToolOutput(call.Id, resultJson);
    //    }

    //    private ToolOutput ErrorOutput(string callId, string message)
    //    {
    //        var errorJson = JsonSerializer.Serialize(new
    //        {
    //            success = false,
    //            error = message
    //        });

    //        _logger.LogWarning("Validation failed: {Error}", message);
    //        return new ToolOutput(callId, errorJson);
    //    }
    //}
}
