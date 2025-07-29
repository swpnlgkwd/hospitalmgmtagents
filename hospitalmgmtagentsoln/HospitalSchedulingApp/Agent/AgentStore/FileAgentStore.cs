using System.Text.Json;

namespace HospitalSchedulingApp.Agent.AgentStore
{

    /// <summary>
    /// A file-based implementation of <see cref="IAgentStore"/> that stores agent IDs in a JSON file.
    /// This provides a simple persistence mechanism across application restarts.
    /// </summary>
    public class FileAgentStore : IAgentStore
    {
        private readonly string _filePath = "agent_store.json";

        /// <summary>
        /// Loads the agent ID associated with the specified agent name from the JSON file.
        /// </summary>
        /// <param name="agentName">The name of the agent to retrieve the ID for.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result contains the agent ID if found, or <c>null</c> otherwise.
        /// </returns>
        public async Task<string?> LoadAgentIdAsync(string agentName)
        {
            if (!File.Exists(_filePath)) return null;

            var json = await File.ReadAllTextAsync(_filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            return data != null && data.TryGetValue(agentName, out var id) ? id : null;
        }

        /// <summary>
        /// Saves or updates the agent ID associated with the specified agent name to the JSON file.
        /// </summary>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="agentId">The ID of the agent to persist.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task SaveAgentIdAsync(string agentName, string agentId)
        {
            Dictionary<string, string> data = new();

            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath);
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }

            data[agentName] = agentId;

            var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, updatedJson);
        }
    }
}
