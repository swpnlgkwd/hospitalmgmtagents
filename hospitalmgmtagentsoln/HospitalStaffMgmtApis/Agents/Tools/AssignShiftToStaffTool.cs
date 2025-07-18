using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class AssignShiftToStaffTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "assignShiftToStaff",
                description: "Schedules a specific staff member for a work shift on a given date and shift type. " +
                "Use this to add a new shift assignment for someone who is available and not already scheduled.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId = new { type = "integer", description = "The ID of the staff to assign" },
                            shiftDate = new { type = "string", description = "The date of the shift in yyyy-MM-dd format" },
                            shiftType = new { type = "string", description = "The type of the shift (e.g., Morning, Evening, Night)" }
                        },
                        required = new[] { "staffId", "shiftDate", "shiftType" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    )
            );
        }
    }
}
