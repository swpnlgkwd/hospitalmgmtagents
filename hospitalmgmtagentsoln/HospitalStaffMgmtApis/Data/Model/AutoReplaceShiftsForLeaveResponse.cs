using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace HospitalStaffMgmtApis.Data.Model
{
    public class AutoReplaceShiftsForLeaveResponse
    {
        [JsonPropertyName("assignedShifts")]
        public List<AutoReplaceShiftsForLeaveResult> AssignedShifts { get; set; } = new();

        [JsonPropertyName("unassignedShifts")]
        public List<AutoReplaceShiftsForLeaveResult> UnassignedShifts { get; set; } = new();
    }


    public class ReassignedShiftInfo
    {
        [JsonPropertyName("shiftDate")]
        public DateOnly ShiftDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; } = string.Empty;
    }

    public class UnassignedShiftInfo
    {
        [JsonPropertyName("shiftDate")]
        public DateOnly ShiftDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }

}
