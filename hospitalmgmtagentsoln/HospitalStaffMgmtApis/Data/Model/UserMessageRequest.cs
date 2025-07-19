namespace HospitalStaffMgmtApis.Data.Model
{
    /// <summary>
    /// Represents the message input sent by the user to the chat interface or AI agent.
    /// </summary>
    public class UserMessageRequest
    {
        /// <summary>
        /// The actual message text submitted by the user.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

