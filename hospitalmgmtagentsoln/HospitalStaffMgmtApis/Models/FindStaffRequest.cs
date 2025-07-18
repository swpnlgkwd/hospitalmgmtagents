using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Models
{
    public class FindStaffRequest
    {
        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }
    }
}
