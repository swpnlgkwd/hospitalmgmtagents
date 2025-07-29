using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Tools.HelperTools
{
    public static class ResolveRelativeDateTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "resolveRelativeDate",
          description: "Use this tool when the user refers to vague or relative date phrases instead of exact dates. " +
"Examples of such phrases include 'next', 'today', 'tomorrow', 'this week', 'next week', 'this weekend', " +
"'last week', 'last month', 'day after tomorrow', and similar natural language time references. " +
"Use this tool especially when the user asks questions like 'When is Emma working next?', 'Show upcoming ICU shifts', or 'Who is on leave tomorrow?'. " +
"Do NOT use this tool if the user has already provided an exact or formatted date like '2025-07-20', 'July 20', or 'Monday at 9 AM'. " +
"The output will be a resolved date or date range in 'yyyy-MM-dd' format based on the phrase.",

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
