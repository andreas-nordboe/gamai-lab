using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.WebApi.Endpoints;

public static class CodeSubmissionEndpoint
{
    public static void MapCodeSubmissionEndpoint(this WebApplication app)
    {
        app.MapPost("/submit-code", async (CodeSubmission codeSubmission) =>
        {
            
        });
    }
}