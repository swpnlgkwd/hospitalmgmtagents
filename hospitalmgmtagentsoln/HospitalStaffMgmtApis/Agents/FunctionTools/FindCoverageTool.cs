using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class FindCoverageTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "findCoverage",
                description: "Use this tool to if we have enough staff members available on a specific date on range of date.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
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
                        required = new[] { "fromDate", "toDate" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
  
}
