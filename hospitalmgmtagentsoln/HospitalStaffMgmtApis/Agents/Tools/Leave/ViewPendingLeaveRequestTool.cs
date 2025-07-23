using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools.Leave
{
    /// <summary>
    /// Tool definition for viewing pending leave requests.
    /// Supports optional filtering by date range, staff name, and department name.
    /// This helps schedulers quickly review leave requests that are awaiting approval.
    /// </summary>
    public static class ViewPendingLeaveRequestsTool
    {
        /// <summary>
        /// Gets the tool definition for use in the AI agent.
        /// Allows retrieval of pending leave requests with optional filters.
        /// </summary>
        /// <returns>Returns a configured <see cref="FunctionToolDefinition"/> for use in agent orchestration.</returns>
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "viewPendingLeaveRequests",
                description: "Retrieves pending leave requests optionally filtered by date range, staff name, or department name. Useful for reviewing approval status or managing schedule impact.",
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
                                description = "Optional. Start date for leave requests in YYYY-MM-DD format. Can also accept relative phrases like 'this week', 'today'."
                            },
                            toDate = new
                            {
                                type = "string",
                                format = "date",
                                description = "Optional. End date for leave requests in YYYY-MM-DD format. Used with fromDate to define a range."
                            },
                            staffName = new
                            {
                                type = "string",
                                description = "Optional. Full or partial name of the staff member to filter leave requests."
                            },
                            departmentName = new
                            {
                                type = "string",
                                description = "Optional. Department name to filter leave requests (e.g., ICU, Pediatrics, General Medicine)."
                            }
                        },
                        required = new string[] { }
                    },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                )
            );
        }
    }
}
