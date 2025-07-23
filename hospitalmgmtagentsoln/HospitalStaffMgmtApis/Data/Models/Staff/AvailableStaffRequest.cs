namespace HospitalStaffMgmtApis.Data.Model
{
    using System;
    using System.Collections.Generic;

    namespace HospitalStaffMgmtApis.Models.Requests
    {
        using System;
        using System.Text.Json.Serialization;

        namespace HospitalStaffMgmtApis.Models.Requests
        {
            /// <summary>
            /// Request model for finding available staff for a potential or existing shift.
            /// </summary>
            public class AvailableStaffRequest
            {
                /// <summary>
                /// Optional. The ID of the department (e.g., ICU, Emergency) to search within.
                /// </summary>
                [JsonPropertyName("departmentId")]
                public int? DepartmentId { get; set; }

                /// <summary>
                /// Optional. The ID of the role (e.g., Nurse, Doctor, Lab Technician) to filter available staff by.
                /// </summary>
                [JsonPropertyName("roleId")]
                public int? RoleId { get; set; }

                /// <summary>
                /// Optional. The date to find availability for. Used if you're checking a single day.
                /// </summary>
                [JsonPropertyName("date")]
                public DateTime? Date { get; set; }

                /// <summary>
                /// Optional. Start of date range for checking availability.
                /// If set, system assumes a date range search.
                /// </summary>
                [JsonPropertyName("fromDate")]
                public DateTime? FromDate { get; set; }

                /// <summary>
                /// Optional. End of date range for checking availability.
                /// </summary>
                [JsonPropertyName("toDate")]
                public DateTime? ToDate { get; set; }

                /// <summary>
                /// Optional. The name of the shift type, such as "Morning", "Evening", or "Night".
                /// </summary>
                [JsonPropertyName("shiftType")]
                public string? ShiftType { get; set; }

                /// <summary>
                /// Optional. Whether to include fatigue check (avoid back-to-back shifts). Default: true.
                /// </summary>
                [JsonPropertyName("includeFatigueCheck")]
                public bool IncludeFatigueCheck { get; set; } = true;
            }
        }

    }

}
