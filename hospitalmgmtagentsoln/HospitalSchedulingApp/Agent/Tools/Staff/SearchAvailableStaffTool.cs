using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Tools.Staff
{


    public static class SearchAvailableStaffTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "searchAvailableStaff",
                description: "Finds staff members who are free and eligible to work during a given date or date range, filtered by shift type and optionally by department. "
                           + "Validates staff availability, approved leaves, existing shift assignments, and fatigue rules to suggest only suitable and unassigned staff. "
                           + "Supports both planning future shifts and replacing staff in existing ones.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            startDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Required. Start date of the availability window (YYYY-MM-DD). Can be inferred from natural language like 'tomorrow', 'next week', etc."
                            },
                            endDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Required. End date of the availability window (YYYY-MM-DD). If same as startDate, it's treated as a single-day query."
                            },
                            shiftType = new
                            {
                                type = "string",
                                description = "Optional. Shift type like 'Morning', 'Evening', or 'Night'."
                            },
                            department = new
                            {
                                type = "string",
                                description = "Optional. Department name to prioritize staff from a specific department (e.g., ICU, Pediatrics, Emergency)."
                            }
                        },
                        required = new[] { "startDate", "endDate"  }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }

}
