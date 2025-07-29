using Azure.AI.Agents.Persistent;
using Azure.Identity;
using HospitalSchedulingApp.Agent;
using HospitalSchedulingApp.Agent.AgentStore;
using HospitalSchedulingApp.Agent.Handlers;
using HospitalSchedulingApp.Agent.Handlers.Department;
using HospitalSchedulingApp.Agent.Handlers.HelperHandlers;
using HospitalSchedulingApp.Agent.Handlers.Shift;
using HospitalSchedulingApp.Agent.Handlers.Staff;
using HospitalSchedulingApp.Agent.Services;
using HospitalSchedulingApp.Agent.Tools.Department;
using HospitalSchedulingApp.Dal.Data;
using HospitalSchedulingApp.Dal.Repositories;
using HospitalSchedulingApp.Services;
using HospitalSchedulingApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});


// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Register Agent infrastructure
builder.Services.AddSingleton<IAgentStore, FileAgentStore>(); 
builder.Services.AddSingleton<IAgentManager, AgentManager>();


// Register PersistentAgentsClient (singleton – shared across app)

builder.Services.AddSingleton<PersistentAgentsClient>(sp =>
{
    var endpoint = configuration["ProjectEndpoint"];
    return new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
});


// Register AgentService with dependencies


builder.Services.AddScoped<IAgentService, AgentService>(sp =>
{
    var agentManager = sp.GetRequiredService<IAgentManager>();
    var agent = agentManager.GetAgent(); // returns a PersistentAgent (already built)
    var client = sp.GetRequiredService<PersistentAgentsClient>();
    var logger = sp.GetRequiredService<ILogger<AgentService>>();
    var toolHandlers = sp.GetServices<IToolHandler>();

    return new AgentService(client, agent, toolHandlers, logger);
});

// Security Tokens
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
var key = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // for dev only
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
    };
});


// Register repositories and services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPlannedShiftService, PlannedShiftService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IShiftTypeService, ShiftTypeService>();

// Tool Handlers
builder.Services.AddScoped<IToolHandler, FilterPlannedShiftsToolHandler>();
builder.Services.AddScoped<IToolHandler, ResolveDepartmentInfoToolHandler>();
builder.Services.AddScoped<IToolHandler, ResolveStaffInfoByNameToolHandler>();
builder.Services.AddScoped<IToolHandler, ResolveRelativeDateToolHandler>();
builder.Services.AddScoped<IToolHandler, SearchAvailableStaffToolHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply CORS policy
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();


// Ensure the agent is created at startup
using (var scope = app.Services.CreateScope())
{
    var agentManager = scope.ServiceProvider.GetRequiredService<IAgentManager>();
    await agentManager.EnsureAgentExistsAsync();
}

app.Run();
