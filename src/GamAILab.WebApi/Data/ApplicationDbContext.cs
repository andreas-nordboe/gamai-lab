using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AICodeEvaluation.Hints;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.Shared.Models.Game;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // Code evaluation
    public DbSet<CodeSubmission> CodeSubmissions => Set<CodeSubmission>();
    public DbSet<CodeTask> CodeTasks => Set<CodeTask>();
    public DbSet<CodeExecutionResult> CodeExecutions => Set<CodeExecutionResult>();
    public DbSet<AICodeTaskFeedback>  AICodeTaskFeedbacks => Set<AICodeTaskFeedback>();
    public DbSet<HallucinationCheckResult>  AIHallucinationCheckResults => Set<HallucinationCheckResult>();
    // AI persona simulation
    public DbSet<AIPersona>  AIPersonas => Set<AIPersona>();
    public DbSet<AIPersonaSimulationResponse> AIPersonaSimulations => Set<AIPersonaSimulationResponse>();
    public DbSet<ClassroomSimulation> ClassroomSimulations { get; set; }
    
    // Gamification
    public DbSet<LearnerGameProgress> LearnerGameProgresses => Set<LearnerGameProgress>();
    public DbSet<CustomData> CustomData => Set<CustomData>();
    public DbSet<GameObjective> GameObjectives => Set<GameObjective>();
    public DbSet<AICodeHintChatLog> AICodeHintChatLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<AICodeTaskFeedback>()
            .HasOne(feedback => feedback.CodeSubmission)
            .WithOne(codeSubmission => codeSubmission.AICodeTaskFeedback)
            .HasForeignKey<AICodeTaskFeedback>(feedback => feedback.CodeSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<AICodeEvaluationPlan>()
            .ComplexCollection(plan => plan.Tests)
            .ToJson();
    }
}