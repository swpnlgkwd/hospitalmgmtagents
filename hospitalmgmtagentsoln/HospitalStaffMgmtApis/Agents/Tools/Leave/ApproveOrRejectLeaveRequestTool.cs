
using System.Text.Json;
using Azure.AI.Agents.Persistent;


namespace HospitalStaffMgmtApis.Agents.Tools.Leave
{

    namespace HospitalStaffMgmtApis.Agents.Tools.Leave
    {
        /// <summary>
        /// Tool for approving or rejecting a leave request.
        /// If leaveRequestId is provided, it will be used directly.
        /// Otherwise, staffId or staffName along with leaveStartDate and leaveEndDate must be provided to identify the leave request.
        /// </summary>
        public static class ApproveOrRejectLeaveRequestTool
        {
            public static FunctionToolDefinition GetTool()
            {
                return new FunctionToolDefinition(
                     name: "approveOrRejectLeaveRequest",
                     description: "Approve or reject a leave request. If approved, this will also return a list of impacted shifts requiring replacement. Provide leaveRequestId directly, or identify the request using staffId or staffName along with leave date range.",
                     parameters: BinaryData.FromObjectAsJson(
                         new
                         {
                             type = "object",
                             properties = new
                             {
                                 leaveRequestId = new
                                 {
                                     type = "integer",
                                     description = "Optional. The ID of the leave request to approve or reject. If not provided, use staffId or staffName with leaveStartDate and leaveEndDate."
                                 },
                                 staffId = new
                                 {
                                     type = "integer",
                                     description = "Optional. Staff ID of the person whose leave is being approved/rejected. Required if leaveRequestId is not provided."
                                 },
                                 staffName = new
                                 {
                                     type = "string",
                                     description = "Optional. Name of the staff member (partial or full). Required if leaveRequestId and staffId are not provided."
                                 },
                                 leaveStartDate = new
                                 {
                                     type = "string",
                                     format = "date",
                                     description = "Optional. Leave start date (yyyy-MM-dd). Required if leaveRequestId is not provided."
                                 },
                                 leaveEndDate = new
                                 {
                                     type = "string",
                                     format = "date",
                                     description = "Optional. Leave end date (yyyy-MM-dd). Required if leaveRequestId is not provided."
                                 },
                                 approvalStatus = new
                                 {
                                     type = "string",
                                     @enum = new[] { "Approved", "Rejected" },
                                     description = "Required. New status of the leave request — either 'Approved' or 'Rejected'."
                                 }
                             },
                             required = new[] { "approvalStatus" }
                         },
                         new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                     )
                 );
            }
        }
    }
}

