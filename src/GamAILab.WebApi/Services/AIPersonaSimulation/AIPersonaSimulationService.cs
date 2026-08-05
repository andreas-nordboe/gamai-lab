using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.LLMService;
using Microsoft.EntityFrameworkCore;
using OllamaSharp.Models.Chat;

namespace GamAILab.WebApi.Services.AIPersonaSimulation;

public class AIPersonaSimulationService : IAIPersonaSimulationService
{
    private readonly ILLMService _llmService;
    private readonly ICodeSubmissionService _codeSubmissionService;
    private readonly ICodeTaskService _codeTaskService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AIPersonaSimulationService> _logger;
    private readonly string _llmModelUsed;
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters =  true,
        //WriteIndented = true
    };
    
    private static readonly JsonNode CodeAttemptJsonSchema = JsonNode.Parse(
        """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "code": {
              "type": "string",
              "minLength": 1
            }
          },
          "required": ["code"]
        }
        """)!;
    

    public AIPersonaSimulationService(ILLMService llmService, ICodeSubmissionService codeSubmissionService, IConfiguration configuration, ApplicationDbContext dbContext, ILogger<AIPersonaSimulationService> logger, ICodeTaskService codeTaskService)
    {
        _llmService = llmService;
        _codeSubmissionService = codeSubmissionService;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
        _codeTaskService = codeTaskService;
        _llmModelUsed = _configuration["Ollama:Model"];
    }

    async Task<AIPersonaSimulationResponse> IAIPersonaSimulationService.RunAIPersonaCodeEvaluationSimulationAsync(AIPersonaSimulationRequest request, CancellationToken cancellationToken)
    {
        // 1. Verify code task and retrieve it from database persistence
        var codeTask = await _codeTaskService.GetCodeTaskById(request.CodeTaskId);
        if (codeTask is null)
        {
            throw new ArgumentNullException(nameof(codeTask));
        }
        
        var startedAt = DateTime.UtcNow;
        var simulationId = Guid.NewGuid();
        
        List<AIPersona> personaList = [];

        // 2. Loop through AI persona list from request and load their attributes (personas could have their own userIds) 
        
        // 3. Run prompts where AI Personas attempts to solve the tasks
        /*
        foreach (var persona in personaList)
        {
            if (persona is null)
            {
                throw new InvalidOperationException("Persona was not found");
            }
            
            // 4. Give AI personas testing instructions for the specified code task and output their code attempt
            var personaCodeAttempt = await AttemptToSolveCodeTaskAsPersonaAsync(persona, codeTask, cancellationToken);
            
            
            var codeSubmission = new CodeSubmissionRequest
            {
                CodeTaskId = codeTask.Id,
                Code = personaCodeAttempt
            };
            
            // 5. Run code attempt for each persona 
            var codeSubmissionAttempt = await _codeSubmissionService.SubmitCodeAsync(codeSubmission,  persona.UserId, cancellationToken);
            
            // 6. Store learning outcomes and struggles for each persona in the database (should be done automatically via CodeSubmissionService, but it might require adjustments for persona-type users)
            
            
        }*/
        
        var requestedPersonaIds = request.PersonaIds
            .Distinct()
            .ToList();

        var personas = await _dbContext.AIPersonas
            .AsNoTracking()
            .Where(persona => requestedPersonaIds.Contains(persona.Id))
            .ToListAsync(cancellationToken);

        var personaResults = new List<AIPersonaSimulationResult>(personas.Count);

        foreach (var persona in personas.OrderBy(persona => requestedPersonaIds.IndexOf(persona.Id)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var attemptedCode = await AttemptToSolveCodeTaskAsPersonaAsync(
                    persona,
                    codeTask,
                    cancellationToken);

                var submissionRequest = new CodeSubmissionRequest
                {
                    CodeTaskId = codeTask.Id,
                    Code = attemptedCode
                };

                var submissionResult =
                    await _codeSubmissionService.SubmitCodeAsync(
                        submissionRequest,
                        persona.UserId,
                        cancellationToken);

                personaResults.Add(new AIPersonaSimulationResult
                {
                    Persona = persona,
                    CodeAttempt = attemptedCode,
                    SubmissionResult = submissionResult
                });

                _logger.LogInformation(
                    "AI persona {PersonaId} completed simulation {SimulationId} for code task {CodeTaskId}",
                    persona.Id,
                    simulationId,
                    codeTask.Id);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "AI persona {PersonaId} failed simulation {SimulationId} for code task {CodeTaskId}",
                    persona.Id,
                    simulationId,
                    codeTask.Id);

                personaResults.Add(new AIPersonaSimulationResult
                {
                    Persona = persona,
                    ErrorMessage = exception.Message
                });
            }
        }

        
        // 7. (Later) Potentially use LLM model service to reason over where learner personas start to struggle or lose focus  
        
        
        var completedAt = DateTime.UtcNow;

        return new AIPersonaSimulationResponse
        {
            SimulationId = simulationId,
            CodeTaskId = codeTask.Id,
            //CodeTaskTitle = codeTask.Title,
            LlmModelUsed = _llmModelUsed,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            PersonaResults = personaResults
        };
    }

    public async Task CreateAIPersona(AIPersona aiPersona, CancellationToken cancellationToken)
    {
        aiPersona.UserId =  Guid.NewGuid().ToString();
        _dbContext.AIPersonas.Add(aiPersona);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<AIPersona?> GetPersonaById(int personaId)
    {
        return await _dbContext.AIPersonas.FindAsync(personaId);
    }

    public async Task<List<AIPersona>> GetAllAIPersonas()
    {
        return await _dbContext.AIPersonas.ToListAsync();
    }

    public async Task<bool> DeleteAIPersonaById(int personaId)
    {
        var aiPersonaRowsDeleted = await _dbContext.AIPersonas.Where(p => p.Id == personaId).ExecuteDeleteAsync();
        return aiPersonaRowsDeleted > 0;
    }

    public async Task<List<AIPersona>> SeedAIPersonas()
    {
        if (_dbContext.AIPersonas.Any())
            return new(); // return empty list even though it's not being used for anything else than seeding any more
        
        var persona1 = new AIPersona()
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "John Parker",
            Background = "A 18 year old male student that just started studying a Computer Science programme at a UK University that is eager to learn new things",
            AssignedDifficulty = CodeTaskDifficulty.Beginner,
            LearningCapabilities = new List<string>
            {
                "Has zero knowledge with programming but is intellectual capable of operating a computer",
                "Excels in maths"
            },
            LearningDifficulties = [],
            AccessibilityRequirements = [],
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona2 = new AIPersona()
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Lucie Graham",
            Background = "A 21 year old female student at a UK University that has background in design, with interest in game development",
            AssignedDifficulty = CodeTaskDifficulty.Beginner,
            LearningCapabilities = new List<string>
            {
                "Has zero knowledge with programming but is intellectual capable of operating a computer",
                "Excels in maths"
            },
            LearningDifficulties = new List<string>
            {
                "Struggles to stay motivated",
                "Loses focus easily",
                "Has difficulties with comprehending longer task descriptions"
            },
            AccessibilityRequirements = new List<string>
            {
                "Has surface dyslexia"
            },
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.AIPersonas.Add(persona1);
        _dbContext.AIPersonas.Add(persona2);
        await _dbContext.SaveChangesAsync();

        return new List<AIPersona>
        {
            persona1,
            persona2
        };
    }

    public async Task<AIPersona> AddOrUpdatePersona(AIPersona persona)
    {
        if (persona.Id == 0)
        {
            await _dbContext.AIPersonas.AddAsync(persona);
        }
        else
        {
            _dbContext.AIPersonas.Update(persona);
        }
        
        await _dbContext.SaveChangesAsync();
        return persona;
    }

    private async Task<string> AttemptToSolveCodeTaskAsPersonaAsync(AIPersona aiPersona, CodeTask codeTask, CancellationToken cancellationToken)
    {
        var codeTaskJson = JsonSerializer.Serialize(codeTask, JsonOptions);
        var personaJson = JsonSerializer.Serialize(aiPersona, JsonOptions);
        
        var prompt = $$"""
           You are a learner with these characteristics:
           
           {{personaJson}}
           
           Attempt to solve the following code task:
           
           {{codeTaskJson}}
           
           Return only JSON. Do not include any explanations, other formats such as markdown or code fences.
           The JSON output must conform exactly to this schema
           Return only the attempted code solution.
       """; // TODO change prompt to return schema instead of just the code response
        
        var promptRequest = new ChatRequest
        {
            Model = _llmModelUsed,
            Format = CodeAttemptJsonSchema, // TODO specify format similarly to other services
            Stream = false, // I don't think ther0+78e is a need for streaming here
            Think =  false, // I need to experiment with this one
            KeepAlive = "30m", // prevent reloading model when requests are happening concurrently
            Options = new()
            {
                Temperature = 0
            },
            Messages = new []
            {
                new Message(ChatRole.System,
                    """
                        You are to attempt to solve a code task as a learner persona.
                        Treat the persona as it would behave as a human learner.
                        Always follow the provided JSON Schema exactly.
                        Do not output text outside the JSON object.
                        Do not use any other formats than JSON.
                    """),
                
                new Message(ChatRole.User, prompt)
            } 
        };

        // Send request to LLM
        var evaluationResponse = await _llmService.ChatAsync(promptRequest, cancellationToken);

        if (string.IsNullOrWhiteSpace(evaluationResponse))
        {
            throw new InvalidOperationException("AI learner persona returned an empty response");
        }
        
        _logger.LogInformation($"AI persona {aiPersona.Id} attempted to solve code task and returned {evaluationResponse}");

        return evaluationResponse;
    }
}