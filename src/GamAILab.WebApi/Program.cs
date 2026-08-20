using System.Security.Claims;
using System.Text;
using GamAILab.Shared.Models;
using Scalar.AspNetCore;
using GamAILab.WebApi;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Endpoints;
using GamAILab.WebApi.Services;
using GamAILab.WebApi.Services.AIPersonaSimulation;
using GamAILab.WebApi.Services.CodeExecution;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.Game;
using GamAILab.WebApi.Services.HallucinationChecker;
using GamAILab.WebApi.Services.LLMService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add CORS for frontend

const string FrontendClientPolicy = "WasmClient"; 
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendClientPolicy, policy =>
    {
        policy .WithOrigins("http://localhost:5123").AllowAnyHeader().AllowAnyMethod();
    });
    
});

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("GamAILabWebApiDb"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()   
    .AddApiEndpoints();

// Feature services
builder.Services.AddScoped<IAICodeEvaluationService, AICodeEvaluationService>();
builder.Services.AddScoped<ICodeTaskService, CodeTaskService>();
builder.Services.AddScoped<ICodeSubmissionService,  CodeSubmissionService>();
builder.Services.AddScoped<IAIHallucinationCheckerService, AIHallucinationCheckerService>();
builder.Services.AddSingleton<ICodeExecutionService, CodeExecutionService>();
builder.Services.AddSingleton<IAIFeedbackService, AIFeedbackService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IAIPersonaSimulationService, AIPersonaSimulationService>();
builder.Services.AddScoped<ILearningEngagementService, LearningLearningEngagementService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!));
});


builder.Services.AddAuthorizationBuilder()
.AddPolicy("RequireAdmin", policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireRole(UserRole.Admin);
});

builder.Services.AddHttpClient<ILLMService, LLMService>((provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["Ollama:BaseUrl"] ?? throw new Exception("Ollama:BaseUrl may be missing in appsettings.json");
    
    client.BaseAddress = new Uri(baseUrl);
    
    // .NET HTTPClient timeout is usually 100 seconds
    // I might adjust this later
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Create role to be used with endpoints (RBAC access policies)
using (var scope = app.Services.CreateScope())
{
    var services  = scope.ServiceProvider;
    
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    
    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
    foreach (var role in UserRole.All)
    {
        if (!await rolesManager.RoleExistsAsync(role))
        {
            await rolesManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seeding for empty initialisations
    if (app.Environment.IsDevelopment())
    {
        // Seed root admin user
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Note to self: I've moved these to a secrets.json for development (.NET user secrets)
        var rootAdminEmail = builder.Configuration["RootAdminUser:Email"];
        var rootAdminPassword = builder.Configuration["RootAdminUser:Password"];

        if (rootAdminEmail != null)
        {
            var rootAdminUser = await userManager.FindByEmailAsync(rootAdminEmail);
    
            if (rootAdminUser is null)
            {
                rootAdminUser = new ApplicationUser
                {
                    UserName = rootAdminEmail,
                    Email = rootAdminEmail,
                };
        
                var createRootAdminUser = await userManager.CreateAsync(rootAdminUser, rootAdminPassword);

                if (!createRootAdminUser.Succeeded)
                {
                    throw new Exception("Failed to create root admin user");
                }
            }

            if (!await userManager.IsInRoleAsync(rootAdminUser, UserRole.Admin))
            {
                var roleResult = await userManager.AddToRoleAsync(rootAdminUser, UserRole.Admin);

                if (!roleResult.Succeeded)
                {
                    throw new Exception("Failed to add root admin role");
                }
            }
        }
        
        // Seed code tasks
        var codeTaskService = services.GetRequiredService<ICodeTaskService>();
        await codeTaskService.SeedCodeTasks();

        // Seed AI personas
        var aiPersonaSimulationService = services.GetRequiredService<IAIPersonaSimulationService>();
        await aiPersonaSimulationService.SeedAIPersonas();
    }
    
   
}

// Map endpoints (TODO refactor this into using a separate endpoint handler later)
app.MapCodeSubmissionEndpoint();
app.MapAuthenticationEndpoints();
app.MapCodeTaskEndpoints();
app.MapCodeExecutionEndpoints();
app.MapGameProgressEndpoints();
app.MapPersonaEvaluationEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors(FrontendClientPolicy);

app.UseAuthentication();
app.UseAuthorization();

// For testing API
app.MapGet("/self", (ClaimsPrincipal claimsPrincipal) =>
{
    return Results.Ok(claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value));
}).RequireAuthorization();

app.MapIdentityApi<ApplicationUser>();

app.MapGet("/claims", (HttpContext context) =>
{
    var claims = context.User.Claims.Select(c => new
    {
        c.Type,
        c.Value
    });
    return Results.Ok(claims);
}).RequireAuthorization();

app.Run();