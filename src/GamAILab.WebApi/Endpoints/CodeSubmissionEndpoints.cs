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
        ILoggerFactory loggerFactory)
    {
        // TODO check user.Identity?.IsAuthenticated is not true and return unauthorised or use a different method

        //var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
        var userId = Guid.NewGuid().ToString(); // TODO Remove fallback to GUID after testing
        
        // TODO return unauthroised if userid is null/whitespace
        
        var logger = loggerFactory.CreateLogger(typeof(AuthenticationEndpoints));
        
        
        
        var codeSubmit = await codeSubmissionService.SubmitCodeAsync(codeSubmission, userId);

        return Results.Ok(codeSubmit);
    }
    
}