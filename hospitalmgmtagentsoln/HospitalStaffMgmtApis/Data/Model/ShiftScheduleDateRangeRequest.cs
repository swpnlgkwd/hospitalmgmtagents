namespace HospitalStaffMgmtApis.Data.Model
{
    /// <summary>
    /// Request model to fetch shift schedules between a date range.
    /// </summary>
    public class ShiftScheduleDateRangeRequest
    {
        /// <summary>
        /// The start date of the range (inclusive).
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// The end date of the range (inclusive).
        /// </summary>
        public DateOnly EndDate { get; set; }
    }
}

