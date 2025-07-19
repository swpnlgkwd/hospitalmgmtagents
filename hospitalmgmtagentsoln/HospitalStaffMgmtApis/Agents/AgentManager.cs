using System.Data.SqlClient;
using System.Net;
using System.Text.Json;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using HospitalStaffMgmtApis.Agents.FunctionTools;
using Microsoft.Extensions.Configuration;

namespace HospitalStaffMgmtApis.Agents
{
    // Class Responsible for Agent Setup
    public class AgentManager : IAgentManager
    {
        private readonly PersistentAgentsClient _client;
        private readonly IConfiguration _config;
        private readonly string _agentName;
        private readonly ILogger<AgentManager> _logger;

        private string agentId = string.Empty; // Use instance field instead of static

        public AgentManager(PersistentAgentsClient persistentAgentClient, IConfiguration config, ILogger<AgentManager> logger)
        {
            _client = persistentAgentClient;
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _agentName = _config["AgentName"] ?? throw new ArgumentNullException("AgentName configuration is missing");
        }

        public PersistentAgent GetAgent()
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new InvalidOperationException("Agent has not been initialized. Call EnsureAgentExistsAsync() first.");

            _logger.LogInformation($"Fetching agent with ID: {agentId}");
            return _client.Administration.GetAgent(agentId);
        }

        public async Task<PersistentAgent> EnsureAgentExistsAsync()
        {
            _logger.LogInformation($"Looking for agent with name: {_agentName}");

            await foreach (var agent in _client.Administration.GetAgentsAsync())
            {
                if (agent.Name.Equals(_agentName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Agent already exists with ID: {agent.Id}");
                    agentId = agent.Id;
                    return agent;
                }
            }

            string systemPromptPath = Path.Combine("SystemInstruction", "systemprompt.txt");

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found at path: {systemPromptPath}");

            string instructions = await File.ReadAllTextAsync(systemPromptPath);
            string modelDeployment = _config["ModelDeploymentName"] ?? throw new ArgumentNullException("ModelDeploymentName configuration is missing");

            var createdAgentResponse = await _client.Administration.CreateAgentAsync(
                model: modelDeployment,
                name: _agentName,
                instructions: instructions,
                tools: ToolDefinitions.All
            );

            agentId = createdAgentResponse.Value.Id;
            _logger.LogInformation($"Created new agent with ID: {agentId}");

            return createdAgentResponse;
        }
    }
}
