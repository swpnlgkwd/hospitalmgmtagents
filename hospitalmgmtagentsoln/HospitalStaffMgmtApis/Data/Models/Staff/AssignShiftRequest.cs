using Azure.AI.Agents.Persistent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.Staff
{

    public class AssignShiftRequest
    {
        [JsonPropertyName("fromStaffId")]
        public int? FromStaffId { get; set; }

        [JsonPropertyName("toStaffId")]
        public int ToStaffId { get; set; }

        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; } = string.Empty;

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("shiftId")]
        public int? ShiftId { get; set; } = null;
    }


}
