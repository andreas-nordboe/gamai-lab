using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.WebApi.Services.LLMService;
using System.Text.Json.Schema;
using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization.Metadata;

namespace GamAILab.WebApi.Services;

public class AICodeEvaluationService : IAICodeEvaluationService
{
    private readonly ILLMService _llmService;
    private readonly  ILogger<AICodeEvaluationService> _logger;
    
    // https://learn.microsoft.com/en-us/dotnet/api/system.text.json.schema.jsonschemaexporter.getjsonschemaasnode?view=net-11.0-pp
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };
    private static readonly JsonNode EvaluationPlanSchema = JsonOptions.GetJsonSchemaAsNode(typeof(AICodeEvaluationPlan));

    public AICodeEvaluationService(ILLMService llmService, ILogger<AICodeEvaluationService> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<AICodeEvaluationPlan> GenerateEvaluationPlanAsync(CodeTask codeTaskContext, CancellationToken cancellationToken = default)
    {
        var codeTaskJson = JsonSerializer.Serialize(codeTaskContext);
        var schemaJson = EvaluationPlanSchema.ToJsonString();

        // TODO Hard-coded for now, this will be moved into CodeTask later (as I want the platform to be fully extensible) 
        var codeLanguage = "Python";

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
            Model = "gemma4",
            Format = "json", // TODO use my own JSON schema instead
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

        var responseBuilder = new StringBuilder();

        var evaluationResponse = await _llmService.ChatAsync(promptRequest, cancellationToken);

        if (string.IsNullOrWhiteSpace(evaluationResponse))
        {
            throw new InvalidOperationException("LLM returned an empty evaluation plan.");
        }

        try
        {
            var evaluationPlan = JsonSerializer.Deserialize<AICodeEvaluationPlan>(evaluationResponse, JsonOptions);
            return evaluationPlan ?? throw new InvalidOperationException("The LLM code evaluation plan was deserialised as null.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Could not deserialise LLM code evaluation plan. Response: {evaluationResponse}");
            throw new InvalidOperationException("LLM returned an empty invalid code evaluation plan.");
        }
    }
}