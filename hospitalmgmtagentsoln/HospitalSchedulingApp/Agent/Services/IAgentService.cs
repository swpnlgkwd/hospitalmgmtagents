using Azure.AI.Agents.Persistent;

namespace HospitalSchedulingApp.Agent.Services
{
    public interface IAgentService
    {
        PersistentAgentThread CreateThread();

        Task AddUserMessageAsync(string threadId, MessageRole role, string message);

        Task<MessageContent?> GetAgentResponseAsync(string threadId, MessageRole role, string message);

        Task<ToolOutput?> GetResolvedToolOutput(RequiredToolCall toolCall);

        Task DeleteThreadForUser(string threadId);
    }
}
