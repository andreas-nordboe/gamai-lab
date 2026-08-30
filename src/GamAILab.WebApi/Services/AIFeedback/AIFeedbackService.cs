using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AICodeEvaluation.Hints;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeExecution;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.LLMService;
using Microsoft.EntityFrameworkCore.Storage;
using OllamaSharp.Models.Chat;

namespace GamAILab.WebApi.Services;

public class AIFeedbackService : IAIFeedbackService
{
    private readonly ILLMService _llmService;
    private readonly ICodeTaskService _codeTaskService;
    private readonly  ILogger<AIFeedbackService> _logger;
    private readonly ICodeExecutionService _codeExecutionService;
    private readonly string _llmModelUsed;
    private readonly ApplicationDbContext _dbContext;
    
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
    
    private static readonly JsonNode AICodeHintSchema = JsonNode.Parse(
        """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "message": {
              "type": "string",
              "minLength": 1
            },
            "hintLevel": {
              "type": "integer",
              "minimum": 1,
              "maximum": 3
            }
          },
          "required": [
            "message",
            "hintLevel"
          ]
        }
        """)!;

    public AIFeedbackService(ILLMService llmService, ILogger<AIFeedbackService> logger, IConfiguration configuration, ApplicationDbContext dbContext, ICodeTaskService codeTaskService, ICodeExecutionService codeExecutionService)
    {
        _llmService = llmService;
        _logger = logger;
        _dbContext = dbContext;
        _codeTaskService = codeTaskService;
        _codeExecutionService = codeExecutionService;
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
          PROGRAMMING TASK:
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
        
        _dbContext.AICodeTaskFeedbacks.Add(codeTaskFeedback);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"AI code feedback for submission {submission.Id} finished after {timer.ElapsedMilliseconds}ms");

        return codeTaskFeedback;
    }

    public async Task<AICodeHintResponse> GenerateCodeHintAsync(AICodeHintRequest aiCodeHintRequest, string userId, CancellationToken cancellationToken = default)
    {
        var codeTask = await _codeTaskService.GetCodeTaskById(aiCodeHintRequest.CodeTaskId);

        if (codeTask is null)
        {
            throw new InvalidOperationException("Failed to load code task");
        }

        if (codeTask.AiCodeEvaluationPlan is null)
        {
            throw new InvalidOperationException("Code task does not have an AI evaluation plan");
        }
        
        // Running this again will definately increase the time for the request to complete, but it ensures that code execution evidence is recent, runs authoritatively on the server and has higher accuracy
        var codeExecution = await _codeExecutionService.ExecuteCodeAsync(aiCodeHintRequest.LearnerCode, codeTask.AiCodeEvaluationPlan, cancellationToken);

        var executionEvidence = JsonSerializer.Serialize(new
        {
            codeExecution.DidComplete,
            codeExecution.TimedOut,
            codeExecution.ExitCode,
            codeExecution.StandardOutput,
            codeExecution.StandardError,
            codeExecution.FatalError,
            codeExecution.EveryTestPassed
        });

        // add them together and calculate a hint level
        var conversation = string.Join("\n", aiCodeHintRequest.ChatLogs.Select(x => $"{x.ChatLogRole}: {x.Content}"));
        var hintLevel = CalculateHintLevel(aiCodeHintRequest.ChatLogs);

        var prompt = $$"""
           You are an AI assistant that aids a learner to complete the following programming task:

           PROGRAMMING TASK TITLE:
           {{codeTask.Title}}

           PROGRAMMING TASK DESCRIPTION:
           {{codeTask.Description}}

           PROGRAMMING TASK REQUIREMENTS:
           {{string.Join("\n", codeTask.Constraints ?? [])}}

           CURRENT LEARNER CODE:
           ```python
           {{aiCodeHintRequest.LearnerCode}}
           ```
           
           LEARNER QUESTION:
           {{aiCodeHintRequest.Question}}

           LAST EXECUTION OUTCOME:
           {{executionEvidence}}

           LAST CONVERSATION:
           {{conversation}}
           

           RULES YOU MUST FOLLOW EXPLICITLY:
           - Help the learner so they can understand the problem.
           - Do NOT provide the complete final solution.
           - Prioritise explanations, questions and small hints.
           - Do NOT rewrite the entire existing code as written by the learner.
           - YOU may provide very small code snippets when necessary.
           - Do NOT invent any additional requirements, description information or test cases that is not part of the PROGRAMMING TASK.
           - Do not invent any execution results.
           - When code evidence is provided, treat it as authoritative and source of truth.
           - Do NOT assume programming task description, requirements, expected outputs, or learner mistakes based on previous knowledge.
           - If the learner asks for the complete solution or complete answers, provide them with guidance or hints instead.
           - Respond only with concise answers, that is focsed on the code task, without additional jargon.
           
           CURRENT HINT LEVEL:
           {{hintLevel}}
           
           HINT LEVEL GUIDANCE:
           Level 1: Give an answer that provides pedagogical programming task context. 
           Level 2: Provide more specific guidance that directs the learner more on what to do.
           Level 3: Provide a more concrete hint or a VERY SAMLL code snippet, but NEVER the complete solution or answers.
           

           Ensure answers are directly targeting the learner.
           """;
            
            var promptRequest = new ChatRequest
            {
                Model = _llmModelUsed,
                Format = AICodeHintSchema,
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
                            You are an AI programming assistant that provides adaptive guidance to a learner who is attempting to solve a programming task.
                            Do NOT provide complete solutions or answers.
                            You MUST strictly follow constraints provided in the user prompt.
                        """),
                
                    new Message(ChatRole.User, prompt)
                } 
            };
        
            var promptResponse = await _llmService.ChatAsync(promptRequest, cancellationToken);

            if (string.IsNullOrWhiteSpace(promptResponse))
            {
                throw new InvalidOperationException("LLM returned an empty feedback response");
            }
        
            var feedbackResponse = JsonSerializer.Deserialize<AICodeHintResponse>(promptResponse, JsonOptions) ?? throw new InvalidOperationException("LLM code hint failed to deserialise");
            
            var aiChatLog = new AICodeHintChatLog
            {
                UserId = userId,
                CodeTaskId = aiCodeHintRequest.CodeTaskId,
                ChatLogRole = AICodeHintChatLogRole.AIAssistant,
                Content = feedbackResponse.Message.Trim(),
                HintLevel = hintLevel
            };
            
            var learnerChatLog = new AICodeHintChatLog
            {
                UserId = userId,
                CodeTaskId = aiCodeHintRequest.CodeTaskId,
                ChatLogRole = AICodeHintChatLogRole.Learner,
                Content = aiCodeHintRequest.Question
            };

            _dbContext.AICodeHintChatLogs.AddRange(aiChatLog, learnerChatLog);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AICodeHintResponse()
            {
                Message = feedbackResponse.Message.Trim(),
                HintLevel = hintLevel // since the LLM can't be trusted to provide an accurate hint level :P
            };
    }

    // Helpers (TODO potentially refactor these helpers into a separate helper class)
    
    private static string? TrimHint(string? hint)
    {
        return string.IsNullOrWhiteSpace(hint) ? null : hint.Trim();
    }
    
    private static int CalculateHintLevel(List<AICodeHintChatLog> chatLog)
    {
        // TODO only AI responses count,
        // but I could potentially change this to a counter inside the ChatLog class instead and make it dynamic and loop through a list of int + string hint level instructions later that is looped through and added to the prompt 
        var aiResponses = chatLog.Count(x => x.ChatLogRole == AICodeHintChatLogRole.AIAssistant);
        return Math.Clamp(aiResponses + 1, 1, 3);
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