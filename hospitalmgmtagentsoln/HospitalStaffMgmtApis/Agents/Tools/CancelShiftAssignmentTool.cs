using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class CancelShiftAssignmentTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "cancelShiftAssignment",
                description: "Removes a staff member from a specific shift if they are already assigned and become unavailable." +
                " Use this when a scheduled staff reports leave, absence, or requests to drop their shift.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId = new { type = "integer", description = "The ID of the staff whose shift is to be cancelled" },
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
