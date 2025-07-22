using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalStaffMgmtApis.Business
{
    /// <summary>
    /// Provides actionable smart suggestions for the scheduler based on uncovered shifts,
    /// pending leave requests, and fatigue indicators.
    /// </summary>
    public class SchedulerSuggestionService : ISchedulerSuggestionService
    {
        private readonly IStaffRepository staffRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerSuggestionService"/> class.
        /// </summary>
        /// <param name="staffRepository">The repository used to access staff and scheduling data.</param>
        public SchedulerSuggestionService(IStaffRepository staffRepository)
        {
            this.staffRepository = staffRepository;
        }

        /// <summary>
        /// Generates smart, actionable suggestions for the scheduler dashboard.
        /// </summary>
        /// <param name="schedulerId">The ID of the logged-in scheduler (reserved for future use or audit).</param>
        /// <returns>A list of <see cref="SmartSuggestion"/> representing tips or alerts.</returns>
        public async Task<List<SmartSuggestion>> GetSmartSuggestionsAsync()
        {
            var suggestions = new List<SmartSuggestion>();

            // Suggestion: Uncovered shifts
            var uncoveredShifts = await staffRepository.GetUncoveredShiftsAsync();
            if (uncoveredShifts.Count > 0)
            {
                suggestions.Add(new SmartSuggestion
                {
                    //Message = $"You have {uncoveredShifts.Count} uncovered shifts this week.",
                    Message = $"📅 {uncoveredShifts.Count} shifts this week are still unassigned",
                    ActionPayload = "show uncovered shifts this week",
                    Type = "UncoveredShift"
                });
            }

            // Suggestion: Pending leave requests
            var pendingLeaves = await staffRepository.GetPendingLeaveRequestsAsync();
            if (pendingLeaves.Count > 0)
            {
                suggestions.Add(new SmartSuggestion
                {
                    //Message = $"You have {pendingLeaves.Count} pending leave requests.",
                    Message = $"📤 {pendingLeaves.Count} leave requests are pending your approval",
                    ActionPayload = "Show me all pending leave requests",
                    Type = "PendingLeave"
                });
            }

            // Suggestion: Fatigued staff with back-to-back night shifts
            var fatiguedStaff = await staffRepository.GetFatiguedStaffAsync();
            if (fatiguedStaff.Count > 0)
            {
                suggestions.Add(new SmartSuggestion
                {
                   // Message = $"You have {fatiguedStaff.Count} staff with back-to-back night shifts.",
                    Message = $"😴 {fatiguedStaff.Count} staff are working back-to-back night shifts",
                    ActionPayload = $"Show me employee working back to back night shifts",
                    Type = "FatigueWarning"
                });
            }

            return suggestions;
        }
    }
}
