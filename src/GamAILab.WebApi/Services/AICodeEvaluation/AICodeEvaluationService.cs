using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.WebApi.Services.LLMService;
using System.Text.Json.Serialization;
using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization.Metadata;

namespace GamAILab.WebApi.Services;

public class AICodeEvaluationService : IAICodeEvaluationService
{
    private readonly ILLMService _llmService;
    private readonly  ILogger<AICodeEvaluationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _llmModelUsed;
    
    // https://learn.microsoft.com/en-us/dotnet/api/system.text.json.schema.jsonschemaexporter.getjsonschemaasnode?view=net-11.0-pp
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters =  true,
        //WriteIndented = true
    };
    
    
    // TODO saving this automated schema generation for later
    //private static readonly JsonNode EvaluationPlanSchema = JsonOptions.GetJsonSchemaAsNode(typeof(AICodeEvaluationPlan), SchemaOptions);
    
    // This manual schema mapping seems to generate more reliable responses, although the model may change over time and would therefore need to be updated   
    private static readonly JsonNode EvaluationPlanSchema = JsonNode.Parse(
        """
          {
            "type": "object",
            "additionalProperties": false,
            "properties": {
              "criteria": {
                "type": "array",
                "items": {
                  "type": "string"
                }
              },
              "commonMistakes": {
                "type": "array",
                "items": {
                  "type": "string"
                }
              },
              "feedbackInstructions": {
                "type": "string"
              },
              "language": {
                "type": "string"
              },
              "tests": {
                "type": "array",
                "items": {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "functionName": {
                      "type": "string"
                    },
                    "arguments": {
                     "type": "array",
                     "items": {
                        "type": "integer"
                     }
                   },
                    "expectedResult": {
                      "type": "integer"
                    }
                  },
                  "required": [
                    "name",
                    "functionName",
                    "arguments",
                    "expectedResult"
                  ]
                }
              }
            },
            "required": [
              "criteria",
              "commonMistakes",
              "feedbackInstructions",
              "language",
              "tests"
            ]
          }
        """)!;
    
    public AICodeEvaluationService(ILLMService llmService, ILogger<AICodeEvaluationService> logger, IConfiguration configuration)
    {
        _llmService = llmService;
        _logger = logger;
        _configuration = configuration;
        _llmModelUsed = _configuration["Ollama:Model"];
    }

    public async Task<AICodeEvaluationPlan> GenerateEvaluationPlanAsync(CodeTask codeTaskContext, CancellationToken cancellationToken = default)
    {
        // TODO Hard-coded for now, this will be moved into CodeTask later (as I want the platform to be fully extensible) 
        var codeLanguage = "Python";
        
        // Timers
        var initiatedAt = DateTimeOffset.UtcNow;
        var startTime = Stopwatch.GetTimestamp();
        
        var codeTaskJson = JsonSerializer.Serialize(codeTaskContext, JsonOptions);
        var schemaJson = EvaluationPlanSchema.ToJsonString(JsonOptions);

        // TODO I might need to be more specific about code fences as it could potentially break output later (as LLMs tend to prefer .md responses)
        var prompt = $$"""
            Generate an evaluation plan for the included programming task.
            
            The evaluation plan will be used to evaluate code in an isolated {{codeLanguage}} execution environment.
            
            Return only JSON. Do not include any explanations, other formats such as markdown or code fences.
            
            The JSON output must conform exactly to this schema
            
            {{schemaJson}}

            Programming task:
            
            {{codeTaskJson}}
        """;

        var promptRequest = new ChatRequest
        {
            Model = _llmModelUsed,
            Format = EvaluationPlanSchema,
            Stream = false, // I don't think there is a need for streaming here
            Think =  false, // I need to experiment with this one
            KeepAlive = "30m", // prevent reloading model when requests are happening concurrently
            Options = new()
            {
                // TODO experiment for more reliable responses (0 is more deterministic)
                Temperature = 0
            },
            Messages = new []
            {
                new Message(ChatRole.System,
                """
                    You are to generate machine-readable code evaluation plans.
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
            throw new InvalidOperationException("LLM returned an empty evaluation plan.");
        }

        AICodeEvaluationPlanOutput evaluationPlanOutput;

        // Deserialise LLM response
        try
        {
            evaluationPlanOutput = JsonSerializer.Deserialize<AICodeEvaluationPlanOutput>(evaluationResponse, JsonOptions)
                ?? throw new JsonException("The LLM code evaluation plan was deserialised as null.");
            
            
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Could not deserialise LLM code evaluation plan. Response: {evaluationResponse}", evaluationResponse);
            throw new InvalidOperationException("LLM returned an invalid JSON evaluation plan", e);
        }
        
        // Validate and throw errors if there are any
        ValidateAICodeEvaluationOutput(evaluationPlanOutput, codeLanguage);
        
        // Map the plan to internal class/database entity
        return new AICodeEvaluationPlan
        {
            Id = Guid.NewGuid().ToString("N"),
            CodeTask = codeTaskContext,
            Criteria = evaluationPlanOutput.Criteria,
            CommonMistakes = evaluationPlanOutput.CommonMistakes,
            FeedbackInstructions = evaluationPlanOutput.FeedbackInstructions,
            Language = codeLanguage,
            Tests = evaluationPlanOutput.Tests,
            ModelUsed = _llmModelUsed, // TODO get from appsettings (eventually move to options)
            InitiatedAt = initiatedAt,
            PlanningDuration = Stopwatch.GetElapsedTime(startTime)
        };
    }

    // TODO This logic could potentially be written into a 
    private static void ValidateAICodeEvaluationOutput(AICodeEvaluationPlanOutput output, string codeLanguage)
    {
        if(output.Criteria.Count == 0 || output.Criteria.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("No evaluation criteria was provided");
        }
        
        if(output.CommonMistakes.Count == 0 || output.CommonMistakes.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("The output criteria contains an empty common mistake");
        }
        
        if (string.IsNullOrWhiteSpace(output.FeedbackInstructions))
        {
            throw new InvalidOperationException("The evaluation does not have any feedback instructions");
        }

        if (string.IsNullOrWhiteSpace(output.Language))
        {
            throw new InvalidOperationException("The evaluation does not have a programming language");
        }

        if (!string.Equals(output.Language, codeLanguage, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"The evaluation plan returned {output.Language} but expected {codeLanguage}");
        }
        
        if(output.Tests.Count == 0)
        {
            throw new InvalidOperationException("No code evaluation plan tests were provided");
        }

        foreach (var test in output.Tests)
        {
            if (string.IsNullOrWhiteSpace(test.Name))
            {
                throw new InvalidOperationException("A code evaluation test has no name");
            }
            
            if (string.IsNullOrWhiteSpace(test.FunctionName))
            {
                throw new InvalidOperationException($"Test '{test.Name}' has no input");
            }

            if (test.ExpectedResult.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                throw new InvalidOperationException($"Test '{test.Name}' has no expected output");
            }
        }
        
    }
}