using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class StaffScheduleRequest
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }
    }
}
