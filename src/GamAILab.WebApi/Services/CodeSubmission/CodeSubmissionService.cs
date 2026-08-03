using System.Text.Json;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.Shared.Models.Game.DTOs;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeExecution;
using GamAILab.WebApi.Services.CodeTasks;
using GamAILab.WebApi.Services.Game;
using GamAILab.WebApi.Services.HallucinationChecker;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services;

public class CodeSubmissionService : ICodeSubmissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICodeTaskService _codeTaskService;
    private readonly IAICodeEvaluationService _aiCodeEvaluationService;
    private readonly ICodeExecutionService _codeExecutionService;
    private readonly IAIFeedbackService _aiFeedbackService;
    private readonly IAIHallucinationCheckerService _aiHallucinationCheckerService;
    private readonly IGameService _gameService;
    private readonly ILogger<CodeSubmissionService> _logger;


    public CodeSubmissionService(ApplicationDbContext dbContext, ICodeTaskService codeTaskService, IAICodeEvaluationService aiCodeEvaluationService, ICodeExecutionService codeExecutionService, ILogger<CodeSubmissionService> logger, IAIFeedbackService aiFeedbackService, IAIHallucinationCheckerService iaiHallucinationCheckerService, IGameService gameService)
    {
        _dbContext = dbContext;
        _codeTaskService = codeTaskService;
        _aiCodeEvaluationService = aiCodeEvaluationService;
        _codeExecutionService = codeExecutionService;
        _logger = logger;
        _aiFeedbackService = aiFeedbackService;
        _aiHallucinationCheckerService = iaiHallucinationCheckerService;
        _gameService = gameService;
    }

    public async Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, string? userId, CancellationToken cancellationToken = default)
    {
        // 1. Store attempted code submission and request task to database
        
        // Retrieve previous task submissions to increment attempts/for timestamps
        var previousAttempts = await _dbContext.CodeSubmissions
            .CountAsync(submission => submission.CodeTaskId == codeSubmission.CodeTaskId 
                                      &&  submission.UserId == userId);
        
        // Construct code submission object for auditing/analysis
        var submission = new CodeSubmission
        {
            UserId = userId,
            CodeTaskId = codeSubmission.CodeTaskId,
            Code = codeSubmission.Code,
            Attempts = previousAttempts + 1,
            SubmittedAt = DateTime.UtcNow
        };
        
        // 2. Request task information
        var codeTask = await _codeTaskService.GetCodeTaskById(codeSubmission.CodeTaskId);

        // 3. Generate an evaluation plan that includes task information (id, description, constraints..)
        if (codeTask is null)
        {
            throw new KeyNotFoundException("CodeTask was not found");
        }
        
        // Save submission after validating that task exists
        _dbContext.CodeSubmissions.Add(submission);
        await _dbContext.SaveChangesAsync();

        var evaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(codeTask, cancellationToken);
        
        // 4. Execute code in isolated docker container (Docker code runner)
        var codeExecution = await _codeExecutionService.ExecuteCodeAsync(submission.Code, evaluationPlan, cancellationToken);
        
        _logger.LogInformation(JsonSerializer.Serialize(codeExecution));
        
        // 5. Send code to AIFeedbackService (I need to look into latency here and possibly feed back partial information or notify the learner)
        var aiFeedback = await _aiFeedbackService.GenerateCodeTaskFeedbackAsync(codeTask, submission, evaluationPlan, codeExecution, cancellationToken);
        
        aiFeedback.CodeSubmissionId = submission.Id;
        aiFeedback.CodeSubmission = submission;
        
        // 6. Verify feedback using AI Hallucination service
        var hallucinationCheckResult = await _aiHallucinationCheckerService.CheckAIFeedbackConsistencyAsync(codeTask, submission, evaluationPlan, codeExecution, aiFeedback, cancellationToken);
            
        _dbContext.AICodeTaskFeedbacks.Add(aiFeedback);
        _dbContext.AIHallucinationCheckResults.Add(hallucinationCheckResult);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation($"Persited feedback '{aiFeedback.Id}' and hallucination check '{hallucinationCheckResult.Id}' for submission {submission.Id} with task outcome {aiFeedback.TaskOutcome} and hallucination check status '{hallucinationCheckResult.Status}'");
        
        // 7. Update progress in Game/Progress Service
        LearnerGameProgressRequest? updatedLearnerGameProgress = null;
        var didCodeSubmissionPassRequirements = codeExecution.DidComplete && codeExecution.EveryTestPassed && string.IsNullOrEmpty(codeExecution.FatalError);
        
        // checks if feedback was verified by hallucination checker BEFORE granting task completion
        // 
        if (didCodeSubmissionPassRequirements && hallucinationCheckResult.Status == HallucinationCheckerStatus.IsConsistent)
        {
            updatedLearnerGameProgress = await _gameService.GrantCodeTaskCompletionRewardsAsync(userId!, codeTask, cancellationToken);
            _logger.LogInformation($"Updated game progresss for learner with id {userId} after completing task {codeTask.Id}");
        }
        
        // TODO Later: Adaptive learning (possibly in a separate service)
        
        // 8. Return results to the client
        return new CodeSubmissionResult
        {
            SubmissionId =  submission.Id,
            CodeTask = codeTask,
            AttemptNumber = submission.Attempts,
            CodeExecution = codeExecution,
            SubmittedCode =  codeSubmission.Code,
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
            HallucinationCheck = hallucinationCheckResult
        };
    }
}