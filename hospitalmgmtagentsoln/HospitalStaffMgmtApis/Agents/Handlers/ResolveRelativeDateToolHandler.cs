using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for resolving relative date phrases such as "today", "next week", etc.
    /// Returns a resolved date or date range in yyyy-MM-dd format.
    /// </summary>
    public class ResolveRelativeDateToolHandler : IToolHandler
    {
        private readonly ILogger<ResolveRelativeDateToolHandler> _logger;

        public ResolveRelativeDateToolHandler(
            IStaffRepository repository,
            ILogger<ResolveRelativeDateToolHandler> logger)
        {
            _logger = logger;
        }

        public string ToolName => ResolveRelativeDateTool.GetTool().Name;

        public Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("phrase", out var phraseElement))
                {
                    _logger.LogWarning("Missing 'phrase' parameter in resolveRelativeDate tool call.");
                    return Task.FromResult<ToolOutput?>(null);
                }

                var phrase = phraseElement.GetString()?.ToLowerInvariant().Trim() ?? string.Empty;
                var today = DateTime.UtcNow.Date;
                string resultJson;

                switch (phrase)
                {
                    case "today":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.ToString("yyyy-MM-dd") });
                        break;

                    case "tomorrow":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddDays(1).ToString("yyyy-MM-dd") });
                        break;

                    case "yesterday":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddDays(-1).ToString("yyyy-MM-dd") });
                        break;

                    case "day after tomorrow":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddDays(2).ToString("yyyy-MM-dd") });
                        break;

                    case "day before yesterday":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddDays(-2).ToString("yyyy-MM-dd") });
                        break;

                    case "next week":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddDays(7).ToString("yyyy-MM-dd") });
                        break;

                    case "last week":
                    case "previous week":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddDays(-7).ToString("yyyy-MM-dd") });
                        break;

                    case "next month":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddMonths(1).ToString("yyyy-MM-dd") });
                        break;

                    case "last month":
                    case "previous month":
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.AddMonths(-1).ToString("yyyy-MM-dd") });
                        break;

                    case "this weekend":
                        var nextSaturday = GetNextWeekday(today, DayOfWeek.Saturday);
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = nextSaturday.ToString("yyyy-MM-dd") });
                        break;

                    case "last weekend":
                        var lastSaturday = GetLastWeekdayBefore(today, DayOfWeek.Saturday);
                        var lastSunday = lastSaturday.AddDays(1);
                        resultJson = JsonSerializer.Serialize(new
                        {
                            startDate = lastSaturday.ToString("yyyy-MM-dd"),
                            endDate = lastSunday.ToString("yyyy-MM-dd")
                        });
                        break;

                    default:
                        resultJson = JsonSerializer.Serialize(new { resolvedDate = today.ToString("yyyy-MM-dd") });
                        break;
                }

                return Task.FromResult<ToolOutput?>(new ToolOutput(call.Id,  resultJson));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving relative date.");
                return Task.FromResult<ToolOutput?>(null);
            }
        }

        private static DateTime GetNextWeekday(DateTime from, DayOfWeek targetDay)
        {
            int daysToAdd = ((int)targetDay - (int)from.DayOfWeek + 7) % 7;
            return from.AddDays(daysToAdd == 0 ? 7 : daysToAdd);
        }

        private static DateTime GetLastWeekdayBefore(DateTime from, DayOfWeek targetDay)
        {
            int daysBack = ((int)from.DayOfWeek - (int)targetDay + 7) % 7;
            return from.AddDays(daysBack == 0 ? -7 : -daysBack);
        }
    }
}
