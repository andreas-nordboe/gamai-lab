using System.Security.Claims;
using System.Text.Json;
using GamAILab.Shared.Models.AICodeEvaluation.Hints;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Services;

namespace GamAILab.WebApi.Endpoints;

public static class CodeSubmissionEndpoints
{
    public static IEndpointRouteBuilder MapCodeSubmissionEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/code-submission")
            .WithTags("CodeSubmission");
        group.MapPost("/submit", SubmitCodeAsync).WithName("SubmitCode")
            .Produces<CodeSubmissionResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireLearner");
        group.MapPost("/code-hint", GenerateAICodeHintAsync).WithName("CodeHint")
            .Produces<AICodeHintResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireLearner");
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
        
        logger.LogInformation($"Submitting code for user {userId}. Data: {JsonSerializer.Serialize(codeSubmission)}");
        
        var codeSubmit = await codeSubmissionService.SubmitCodeAsync(codeSubmission, userId);

        return Results.Ok(codeSubmit);
    }

    private static async Task<IResult> GenerateAICodeHintAsync(AICodeHintRequest aiCodeHintRequest, IAIFeedbackService aiFeedbackService, ClaimsPrincipal user, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
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
        
        logger.LogInformation($"Generating AI assistant code hint for user {userId}. Data: {JsonSerializer.Serialize(aiCodeHintRequest)}");
        
        // Validate inputs
        if (string.IsNullOrWhiteSpace(aiCodeHintRequest.LearnerCode))
        {
            return Results.BadRequest("Learner code is empty");
        }

        if (string.IsNullOrWhiteSpace(aiCodeHintRequest.Question))
        {
            return Results.BadRequest("Learner question is empty");
        }

        var codeHint = await aiFeedbackService.GenerateCodeHintAsync(aiCodeHintRequest, userId, cancellationToken);
        return Results.Ok(codeHint);
    }
    
}