using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.Authentication;
using GamAILab.WebApi.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace GamAILab.WebApi.Endpoints;

public static class AuthenticationEndpoints
{
    
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("register", RegisterAsync).AllowAnonymous();
        group.MapPost("login", LoginAsync).AllowAnonymous();
        
        // Will be used later for admins to change user privileges 
        group.MapPost("update-user-role", UpdateUserRoleAsync)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.Admin));

        return app;
    }
    
    // Login User
    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration) // TODO replace with options instead
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest("Please provide email and password");
        }
        
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is  null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Unauthorized();
        }
            
        var roles = await userManager.GetRolesAsync(user);
            
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
            
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email,user.Email!),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];
        
        var tokenExpiry = DateTime.UtcNow.AddMinutes(
            configuration.GetValue<int>("Jwt:ExpirationTimeInMins"));

        var tokenDescriptior = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationTimeInMins")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var tokenHandler = new JsonWebTokenHandler();
        string accessToken = tokenHandler.CreateToken(tokenDescriptior);

        return Results.Ok(new AuthenticationResponse
        {
            AccessToken = accessToken,
            ExpiresAt = tokenExpiry,
            User = new AuthenticationUserResponse()
            {
                Id = user.Id, 
                Email = 
                user.Email, 
                Roles = roles
            }
        });
    }

    // Register User
    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthenticationEndpoints));
        
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            //DisplayName = request // TODO add username later
        };
        
        IdentityResult identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return Results.BadRequest(identityResult.Errors);
        }
        
        // Allocate learner role by default (can be changed by admin)
        if (!await userManager.IsInRoleAsync(user, UserRole.Learner))
        {
            var roleResult = await userManager.AddToRoleAsync(user, UserRole.Learner);

            if (!roleResult.Succeeded)
            {
                logger.Log(LogLevel.Error, $"Failed to create role for user: {user.Email}");
            }
        }
            
        return Results.Created($"/users/{user.Id}", new
        {
            user.Id,
            user.Email
        });
    }
    
    private static async Task<IResult> UpdateUserRoleAsync(
        string email,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is not null && !await userManager.IsInRoleAsync(user, UserRole.Admin))
        {
            await userManager.AddToRoleAsync(user, UserRole.Admin);
            return Results.Ok();
        }
        
        return Results.BadRequest();
    }
    
}