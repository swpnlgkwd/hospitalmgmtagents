namespace HospitalSchedulingApp.Agent.AgentStore
{
    /// <summary>
    /// Defines a contract for storing and retrieving persistent agent IDs by name.
    /// This interface is useful for maintaining agent continuity across application restarts.
    /// </summary>
    public interface IAgentStore
    {
        /// <summary>
        /// Asynchronously loads the persisted agent ID associated with the specified agent name.
        /// </summary>
        /// <param name="agentName">The name of the agent to look up.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result contains the agent ID if found,
        /// or <c>null</c> if no ID is stored for the specified agent name.
        /// </returns>
        Task<string?> LoadAgentIdAsync(string agentName);

        /// <summary>
        /// Asynchronously saves the agent ID associated with the specified agent name.
        /// </summary>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="agentId">The ID of the agent to store.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveAgentIdAsync(string agentName, string agentId);
    }
}
