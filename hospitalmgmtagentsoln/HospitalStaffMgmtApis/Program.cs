using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using HospitalStaffMgmtApis.Agents;
using HospitalStaffMgmtApis.Agents.AgentStore;
using HospitalStaffMgmtApis.Agents.Handlers;
using HospitalStaffMgmtApis.Business;
using HospitalStaffMgmtApis.Data.Repository;
using HospitalStaffMgmtApis.Service;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Read configuration
var configuration = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin() // ?? allows any origin
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Register StaffRepository with connection string
builder.Services.AddSingleton<IStaffRepository>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Default Connection Missing");
    return new StaffRepository(connectionString);
});

// Register PersistentAgentsClient (singleton – shared across app)
builder.Services.AddSingleton<PersistentAgentsClient>(sp =>
{
    var endpoint = configuration["ProjectEndpoint"];
    return new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
});

// Register Tool Handlers
builder.Services.AddScoped<IToolHandler, StaffNameResolverToolHandler>();
builder.Services.AddScoped<IToolHandler, GetShiftScheduleToolHandler>();
builder.Services.AddScoped<IToolHandler, ApplyForLeaveToolHandler>();
builder.Services.AddScoped<IToolHandler, AutoReplaceShiftsForLeaveToolHandler>();
builder.Services.AddScoped<IToolHandler, ResolveRelativeDateToolHandler>();
builder.Services.AddScoped<IToolHandler, ShiftSwapToolHandler>();

// Register Agent infrastructure
builder.Services.AddSingleton<IAgentStore, FileAgentStore>();
builder.Services.AddSingleton<IAgentManager, AgentManager>();
builder.Services.AddSingleton<IScheduleManager, ScheduleManager>();

// Register AgentService with dependencies
builder.Services.AddScoped<AgentService>(sp =>
{
    var agentManager = sp.GetRequiredService<IAgentManager>();
    var agent = agentManager.GetAgent(); // Should return already-created PersistentAgent
    var client = sp.GetRequiredService<PersistentAgentsClient>();
    var staffRepo = sp.GetRequiredService<IStaffRepository>();
    var logger = sp.GetRequiredService<ILogger<AgentService>>();
    var toolHandlers = sp.GetServices<IToolHandler>(); // Resolves all registered handlers

    return new AgentService(client, agent, staffRepo, toolHandlers, logger);
});

// Add MVC and Swagger support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Apply CORS policy
app.UseCors("AllowAll");

// Configure middleware and pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure the agent is created at startup
using (var scope = app.Services.CreateScope())
{
    var agentManager = scope.ServiceProvider.GetRequiredService<IAgentManager>();
    await agentManager.EnsureAgentExistsAsync();
}

app.Run();
