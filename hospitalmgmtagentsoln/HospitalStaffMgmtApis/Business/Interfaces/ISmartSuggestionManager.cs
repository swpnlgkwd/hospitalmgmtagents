using HospitalStaffMgmtApis.Data.Models.SmartSuggestions;

namespace HospitalStaffMgmtApis.Business.Interfaces
{
    /// <summary>
    /// Defines the contract for generating smart shift suggestions 
    /// for the scheduler based on uncovered shifts, staff availability, and workload balancing.
    /// </summary>
    public interface ISmartSuggestionManager
    {
        /// <summary>
        /// Retrieves a list of smart scheduling suggestions to help 
        /// fill uncovered shifts with the most suitable available staff.
        /// </summary>
        /// <returns>List of smart suggestions including staff and shift matching scores.</returns>
        Task<List<SmartSuggestion>> GetSmartSuggestionsAsync();
    }
}
