namespace HospitalStaffMgmtApis.Data.Models.SmartSuggestions
{
    public class SmartSuggestion
    {
        /// <summary>
        /// Text to display in the chip.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The action payload or command to send to the assistant/chatbot when clicked.
        /// </summary>
        public string ActionPayload { get; set; } = string.Empty;

        /// <summary>
        /// Optional type of suggestion (e.g., UncoveredShift, PendingLeave, etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public string? ActionLabel { get; set; }    // e.g., "Rebalance"
        public string? ActionIcon { get; set; }     // e.g., "🔁"
        public string? ActionName { get; set; }     // e.g., "autoRebalanceFatiguedShifts"
        public object? ActionData { get; set; }     // e.g., staffIds list
        public string? ActionText { get; set; }
    }

}
