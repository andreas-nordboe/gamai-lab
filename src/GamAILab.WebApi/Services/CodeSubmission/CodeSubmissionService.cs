using System.Text.Json;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AICodeEvaluation.DTOs;
using GamAILab.Shared.Models.AICodeEvaluation.Hints;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.Shared.Models.Game.DTOs;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Hubs;
using GamAILab.WebApi.Services.CodeExecution;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.Game;
using GamAILab.WebApi.Services.HallucinationChecker;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services;

public class CodeSubmissionService : ICodeSubmissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICodeTaskService _codeTaskService;
    //private readonly IAICodeEvaluationService _aiCodeEvaluationService;
    private readonly ICodeExecutionService _codeExecutionService;
    private readonly IAIFeedbackService _aiFeedbackService;
    private readonly IAIHallucinationCheckerService _aiHallucinationCheckerService;
    private readonly IGameService _gameService;
    private readonly ILogger<CodeSubmissionService> _logger;
    private readonly IHubContext<CodeEvaluationHub> _hubContext;

    public CodeSubmissionService(ApplicationDbContext dbContext, ICodeTaskService codeTaskService/*, IAICodeEvaluationService aiCodeEvaluationService*/, ICodeExecutionService codeExecutionService, ILogger<CodeSubmissionService> logger, IAIFeedbackService aiFeedbackService, IAIHallucinationCheckerService iaiHallucinationCheckerService, IGameService gameService, IHubContext<CodeEvaluationHub> hubContext)
    {
        _dbContext = dbContext;
        _codeTaskService = codeTaskService;
        //_aiCodeEvaluationService = aiCodeEvaluationService;
        _codeExecutionService = codeExecutionService;
        _logger = logger;
        _aiFeedbackService = aiFeedbackService;
        _aiHallucinationCheckerService = iaiHallucinationCheckerService;
        _gameService = gameService;
        _hubContext = hubContext;
    }

    public async Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, string? userId, bool updateGameProgress = true, CancellationToken cancellationToken = default)
    {
        // 1. Store attempted code submission and request task to database
        await BroadcastWebSocketStatus(userId, codeSubmission.CodeTaskId, CodeEvaluationStep.SubmissionInitiated, "Executing your code");

        // Retrieve previous task submissions to increment attempts/for timestamps
        var previousAttempts = await _dbContext.CodeSubmissions
            .CountAsync(submission => submission.CodeTaskId == codeSubmission.CodeTaskId 
                                      &&  submission.UserId == userId);
        
        // Construct code submission object for auditing/analysis
        var submission = new CodeSubmission
        {
            UserId = userId,
            CodeTaskId = codeSubmission.CodeTaskId,
            Code = codeSubmission.CodeAttempt,
            Attempts = previousAttempts + 1,
            SubmittedAt = DateTime.UtcNow
        };
        
        // 2. Request task information
        var codeTask = await _codeTaskService.GetCodeTaskById(codeSubmission.CodeTaskId);
    
        if (codeTask is null)
        {
            throw new KeyNotFoundException("CodeTask was not found");
        }
        
        // Save submission after validating that task exists
        _dbContext.CodeSubmissions.Add(submission);
        await _dbContext.SaveChangesAsync();

        //var evaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(codeTask, cancellationToken);
        if (codeTask.AiCodeEvaluationPlan is null)
        {
            throw new KeyNotFoundException($"Code evaluation plan was not found for task with id {codeTask.Id}");
        }
        
        // 4. Execute code in isolated docker container (Docker code runner)
        await BroadcastWebSocketStatus(userId, codeSubmission.CodeTaskId, CodeEvaluationStep.ExecutingCode, "Executing your code");
        var codeExecution = await _codeExecutionService.ExecuteCodeAsync(submission.Code, codeTask.AiCodeEvaluationPlan, cancellationToken);
        
        _logger.LogInformation(JsonSerializer.Serialize(codeExecution));
        
        
        // 5. Send code to AIFeedbackService (I need to look into latency here and possibly feed back partial information or notify the learner)
        await BroadcastWebSocketStatus(userId, codeSubmission.CodeTaskId, CodeEvaluationStep.GeneratingAIFeedback, "An AI is evaluating the code and generating feedback");
        var aiFeedback = await _aiFeedbackService.GenerateCodeTaskFeedbackAsync(codeTask, submission, codeTask.AiCodeEvaluationPlan, codeExecution, cancellationToken);
        
        aiFeedback.CodeSubmissionId = submission.Id;
        aiFeedback.CodeSubmission = submission;
        
        // 6. Verify feedback using AI Hallucination service
        await BroadcastWebSocketStatus(userId, codeSubmission.CodeTaskId, CodeEvaluationStep.RunningHallucinationChecker, "A hallucination checker is verifying the AI-generated consistency against your code execution results");
        var hallucinationCheckResult = await _aiHallucinationCheckerService.CheckAIFeedbackConsistencyAsync(codeTask, submission, codeTask.AiCodeEvaluationPlan, codeExecution, aiFeedback, cancellationToken);
        
        _dbContext.AIHallucinationCheckResults.Add(hallucinationCheckResult);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation($"Persited feedback '{aiFeedback.Id}' and hallucination check '{hallucinationCheckResult.Id}' for submission {submission.Id} with task outcome {aiFeedback.TaskOutcome} and hallucination check status '{hallucinationCheckResult.Status}'");
        
        // 7. Update progress in Game/Progress Service
        LearnerGameProgressRequest? updatedLearnerGameProgress = null;
        var didCodeExecutionPassCompletion = codeExecution.DidComplete && !codeExecution.TimedOut && codeExecution.ExitCode == 0 && codeExecution.EveryTestPassed && string.IsNullOrEmpty(codeExecution.FatalError);
        
        // checks if feedback was verified by hallucination checker BEFORE granting task completion
        if (didCodeExecutionPassCompletion && aiFeedback.TaskOutcome == CodeTaskOutcome.Correct && hallucinationCheckResult.Status == HallucinationCheckerStatus.IsConsistent)
        {
            await BroadcastWebSocketStatus(userId, codeSubmission.CodeTaskId, CodeEvaluationStep.UpdatingGameProgress, "Your code passed the test requirements, well done! Updating your game progress.");
            updatedLearnerGameProgress = await _gameService.GrantCodeTaskCompletionRewardsAsync(userId!, codeTask, cancellationToken);
            _logger.LogInformation($"Updated game progress for learner with id {userId} after completing task {codeTask.Id}");
        }
        
        // TODO Later: Adaptive learning (possibly in a separate service)
        
        await BroadcastWebSocketStatus(userId, codeSubmission.CodeTaskId, CodeEvaluationStep.Finished, "Your code passed the test requirements, well done! Updating your game progress.");
        
        // 8. Return results to the client
        return new CodeSubmissionResult
        {
            SubmissionId =  submission.Id,
            CodeTask = codeTask,
            AttemptNumber = submission.Attempts,
            CodeExecution = codeExecution,
            SubmittedCode =  codeSubmission.CodeAttempt,
            ExecutionDuration = codeExecution.ExecutionDuration,
            AIFeedback = new AICodeTaskFeedbackDTO
            {
                Id =  aiFeedback.Id,
                TaskOutcome = aiFeedback.TaskOutcome,
                Explanation =  aiFeedback.Explanation,
                HintMessage =  aiFeedback.HintMessage,
                CodeTaskExecutionEvidence =  aiFeedback.CodeTaskExecutionEvidence,
                LLMModelUsed =   aiFeedback.LLMModelUsed,
                CreatedAt =   aiFeedback.CreatedAt,
                GeneationTimeInMs =  aiFeedback.GeneationTimeInMs
            },
            HallucinationCheck = hallucinationCheckResult,
            GameProgress = updatedLearnerGameProgress
        };
    }

    public async Task<CodeTaskLearnerProgress?> LoadCodeTaskProgress(string? userId, int codeTaskId, CancellationToken cancellationToken = default)
    {
        var doesCodeTaskExist = await _dbContext.CodeTasks.AnyAsync(x => x.Id == codeTaskId, cancellationToken);

        if (!doesCodeTaskExist)
        {
            return null;
        }

        var attempts = await _dbContext.CodeSubmissions.CountAsync(x => x.UserId == userId && x.CodeTaskId == codeTaskId, cancellationToken);
        var hintChatLogs = await _dbContext.AICodeHintChatLogs
        .Where(x => x.UserId == userId && x.CodeTaskId == codeTaskId)
        .OrderBy(x => x.CreatedAt) // TODO I'll potentially change this later
        .ToListAsync(cancellationToken);

        return new CodeTaskLearnerProgress
        {
            Attempts = attempts,
            HintsUsed = hintChatLogs.Count(x => x.ChatLogRole == AICodeHintChatLogRole.AIAssistant),
            ChatLogs = hintChatLogs
        };
    }

    private Task BroadcastWebSocketStatus(string? userId, int codeTaskId, CodeEvaluationStep codeEvaluationStep, string message)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        return _hubContext.Clients.User(userId).SendAsync("CodeEvaluationStatusChanged", new CodeEvaluationStatus
        {
            CodeTaskId = codeTaskId,
            CodeEvaluationStep = codeEvaluationStep,
            Message = message,
        });
    }
}