using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class GetShiftScheduleTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "getShiftSchedule",
                description: "Fetches the shift schedule for a specific staff member or department, filtered by optional date range. "
                           + "If staffId is provided, returns only their assigned shifts. If departmentId is provided, shows schedule for that department. "
                           + "Supports optional date filters for FromDate and ToDate.",
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
