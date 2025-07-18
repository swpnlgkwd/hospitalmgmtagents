using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class ShiftRecord
    {
        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; }
    }
}
