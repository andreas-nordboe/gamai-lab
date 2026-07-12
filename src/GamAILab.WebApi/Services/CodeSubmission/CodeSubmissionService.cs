using System.Text.Json;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services;

public class CodeSubmissionService : ICodeSubmissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICodeTaskService _codeTaskService;
    private readonly IAICodeEvaluationService _aiCodeEvaluationService;


    public CodeSubmissionService(ApplicationDbContext dbContext, ICodeTaskService codeTaskService, IAICodeEvaluationService aiCodeEvaluationService)
    {
        _dbContext = dbContext;
        _codeTaskService = codeTaskService;
        _aiCodeEvaluationService = aiCodeEvaluationService;
    }

    public async Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, string? userId)
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
        
        _dbContext.Add(submission);
        await _dbContext.SaveChangesAsync();
        
        // 2. Request task information
        var codeTask = await _codeTaskService.GetCodeTaskById(codeSubmission.CodeTaskId);

        // 3. Generate an evaluation plan that includes task information (id, description, constraints..)
        var evaluationPlan = await _aiCodeEvaluationService.GenerateEvaluationPlanAsync(codeTask);

        Console.WriteLine(JsonSerializer.Serialize(evaluationPlan));
        
        // 4. Execute code in isolated docker container (Docker code runner)
        
        // 5. Send code to AIFeedbackService (I need to look into latency here and possibly feed back partial information or notify the learner)
        
        // 6. Verify feedback using AI Hallucination service (TODO I dedicated week 5 to this)
        
        // 7. Update progress in Game/Progress Service
        
        // Later: Adaptive learning (possibly in a separate service)
        
        // 8. Return results to the client


        return new CodeSubmissionResult
        {
            CodeTask = codeTask
        };
    }
}