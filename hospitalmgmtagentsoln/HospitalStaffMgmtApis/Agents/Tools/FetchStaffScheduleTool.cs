using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class FetchStaffScheduleTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "fetchStaffSchedule",
                description: "Retrieves the personal shift schedule of a specific staff member, showing only their assigned shifts," +
                " dates, and shift types..",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId = new { type = "integer", description = "The ID of the staff whose schedule is to be fetched" }
                        },
                        required = new[] { "staffId" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    )
            );
        }
    }
}
