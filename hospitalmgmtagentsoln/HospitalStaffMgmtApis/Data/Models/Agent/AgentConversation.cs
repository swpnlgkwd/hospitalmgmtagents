using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.Agent
{
    public class AgentConversation
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("thread_id")]
        public string ThreadId { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
