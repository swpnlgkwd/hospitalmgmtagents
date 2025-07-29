using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.AgentStore;
using HospitalSchedulingApp.Agent.Tools;

namespace HospitalSchedulingApp.Agent
{
    /// <summary>
    /// Responsible for managing the lifecycle of a persistent Azure OpenAI agent.
    /// Ensures the agent exists, handles lookup or creation, and retrieves it when needed.
    /// </summary>
    public class AgentManager : IAgentManager
    {
        private readonly PersistentAgentsClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<AgentManager> _logger;
        private readonly IAgentStore _agentStore;
        private readonly string _agentName;
        private string agentId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentManager"/> class.
        /// </summary>
        /// <param name="persistentAgentClient">The client used to interact with the Azure OpenAI agent service.</param>
        /// <param name="config">Application configuration (e.g., for agent name and model deployment).</param>
        /// <param name="logger">Logger for diagnostics and status reporting.</param>
        /// <param name="agentStore">Agent ID storage provider (e.g., file-based).</param>
        public AgentManager(
            PersistentAgentsClient persistentAgentClient,
            IConfiguration config,
            ILogger<AgentManager> logger,
            IAgentStore agentStore)
        {
            _client = persistentAgentClient;
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _agentStore = agentStore ?? throw new ArgumentNullException(nameof(agentStore));
            _agentName = _config["AgentName"] ?? throw new ArgumentNullException("AgentName configuration is missing");
        }

        /// <summary>
        /// Returns the currently loaded persistent agent instance.
        /// </summary>
        /// <returns>The persistent agent instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the agent has not been initialized yet.</exception>
        public PersistentAgent GetAgent()
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new InvalidOperationException("Agent has not been initialized. Call EnsureAgentExistsAsync() first.");

            _logger.LogInformation($"Fetching agent with ID: {agentId}");
            return _client.Administration.GetAgent(agentId);
        }

        /// <summary>
        /// Ensures that a persistent agent exists:
        /// - Reuses the stored agent ID if valid.
        /// - Searches for an agent by name.
        /// - Creates a new agent if not found or broken.
        /// Also persists the agent ID to disk or store for reuse.
        /// </summary>
        /// <returns>The active <see cref="PersistentAgent"/> instance.</returns>
        public async Task<PersistentAgent> EnsureAgentExistsAsync()
        {
            // Load previously stored agentId (if any)
            agentId = await _agentStore.LoadAgentIdAsync(_agentName) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(agentId))
            {
                try
                {
                    var testAgent = _client.Administration.GetAgent(agentId);
                    _logger.LogInformation($"Using previously stored agent ID: {agentId}");
                    return testAgent;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Stored agent ID {agentId} is invalid or unreachable: {ex.Message}");
                }
            }

            // Search existing agents by name
            _logger.LogInformation($"Searching for agent with name: {_agentName}");

            await foreach (var agent in _client.Administration.GetAgentsAsync())
            {
                if (agent.Name.Equals(_agentName, StringComparison.OrdinalIgnoreCase))
                {
                    agentId = agent.Id;
                    _logger.LogInformation($"Found existing agent with ID: {agentId}");
                    await _agentStore.SaveAgentIdAsync(_agentName, agentId);
                    return agent;
                }
            }

            // Not found — create new agent
            string systemPromptPath = Path.Combine("SystemPrompt", "SystemPrompt.txt");

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found at: {systemPromptPath}");

            string instructions = await File.ReadAllTextAsync(systemPromptPath);
            string modelDeployment = _config["ModelDeploymentName"] ?? throw new ArgumentNullException("ModelDeploymentName configuration is missing");

            var newAgent = await _client.Administration.CreateAgentAsync(
                model: modelDeployment,
                name: _agentName,
                instructions: instructions,
                tools: ToolDefinitions.All
            );

            agentId = newAgent.Value.Id;
            _logger.LogInformation($"Created new agent with ID: {agentId}");

            await _agentStore.SaveAgentIdAsync(_agentName, agentId);

            return newAgent;
        }
    }
}
