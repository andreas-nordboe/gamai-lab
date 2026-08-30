using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.DTOs;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.LLMService;
using Microsoft.EntityFrameworkCore;
using OllamaSharp.Models.Chat;

namespace GamAILab.WebApi.Services.CodeTasks;

public class CodeTaskService : ICodeTaskService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CodeTaskService> _logger;
    private readonly IAICodeEvaluationService _aiCodeEvaluationService;
    private readonly ILLMService _llmService;
    private string _llmModelUsed;
    
    public CodeTaskService(ApplicationDbContext dbContext, IAICodeEvaluationService aiCodeEvaluationService, ILogger<CodeTaskService> logger, ILLMService llmService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _aiCodeEvaluationService = aiCodeEvaluationService;
        _logger = logger;
        _llmService = llmService;
        _llmModelUsed = configuration["Ollama:Model"];
    }
    
    public async Task AddCodeTask(CodeTask codeTask, bool generateEvaluationPlan = true)
    {
        if (generateEvaluationPlan)
        {
            // Generate code evaluation plan
            try
            {
                var codeEvaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(codeTask);

                if (codeEvaluationPlan is null)
                {
                    throw new ArgumentNullException(nameof(codeEvaluationPlan));
                }
            
                codeTask.AiCodeEvaluationPlan = codeEvaluationPlan;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to generate evaluation plan for code task", e);
            }
        }
        
        codeTask.CreatedAt = DateTime.Now;
        
        _dbContext.CodeTasks.Add(codeTask);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<CodeTask?> GetCodeTaskById(int codeTaskId)
    {
        return await _dbContext.CodeTasks
            .Include(x => x.AiCodeEvaluationPlan)
            .FirstOrDefaultAsync(x => x.Id == codeTaskId);
    }

    public async Task<List<CodeTask>> GetAllCodeTasks()
    {
        return await _dbContext.CodeTasks.ToListAsync();
    }

    public async Task<bool> DeleteCodeTaskById(int codeTaskId)
    {
        var deleteCodeTaskRowsDeleted = await _dbContext.CodeTasks.Where(p => p.Id == codeTaskId).ExecuteDeleteAsync();
        return deleteCodeTaskRowsDeleted > 0;
    }

    public async Task<bool> DoesTaskExist(int codeTaskId)
    {
        return await _dbContext.CodeTasks.FindAsync(codeTaskId) is not null;
    }
    
    // TODO These are currently hard-coded but I will create a 
    // spreadsheet import system or bake task CRUD operations into
    // the educator monitoring/management system
    // Ideas: Repair robot, calibrate sensors, sort lab equipment, analyse an evil AI researcher stealing literature

    public async Task<List<CodeTask>> SeedCodeTasks()
    {
        try
        {
            var json = await File.ReadAllTextAsync("SeedAppData/CodeTasks/code-tasks.json");

            var codeTasks = JsonSerializer.Deserialize<List<CodeTask>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];

            foreach (var codeTask in codeTasks)
            {
                if (await _dbContext.CodeTasks.AnyAsync(task => task.Title == codeTask.Title))
                {
                    continue;
                }

                await AddCodeTask(codeTask, false);
            }

            return codeTasks;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to seed code tasks, either files are missing or Docker can't load them from json");
            return [];
        }
    }

    public async Task<CodeTask> AddOrUpdateCodeTask(CodeTask codeTask)
    {
        if (codeTask.Id == 0)
        {
            await _dbContext.CodeTasks.AddAsync(codeTask);
        }
        else
        {
            codeTask.Version++;
            _dbContext.CodeTasks.Update(codeTask);
        }
        
        await _dbContext.SaveChangesAsync();
        return codeTask;
    }

    public async Task<CodeTask?> ReGenerateCodeEvaluationPlanAsync(int codeTaskId)
    {
        var existingCodeTask = await GetCodeTaskById(codeTaskId);
        
        // Verify that the code task exists
        if (existingCodeTask is null)
        {
            throw new InvalidOperationException("Code task was not found while re-generating evaluation plan");
        }
        
        var newCodeEvaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(existingCodeTask);
        existingCodeTask.AiCodeEvaluationPlan = newCodeEvaluationPlan;

        // Increment code task version for auditability 
        // as different code evaluation plans can easily skew measured outputs
        // and this should ensure consistency across the final report analysis
        // TODO (remember to mention this as an argument in the report)
        existingCodeTask.Version++;
        
        _dbContext.CodeTasks.Update(existingCodeTask);
        await _dbContext.SaveChangesAsync();
        
        return existingCodeTask;
    }
    
    private static readonly JsonNode CodeTaskJsonSchema = JsonNode.Parse(
        """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "title": {
              "type": "string"
            },
            "description": {
              "type": "string"
            },
            "defaultCode": {
              "type": "string"
            },
            "examples": {
              "type": "array",
              "items": {
                "type": "string"
              }
            },
            "constraints": {
              "type": "array",
              "items": {
                "type": "string"
              }
            },
            "difficulty": {
              "type": "integer",
              "minimum": 0,
              "maximum": 2
            },
            "currencyReward": {
              "type": "integer",
              "minimum": 1
            }
          },
          "required": [
            "title",
            "description",
            "defaultCode",
            "examples",
            "constraints",
            "difficulty",
            "currencyReward"
          ]
        }
        """)!;

    public async Task<CodeTask?> GenerateCodeTaskAsync(GenerateCodeTaskRequest generateCodeTaskRequest, CancellationToken cancellationToken = default)
    {
        string codeTaskDescription = string.IsNullOrWhiteSpace(generateCodeTaskRequest.Description) ? "Generate a random programming task using the Python programming language." : generateCodeTaskRequest.Description;
        string gameStory = string.IsNullOrWhiteSpace(generateCodeTaskRequest.Description) ? "Game AI Lab is robot laboratory environment where autonomous robots conduct research and experiments. This sci-fi-themed lab has various lab equipment, including advanced technical computer systems, futuristic XR headsets, and engineering tools/components . Create a code task where the player needs to solve a task in GamAI Lab." : generateCodeTaskRequest.GameStory;
        
        // TODO Python is hard-coded here but I'm mentioning other languages in the report
        var prompt = $$"""
           Generate a programming task using Python.
           The programming task is aimed at a learner at an {{generateCodeTaskRequest.CodeTaskDifficulty.ToString()}}.
           
           CODE TASK GUIDANCE:
           {{codeTaskDescription}}
           
           GAME CONTEXT:
           {{gameStory}}
           
           Create a programming task that aligns with this game context.
           The description must include what the player needs to solve with a short reason that explains why it fits with the game context.
           
           Task output MUST include:
           - a short title
           - a detailed description
           - default code (this can be a brief guidance and MUST NOT reveal or contain the solution)
           - useful examples
           - the same difficulty that was provided
           - a random currency reward that aligns with the educational difficulty, with a maximum deviation of 15 from the default point allocation
           
           Default point allocation:
           - Beginner = 5
           - Intermediate = 10 
           - Advanced = 20
           - Expert = 30
           
           GUIDELINES:
           - The task must be testable to assess learners fail, partial and success outcomes 
           - Requirements must be clearly state what the learner must do (for example: print, what a function should return, which parameters to use)
           - Align the task to the requested difficulty.
           - Default code may also include comments, partially incomplete functions and variables
           - Examples must be suitable with the description and constraints.
           - Constraints must be specific as they are later used to generate automated tests.
           - The game story should be relevant to the task but not override the learning objective from the task itself
           - You are allowed to be creative and whimsical with the game story setting.
           - The JSON output must conform exactly to this schema.
           - Do not include UserId, timestamps or other metadata.
       """;
        
        var promptRequest = new ChatRequest
        {
            Model = _llmModelUsed,
            Format = CodeTaskJsonSchema, 
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
                        You generate a programming tasks for GamAI Lab, an educational game that teaches learners the Python programming language.
                        - Tasks must be clearly explained, fully solvable, realistic and testable. 
                        - Do not output text or markdown outside the JSON object.
                        - Do not use any other formats than JSON.
                    """),
                
                new Message(ChatRole.User, prompt)
            } 
        };

        // Send request to LLM
        var generatedCodeTask = await _llmService.ChatAsync(promptRequest, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(generatedCodeTask))
        {
            throw new InvalidOperationException("Generating code task returned an empty response");
        }
        
        var codeTask = JsonSerializer.Deserialize<CodeTask>(generatedCodeTask, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (codeTask is null)
        {
            throw new InvalidOperationException("Generated code task is invalid");
        }
        
        // Generate plan as well
        var codeEvaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(codeTask);
        codeTask.AiCodeEvaluationPlan = codeEvaluationPlan;
        
        _logger.LogInformation("Raw generated AI evaluation plan: {GeneratedPlan}", codeEvaluationPlan);
        
        codeTask.CreatedAt = DateTime.Now;
        codeTask.Version = 1;

        return codeTask;
    }
}