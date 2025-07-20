using HospitalStaffMgmtApis.Data.Model;

namespace HospitalStaffMgmtApis.Business
{
    /// <summary>
    /// Interface for managing schedule-related business logic.
    /// </summary>
    public interface IScheduleManager
    {
        /// <summary>
        /// Retrieves shift schedule data between the specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range (inclusive).</param>
        /// <param name="endDate">The end date of the range (inclusive).</param>
        /// <returns>A list of shift schedules within the given range.</returns>
        Task<List<ShiftScheduleResponse>> FetchShiftInformation(DateOnly startDate, DateOnly endDate);
    }
}
