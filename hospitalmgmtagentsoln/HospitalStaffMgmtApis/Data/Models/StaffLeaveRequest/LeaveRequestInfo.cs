using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest
{
    public class LeaveRequestInfo
    {
        [JsonPropertyName("leaveRequestId")]
        public int LeaveRequestId { get; set; }

        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("leaveStart")]
        public DateTime LeaveStart { get; set; }

        [JsonPropertyName("leaveEnd")]
        public DateTime LeaveEnd { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending"; // e.g. Pending, Approved, Rejected

    }
}
