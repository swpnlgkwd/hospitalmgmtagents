using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class FindAvailableStaffTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "findAvailableStaff",
                description: "Finds staff members who are free and unassigned for a specific role, shift, and department on a given date." +
                " Use this when you need to fill a shift.Use this tool to check which staff members are available to work on a specific date, shift, " +
                "and optionally for a specific role or department. This is helpful when you need to assign, reassign, or find a replacement for a shift.",
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