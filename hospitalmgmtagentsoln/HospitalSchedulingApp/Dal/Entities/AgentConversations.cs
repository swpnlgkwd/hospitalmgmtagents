namespace HospitalSchedulingApp.Dal.Entities
{
    public class AgentConversations
    {
        public string UserId { get; set; } = string.Empty;
        public string? ThreadId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
