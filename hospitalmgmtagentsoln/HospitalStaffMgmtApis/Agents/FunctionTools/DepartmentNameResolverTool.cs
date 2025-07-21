using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtAgent.Tools
{
    /// <summary>
    /// Tool definition for resolving department name (full or partial) to department ID.
    /// Useful when the user mentions a department like 'ICU' or 'Pediatrics' in the prompt.
    /// </summary>
    public static class DepartmentNameResolverTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "departmentNameResolverTool",
                description: "Resolves a partial or full department name (e.g., 'ICU', 'Pediatrics') to a matching department and its unique ID. Use this when the user mentions a department name in their request.",
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
