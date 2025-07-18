using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class FindStaffRequest
    {
        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; } = string.Empty;

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string? Role { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string? Department { get; set; }
    }
}
