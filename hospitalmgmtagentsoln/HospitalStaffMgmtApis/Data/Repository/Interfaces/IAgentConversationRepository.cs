using HospitalStaffMgmtApis.Data.Models.Agent;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    public interface IAgentConversationRepository
    {
        Task<AgentConversation> AddThreadForUser(AgentConversation agentConversation);

        Task<AgentConversation> FetchThreadForUser(string userId);

        Task DeleteThreadForUser(string threadId);

    }
}
