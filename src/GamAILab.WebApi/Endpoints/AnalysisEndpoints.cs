using System.Security.Claims;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Services.Analysis;
using GamAILab.WebApi.Services.CodeTasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GamAILab.WebApi.Endpoints;

public static class AnalysisEndpoints
{
    public static IEndpointRouteBuilder MapAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analysis")
            .WithTags("Analysis");
        
        // Get a single analysis summary
        group.MapGet("/{analysisSummary:int}",
            async Task<Results<Ok<AIPersonaSimulationResponse>, NotFound>> (int analysisId, IAnalysisService analysisService) =>
            {
                var analysisSummary = await analysisService.GetAIPersonaAnalysisSummaryByIdAsync(analysisId);
        
                return analysisSummary is null ? TypedResults.NotFound() : TypedResults.Ok(analysisSummary);
            }).WithName("GetAnalysisSummaryById")
        .Produces<AIPersonaSimulationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // List all analysis summaries
        group.MapGet("/summary",
            async Task<Results<Ok<List<AIPersonaSimulationResponse>>, NotFound>> (IAnalysisService analysisService,
                ClaimsPrincipal user,
                ILoggerFactory loggerFactory,
                CancellationToken cancellationToken) =>
            {
                var analysisSummary = await analysisService.GetAIPersonaAnalysisSummaryAsync(cancellationToken);

                return TypedResults.Ok(analysisSummary ?? []);
            }).WithName("GetAllAnalysisSummaries")
        .Produces<List<CodeTask>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // Delete analysis summary (TODO this should not allowed in a production environment as it would void ethical considerations)
        group.MapDelete("/delete/{analysisId:int}",
                async Task<Results<Ok<bool>, NotFound>> (IAnalysisService analysisService, int analysisId) =>
                {
                    var deletedAnalysisSummary = await analysisService.DeleteAIPersonaAnalysisSummaryAsync(analysisId);

                    return TypedResults.Ok(deletedAnalysisSummary);
                }).WithName("DeleteAnalysisSummary")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        return app;
    }
    
    
}