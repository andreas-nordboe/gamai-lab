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
        
        // Task + personas(s)
        group.MapPost("/run-simulation", RunPersonaSimulationAsync)
            .WithName("RunPersonaCodeEvaluationSimulation")
            .Produces<AIPersonaSimulationResponse>(StatusCodes.Status200OK)
            .RequireAuthorization("RequireResearcher");
            
        // Classroom
        group.MapPost("/run-classroom-simulation", RunClassroomSimulationAsync)
            .WithName("RunClassroomSimulation")
            .Produces<List<AIPersonaSimulationResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization("RequireResearcher");
            
    
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
            .Produces<AIPersona>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
    
        // Delete persona
        group.MapDelete("/delete/{aiPersonaId:int}",
                async Task<Results<Ok<bool>, NotFound>> (IAIPersonaSimulationService aiPersonaService, int aiPersonaId) =>
                {
                    var deletedAIPersona = await aiPersonaService.DeleteAIPersonaById(aiPersonaId);

                    return TypedResults.Ok(deletedAIPersona);
                }).WithName("DeleteAIPersona")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // Generate persona
        group.MapPost("/generate",
                async Task<Results<Ok<AIPersona>, NotFound>> (IAIPersonaSimulationService aiPersonaService, GenerateAIPersonaRequest aiPersonaDescription) =>
                {
                    var aiPersonas = await aiPersonaService.GenerateAIPersona(aiPersonaDescription.Description);

                    return TypedResults.Ok(aiPersonas);
                }).WithName("GenerateAIPersona")
            .Produces<AIPersona>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound).RequireAuthorization("RequireResearcher");
        
        // List classroom simulations (analysis and also frontend dashboard where simulations are shown)
        group.MapGet("/classroom-simulations", async (IAIPersonaSimulationService aiPersonaSimulationService, CancellationToken cancellationToken) =>
        {
            var classroomSimulations = await aiPersonaSimulationService.ListClassroomSimulationsAsync(cancellationToken);
            return Results.Ok(classroomSimulations);
        });
        
        // Get specific classroom simulation id
        group.MapGet("/classroom-simulations/{classroomSimulationId:guid}", async (IAIPersonaSimulationService aiPersonaSimulationService, Guid classroomSimulationId, CancellationToken cancellationToken) =>
        {
            var classroomSimulation = await aiPersonaSimulationService.GetClassroomSimulationAsync(classroomSimulationId, cancellationToken);
            return classroomSimulation is null ? Results.NotFound() : Results.Ok(classroomSimulation);
        });
            
        return app;
    }
    

    private static async Task<AIPersonaSimulationResponse> RunPersonaSimulationAsync(AIPersonaSimulationRequest personaSimulationRequest, IAIPersonaSimulationService personaSimulationService, ClaimsPrincipal user, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        return await personaSimulationService.RunAIPersonaCodeEvaluationSimulationAsync(personaSimulationRequest, cancellationToken);
    }
    
    private static async Task<List<AIPersonaSimulationResponse>> RunClassroomSimulationAsync(ClassroomSimulationRequest classroomSimulationRequest, ClaimsPrincipal user, IAIPersonaSimulationService personaSimulationService, CancellationToken cancellationToken)
    {
        
        return await personaSimulationService.RunClassroomSimulationAsync(classroomSimulationRequest, GetUserId(user), cancellationToken);
    }   
    
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
    }
    
    
}