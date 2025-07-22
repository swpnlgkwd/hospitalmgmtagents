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
        /// <summary>
        /// Generates smart, actionable suggestions for the scheduler dashboard.
        /// </summary>
        /// <param name="schedulerId">The ID of the logged-in scheduler (reserved for future use or audit).</param>
        /// <returns>A list of <see cref="SmartSuggestion"/> representing tips or alerts.</returns>
        public async Task<List<SmartSuggestion>> GetSmartSuggestionsAsync()
        {
            var suggestions = new List<SmartSuggestion>();

            // Suggestion 1: Uncovered Shifts
            var uncoveredShifts = await staffRepository.GetUncoveredShiftsAsync();
            if (uncoveredShifts.Count > 0)
            {
                var shiftLines = uncoveredShifts
                    .Take(3)
                    .Select(s => $"- {s.ShiftDate:dd MMM}, {s.DepartmentName} {s.ShiftTypeName} shift")
                    .ToList();

                var actionText = $"Auto-assign available staff to the following uncovered shifts this week:\n" +
                                 string.Join("\n", shiftLines);
                if (uncoveredShifts.Count > 3)
                    actionText += $"\n(and {uncoveredShifts.Count - 3} more)";

                suggestions.Add(new SmartSuggestion
                {
                    Message = $"📅 {uncoveredShifts.Count} shifts this week are still unassigned",
                    ActionPayload = "Show uncovered shifts this week",
                    Type = "UncoveredShift",
                    ActionLabel = "Auto Assign",
                    ActionIcon = "🪄",
                    ActionName = "autoAssignUncoveredShifts",
                    ActionData = new { shiftIds = uncoveredShifts.Select(s => s.PlannedShiftId).ToList() },
                    ActionText = actionText
                });
            }

            // Suggestion 2: Pending Leave Requests
            var pendingLeaves = await staffRepository.GetPendingLeaveRequestsAsync();
            if (pendingLeaves.Count > 0)
            {
                var leaveLines = pendingLeaves
                    .Take(3)
                    .Select(l => $"- {l.StaffId} ({l.LeaveStart:dd MMM} – {l.LeaveEnd:dd MMM})")
                    .ToList();

                var actionText = "Approve the following pending leave requests:\n" + string.Join("\n", leaveLines);
                if (pendingLeaves.Count > 3)
                    actionText += $"\n(and {pendingLeaves.Count - 3} more)";

                suggestions.Add(new SmartSuggestion
                {
                    Message = $"📤 {pendingLeaves.Count} leave requests are pending your approval",
                    ActionPayload = "Show me all pending leave requests",
                    Type = "PendingLeave",
                    ActionLabel = "Approve",
                    ActionIcon = "✅", 
                    ActionName = "approveLeaveRequests",
                    ActionData = new { leaveRequestIds = pendingLeaves.Select(l => l.LeaveRequestId).ToList() },
                    ActionText = actionText
                });
            }

            // Suggestion 3: Fatigued Staff (Back-to-Back Night Shifts)
            var fatiguedStaff = await staffRepository.GetFatiguedStaffAsync();
            if (fatiguedStaff.Count > 0)
            {
                var fatigueLines = fatiguedStaff
                    .Take(3)
                    .Select(f => $"- {f.AssignedStaffId} on {f.ShiftDate:dd MMM} in {f.DepartmentName}")
                    .ToList();

                var actionText = "Reassign shifts to relieve fatigue for the following staff:\n" + string.Join("\n", fatigueLines);
                if (fatiguedStaff.Count > 3)
                    actionText += $"\n(and {fatiguedStaff.Count - 3} more)";

                suggestions.Add(new SmartSuggestion
                {
                    Message = $"😴 {fatiguedStaff.Count} staff are working back-to-back night shifts",
                    ActionPayload = "Show me back-to-back night shifts",
                    Type = "FatigueWarning",
                    ActionLabel = "Rebalance",
                    ActionIcon = "🔁",
                    ActionName = "autoRebalanceFatiguedShifts",
                    ActionData = new { staffIds = fatiguedStaff.Select(s => s.AssignedStaffId).ToList() },
                    ActionText = actionText
                });
            }

            return suggestions;
        }


    }
}
