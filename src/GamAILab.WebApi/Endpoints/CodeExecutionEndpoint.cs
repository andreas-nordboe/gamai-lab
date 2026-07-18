using System.Security.Claims;
using System.Text.Json;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Services;
using GamAILab.WebApi.Services.CodeExecution;

namespace GamAILab.WebApi.Endpoints;

public static class CodeExecutionEndpoint
{
    public static IEndpointRouteBuilder MapCodeExecutionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/code-execution")
            .WithTags("CodeExecution");
        group.MapPost("/execute", ExecuteCodeAsync);
        return app;
    }

    private static async Task<IResult> ExecuteCodeAsync(
        CodeExecutionRequest codeExecutionRequest, 
        ICodeExecutionService codeExecutionService,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        // TODO potentially store code execution for traceability

        return Results.Ok(await codeExecutionService.ExecuteCodeNoTests(codeExecutionRequest.Code, cancellationToken));
    }
}