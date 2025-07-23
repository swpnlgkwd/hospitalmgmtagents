using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Models.SmartSuggestions;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalStaffMgmtApis.Business
{
    /// <summary>
    /// Provides actionable smart suggestions for the scheduler based on uncovered shifts,
    /// pending leave requests, and fatigue indicators.
    /// </summary>
    public class SmartSuggestionManager : ISmartSuggestionManager
    {
        private readonly IStaffRepository staffRepository;
        private readonly IShiftRepository shiftRepository;
        private readonly ILeaveRequestRepository leaveRequestRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerSuggestionService"/> class.
        /// </summary>
        /// <param name="staffRepository">The repository used to access staff and scheduling data.</param>
        public SmartSuggestionManager(IShiftRepository shiftRepository,
            IStaffRepository staffRepository,
            ILeaveRequestRepository leaveRequestRepository)
        {
            this.shiftRepository = shiftRepository;
            this.staffRepository = staffRepository;
            this.leaveRequestRepository = leaveRequestRepository;
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
            var uncoveredShifts = await shiftRepository.GetUncoveredShiftsAsync();
            if (uncoveredShifts.Count > 0)
            {
                var shiftLines = uncoveredShifts
                    .Select(s => $"- {s.ShiftDate:dd MMM yyyy} shift")
                    .ToList(); 

                var actionText = $"Auto-assign available staff to the following uncovered shifts this week:\n" +
                                 string.Join("\n", shiftLines);

                var actionPayload = "Show uncovered Shifts information This Week:\n" + string.Join("\n", shiftLines);
                suggestions.Add(new SmartSuggestion
                {
                    Message = $"📅 {uncoveredShifts.Count} shifts this week are still unassigned",
                    ActionPayload = actionPayload,
                    Type = "UncoveredShift",
                    ActionLabel = "Auto Assign",
                    ActionIcon = "🪄",
                    ActionName = "autoAssignUncoveredShifts",
                    ActionData = new { shiftIds = uncoveredShifts.Select(s => s.PlannedShiftId).ToList() },
                    ActionText = actionText
                });
            }

            // Suggestion 2: Pending Leave Requests
            var pendingLeaves = await leaveRequestRepository.FetchPendingLeaveRequestsAsync();
            if (pendingLeaves.Count > 0)
            {
                var leaveLines = pendingLeaves
                    .Select(l => $"- {l.StaffName}")
                    .ToList();

                var actionText = "Approve the following pending leave requests:\n" + string.Join("\n", leaveLines);
                var actionPayload = "Show pending Leave Requests:\n" + string.Join("\n", leaveLines);

                suggestions.Add(new SmartSuggestion
                {
                    Message = $"📤 {pendingLeaves.Count} leave requests are pending your approval",
                    ActionPayload = actionPayload,
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
                //var fatigueLines = fatiguedStaff
                //    .Select(f => $"- {f.AssignedStaffId} on {f.ShiftDate:dd MMM} in {f.DepartmentName} for {f.StaffName} {f.DepartmentName} {f.ShiftTypeName}")
                //    .ToList();

                var groupedFatigue = fatiguedStaff
                    .GroupBy(f => new { f.AssignedStaffId, f.StaffName, f.DepartmentName })
                    .Select(g =>
                    {
                        var shiftInfo = g
                            .OrderBy(s => s.ShiftDate)
                            .Select(s => $"{s.ShiftDate:dd MMM yyyy} ({s.ShiftTypeName})")
                            .ToList();

                        return $"- {g.Key.StaffName} ({g.Key.DepartmentName}): {string.Join(", ", shiftInfo)}";
                    })
                    .ToList();

                var actionText = "Reassign shifts to relieve fatigue for the following staff:\n" + string.Join("\n", groupedFatigue) ;
                var actionPayload = "Show shift information for Back-to-Back Night Shifts:\n" + string.Join("\n", groupedFatigue);
                //var actionPayload = "View Back to back night shift information" ;

                suggestions.Add(new SmartSuggestion
                {
                    Message = $"😴 {fatiguedStaff.Count} staff are working back-to-back night shifts ",
                    ActionPayload = actionPayload,
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
