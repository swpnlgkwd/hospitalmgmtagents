using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Tools
{

    public static class SubmitLeaveRequestTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "submitLeaveRequest",
                description: "Use this tool when a staff member says they want to take leave or particular staff member is on leave, will be absent, unavailable, or not working on specific dates. This submits a leave request with start and end dates, leave type (e.g., sick, vacation), and sets the status to Pending.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId = new { type = "integer", description = "The ID of the staff requesting leave" },
                            leaveStart = new { type = "string", description = "Leave start date in yyyy-MM-dd format" },
                            leaveEnd = new { type = "string", description = "Leave end date in yyyy-MM-dd format" },
                            leaveType = new { type = "string", description = "Type of leave (e.g., Sick, Vacation, Emergency)" }
                        },
                        required = new[] { "staffId", "leaveStart", "leaveEnd", "leaveType" }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    )
            );
        }
    }
}