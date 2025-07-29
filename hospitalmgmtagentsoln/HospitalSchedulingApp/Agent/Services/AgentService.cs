using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Handlers;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Services
{
    /// <summary>
    /// Service responsible for handling interactions with the Persistent Agent,
    /// including sending messages, receiving responses, and resolving tool calls.
    /// </summary>
    public class AgentService : IAgentService
    {
        private readonly PersistentAgentsClient _client;
        private readonly PersistentAgent _agent;
        private readonly ILogger<AgentService> _logger;
        private readonly IEnumerable<IToolHandler> _toolHandlers;

        public AgentService(
            PersistentAgentsClient persistentAgentsClient,
            PersistentAgent agent,
            IEnumerable<IToolHandler> toolHandlers,
            ILogger<AgentService> logger)
        {
            _client = persistentAgentsClient;
            _agent = agent;
            _toolHandlers = toolHandlers;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new persistent agent thread for communication.
        /// </summary>
        public PersistentAgentThread CreateThread()
        {
            return _client.Threads.CreateThread();
        }

        /// <summary>
        /// Adds a user message to the provided thread.
        /// </summary>
        public Task AddUserMessageAsync(string threadId, MessageRole role, string message)
        {
            _logger.LogInformation($"Adding user message to thread {threadId}: {message}");
            _client.Messages.CreateMessage(threadId, role, message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a user message to the provided thread.
        /// </summary>
        public async Task DeleteThreadForUser(string threadId)
        {
            //// Clean up thread
            await _client.Threads.DeleteThreadAsync(threadId);
            _logger.LogInformation($"Thread {threadId} deleted.");

        }

        /// <summary>
        /// Sends a message to the agent and waits for its final response.
        /// </summary>
        public async Task<MessageContent?> GetAgentResponseAsync(string threadId, MessageRole role, string message)
        {
            var thread = CreateThread();
            threadId =  thread.Id;

            try
            {
                await AddUserMessageAsync(threadId, role, message);
               
                ThreadRun run = _client.Runs.CreateRun(threadId, _agent.Id);

                do
                {
                    Thread.Sleep(500);
                    run = _client.Runs.GetRun(threadId, run.Id);

                    if (run.Status == RunStatus.RequiresAction &&
                        run.RequiredAction is SubmitToolOutputsAction action)
                    {
                        var toolOutputs = new List<ToolOutput>();

                        foreach (var toolCall in action.ToolCalls)
                        {
                            var result = await GetResolvedToolOutput(toolCall);
                            if (result != null)
                                toolOutputs.Add(result);
                        }

                        run = _client.Runs.SubmitToolOutputsToRun(threadId, run.Id, toolOutputs);
                    }

                } while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction);

                var messages = _client.Messages.GetMessages(
                    threadId: threadId,
                    order: ListSortOrder.Descending
                );

                foreach (var msg in messages)
                {
                    var messageText = msg.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
                    _logger.LogInformation(messageText?.Text);
                    return messageText;
                }

                return null;
            }
            finally
            {
                //// Clean up thread
                //await _client.Threads.DeleteThreadAsync(thread.Id);
                //_logger.LogInformation($"Thread {thread.Id} deleted.");
            }
        }

        /// <summary>
        /// Resolves a single tool call by matching it with a registered IToolHandler.
        /// </summary>
        public async Task<ToolOutput?> GetResolvedToolOutput(RequiredToolCall toolCall)
        {
            if (toolCall is not RequiredFunctionToolCall functionToolCall)
                return null;

            _logger.LogInformation("Tool invoked: {ToolName} | ID: {ToolId}", functionToolCall.Name, toolCall.Id);
            _logger.LogInformation("Arguments: {Arguments}", functionToolCall.Arguments);

            try
            {
                using var doc = JsonDocument.Parse(functionToolCall.Arguments);
                var root = doc.RootElement;

                var handler = _toolHandlers.FirstOrDefault(h => h.ToolName == functionToolCall.Name);
                if (handler == null)
                {
                    _logger.LogWarning("No handler found for tool: {ToolName}", functionToolCall.Name);
                    return null;
                }

                return await handler.HandleAsync(functionToolCall, root);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while resolving tool output for {ToolName}", functionToolCall.Name);
                return null;
            }
        }


    }
}
