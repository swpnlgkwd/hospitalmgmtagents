using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Tools.Shift
{
    public static class FilterShiftScheduleTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "filterShiftSchedule",
                description: "Retrieves planned shift schedules using optional filters like department name, staff name, shift type, shift status, and date range. Useful for querying when, where, and what type of shifts are scheduled for specific individuals or departments.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            departmentName = new
                            {
                                type = "string",
                                description = "Optional. The name of the department to filter shift schedule (e.g., ICU, OPD, Pediatrics)."
                            },
                            staffName = new
                            {
                                type = "string",
                                description = "Optional. The full or partial name of the staff member to search shifts for (e.g., Asha Patil)."
                            },
                            shiftTypeName = new
                            {
                                type = "string",
                                description = "Optional. The name of the shift type, such as 'Morning', 'Evening', or 'Night'."
                            },
                            shiftStatusName = new
                            {
                                type = "string",
                                description = "Optional. The shift status like 'Scheduled', 'Assigned', or 'Vacant'."
                            },
                            fromDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. Start of the date range (inclusive), in YYYY-MM-DD format."
                            },
                            toDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. End of the date range (inclusive), in YYYY-MM-DD format."
                            }
                        }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }

}
