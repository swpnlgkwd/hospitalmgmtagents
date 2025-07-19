using Azure.AI.Agents.Persistent;

namespace HospitalStaffMgmtApis.Agents
{
    // Interface to define agent management contract
    public interface IAgentManager
    {
        // Ensures a Persistent Agent is created and returned
        Task<PersistentAgent> EnsureAgentExistsAsync();

        // Fetch the Agent
        PersistentAgent GetAgent();
    }
}
