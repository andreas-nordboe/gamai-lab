using System.Security.Claims;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.DTOs;
using GamAILab.Shared.Models.Game.DTOs;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.Game;
using GamAILab.WebApi.Services.HallucinationChecker;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Endpoints;

public static class CodeTasksEndpoint
{
    public static IEndpointRouteBuilder MapCodeTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/code-tasks")
            .WithTags("CodeTasks");
        
        // Get a single task
        group.MapGet("/{codeTaskId:int}",
            async Task<Results<Ok<CodeTask>, NotFound>> (int codeTaskId, ICodeTaskService codeTaskService) =>
            {
                var codeTask = await codeTaskService.GetCodeTaskById(codeTaskId);

                return codeTask is null ? TypedResults.NotFound() : TypedResults.Ok(codeTask);
            }).WithName("GetCodeTaskById")
        .Produces<CodeTask>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireLearner");
        
        // List code tasks
        group.MapGet("/tasks",
            async Task<Results<Ok<List<CodeTask>>, NotFound>> (ICodeTaskService codeTaskService) =>
            {
                var codeTasks = await codeTaskService.GetAllCodeTasks();

                return TypedResults.Ok(codeTasks ?? []);
            }).WithName("GetAllCodeTasks")
        .Produces<List<CodeTask>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireLearner");
        
        // Code Task management for higher privileged users
        
        // Add or update code task
        group.MapPost("/add-or-update",
            async Task<Results<Ok<CodeTask>, NotFound>> (ICodeTaskService codeTaskService, CodeTask codeTask) =>
            {
                var codeTasks = await codeTaskService.AddOrUpdateCodeTask(codeTask);

                return TypedResults.Ok(codeTasks);
            }).WithName("AddOrUpdateCodeTask")
        .Produces<List<CodeTask>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // Delete code task
        group.MapDelete("/delete/{codeTaskId:int}",
            async Task<Results<Ok<bool>, NotFound>> (ICodeTaskService codeTaskService, int codeTaskId) =>
            {
                var deletedCodeTask = await codeTaskService.DeleteCodeTaskById(codeTaskId);

                return TypedResults.Ok(deletedCodeTask);
            }).WithName("DeleteCodeTask")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // Re-generate code evaluation plan
        group.MapPut("/re-generate-code-evaluation-plan/{codeTaskId:int}",
            async Task<Results<Ok<CodeTask>, NotFound>> (ICodeTaskService codeTaskService, int codeTaskId) =>
            {
                var codeTasks = await codeTaskService.ReGenerateCodeEvaluationPlanAsync(codeTaskId);

                return TypedResults.Ok(codeTasks);
            }).WithName("ReGenerateCodeEvaluationPlan")
        .Produces<List<CodeTask>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // Add or update code task
        group.MapPost("/generate",
            async Task<Results<Ok<CodeTask>, NotFound>> (ICodeTaskService codeTaskService, GenerateCodeTaskRequest generateCodeTaskRequest) =>
            {
                var codeTasks = await codeTaskService.GenerateCodeTaskAsync(generateCodeTaskRequest);

                return TypedResults.Ok(codeTasks);
            }).WithName("GenerateCodeTask")
        .Produces<List<CodeTask>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // Export code tasks
        app.MapGet("/api/code-tasks/export", async (ApplicationDbContext db) =>
        {
            var tasks = await db.CodeTasks.Include(x => x.AiCodeEvaluationPlan).ToListAsync();

            return Results.Ok(tasks);
        })
        .RequireAuthorization("RequireResearcher");
        
        // Export verified examples
        group.MapGet("/verified-code-examples/export", async (VerifiedCodeEvaluationsService service) =>
        {
            var examples = await service.ExportVerifiedCodeEvaluationExamples();
            return Results.Ok(examples);
        })
        .RequireAuthorization("RequireResearcher");
        
        group.MapGet("/progress/{codeTaskId:int}", async Task<Results<Ok<CodeTaskLearnerProgress>, NotFound>> (int codeTaskId, ICodeSubmissionService codeSubmissionService, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
        
            var progress = await codeSubmissionService.LoadCodeTaskProgress(userId, codeTaskId, cancellationToken);
        
            return progress is null ? TypedResults.NotFound() : TypedResults.Ok(progress);
        })
        .WithName("GetCodeTaskProgress")
        .Produces<CodeTaskLearnerProgress>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireLearner");
        
        return app;
    }
    
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
    }
}