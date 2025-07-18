using Azure;
using Azure.AI.Agents;
using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.ToolDefinitions;
using HospitalStaffMgmtApis.Functions;
using HospitalStaffMgmtApis.Models;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents
{
    public class AgentService
    {
        private readonly PersistentAgentsClient _client;
        private readonly PersistentAgent _agent;
        private readonly IStaffRepository _staffRepository;

        public AgentService(PersistentAgentsClient persistentAgentsClient, PersistentAgent agent, IStaffRepository staffRepository)
        {
            _client = persistentAgentsClient;
            _agent = agent;
            _staffRepository = staffRepository;
        }

        /// <summary>
        /// Creates a new thread for a persistent agent conversation.
        /// </summary>
        public PersistentAgentThread CreateThread()
        {
            return _client.Threads.CreateThread();
        }

        /// <summary>
        /// Adds a user message to the thread.
        /// </summary>
        public Task AddUserMessageAsync(PersistentAgentThread thread, MessageRole role, string message)
        {
            _client.Messages.CreateMessage(thread.Id, role, message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts a run and retrieves the final response message from the agent.
        /// </summary>
        public async Task<MessageContent?> GetAgentResponseAsync(MessageRole role, string message)
        {
            // Step 1: Create a new thread and add user message
            var thread = CreateThread();
            await AddUserMessageAsync(thread, role, message);

            // Step 2: Start a new run
            ThreadRun run = _client.Runs.CreateRun(thread.Id, _agent.Id);

            // Step 3: Poll for completion
            do
            {
                Thread.Sleep(500); // Polling interval
                run = _client.Runs.GetRun(thread.Id, run.Id);

                // If tools are required, resolve and submit tool outputs
                if (run.Status == RunStatus.RequiresAction &&
                    run.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
                {
                    var toolOutputs = new List<ToolOutput>();

                    foreach (var toolCall in submitToolOutputsAction.ToolCalls)
                    {
                        var result = await GetResolvedToolOutput(toolCall);
                        if (result != null)
                            toolOutputs.Add(result);
                    }
                    run = _client.Runs.SubmitToolOutputsToRun(thread.Id, run.Id, toolOutputs);
                }
            }
            while (run.Status == RunStatus.Queued ||
                   run.Status == RunStatus.InProgress ||
                   run.Status == RunStatus.RequiresAction);

            // Step 4: Return the last agent response
            var messages = _client.Messages.GetMessages(
                threadId: thread.Id,
                order: ListSortOrder.Descending
            );


            //var messageContent = messages.First();

            //return messageContent.ContentItems.OfType<MessageTextContent>().FirstOrDefault();


            // Return the latest assistant message (if any)
            foreach (var msg in messages)
            {
                var messg = msg.ContentItems.OfType<MessageTextContent>().FirstOrDefault();

                return messg;
            }

            return null;
        }

        /// <summary>
        /// Handles resolving specific tool calls triggered by the agent.
        /// </summary>
        private async Task<ToolOutput?> GetResolvedToolOutput(RequiredToolCall toolCall)
        {
            if (toolCall is RequiredFunctionToolCall functionToolCall)
            {
                using var argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);

                Console.WriteLine($"Tool invoked: {toolCall.Id} at {DateTime.Now} for  {functionToolCall.Name}" );
                if (functionToolCall.Name == FindAvailableStaffTool.GetTool().Name)
                {
                    var root = argumentsJson.RootElement;
                    var request = new FindStaffRequest
                    {
                        ShiftDate = root.TryGetProperty("shiftDate", out var shiftDateProp) ? shiftDateProp.GetString() ?? "" : "",
                        ShiftType = root.TryGetProperty("shiftType", out var shiftTypeProp) ? shiftTypeProp.GetString() ?? "" : "",
                        Role = root.TryGetProperty("role", out var roleProp) ? roleProp.GetString() : null,
                        Department = root.TryGetProperty("department", out var deptProp) ? deptProp.GetString() : null
                    };

                    var staffResults = await _staffRepository.FindAvailableStaffAsync(request);
                    var resultJson = JsonSerializer.Serialize(staffResults, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });


                    return new ToolOutput(functionToolCall.Id, resultJson);
                }
            }

            return null;
        }
    }
}
