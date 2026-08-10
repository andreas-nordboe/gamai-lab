using GamAILab.Shared.Models;
using GamAILab.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services.CodeTasks;

public class CodeTaskService : ICodeTaskService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CodeTaskService> _logger;
    private readonly IAICodeEvaluationService _aiCodeEvaluationService;
    
    public CodeTaskService(ApplicationDbContext dbContext, IAICodeEvaluationService aiCodeEvaluationService, ILogger<CodeTaskService> logger)
    {
        _dbContext = dbContext;
        _aiCodeEvaluationService = aiCodeEvaluationService;
        _logger = logger;
    }
    
    public async Task AddCodeTask(CodeTask codeTask)
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
    public async Task SeedCodeTasks()
    {
        var codeTaskPythonAdding = new CodeTask
        {
            Title = "Adding two numbers in Python",
            Description = "Adding two numbers in Python",
            DefaultCode = "def add(a, b):\n    # write python code here",
            Examples = new List<string>
            {
                "add(5,5) should return 10"
            },
            Constraints = new List<string>
            {
                "- The function must be called add",
                "- It must return the answer (10)",
                "- Do not use input",
                "- Do not only print the output/result"
            },
            Difficulty = CodeTaskDifficulty.Beginner,
            CreatedAt =  DateTime.Now
        };
        
        var taskExists = await _dbContext.CodeTasks.AnyAsync(p => p.Id == codeTaskPythonAdding.Id);
        if (!taskExists)
        {
            return;
        }
        
        await AddCodeTask(codeTaskPythonAdding);
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
}