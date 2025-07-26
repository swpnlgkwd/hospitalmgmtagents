using HospitalStaffMgmtApis.Data.Model.HospitalStaffMgmtApis.Models.Requests.HospitalStaffMgmtApis.Models.Requests;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Models.Shift;
using HospitalStaffMgmtApis.Data.Models.Staff;
using HospitalStaffMgmtApis.Models;
using HospitalStaffMgmtApis.Models.Requests;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    /// <summary>
    /// Interface defining the contract for all staff-related data operations including 
    /// scheduling, leave, shift replacement, and smart suggestions.
    /// </summary>
    public interface IStaffRepository
    {
        /// <summary>
        /// Resolves staff names matching the partial input provided.
        /// Used for auto-suggest/autocomplete functionality.
        /// </summary>
        /// <param name="namePart">Partial name string to search for.</param>
        /// <returns>List of staff members matching the input.</returns>
        Task<List<StaffIdResult>> ResolveStaffNameAsync(string namePart);


        /// <summary>
        /// Finds available staff members for a given shift type, department, and date.
        /// </summary>
        /// <param name="findAvailableStaffRequest">Search parameters for availability.</param>
        /// <returns>List of available and eligible staff for assignment.</returns>
        Task<List<AvailableStaffResponse>> FindAvailableStaffAsync(AvailableStaffRequest findAvailableStaffRequest);




        /// <summary>
        /// Retrieves all fatique shifts (shifts that are not assigned to any staff).
        /// </summary>
        /// <returns>List of planned shifts.</returns>
        Task<List<PlannedShift>> GetFatiguedStaffAsync(FatiqueStaffRequest? fatiqueStaffRequest = null);

        Task<ShiftScheduleResponse> AssignStaffToShift(AssignShiftRequest request);
    }
}
