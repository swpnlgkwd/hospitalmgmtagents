using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest
{
    public class ApplyForLeaveRequest
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("leaveStart")]
        public DateTime LeaveStart { get; set; }

        [JsonPropertyName("leaveEnd")]
        public DateTime LeaveEnd { get; set; }

        [JsonPropertyName("leaveType")]
        public string LeaveType { get; set; } = string.Empty;
    }

}
