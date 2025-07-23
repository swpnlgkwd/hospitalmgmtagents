using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools.Shift
{
    public static class BackToBackWeeklyShiftTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "backToBackWeeklyShiftTool",
                description: "Fetches a list of staff members who are assigned back-to-back shifts within the same week. "
                           + "Supports optional filters such as date range and department. Useful for identifying fatigue or over-scheduling issues.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            startDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. Start of the week for evaluating back-to-back shifts (format: YYYY-MM-DD)."
                            },
                            endDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. End of the week for evaluating back-to-back shifts (format: YYYY-MM-DD)."
                            },
                            departmentName = new
                            {
                                type = "string",
                                description = "Optional. The name of the department to filter staff by."
                            }
                        }
                        // No required fields, so no "required" array
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
}
