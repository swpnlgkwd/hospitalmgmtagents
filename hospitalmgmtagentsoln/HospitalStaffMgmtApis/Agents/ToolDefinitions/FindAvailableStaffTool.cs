using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.ToolDefinitions
{
    public static class FindAvailableStaffTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
            name: "findAvailableStaff",
                description: "Finds available hospital staff for a given shift, role, and department.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        Type = "object",
                        Properties = new
                        {
                            shiftDate = new { Type = "string", Description = "Shift date (yyyy-MM-dd)" },
                            shiftType = new { Type = "string", Description = "Shift type (e.g., Day, Night)" },
                            role = new { Type = "string", Description = "Staff role (optional)" },
                            department = new { Type = "string", Description = "Department (optional)" }
                        },
                        Required = new[] { "shiftDate", "shiftType" }
                    },
                    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
}