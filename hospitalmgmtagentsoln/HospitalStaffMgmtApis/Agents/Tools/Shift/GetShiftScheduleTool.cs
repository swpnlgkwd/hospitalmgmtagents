using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools.Shift
{
    public static class GetShiftScheduleTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "getShiftSchedule",
                description: "Fetches the shift schedule for a specific staff member or department, optionally filtered by date range and shift type. "
                           + "If 'staffId' is provided, returns only that staff member's assigned shifts. "
                           + "If 'departmentId' is provided, shows the schedule for that department. "
                           + "If 'shiftType' is provided, filters the results to that specific shift type (e.g., Morning, Evening, Night). "
                           + "Supports optional 'fromDate' and 'toDate' for custom date range filtering.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId = new
                            {
                                type = "integer",
                                description = "Optional. The ID of the staff member to fetch shifts for."
                            },
                            departmentId = new
                            {
                                type = "integer",
                                description = "Optional. The ID of the department to fetch shift schedule for."
                            },
                            fromDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. Start date for the shift schedule in YYYY-MM-DD format."
                            },
                            toDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. End date for the shift schedule in YYYY-MM-DD format."
                            },
                            shiftType = new
                            {
                                type = "string",
                                description = "Optional. The name of the shift type to filter by, such as 'Morning', 'Evening', or 'Night'."
                            }

                        }
                        // All fields optional, so no "required" array needed
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
}

//"shiftType": { "type": "string", "description": "Shift type such as Morning, Evening, or Night" },
