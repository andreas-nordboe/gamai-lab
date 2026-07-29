using System.Security.Claims;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Services;

namespace GamAILab.WebApi.Endpoints;

public static class CodeSubmissionEndpoints
{
    public static IEndpointRouteBuilder MapCodeSubmissionEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/code-submission")
            .WithTags("CodeSubmission");
        group.MapPost("/submit", SubmitCodeAsync);
        return app;
    }

    private static async Task<IResult> SubmitCodeAsync(
        CodeSubmissionRequest codeSubmission, 
        ICodeSubmissionService codeSubmissionService,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }
        
        var logger = loggerFactory.CreateLogger(typeof(AuthenticationEndpoints));
        
        var codeSubmit = await codeSubmissionService.SubmitCodeAsync(codeSubmission, userId);

        return Results.Ok(codeSubmit);
    }
    
}