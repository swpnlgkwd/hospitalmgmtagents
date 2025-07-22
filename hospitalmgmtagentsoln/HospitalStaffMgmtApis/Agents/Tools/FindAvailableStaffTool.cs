using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class FindAvailableStaffTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "findAvailableStaff",
                description: "Finds staff members who are free and eligible to work during a given date or date range, filtered by shift type, department, and/or role. "
                           + "Supports both planning future shifts and replacing staff in existing ones. "
                           + "Validates staff availability, approved leaves, existing shift assignments, and fatigue rules to suggest only suitable and unassigned staff. "
                           + "Works for single-day queries (e.g., 'Who is free tomorrow night?') and range-based queries (e.g., 'Who’s free next week for morning shifts?').",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            fromDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Required. Start date of the availability window (YYYY-MM-DD). Can be inferred from terms like 'next week', 'this weekend', etc."
                            },
                            toDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. End date of the availability window (YYYY-MM-DD). If not provided, only fromDate is considered."
                            },
                            shiftType = new
                            {
                                type = "string",
                                description = "Optional. Shift type like 'Morning', 'Evening', or 'Night'."
                            },
                            departmentId = new
                            {
                                type = "integer",
                                description = "Optional. Department ID to filter staff by department (e.g., ICU, Emergency, Pediatrics)."
                            },
                            role = new
                            {
                                type = "string",
                                description = "Optional. Role or specialization (e.g., doctor, nurse, cardiologist, lab technician)."
                            }
                        },
                        required = new[] { "fromDate" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
}

