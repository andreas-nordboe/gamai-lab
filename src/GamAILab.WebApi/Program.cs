using System.Security.Claims;
using System.Text;
using GamAILab.Shared.Models;
using Scalar.AspNetCore;
using GamAILab.WebApi;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("GamAILabWebApiDb"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

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

builder.Services.AddAuthorization();

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

    // Seed root admin user
    if (app.Environment.IsDevelopment())
    {
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
                    Email = rootAdminEmail
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
    }
    
}

// Map endpoints (TODO refactor this into using a separate endpoint handler later)
app.MapCodeSubmissionEndpoint();
app.MapAuthenticationEndpoints();

app.UseHttpsRedirection();

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