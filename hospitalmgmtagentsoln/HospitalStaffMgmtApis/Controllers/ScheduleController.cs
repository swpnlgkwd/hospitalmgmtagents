
using HospitalStaffMgmtApis.Business;
using HospitalStaffMgmtApis.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    /// <summary>
    /// API controller responsible for providing shift schedule data based on date range.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleManager _scheduleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleController"/> class.
        /// </summary>
        /// <param name="scheduleManager">The schedule manager used to fetch shift schedules.</param>
        public ScheduleController(IScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager;
        }

        /// <summary>
        /// Fetches the shift schedules for all staff between the given date range.
        /// </summary>
        /// <param name="startDate">The start date of the schedule window (inclusive).</param>
        /// <param name="endDate">The end date of the schedule window (inclusive).</param>
        /// <returns>A list of shift schedule responses.</returns>
        [HttpPost("fetch")]
        public async Task<List<ShiftScheduleResponse>> FetchShiftInformation()
        {
            // Hardcoded date range: 1st July 2025 to 31st July 2025
            var startDate = new DateOnly(2025, 7, 1);
            var endDate = new DateOnly(2025, 7, 31);

            return await _scheduleManager.FetchShiftInformation(startDate, endDate);
        }

    }
}
