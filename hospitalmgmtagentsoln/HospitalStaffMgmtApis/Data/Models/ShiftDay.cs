using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class ShiftDay
    {
        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; } = string.Empty;  // e.g., "2025-07-20"

        [JsonPropertyName("staff")]
        public List<StaffShift> Staff { get; set; } = new List<StaffShift>();
    }
}
