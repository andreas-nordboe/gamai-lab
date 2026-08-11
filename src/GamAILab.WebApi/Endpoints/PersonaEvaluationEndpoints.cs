using System.Security.Claims;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.WebApi.Services.AIPersonaSimulation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GamAILab.WebApi.Endpoints;

public static class PersonaEvaluationEndpoints
{
    public static IEndpointRouteBuilder MapPersonaEvaluationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai-personas")
            .WithTags("AIPersonas");
        group.MapPost("/run-simulation", RunPersonaSimulationAsync)
            .WithName("RunPersonaCodeEvaluationSimulation")
            .Produces<AIPersonaSimulationResponse>(StatusCodes.Status200OK);
            //.Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireAdmin"); // TODO remember to force admin authorisation
            
            // List AI Personas
            group.MapGet("/ai-personas",
                    async Task<Results<Ok<List<AIPersona>>, NotFound>> (IAIPersonaSimulationService aiPersonaService) =>
                    {
                        var aiPersonas = await aiPersonaService.GetAllAIPersonas();

                        return TypedResults.Ok(aiPersonas ?? []);
                    }).WithName("GetAllAIPersonas")
                .Produces<List<AIPersona>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound).RequireAuthorization();
            
            // Add or update persona
            group.MapPost("/add-or-update",
                    async Task<Results<Ok<AIPersona>, NotFound>> (IAIPersonaSimulationService aiPersonaService, AIPersona aiPersona) =>
                    {
                        var aiPersonas = await aiPersonaService.AddOrUpdatePersona(aiPersona);

                        return TypedResults.Ok(aiPersonas);
                    }).WithName("AddOrUpdateAIPersona")
                .Produces<List<AIPersona>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireAdmin");
        
            // Delete persona
            group.MapDelete("/delete/{aiPersonaId:int}",
                    async Task<Results<Ok<bool>, NotFound>> (IAIPersonaSimulationService aiPersonaService, int aiPersonaId) =>
                    {
                        var deletedAIPersona = await aiPersonaService.DeleteAIPersonaById(aiPersonaId);

                        return TypedResults.Ok(deletedAIPersona);
                    }).WithName("DeleteAIPersona")
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireAdmin");
            
        return app;
    }

    private static async Task<AIPersonaSimulationResponse> RunPersonaSimulationAsync(
        AIPersonaSimulationRequest personaSimulationRequest,
        IAIPersonaSimulationService personaSimulationService,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return await personaSimulationService.RunAIPersonaCodeEvaluationSimulationAsync(personaSimulationRequest, cancellationToken);
    }
    
    
}