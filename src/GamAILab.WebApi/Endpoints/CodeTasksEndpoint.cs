using GamAILab.Shared.Models;
using GamAILab.WebApi.Services.CodeTasks;
using Microsoft.AspNetCore.Http.HttpResults;

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
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization();
        
        // List code tasks
        group.MapGet("/tasks",
            async Task<Results<Ok<List<CodeTask>>, NotFound>> (ICodeTaskService codeTaskService) =>
            {
                var codeTasks = await codeTaskService.GetAllCodeTasks();

                return TypedResults.Ok(codeTasks ?? []);
            }).WithName("GetAllCodeTasks")
        .Produces<List<CodeTask>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization();
        
        // Add or update code task
        group.MapPost("/add-or-update",
                async Task<Results<Ok<CodeTask>, NotFound>> (ICodeTaskService codeTaskService, CodeTask codeTask) =>
                {
                    var codeTasks = await codeTaskService.AddOrUpdateCodeTask(codeTask);

                    return TypedResults.Ok(codeTasks);
                }).WithName("AddOrUpdateCodeTask")
            .Produces<List<CodeTask>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireAdmin");
        
        // Delete code task
        group.MapDelete("/delete/{codeTaskId:int}",
                async Task<Results<Ok<bool>, NotFound>> (ICodeTaskService codeTaskService, int codeTaskId) =>
                {
                    var deletedCodeTask = await codeTaskService.DeleteCodeTaskById(codeTaskId);

                    return TypedResults.Ok(deletedCodeTask);
                }).WithName("DeleteCodeTask")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireAdmin");
        
        // Re-generate code evaluation plan
        group.MapPut("/re-generate-code-evaluation-plan/{codeTaskId:int}",
                async Task<Results<Ok<CodeTask>, NotFound>> (ICodeTaskService codeTaskService, int codeTaskId) =>
                {
                    var codeTasks = await codeTaskService.ReGenerateCodeEvaluationPlanAsync(codeTaskId);

                    return TypedResults.Ok(codeTasks);
                }).WithName("ReGenerateCodeEvaluationPlan")
            .Produces<List<CodeTask>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireAdmin");
        
        return app;
    }
}