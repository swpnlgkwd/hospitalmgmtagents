using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dtos.Staff;
using HospitalSchedulingApp.Dtos.Staff.Requests;

namespace HospitalSchedulingApp.Services.Interfaces
{
    /// <summary>
    /// Defines operations related to staff management and availability in the scheduling system.
    /// </summary>
    public interface IStaffService
    {
        /// <summary>
        /// Retrieves a list of active staff members whose names contain the specified pattern.
        /// Useful for search and autocomplete functionality.
        /// </summary>
        /// <param name="namePart">Partial or full name to search for.</param>
        /// <returns>A list of matching active staff members as DTOs.</returns>
        Task<List<StaffDto?>> FetchActiveStaffByNamePatternAsync(string namePart);

        /// <summary>
        /// Searches for staff who are available for a given shift date range, shift type,
        /// and optional department and role filters.
        /// </summary>
        /// <param name="availableStaffFilter">Filter criteria for availability search.</param>
        /// <returns>A list of available staff matching the provided criteria.</returns>
        Task<List<StaffDto?>> SearchAvailableStaffAsync(AvailableStaffFilterDto filter);
    }
}
