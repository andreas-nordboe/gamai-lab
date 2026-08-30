using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.EducatorMonitoring;
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
    private readonly IEducatorMonitoringService _educatorMonitoringService;
    private readonly string _llmModelUsed;
    
    public AIPersonaSimulationService(ILLMService llmService, 
        ICodeSubmissionService codeSubmissionService, 
        IConfiguration configuration, 
        ApplicationDbContext dbContext, 
        ILogger<AIPersonaSimulationService> logger, 
        ICodeTaskService codeTaskService, IEducatorMonitoringService educatorMonitoringService)
    {
        _llmService = llmService;
        _codeSubmissionService = codeSubmissionService;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
        _codeTaskService = codeTaskService;
        _educatorMonitoringService = educatorMonitoringService;
        _llmModelUsed = _configuration["Ollama:Model"];
    }
    
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
            },
            "struggles": {
              "type": "array",
              "items": {
                "type": "string"
              }
            },
            "learningOutcomes": {
              "type": "array",
              "items": {
                "type": "string"
              }
            },
            "engagementScore": {
              "type": "integer",
              "minimum": 0,
              "maximum": 100
            }
          },
          "required": ["code", "struggles", "learningOutcomes", "engagementScore"]
        }
        """)!;
    
    private static readonly JsonNode PersonaJsonSchema = JsonNode.Parse(
        """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "name": {
              "type": "string"
            },
            "background": {
              "type": "string"
            },
            "assignedDifficulty": {
              "type": "integer",
              "minimum": 0,
              "maximum": 2
            },
            "learningCapabilities": {
              "type": "array",
              "items": {
                "type": "string"
              }
            },
            "learningDifficulties": {
              "type": "array",
              "items": {
                "type": "string"
              }
            },
            "accessibilityRequirements": {
              "type": "array",
              "items": {
                "type": "string"
              }
            }
          },
          "required": [
            "name",
            "background",
            "assignedDifficulty",
            "learningCapabilities",
            "learningDifficulties",
            "accessibilityRequirements"
          ]
        }
        """)!;

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
        
        var requestedPersonaIds = request.PersonaIds
            .Distinct()
            .ToList();

        // 2. Loop through AI persona list from request and load their attributes (personas could have their own userIds)
        // using single db query
        var personas = await _dbContext.AIPersonas
            .Where(persona => requestedPersonaIds.Contains(persona.Id))
            .ToListAsync(cancellationToken);

        var personaResults = new List<AIPersonaSimulationResult>(personas.Count);

        foreach (var persona in personas.OrderBy(persona => requestedPersonaIds.IndexOf(persona.Id)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // 3. Run prompts where AI Personas attempts to solve the tasks
                // 4. Give AI personas testing instructions for the specified code task and output their code attempt
                // 5. Run code attempt for each persona 
                
                // Gets memory state
                AIPersonaMemoryState? memoryState = null;
                if (request.PersonaStates is not null)
                {
                    request.PersonaStates.TryGetValue(persona.Id, out memoryState);
                }
                
                var attemptedCode = await AttemptToSolveCodeTaskAsPersonaAsync(persona, codeTask, memoryState, cancellationToken);
                
                _logger.LogInformation($"AI Persona code: {attemptedCode}");
                
                // Replace newlines with spaces

                var submissionRequest = new CodeSubmissionRequest
                {
                    CodeTaskId = codeTask.Id,
                    CodeAttempt = attemptedCode.Code
                };

                var submissionResult = await _codeSubmissionService.SubmitCodeAsync(submissionRequest, persona.UserId, updateGameProgress: false, cancellationToken);

                personaResults.Add(new AIPersonaSimulationResult
                {
                    Persona = persona,
                    CodeAttempt = attemptedCode,
                    SubmissionResult = submissionResult,
                    LearningOutcomes = attemptedCode.LearningOutcomes,
                    Struggles = attemptedCode.Struggles,
                    EngagementScore = attemptedCode.EngagementScore,
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
        
        var completedAt = DateTime.UtcNow;
        
        var simulationResult = new AIPersonaSimulationResponse
        {
            PersonasUsed = personas,
            SimulationId = simulationId,
            CodeTaskId = codeTask.Id,
            CodeTaskTitle = codeTask.Title,
            LlmModelUsed = _llmModelUsed,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            PersonaResults = personaResults,
            ClassroomSimulationId = request.ClassroomSimulationId,
            SimulationTimeStepIndex = request.SimulationTimeStepIndex,
            SimulatedMinute = request.SimulatedMinute,
            AttemptNumber = request.AttemptNumber,
        };
        
        // 6. Store learning outcomes and struggles for each persona in the database (should be done automatically via CodeSubmissionService, but it might require adjustments for persona-type users)
        _dbContext.AIPersonaSimulations.Add(simulationResult);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // 7. (Later) Potentially use LLM model service to reason over where learner personas start to struggle or lose focus  
        // TODO also: ensure personas respond with struggles and learning outcomes (using a single/the same prompt?)
        
        return simulationResult;
    }

    public async Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync(CancellationToken cancellationToken = default)
    {
        var classroomSimulations = await _dbContext.ClassroomSimulations
        .Include(x => x.SimulationResponses).ThenInclude(x => x.PersonaResults).ThenInclude(x => x.Persona)
        .OrderByDescending(x => x.StartedAt)
        .ToListAsync(cancellationToken);

        return classroomSimulations;
    }

    public async Task<ClassroomSimulation?> GetClassroomSimulationAsync(Guid classroomSimulationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClassroomSimulations
        .Include(x => x.SimulationResponses).ThenInclude(x => x.PersonaResults).ThenInclude(x => x.Persona)
        .FirstOrDefaultAsync(x => x.Id == classroomSimulationId, cancellationToken);
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
        try
        {
            if (_dbContext.AIPersonas.Any())
                return
                    new(); // return empty list even though it's not being used for anything else than seeding any more

            var persona1 = new AIPersona()
            {
                UserId = Guid.NewGuid().ToString(),
                Name = "John Parker",
                Background =
                    "A young student that just started studying at an undergraduate Computer Science programme at the University of Lincoln (UK), that is eager to learn new things",
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
                Background =
                    "A 21 year old female student at a UK University that has background in design, with interest in game development",
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
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to seed AI personas");
            return [];
        }
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

    public async Task<AIPersona> GenerateAIPersona(string aiPersonaDescription, CancellationToken cancellationToken = default)
    {
        string aiPersonaInfo = string.IsNullOrWhiteSpace(aiPersonaDescription) ? "Generate a realistic varied learner at a UK University studying a Computer Science Programme" : aiPersonaDescription;
        
        var prompt = $$"""
           Generate a realistic learner persona that will be used for educational programming task simulations.
           The student will be a learner at a Computer Science programme at a UK University.
           
           Guidance on persona description:
           {{aiPersonaInfo}}
           
           Persona output MUST include:
           - name
           - background
           - assigned difficulty (beginner, intermediate, advanced)
           - learning capabilities
           - learning difficulties
           - accessibility requirements
           
           GUIDELINES:
           The JSON output must conform exactly to this schema.
           Do not include UserId, timestamps or other metadata.
       """;
        
        var promptRequest = new ChatRequest
        {
            Model = _llmModelUsed,
            Format = PersonaJsonSchema, 
            Stream = false, 
            Think =  false, 
            KeepAlive = "30m",
            Options = new()
            {
                Temperature = 0.7f // some creativity will be good for these I think, as they require less deterministic responses than task evaluations
                                   // (0.8 is library default, but I think 0.7 is better)
            },
            Messages = new []
            {
                new Message(ChatRole.System,
                    """
                        You will generate a realistic learner persona that will be used for educational programming task simulations.
                        - Always follow the provided JSON Schema exactly.
                        - Do not output text or markdown outside the JSON object.
                        - Do not use any other formats than JSON.
                    """),
                
                new Message(ChatRole.User, prompt)
            } 
        };

        // Send request to LLM
        var generatedAIPersona = await _llmService.ChatAsync(promptRequest, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(generatedAIPersona))
        {
            throw new InvalidOperationException("Generating AI persona returned an empty response");
        }
        
        var aiPersona = JsonSerializer.Deserialize<AIPersona>(generatedAIPersona, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (aiPersona is null)
        {
            throw new InvalidOperationException("Generated AI persona is invalid");
        }

        return aiPersona;
    }

    private async Task<AIPersonaCodeResponse> AttemptToSolveCodeTaskAsPersonaAsync(AIPersona aiPersona, CodeTask codeTask, AIPersonaMemoryState? memoryState, CancellationToken cancellationToken)
    {
        var codeTaskJson = JsonSerializer.Serialize(codeTask, JsonOptions);
        var personaJson = JsonSerializer.Serialize(aiPersona, JsonOptions);
        var memoryStateJsonFormatted = memoryState is null ? "There is no previous classroom activity. Treat this this as first attempt." : JsonSerializer.Serialize(memoryState, JsonOptions);
        
        var prompt = $$"""
           You are a learner with these CHARACTERISTICS:
           {{personaJson}}
           
           Attempt to solve the following CODE TASK:
           {{codeTaskJson}}
           
           Your previous classroom activity:
           {{memoryStateJsonFormatted}}
           
           Return only the specified JSON schema:
           - the attempted code solution
           - learning outcomes you gained from the attempt
           - struggles you experienced during the attempt
           - an engagement score (0-100) that represents the persona's characteristics including motivation, focus, learning engagement, background, capabilities, learning difficulties, accessibility requirements, etc. 

           GUIDELINES:
           - Consider both the persona's characteristics and the previous classroom activity during the attempt.
           - Your engagement may increase or decrease as you continue to practice.
           - Previous struggles may affect the current attempt and learning outcomes may change the learners ability to complete the task.
           - Base struggles, engagement score and learning outcomes based on the persona characteristics and this task attempt.
           - Do not include any explanations, other formats such as markdown or code fences.
           - The JSON output must conform exactly to this schema
       """; // TODO change prompt to return schema instead of just the code response
        
        var promptRequest = new ChatRequest
        {
            Model = _llmModelUsed,
            Format = CodeAttemptJsonSchema, 
            Stream = false, // I don't think there is a need for streaming here
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
                        You will attempt to solve a code task as a learner persona.
                        Act and behave as a real human learner.
                        
                        Always follow the provided JSON Schema exactly.
                        Do not output text outside the JSON object.
                        The code will be executed so do not return newLine characters that could result in execution errors.
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
        
        var codeResponse = JsonSerializer.Deserialize<AIPersonaCodeResponse>(evaluationResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (codeResponse is null || string.IsNullOrEmpty(codeResponse.Code))
        {
            throw new InvalidOperationException("AI persona code response is invalid");
        }
        
        _logger.LogInformation($"AI persona {aiPersona.Name} attempted to solve code task and returned {evaluationResponse}");

        return codeResponse;
    }
    
    public async Task<List<AIPersonaSimulationResponse>> RunClassroomSimulationAsync(ClassroomSimulationRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var classroomSimulation = new ClassroomSimulation
        {
            InitiatedByUserId = userId,
            Status = ClassroomSimulationStatus.Running,
            StartedAt = DateTime.UtcNow,
            LearnerCount = request.PersonaIds.Count
        };

        _dbContext.ClassroomSimulations.Add(classroomSimulation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _educatorMonitoringService.PublishClassroomSimulationStartedAsync(classroomSimulation, cancellationToken);;
        
        try
        {
            //var classroomSessionId = Guid.NewGuid(); // I could potentially use int instead for EF Core but this would be easy to lookup too
            var simulationResponses = new List<AIPersonaSimulationResponse>();

            // Cache works for now, but these could potentially be stored in a database table
            var personaStates = new Dictionary<int, AIPersonaMemoryState>();

            var currentTimeStep = 0;
            
            // Runs the function that actually runs the simulation
            for (var taskIndex = 0; taskIndex < request.CodeTaskIds.Count; taskIndex++)
            {
                var codeTaskId = request.CodeTaskIds[taskIndex];
                var personasToAttemt = request.PersonaIds.Distinct().ToList();

                for (var attempt = 0; attempt <= request.MaxRetriesPerTask && personasToAttemt.Count > 0; attempt++)
                {
                    var simulationRequest = new AIPersonaSimulationRequest
                    {
                        ClassroomSimulationId = classroomSimulation.Id, // it's easier to keep track of this after client generates it, although the client could also send untrusted Ids (consideration for later)
                        ExecutionCounts = 1,
                        CodeTaskId = codeTaskId,
                        PersonaIds = personasToAttemt,
                        SimulationTimeStepIndex = currentTimeStep,
                        SimulatedMinute = currentTimeStep * request.MinutesEveryStep,
                        PersonaStates = personaStates,
                        AttemptNumber = attempt + 1
                    };
                    
                    var aiPersonaSimulationAttempt = await ((IAIPersonaSimulationService)this).RunAIPersonaCodeEvaluationSimulationAsync(simulationRequest, cancellationToken);
                    simulationResponses.Add(aiPersonaSimulationAttempt);
                    classroomSimulation.SimulationResponses.Add(aiPersonaSimulationAttempt);
                    
                    var failedPersonas = new List<int>();
                    
                    
                    foreach (var personaResult in aiPersonaSimulationAttempt.PersonaResults)
                    {
                        var personaId = personaResult.Persona.Id;
                        var taskOutcome = personaResult.SubmissionResult?.AIFeedback?.TaskOutcome;
                        
                        personaStates[personaResult.Persona.Id] = new AIPersonaMemoryState()
                        {
                            PreviousEngagementScore = personaResult.EngagementScore,
                            PreviousStruggles = personaResult.Struggles,
                            PreviousLearningOutcomes = personaResult.LearningOutcomes,
                            PreviousTaskOutcome = personaResult.SubmissionResult?.AIFeedback?.TaskOutcome.ToString()
                        };

                        var update = new LearnerEngagementLiveUpdate()
                        {
                            ClassroomSimulationId = classroomSimulation.Id,
                            PersonaId = personaResult.Persona.Id,
                            PersonaName = personaResult.Persona.Name,
                            EngagementScore = personaResult.EngagementScore,
                            SimulatedMinute = simulationRequest.SimulatedMinute,
                            Struggles = personaResult.Struggles,
                            LearningOutcomes = personaResult.LearningOutcomes
                        };

                        var passedTask = taskOutcome == CodeTaskOutcome.Correct;
                        if (!passedTask)
                        {
                            failedPersonas.Add(personaId);
                        }

                        await _educatorMonitoringService.PublishLearnerEngagementUpdateAsync(update, cancellationToken);
                    }

                    personasToAttemt = failedPersonas;
                    currentTimeStep++;

                }
                
            }
            
            // persistence for UI dashboard
            classroomSimulation.Status = ClassroomSimulationStatus.Completed;
            classroomSimulation.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return simulationResponses;
        }
        catch (Exception e)
        {
            classroomSimulation.Status = ClassroomSimulationStatus.Failed;
            classroomSimulation.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        
    }
}