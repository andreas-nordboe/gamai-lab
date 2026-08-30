using System.Text.Json;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AICodeEvaluation.DTOs;
using GamAILab.WebApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.WebApi.Services.HallucinationChecker;

// Loading and code tasks from (/SeedAppData/CodeTasks/verified-code-evaluations.json
// this service aims to showcase a simplified RAG system that loads tasks to feed the hallucination checker with historical data
// the idea is to present this in the report as a simplification and mention how this could be implemented using a vector database that uses embeddings and indexing
public class VerifiedCodeEvaluationsService
{
    private readonly List<VerifiedCodeEvaluationExample> _verifiedCodeEvaluations;
    private readonly ApplicationDbContext _dbContext;
    
    public VerifiedCodeEvaluationsService(IWebHostEnvironment environment, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        var jsonPath = Path.Combine(environment.ContentRootPath, "SeedAppData", "CodeTasks", "verified-code-evaluations.json");
        var jsonData = File.ReadAllText(jsonPath);
        _verifiedCodeEvaluations = JsonSerializer.Deserialize<List<VerifiedCodeEvaluationExample>>(jsonData) ?? [];
    }

    // Loads verified code evaluation examples from the JSON file
    public IReadOnlyList<VerifiedCodeEvaluationExample> GetVerifiedCodeEvaluationExamples(int taskId, int maxAmount = 3)
    {
        // finds X amount of tasks  that matches the code task ID from the internal JSON file
        // TODO this could potentially retrieve results from the database but it would also defeat the purpose of "verified" examples if it just filtered out any submissions
        // TODO as it could simply make the consistency results even less accurate 
        return _verifiedCodeEvaluations.Where(x => x.CodeTaskId == taskId).Take(maxAmount).ToList();
    }
    
    // Loads verified code evaluations from the database
    public async Task<List<VerifiedCodeEvaluationExample>> ExportVerifiedCodeEvaluationExamples()
    { 
        return await _dbContext.Set<CodeSubmission>()
            .Where(x => x.AICodeTaskFeedback != null)
            .OrderByDescending(x => x.SubmittedAt)
            .Select(x => new VerifiedCodeEvaluationExample
            {
                CodeTaskId = x.CodeTaskId,
                CodeSubmission = x.Code ?? string.Empty, // testing empty!
                CodeExecutionEvidence = x.AICodeTaskFeedback!.CodeTaskExecutionEvidence,
                PreviousFeedback = x.AICodeTaskFeedback.Explanation
            })
            .ToListAsync();
    }
    
}