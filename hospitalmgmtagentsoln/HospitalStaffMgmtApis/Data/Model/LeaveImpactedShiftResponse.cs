using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{ 
    public class LeaveImpactedShiftResponse
    {
        [JsonPropertyName("shiftId")]
        public int ShiftId { get; set; }

        [JsonPropertyName("shiftDate")]
        public DateTime ShiftDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = "";

        [JsonPropertyName("department")]
        public string Department { get; set; } = "";

        [JsonPropertyName("role")]
        public string Role { get; set; } = "";
    }

}
