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

        //group.MapPost("get-code-task", GetCodeTaskAsync)
        //.RequireAuthorization();

        group.MapGet("task",
            async Task<Results<Ok<CodeTask>, NotFound>> (int codeTaskId, ICodeTaskService codeTaskService) =>
            {
                var codeTask = await codeTaskService.GetCodeTaskById(codeTaskId);

                return codeTask is null ? TypedResults.NotFound() : TypedResults.Ok(codeTask);
            }).WithName("GetCodeTaskById")
        .Produces<CodeTask>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
            
        return app;
    }
    
    /*private static async Task<CodeTask> GetCodeTaskAsync(ICodeTaskService codeTaskService)
    {
        return codeTaskService.GetCodeTaskById()
    }*/
}