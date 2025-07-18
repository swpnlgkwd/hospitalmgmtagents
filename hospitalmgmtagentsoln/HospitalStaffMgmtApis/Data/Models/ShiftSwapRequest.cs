using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class ShiftSwapRequest
    {
        [JsonPropertyName("originalStaffId")]
        public int OriginalStaffId { get; set; }

        [JsonPropertyName("replacementStaffId")]
        public int ReplacementStaffId { get; set; }

        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; } = string.Empty;

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;
    }
}
