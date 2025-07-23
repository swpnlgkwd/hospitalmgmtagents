using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools.Helpers
{
    /// <summary>
    /// Provides tool definition for resolving relative date phrases into exact yyyy-MM-dd format.
    /// </summary>
    public static class ResolveRelativeDateTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "resolveRelativeDate",
                description: "Only use this tool when the user mentions vague or relative date phrases like" +
                " 'today', 'tomorrow', 'next week', 'this weekend', 'day after tomorrow','day before yesterday'," +
                "'last week', 'last month', 'this week' etc. " +
                "Do NOT use this tool for absolute or formatted dates (e.g., '2025-07-20', '20th July', or 'next Monday at 5 PM'). " +
                "The result will be a resolved date or date range in yyyy-MM-dd format.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            phrase = new
                            {
                                type = "string",
                                description = "A relative or natural language date phrase like 'today', 'tomorrow', 'next week', 'last weekend', etc. Avoid calling this tool if an exact or formatted date is already available."
                            }
                        },
                        required = new[] { "phrase" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
}
