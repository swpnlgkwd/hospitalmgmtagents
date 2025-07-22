using HospitalStaffMgmtApis.Data.Models;

namespace HospitalStaffMgmtApis.Business.Interfaces
{
    /// <summary>
    /// Defines the contract for generating smart shift suggestions 
    /// for the scheduler based on uncovered shifts, staff availability, and workload balancing.
    /// </summary>
    public interface ISchedulerSuggestionService
    {
        /// <summary>
        /// Retrieves a list of smart scheduling suggestions to help 
        /// fill uncovered shifts with the most suitable available staff.
        /// </summary>
        /// <returns>List of smart suggestions including staff and shift matching scores.</returns>
        Task<List<SmartSuggestion>> GetSmartSuggestionsAsync();
    }
}
