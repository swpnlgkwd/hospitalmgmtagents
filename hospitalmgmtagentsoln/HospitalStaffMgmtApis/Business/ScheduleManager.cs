using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Models.Shift;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HospitalStaffMgmtApis.Business
{
    /// <summary>
    /// Business layer responsible for managing shift schedule operations.
    /// </summary>
    public class ScheduleManager : IScheduleManager
    {
        private readonly IShiftRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleManager"/> class.
        /// </summary>
        /// <param name="staffRepository">Repository used to access staff scheduling data.</param>
        public ScheduleManager(IShiftRepository staffRepository, 
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = staffRepository;
        }

        /// <summary>
        /// Retrieves the shift schedules between the specified start and end dates.
        /// </summary>
        /// <param name="startDate">The start date of the date range (inclusive).</param>
        /// <param name="endDate">The end date of the date range (inclusive).</param>
        /// <returns>A list of shift schedules within the given date range.</returns>
        public async Task<List<ShiftScheduleResponse>> FetchShiftInformation(DateOnly startDate, DateOnly endDate)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var role = user?.FindFirst(ClaimTypes.Role)?.Value;

            var staffId = Convert.ToInt32( user?.FindFirst(ClaimTypes.NameIdentifier).Value );       
         
            if (role.Trim() == "Scheduler")
            {
                // Show all shifts
                return await _repository.GetShiftScheduleBetweenDatesAsync(startDate, endDate);
            }
            else
            {
                // Show only this user's shifts
                return await _repository.FetchShiftInformationByStaffId(staffId, startDate, endDate);
            }
        }

    }
}
