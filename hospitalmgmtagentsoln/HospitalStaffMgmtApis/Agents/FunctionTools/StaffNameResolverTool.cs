using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class StaffNameResolverTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "staffNameResolverTool",
                description: "Resolves a partial or full staff name (e.g., 'Dr Amit') to a list of matching staff members with their unique IDs. Use this when you need to identify a staff member from a name.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string", description = "Full or partial name of the staff member (e.g., 'Amit', 'Dr Patel')" }
                        },
                        required = new[] { "name" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }


}
