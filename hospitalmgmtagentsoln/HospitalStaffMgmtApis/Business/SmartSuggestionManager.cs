using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Models.Agent;
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

                var actionText = "Reassign shifts to relieve fatigue for the following staff:\n" + string.Join("\n", groupedFatigue);
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

        public async Task<string> GetAgentInsights()
        {
            var insights = new List<string>();
            var today = DateTime.Today.Date;
            insights.Add($"👋 Hello! Here's your daily summary for today {today}:\n");

            //if (role == "Scheduler")
            //{
            var uncoveredShifts = await shiftRepository.GetUncoveredShiftsForTodayAsync();
            if (uncoveredShifts.Any())
                insights.Add($"📅 Uncovered Shifts: {uncoveredShifts.Count} shift is currently unassigned.");

            //var fatigued = await staffRepository.GetFatiguedStaffAsync();
            //if (fatigued.Any())
            //    insights.Add($"{fatigued.Count} staff members may be at fatigue risk due to back-to-back shifts.");

            //var pendingLeaves = await leaveRequestRepository.FetchPendingLeaveRequestsAsync();
            //if (pendingLeaves.Any())
            //    insights.Add($"You have {pendingLeaves.Count} leave requests pending approval.");
            // }
            //else if (role == "Employee")
            //{
            //    var isOnLeave = await _repository.IsStaffOnLeaveToday(userId);
            //    if (isOnLeave)
            //        insights.Add("You are currently on leave today. Rest well!");

            //    var upcomingShifts = await _repository.GetShiftsForStaffAsync(userId, DateTime.Today, DateTime.Today.AddDays(2));
            //    if (upcomingShifts.Any())
            //        insights.Add($"You have {upcomingShifts.Count} upcoming shift(s) in the next 2 days.");
            //}

            if (insights.Count == 0)
                return "Hello! 👋 Everything looks good at the moment. No urgent issues to address.";

            return $" \n- {string.Join("\n- ", insights)}\nWould you like help with any of these?";
        }


        public async Task<AgentSummaryResponse> GetDailySchedulerSummaryAsync()
        {
            var uncoveredCount = await shiftRepository.GetUncoveredShiftsForTodayAsync();
            var pendingLeaves = await leaveRequestRepository.FetchPendingLeaveRequestsAsync();

            var parts = new List<string>();

            if (uncoveredCount.Count > 0)
                parts.Add($"• 🕒 {uncoveredCount.Count} shift{(uncoveredCount.Count > 1 ? "s" : "")} remain unassigned and may impact coverage.");

            if (pendingLeaves.Count > 0)
                parts.Add($"• 📥 {pendingLeaves.Count} leave request{(pendingLeaves.Count > 1 ? "s" : "")} are pending your approval.");

            string message = "";


            

            if (parts.Count == 0)
            {
                message = "✅ All set! There are no uncovered shifts or pending leave requests for today. Keep up the great work!";
            }
            else
            {
                message = "👋 Good morning! Here's a quick summary of today's staffing situation:\n\n"
                        + string.Join("\n", parts)
                        + "\n\n👉 Would you like to review one of these now?";
            }

            var quickReplies = new List<QuickReply>();
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            if (uncoveredCount.Count > 0)
                quickReplies.Add(new QuickReply { Label = "📅 Review Coverage", Value = $"show uncovered shifts for {today}" });

            if (pendingLeaves.Count > 0)
                quickReplies.Add(new QuickReply { Label = "✅ View Pending Leaves", Value = "show/view pending leave requests" });

            return new AgentSummaryResponse
            {
                SummaryMessage = message,
                QuickReplies = quickReplies
            };
        }
    }
}
