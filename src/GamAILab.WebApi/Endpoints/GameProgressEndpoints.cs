using System.Security.Claims;
using GamAILab.Shared.Models.Game;
using GamAILab.Shared.Models.Game.DTOs;
using GamAILab.WebApi.Services.Game;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GamAILab.WebApi.Endpoints;

public static class GameProgressEndpoints
{
    public static IEndpointRouteBuilder MapGameProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/game")
            .WithTags("GameProgress");
        
        // Game progress
        
        group.MapPost("/progress", async Task<Results<NoContent, BadRequest<string>>> (IGameService gameService, ClaimsPrincipal user, LearnerGameProgressRequest progress) =>
        {
            var userId = GetUserId(user);
            if (progress == null)
            {
                return TypedResults.BadRequest("Progress parameter is empty");
            }
            
            await gameService.SaveLearnerGameProgress(userId, progress);
            return TypedResults.NoContent();
        })
        .WithName("SaveLearnerGameProgress")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
        
        group.MapGet("/game-progress",
        async Task<Results<Ok<List<LearnerGameProgress>>, NotFound, UnauthorizedHttpResult>> (IGameService gameService, ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return TypedResults.Unauthorized();
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";

            if (string.IsNullOrWhiteSpace(userId))
            {
                return TypedResults.Unauthorized();
            }
            
            var learnerProgress = await gameService.LoadLearnerGameProgress(userId);

            return learnerProgress is null ? TypedResults.NotFound() : TypedResults.Ok(learnerProgress);
        }).WithName("GetLearnerGameProgress")
        .Produces<LearnerGameProgress>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization().RequireAuthorization();
        
        
        // Custom Data
        
        group.MapGet("/custom-data", async Task<Ok<List<CustomData>>> (IGameService gameService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var customData = await gameService.ListCustomData(userId);
            return TypedResults.Ok(customData);
        })
        .WithName("ListCustomData")
        .Produces<List<CustomData>>(StatusCodes.Status200OK).RequireAuthorization();
        
        
        group.MapGet("/custom-data/{key}", async Task<Results<Ok<CustomData>, NotFound, BadRequest<string>>> (string key, IGameService gameService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            if (string.IsNullOrWhiteSpace(key))
            {
                return TypedResults.BadRequest("Key is empty");
            }
                

            var data = await gameService.GetCustomData(userId, key);
            return data is null ? TypedResults.NotFound() : TypedResults.Ok(data);
        })
        .WithName("GetCustomData")
        .Produces<CustomData>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization();
        
        group.MapPost("/custom-data", async Task<Results<NoContent, BadRequest<string>>> (IGameService gameService, ClaimsPrincipal user, CustomDataRequest customData) =>
        {
            var userId = GetUserId(user);
            if (customData == null || string.IsNullOrWhiteSpace(customData.Key)) 
                return TypedResults.BadRequest("Valid custom data with a key is required.");
            
            Console.WriteLine(userId);

            await gameService.SaveCustomData(userId, customData);
            return TypedResults.NoContent();
        })
        .WithName("SaveCustomData")
        .Produces(StatusCodes.Status204NoContent).RequireAuthorization();
        
        // Game Objectives
        
        group.MapGet("/objectives", async Task<Ok<List<GameObjective>>> (IGameService gameService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var objectives = await gameService.LoadGameObjectives(userId);
            return TypedResults.Ok(objectives);
        })
        .WithName("LoadGameObjectives")
        .Produces<List<GameObjective>>(StatusCodes.Status200OK).RequireAuthorization();

        group.MapGet("/objectives/{objectiveId}", async Task<Results<Ok<GameObjective>, NotFound, BadRequest<string>>> (string objectiveId, IGameService gameService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            if (string.IsNullOrWhiteSpace(objectiveId))
            {
                return TypedResults.BadRequest("Objective id is empty");
            }

            var objective = await gameService.GetGameObjective(userId, objectiveId);
            return objective is null ? TypedResults.NotFound() : TypedResults.Ok(objective);
        })
        .WithName("GetGameObjective")
        .Produces<GameObjective>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization();
        

        group.MapPost("/objectives", async Task<Results<NoContent, BadRequest<string>>> (IGameService gameService, ClaimsPrincipal user, GameObjectiveRequest objective) =>
        {
            var userId = GetUserId(user);
            if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
            {
                return TypedResults.BadRequest("Objective is null or has an empty objective id");
            }

            await gameService.SaveGameObjectives(userId, objective);
            return TypedResults.NoContent();
        })
        .WithName("SaveGameObjective")
        .Produces(StatusCodes.Status204NoContent).RequireAuthorization();
        
        return app;
    }
    
    
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
    }
}