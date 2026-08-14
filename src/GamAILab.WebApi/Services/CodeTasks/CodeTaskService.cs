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
        List<CodeTask> codeTasks = [];

        codeTasks.Add(new CodeTask
        {
            Title = "Using a function to add two numbers in Python",
            Description = "Create a Python function that accepts two numbers and return the sum of those numbers.",
            DefaultCode = """
                          def add(a, b):
                            # write python code here
                            
                          # change
                          print((
                          """,
            Examples = new List<string>
            {
                "add(5,5) should return 10",
                "add(4,8) should return 12",
                "add(-2,1) should return -1",
            },
            Constraints = new List<string>
            {
                "The function must be called add",
                "The function must return a numbered answer (for example 10)",
                "Do not use input()",
                "Do not only print the output/result",
                "The function must only accept 2 arguments"
            },
            Difficulty = CodeTaskDifficulty.Beginner,
            CreatedAt = DateTime.Now,
            CurrencyReward = 10
        });

        codeTasks.Add(new CodeTask
        {
            Title = "Using a function to subtract three numbers in Python",
            Description =
                "Create a Python function that accepts three numbers, subtracts and return the sum of those numbers.",
            DefaultCode = """
                          def subtract(a, b, c):
                              # write python code here
                          """,
            Examples = new List<string>
            {
                "subtract(10,5) should return 5",
                "subtract(2,1) should return 1",
                "subtract(-2,5) should return -7",
            },
            Constraints = new List<string>
            {
                "The function must be called subtract",
                "The function must return a numbered answer (for example 10)",
                "Do not use input()",
                "Do not only print the output/result",
                "The function must only accept 3 arguments"
            },
            Difficulty = CodeTaskDifficulty.Beginner,
            CreatedAt = DateTime.Now
        });
        
        // Testing standard output code task type
        
        codeTasks.Add(new CodeTask
        {
            Title = "Printing welcome messages to the console",
            Description =
                "Write the following in so it prints to the Python output console: Hello GamAI Lab!",
            DefaultCode = """
                          # write python code here
                          """,
            Examples = new List<string>
            {
                "Output: Hello GamAI Lab!",
            },
            Constraints = new List<string>
            {
                "Output must match the following exactly: Hello GamAI Lab!",
                "Do not use input()",
                "The Python code must not include a function",
                "Do not only print any additional text"
            },
            Difficulty = CodeTaskDifficulty.Beginner,
            CreatedAt = DateTime.Now
        });
        

        foreach (var codeTask in codeTasks)
        {
            // checks if task exists before adding it 
            if (await _dbContext.CodeTasks.AnyAsync(task => task.Title == codeTask.Title))
            {
                continue;
            }
            
            await AddCodeTask(codeTask);
        }
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