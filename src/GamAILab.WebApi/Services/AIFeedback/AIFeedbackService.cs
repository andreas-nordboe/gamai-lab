using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Services.LLMService;
using Microsoft.EntityFrameworkCore.Storage;
using OllamaSharp.Models.Chat;

namespace GamAILab.WebApi.Services;

public class AIFeedbackService : IAIFeedbackService
{
    private readonly ILLMService _llmService;
    private readonly  ILogger<AIFeedbackService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _llmModelUsed;
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters =  true,
        PropertyNameCaseInsensitive =  true,
        //WriteIndented = true
    };
    
    
    private static readonly JsonNode AIFeedbackSchema = JsonNode.Parse(
        """
          {
            "type": "object",
            "additionalProperties": false,
            "properties": {
              "outcome": {
                "type": "string",
                "enum": [
                  "Incorrect",
                  "Correct",
                  "Partial",
                  "ExecutionError"
                  
                ]
              },
              "explanation": {
                "type": "string",
                "minLength": 1
              },
              "hint": {
                "type": ["string", "null"]
              },
              "evidence": {
                "type": "array",
                "minItems": 1,
                "items":{
                  "type": "string",
                  "minLength": 1
                }
            }
          },
          "required": [
            "outcome",
            "explanation",
            "hint",
            "evidence"
          ]
        }
        """)!;

    public AIFeedbackService(ILLMService llmService, ILogger<AIFeedbackService> logger, IConfiguration configuration)
    {
        _llmService = llmService;
        _logger = logger;
        _llmModelUsed = configuration["Ollama:Model"];
    }

    public async Task<AICodeTaskFeedback> GenerateCodeTaskFeedbackAsync(CodeTask codeTask, CodeSubmission submission,
        AICodeEvaluationPlan codeEvaluationPlan, CodeExecutionResult executionResult,
        CancellationToken cancellationToken = default)
    {
        var timer = new Stopwatch();
        timer.Start();

        var evaluationPlan = JsonSerializer.Serialize(
            new
            {
                codeEvaluationPlan.Criteria,
                codeEvaluationPlan.CommonMistakes,
                codeEvaluationPlan.FeedbackInstructions
            },
            JsonOptions);
        
        var codeEvidence = JsonSerializer.Serialize(
            new
            {
                executionResult.DidComplete,
                executionResult.EveryTestPassed,
                executionResult.ExitCode,
                executionResult.StandardOutput,
                executionResult.StandardError,
                executionResult.FatalError,
                executionResult.CodeTests
            },
            JsonOptions);
        
        // TODO replace hard-coded 'Python' with language from CodeTask 
        string combinedCodePrompt = $$"""
          CODE TASK:
          Title: {{codeTask.Title}} 
          Description: {{codeTask.Description}}
          Constraints: {{codeTask.Constraints}}
          
          LEARNER CODE:
          ```python
            {{submission.Code}}
          ```
          
          CODE EVALUATION PLAN:
          {{evaluationPlan}}
          
          CODE EXECUTION EVIDENCE:
          {{codeEvidence}}
          
          Respond with structure JSON that contains:
          * outcome: Correct, Partial, Incorrect or ExecutionError
          * explanation:
          * hint: 
          * evidence:
          
          Constraints:
          * Do not return or reveal solutions or hidden test inputs in the output.
          * Do not claim that code tests passed unless the evidence clearly says it passed.
          * Rely only on the provided evidence, do NOT invent new test results.
          * Ensure that learner code errors are distinguished from platform execution errors.
          * Ensure explanations are appropriate to the learner's capability
          
          """;
        
        var promptRequest = new ChatRequest
        {
            Model = _llmModelUsed, // TODO possibly make part of task (configurable from the frontend)
            Format = AIFeedbackSchema,
            Stream = false,
            Think =  false,
            KeepAlive = "30m",
            Options = new()
            {
                Temperature = 0
            },
            Messages = new []
            {
                // TODO replace learner traits to parameters (beginner, intermediate..) that can be set either from
                // personas or as learner profile attributes
                new Message(ChatRole.System,
                    """
                        You are to evaluate a beginner learner's Python code submission.
                        Always follow the provided JSON Schema exactly.
                        Do not output text outside the JSON object.
                        Do not use any other formats than JSON.
                        Reason and conclude on the provided execution evidence.
                    """),
                
                new Message(ChatRole.User, combinedCodePrompt)
            } 
        };
        
        var promptResponse = await _llmService.ChatAsync(promptRequest, cancellationToken);
        
        timer.Stop();
        
        // Deserialise feedback (TODO Move to separate function)
        if (string.IsNullOrWhiteSpace(promptResponse))
        {
            throw new InvalidOperationException("LLM returned an empty feedback response");
        }
        
        var feedbackResponse = JsonSerializer.Deserialize<AICodeFeedbackResponse>(promptResponse, JsonOptions) ?? throw new InvalidOperationException("LLM response failed to deserialise");
        
        // Validate prompt TODO cleanup / maybe move to separate function
        if (string.IsNullOrEmpty(feedbackResponse.TaskOutcome))
        {
            throw new InvalidOperationException("LLM response does not containn a task outcome");
        }

        if (string.IsNullOrWhiteSpace(feedbackResponse.Explanation))
        {
            throw new InvalidOperationException("LLM response does not contain an explanation");
        }

        if (feedbackResponse.CodeTaskExecutionEvidence is null ||
            feedbackResponse.CodeTaskExecutionEvidence.Count == 0)
        {
            throw new InvalidOperationException("LLM response does not contain any code execution evidence");
        }

        if (feedbackResponse.CodeTaskExecutionEvidence.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("Either one or more code execution evidences were empty");
        }
        
        // Parse outcome enum
        var codeTaskOutcome = ParseCodeTaskOutcome(feedbackResponse.TaskOutcome);
        if (codeTaskOutcome == CodeTaskOutcome.Correct && !executionResult.EveryTestPassed)
        {
            throw new InvalidOperationException("LLM claims that submission is correct even though all tests did not pass");
        }

        if (codeTaskOutcome == CodeTaskOutcome.Correct && !string.IsNullOrWhiteSpace(executionResult.FatalError))
        {
            throw new InvalidOperationException("LLM claims that submission is correct even though there was a fatal execution error");
        }
        
        if ((codeTaskOutcome == CodeTaskOutcome.Incorrect || codeTaskOutcome == CodeTaskOutcome.Partial) && string.IsNullOrWhiteSpace(feedbackResponse.Hint))
        {
            throw new InvalidOperationException("LLM did not provide any hints for an incorrect submission attempt");
        }
        
        if (codeTaskOutcome == CodeTaskOutcome.ExecutionError && string.IsNullOrWhiteSpace(executionResult.FatalError))
        {
            throw new InvalidOperationException("LLM claims that submission had an execution error without fatal execution evidence");
        }

        var codeTaskFeedback = new AICodeTaskFeedback
        {
            CodeSubmission = submission,
            TaskOutcome = codeTaskOutcome,
            Explanation =  feedbackResponse.Explanation.Trim(),
            HintMessage = TrimHint(feedbackResponse.Hint),
            CodeTaskExecutionEvidence = JsonSerializer.Serialize(feedbackResponse.CodeTaskExecutionEvidence, JsonOptions), //deserialise
            LLMModelUsed = promptRequest.Model,
            CreatedAt = DateTime.UtcNow,
            GeneationTimeInMs =  timer.ElapsedMilliseconds,
        };

        _logger.LogInformation($"AI code feedback for submission {submission.Id} finished after {timer.ElapsedMilliseconds}ms");

        return codeTaskFeedback;
    }

    // Helpers (TODO move to a separate helper class)
    
    private static string? TrimHint(string? hint)
    {
        return string.IsNullOrWhiteSpace(hint) ? null : hint.Trim();
    }

    private static CodeTaskOutcome ParseCodeTaskOutcome(string codeTaskOutcome)
    {
        if (!Enum.TryParse<CodeTaskOutcome>(codeTaskOutcome, true, out var outcome))
        {
            throw new InvalidOperationException($"The outcome is invalid or unsupported: {codeTaskOutcome}");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new InvalidOperationException($"The outcome is not defined: {codeTaskOutcome}");
        }
        
        return outcome;
    }
}