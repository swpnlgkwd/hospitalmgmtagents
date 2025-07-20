using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Repository;

namespace HospitalStaffMgmtApis.Business
{
    /// <summary>
    /// Business layer responsible for managing shift schedule operations.
    /// </summary>
    public class ScheduleManager : IScheduleManager
    {
        private readonly IStaffRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleManager"/> class.
        /// </summary>
        /// <param name="staffRepository">Repository used to access staff scheduling data.</param>
        public ScheduleManager(IStaffRepository staffRepository)
        {
            _repository = staffRepository;
        }

        /// <summary>
        /// Retrieves the shift schedules between the specified start and end dates.
        /// </summary>
        /// <param name="startDate">The start date of the date range (inclusive).</param>
        /// <param name="endDate">The end date of the date range (inclusive).</param>
        /// <returns>A list of shift schedules within the given date range.</returns>
        public Task<List<ShiftScheduleResponse>> FetchShiftInformation(DateOnly startDate, DateOnly endDate)
        {
            return _repository.GetShiftScheduleBetweenDatesAsync(startDate, endDate);
        }
    }
}
