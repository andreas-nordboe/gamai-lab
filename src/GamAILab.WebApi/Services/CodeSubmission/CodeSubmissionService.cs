using System.Text.Json;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeExecution;
using GamAILab.WebApi.Services.CodeTasks;
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
    private readonly ILogger<CodeSubmissionService> _logger;


    public CodeSubmissionService(ApplicationDbContext dbContext, ICodeTaskService codeTaskService, IAICodeEvaluationService aiCodeEvaluationService, ICodeExecutionService codeExecutionService, ILogger<CodeSubmissionService> logger, IAIFeedbackService aiFeedbackService)
    {
        _dbContext = dbContext;
        _codeTaskService = codeTaskService;
        _aiCodeEvaluationService = aiCodeEvaluationService;
        _codeExecutionService = codeExecutionService;
        _logger = logger;
        _aiFeedbackService = aiFeedbackService;
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
        
        _dbContext.CodeSubmissions.Add(submission);
        await _dbContext.SaveChangesAsync();
        
        // 2. Request task information
        var codeTask = await _codeTaskService.GetCodeTaskById(codeSubmission.CodeTaskId);

        // 3. Generate an evaluation plan that includes task information (id, description, constraints..)
        if (codeTask is null)
        {
            throw new KeyNotFoundException("CodeTask was not found");
        }

        var evaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(codeTask, cancellationToken);
        
        // 4. Execute code in isolated docker container (Docker code runner)
        var codeExecution = await _codeExecutionService.ExecuteCodeAsync(submission.Code, evaluationPlan, cancellationToken);
        
        _logger.LogInformation(JsonSerializer.Serialize(codeExecution));
        
        // 5. Send code to AIFeedbackService (I need to look into latency here and possibly feed back partial information or notify the learner)
        var aiFeedback = await _aiFeedbackService.GenerateCodeTaskFeedbackAsync(codeTask, submission, evaluationPlan, codeExecution, cancellationToken);
        
        aiFeedback.CodeSubmissionId = submission.Id;
        aiFeedback.CodeSubmission = submission;
        
        _dbContext.AICodeTaskFeedbacks.Add(aiFeedback);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation($"Persited feedback for submission {submission.Id} with task outcome {aiFeedback.TaskOutcome}");
            
        // TODO Handle null (maybe just exception too)
        
        // 6. Verify feedback using AI Hallucination service (TODO I dedicated week 5 to this)
        
        // 7. Update progress in Game/Progress Service
        
        // Later: Adaptive learning (possibly in a separate service)
        
        // 8. Return results to the client

        return new CodeSubmissionResult
        {
            SubmissionId =  submission.Id,
            CodeTask = codeTask,
            AttemptNumber = submission.Attempts,
            CodeExecution = codeExecution,
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
            }
        };
    }
}