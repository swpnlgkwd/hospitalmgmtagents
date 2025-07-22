using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{

    namespace HospitalStaffMgmtApis.Agents.Tools
    {
        public static class UncoverShiftsTool
        {
            public static FunctionToolDefinition GetTool()
            {
                return new FunctionToolDefinition(
                    name: "uncoverShifts",
                    description: "Retrieves all uncovered (unassigned) shifts during a specific date or date range. Useful for identifying where staff need to be assigned. Supports filtering by department, role, or shift type. Helps with proactive shift planning or identifying scheduling gaps.",
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
                                    description = "Required. Start date for finding uncovered shifts (YYYY-MM-DD). Can be a relative phrase like 'today', 'this week', etc."
                                },
                                toDate = new
                                {
                                    type = "string",
                                    format = "date",
                                    description = "Optional. End date (YYYY-MM-DD). If not specified, only fromDate is considered."
                                },
                                departmentId = new
                                {
                                    type = "integer",
                                    description = "Optional. Filter uncovered shifts by department ID (e.g., ICU, Pediatrics)."
                                },
                                role = new
                                {
                                    type = "string",
                                    description = "Optional. Filter by role or specialization required for the shift (e.g., nurse, doctor, lab tech)."
                                },
                                shiftType = new
                                {
                                    type = "string",
                                    description = "Optional. Filter by shift type like Morning, Evening, Night."
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
}
