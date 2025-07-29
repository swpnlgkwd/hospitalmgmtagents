using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Tools.Staff
{
    public static class ResolveStaffInfoByNameTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "resolveStaffInfoByName",
                description: "Finds one or more active staff members based on a full or partial name (e.g., 'Asha', 'Patil'). "
                           + "Returns matching staff details such as staff ID, name, role, and department.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new
                            {
                                type = "string",
                                description = "Full or partial staff name (e.g., 'Asha', 'Patil', 'Sunita')"
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
