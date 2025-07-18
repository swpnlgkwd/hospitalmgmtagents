using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class ShiftSwapTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "shiftSwapAsync",
                description: "Use this tool when a staff member wants to swap or give away their shift to another. It reassigns a shift from one staff member to another for a specific date and shift type, " +
                "ensuring eligibility and no conflicts.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            shiftDate = new { type = "string", description = "The date of the shift in yyyy-MM-dd format" },
                            shiftType = new { type = "string", description = "The type of the shift (e.g., Morning, Evening, Night)" },
                            originalStaffId = new { type = "integer", description = "The staff ID of the original assigned staff" },
                            replacementStaffId = new { type = "integer", description = "The staff ID of the replacement staff" }
                        },
                        required = new[] { "shiftDate", "shiftType", "originalStaffId", "replacementStaffId" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    )
            );
        }
    }
}