using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest
{
    public class LeaveActionRequest
    {
        [JsonPropertyName("leaveRequestId")]
        public int? LeaveRequestId { get; set; }  // Optional direct ID

        [JsonPropertyName("staffId")]
        public int? StaffId { get; set; }  // Optional fallback

        [JsonPropertyName("staffName")]
        public string? StaffName { get; set; }  // Used if ID is missing

        [JsonPropertyName("leaveStartDate")]
        public string? LeaveStartDate { get; set; }  // Format: "yyyy-MM-dd"

        [JsonPropertyName("leaveEndDate")]
        public string? LeaveEndDate { get; set; }  // Format: "yyyy-MM-dd"

        [JsonPropertyName("approvalStatus")]
        public string ApprovalStatus { get; set; } = "";  // "Approved" or "Rejected"
    }

}

