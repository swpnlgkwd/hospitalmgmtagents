namespace HospitalStaffMgmtApis.Data.Models
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
    }

}
