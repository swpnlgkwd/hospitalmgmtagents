using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using HospitalStaffMgmtApis.Agents;
using HospitalStaffMgmtApis.Data.Repository;

var builder = WebApplication.CreateBuilder(args);


// Read configuration
var configuration = builder.Configuration;

builder.Services.AddSingleton<IStaffRepository>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new StaffRepository(connectionString);
});

// Register PersistentAgentsClient (singleton – shared across app)
builder.Services.AddSingleton<PersistentAgentsClient>(sp =>
{
    var endpoint = configuration["ProjectEndpoint"]; ;
    return new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
});

// Register AgentManager as a Singleton through IAgentManager interface
builder.Services.AddSingleton<IAgentManager, AgentManager>();

builder.Services.AddScoped<AgentService>( sp =>
{
    var agentManager = sp.GetRequiredService<IAgentManager>();
    var agent =  agentManager.GetAgent(); // This should return the already-created PersistentAgent
    var client = sp.GetRequiredService<PersistentAgentsClient>();
    var staffRepo = sp.GetRequiredService<IStaffRepository>();
    var loggerService = sp.GetRequiredService<ILogger<AgentService>>();

    return new AgentService(client, agent, staffRepo, loggerService);
});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Ensure the agent exists at application startup
using (var scope = app.Services.CreateScope())
{
    var agentManager = scope.ServiceProvider.GetRequiredService<IAgentManager>();
    await agentManager.EnsureAgentExistsAsync();
}

app.Run();
