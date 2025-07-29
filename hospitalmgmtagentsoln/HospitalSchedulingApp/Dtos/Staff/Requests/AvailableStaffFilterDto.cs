using System;

namespace HospitalSchedulingApp.Dtos.Staff.Requests
{
    /// <summary>
    /// Represents filtering criteria for searching available staff 
    /// for a specific date range, shift type, and optional department and role.
    /// </summary>
    public class AvailableStaffFilterDto
    {
        /// <summary>
        /// The start date (inclusive) of the shift range to search availability for.
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// The end date (inclusive) of the shift range to search availability for.
        /// </summary>
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// The type of shift to filter by (e.g., "Morning", "Evening", "Night").
        /// </summary>
        public string? ShiftType { get; set; } =  string.Empty;

        /// <summary>
        /// Optional department name to prioritize staff from a specific department.
        /// </summary>
        public string? Department { get; set; }

    }
}
