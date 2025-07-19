using Azure.AI.Agents.Persistent;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HospitalStaffMgmtApis.Data.Model
{
    public class ShiftScheduleResponse
    {
        [JsonPropertyName("staffName")]
        public string StaffName { get; set; } = string.Empty;

        [JsonPropertyName("departmentName")] 
        public string DepartmentName { get; set; } = string.Empty;

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("shiftDate")]
        public DateTime ShiftDate { get; set; }

        [JsonPropertyName("shiftStatus")]
        public string ShiftStatus { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

    }
}
