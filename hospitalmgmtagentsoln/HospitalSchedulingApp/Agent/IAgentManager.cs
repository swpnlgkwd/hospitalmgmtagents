using Azure.AI.Agents.Persistent;

namespace HospitalSchedulingApp.Agent
{
    /// <summary>
    /// Interface to define the contract for managing persistent Azure OpenAI agents.
    /// </summary>
    public interface IAgentManager
    {
        /// <summary>
        /// Ensures that a persistent agent exists.
        /// Reuses an existing agent if found or creates a new one if necessary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="PersistentAgent"/> instance.</returns>
        Task<PersistentAgent> EnsureAgentExistsAsync();

        /// <summary>
        /// Retrieves the currently initialized persistent agent.
        /// </summary>
        /// <returns>The <see cref="PersistentAgent"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the agent has not yet been initialized.</exception>
        PersistentAgent GetAgent();
    }
}
