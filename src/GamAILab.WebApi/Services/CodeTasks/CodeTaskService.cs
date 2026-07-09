using GamAILab.Shared.Models;
using GamAILab.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services.CodeTasks;

public class CodeTaskService : ICodeTaskService
{
    private readonly ApplicationDbContext _dbContext;
    
    public CodeTaskService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddCodeTask(CodeTask codeTask)
    {
        _dbContext.Add(codeTask);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<CodeTask?> GetCodeTaskById(int codeTaskId)
    {
        return await _dbContext.CodeTasks.FindAsync(codeTaskId);
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
        
        await AddCodeTask(codeTaskPythonAdding);
    }
}