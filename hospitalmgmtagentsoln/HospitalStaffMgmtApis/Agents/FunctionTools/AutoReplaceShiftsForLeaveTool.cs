using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class AutoReplaceShiftsForLeaveTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "autoReplaceShiftsForLeave",
                description: "Automatically finds and assigns replacement staff for all shifts impacted by a staff member's leave. Use this when a staff member applies for leave and you want to auto-assign replacements for their scheduled shifts.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId = new
                            {
                                type = "integer",
                                description = "The ID of the staff member who applied for leave"
                            },
                            fromDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Start date of the leave in yyyy-MM-dd format"
                            },
                            toDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "End date of the leave in yyyy-MM-dd format"
                            }
                        },
                        required = new[] { "staffId", "fromDate", "toDate" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }

}
