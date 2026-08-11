using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.Shared.Models.Game;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CodeSubmission> CodeSubmissions => Set<CodeSubmission>();
    public DbSet<CodeTask> CodeTasks => Set<CodeTask>();
    public DbSet<AICodeTaskFeedback>  AICodeTaskFeedbacks => Set<AICodeTaskFeedback>();
    public DbSet<HallucinationCheckResult>  AIHallucinationCheckResults => Set<HallucinationCheckResult>();
    public DbSet<AIPersona>  AIPersonas => Set<AIPersona>();
    
    // Gamification
    public DbSet<LearnerGameProgress> LearnerGameProgresses => Set<LearnerGameProgress>();
    public DbSet<CustomData> CustomData => Set<CustomData>();
    public DbSet<GameObjective> GameObjectives => Set<GameObjective>();
    public DbSet<AIPersonaSimulationResponse> AIPersonaSimulationResponses => Set<AIPersonaSimulationResponse>();

    
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