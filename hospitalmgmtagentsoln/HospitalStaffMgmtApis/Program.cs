using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using HospitalStaffMgmtApis.Agents;
using HospitalStaffMgmtApis.Agents.AgentStore;
using HospitalStaffMgmtApis.Agents.Handlers;
using HospitalStaffMgmtApis.Agents.Services;
using HospitalStaffMgmtApis.Business;
using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Repository;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Read configuration
var configuration = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin() // 👈 allows any origin
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

builder.Services.AddScoped<IAuthRepository>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException("Default Connection Missing");

    return new AuthRepository(connectionString);
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
builder.Services.AddScoped<IToolHandler, DepartmentNameResolverToolHandler>();
builder.Services.AddScoped<IToolHandler, FindAvailableStaffToolHandler>();
builder.Services.AddScoped<IToolHandler, UncoverShiftToolHandler>();
builder.Services.AddScoped<IToolHandler, ViewPendingLeaveRequestToolHandler>();


// Register Agent infrastructure
builder.Services.AddSingleton<IAgentStore, FileAgentStore>();
builder.Services.AddSingleton<IAgentManager, AgentManager>();
builder.Services.AddSingleton<IScheduleManager, ScheduleManager>();

builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ISchedulerSuggestionService, SchedulerSuggestionService>();


var key = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // for dev only
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
}); 
builder.Services.AddAuthorization();

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

builder.Services.AddHttpContextAccessor();

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

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure the agent is created at startup
using (var scope = app.Services.CreateScope())
{
    var agentManager = scope.ServiceProvider.GetRequiredService<IAgentManager>();
    await agentManager.EnsureAgentExistsAsync();
}

app.Run();
