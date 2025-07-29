using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Tools.Department
{
    public static class ResolveDepartmentInfoTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "resolveDepartmentInfo",
                description: "Finds the most relevant department given a partial or full name (e.g., 'ICU', 'Pediatrics') and returns its details including the unique department ID. Useful when a user mentions a department name in conversation.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new
                            {
                                type = "string",
                                description = "Full or partial department name (e.g., 'ICU', 'Cardio')"
                            }
                        },
                        required = new[] { "name" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }

}
