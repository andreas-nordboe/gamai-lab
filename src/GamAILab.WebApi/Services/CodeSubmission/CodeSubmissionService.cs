using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;

namespace GamAILab.WebApi.Services;

public class CodeSubmissionService : ICodeSubmissionService
{
    public async Task SubmitCodeAsync(
        CodeSubmission codeSubmission,
        ApplicationDbContext dbContext,
        ICodeTaskService codeTaskService)
    {
        // 1. Store attempted code submission and request task to database
        dbContext.Add(codeSubmission);
        await dbContext.SaveChangesAsync();
        
        // 2. Request task information
        var codeTask = await codeTaskService.GetCodeTaskById(codeSubmission.CodeTaskId);

        // 3. Generate an evaluation plan that includes task information (id, description, constraints..)

        // 3. 

    }
}