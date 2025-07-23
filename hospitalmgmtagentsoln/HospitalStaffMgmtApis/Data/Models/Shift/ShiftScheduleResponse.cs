using Azure.AI.Agents.Persistent;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HospitalStaffMgmtApis.Data.Models.Shift
{
    public class ShiftScheduleResponse
    {
        [JsonPropertyName("staffId")]
        public int? StaffId { get; set; } 

        [JsonPropertyName("departmentId")]
        public int? DepartmentId { get; set; } 

        [JsonPropertyName("staffName")]
        public string? StaffName { get; set; } = string.Empty;

        [JsonPropertyName("departmentName")] 
        public string? DepartmentName { get; set; } = string.Empty;

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("shiftDate")]
        public DateTime ShiftDate { get; set; }

        [JsonPropertyName("shiftStatus")]
        public string ShiftStatus { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("roleId")]
        public int? RoleId { get; set; }

    }
}
