using System.Data.SqlClient;
using System.Net;
using System.Text.Json;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using HospitalStaffMgmtApis.Agents.Tools;
using Microsoft.Extensions.Configuration;

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

    // Concrete implementation of IAgentManager
    public class AgentManager : IAgentManager
    {
        private readonly PersistentAgentsClient _client;   // Persistent client to manage agents
        private readonly IConfiguration _config;           // Configuration for app settings
        private readonly string _agentName;              // Name of the agent to manage
        private readonly ILogger<AgentManager> _logger;

        // Constructor receives injected dependencies
        public AgentManager(PersistentAgentsClient persistentAgentClient, IConfiguration config, ILogger<AgentManager> logger)
        {
            _client = persistentAgentClient;
            _config = config;
            _agentName = _config["AgentName"] ?? throw new ArgumentNullException("AgentName configuration is missing");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Fetches the agent by name
        public PersistentAgent GetAgent()
        {
            _logger.LogInformation($"Fetching agent with name: {_agentName}");
            return _client.Administration.GetAgent(_agentName); // Get agent by name
        }

        // Creates and returns the agent, reading prompt and tool definitions
        public async Task<PersistentAgent> EnsureAgentExistsAsync()
        {

            // Try to find agent with this name
            await foreach (var agent in _client.Administration.GetAgentsAsync())
            {
                if (agent.Name.Equals(_agentName, StringComparison.OrdinalIgnoreCase))
                {
                    return agent; // Agent already exists
                }

            }
            // Path to system instructions (LLM prompt)
            string systemPromptPath = Path.Combine("SystemInstruction", "systemprompt.txt");

            // Read the prompt text from the file
            string instructions = await File.ReadAllTextAsync(systemPromptPath);

            // Read configuration for model deployment and agent name
            string modelDeployment = _config["ModelDeploymentName"];

            // Create the agent with instructions and registered tools
            return await _client.Administration.CreateAgentAsync(
                model: modelDeployment,
                name: _agentName,
                instructions: instructions,
                tools: ToolDefinitions.All  // Static list of tool definitions
            );

        }
    }
}
