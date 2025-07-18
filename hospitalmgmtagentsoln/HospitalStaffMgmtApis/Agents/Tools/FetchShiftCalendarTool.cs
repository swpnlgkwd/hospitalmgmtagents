using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class FetchShiftCalendarTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "fetchShiftCalendar",
                description: "Use this tool when a user wants to view scheduled shifts over a date range" +
                ".Fetches the complete hospital shift calendar showing all staff assignments between two dates. " +
                "Use this to view overall staffing for departments and shifts." +
                " It returns the complete shift calendar showing which staff are assigned to which shifts " +
                "between the specified start and end dates.",
                parameters: BinaryData.FromObjectAsJson(
                new
                {
                    type = "object",
                    properties = new
                    {
                        startDate = new { type = "string", description = "Start date of the range in yyyy-MM-dd format" },
                        endDate = new { type = "string", description = "End date of the range in yyyy-MM-dd format" }
                    },
                    required = new[] { "startDate", "endDate" }
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                 )
            );
        }
    }
}
