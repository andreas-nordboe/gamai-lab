using GamAILab.Shared.Models;
using GamAILab.Shared.Models.Game;
using GamAILab.Shared.Models.Game.DTOs;
using GamAILab.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services.Game;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _dbContext;

    public GameService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    // Learner game progress

    public async Task<List<LearnerGameProgress>> LoadLearnerGameProgress(string userId)
    {
        ValidateUser(userId);
        
        return await _dbContext.LearnerGameProgresses
            .AsNoTracking()
            .Where(prog => prog.UserId == userId)
            .ToListAsync();
        
        
    }
    
    public async Task SaveLearnerGameProgress(string userId, LearnerGameProgressRequest learnerGameProgress)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(learnerGameProgress);
        
        //learnerGameProgress.UserId = userId;
        
        var existingGameProgress = await _dbContext.LearnerGameProgresses.SingleOrDefaultAsync(prog => prog.UserId == userId);

        if (existingGameProgress is null)
        {
            // Handle achievements
            List<Achievement> achievements = new List<Achievement>();
            foreach (var achievement in learnerGameProgress.Achievements)
            {
                achievements.Add(new Achievement
                {
                    AchievementId = achievement.AchievementId,
                    Description = achievement.Description,
                    Title = achievement.Title
                });
            }
            
            //learnerGameProgress.UserId = userId;
            _dbContext.LearnerGameProgresses.Add(new LearnerGameProgress
            {
                UserId = userId,
                Level = learnerGameProgress.Level,
                Currency = learnerGameProgress.Currency,
                Achievements = achievements,
                LastUpdated = DateTime.UtcNow,
            });
        }
        else
        {
            _dbContext.Entry(existingGameProgress).CurrentValues.SetValues(learnerGameProgress);
            existingGameProgress.UserId = userId; // prevents ownership changes
        }
        
        
        await _dbContext.SaveChangesAsync();
    }
    
    // Custom data

    public async Task<List<CustomData>> ListCustomData(string userId)
    {
        ValidateUser(userId);
        
        
        return await _dbContext.CustomData
            .AsNoTracking()
            .Where(data => data.UserId == userId)
            .ToListAsync();
    }

    public async Task<CustomData?> GetCustomData(string userId, string key)
    {
        ValidateUser(userId);
        ValidateValue(key, nameof(key));

        return await _dbContext.Set<CustomData>()
            .AsNoTracking()
            .SingleOrDefaultAsync(data => data.UserId == userId && data.Key == key);
    }

    public async Task SaveCustomData(string userId, CustomDataRequest customData)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(customData);
        ValidateValue(customData.Key, nameof(customData.Key));
        
        var dataSet = _dbContext.Set<CustomData>();
        
        var existingData = _dbContext.Set<CustomData>().SingleOrDefault(data => data.UserId == userId && data.Key == customData.Key);

        if (existingData is null)
        {
            dataSet.Add(new CustomData
            {
                UserId = userId,
                Key = customData.Key,
                Value = customData.Value
            });
        }
        else
        {
            existingData.Value = customData.Value;
        }
        
        await _dbContext.SaveChangesAsync();
    }
    
    // Game objectives

    public async Task<List<GameObjective>> LoadGameObjectives(string userId)
    {
        ValidateUser(userId);
        
        return await _dbContext.Set<GameObjective>()
            .AsNoTracking()
            .Where(objective => objective.UserId == userId)
            .ToListAsync();
    }

    public async Task<GameObjective?> GetGameObjective(string userId, string objectiveId)
    {
        ValidateUser(userId);
        ValidateValue(objectiveId, nameof(objectiveId));

        return await _dbContext.Set<GameObjective>()
            .AsNoTracking()
            .SingleOrDefaultAsync(objective =>
                objective.UserId == userId &&
                objective.ObjectiveId == objectiveId);

    }

    public async Task SaveGameObjectives(string userId, GameObjectiveRequest gameObjective)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(gameObjective);

        ValidateValue(gameObjective.ObjectiveId, nameof(gameObjective.ObjectiveId));

        var objectiveSet = _dbContext.Set<GameObjective>();

        var existingObjective = await objectiveSet.SingleOrDefaultAsync(
            objective =>
                objective.UserId == userId &&
                objective.ObjectiveId ==
                gameObjective.ObjectiveId);

        if (existingObjective is null)
        {
            objectiveSet.Add(new GameObjective
            {
                UserId = userId,
                ObjectiveId = gameObjective.ObjectiveId,
                Title = gameObjective.Title,
                Description = gameObjective.Description,
                IsCompleted = gameObjective.IsCompleted,
                TargetValue = gameObjective.TargetValue,
                CurrentValue = gameObjective.CurrentValue
            });
        }
        else
        {
            _dbContext.Entry(existingObjective).CurrentValues.SetValues(gameObjective);

            existingObjective.UserId = userId;
            existingObjective.ObjectiveId = gameObjective.ObjectiveId;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<LearnerGameProgressRequest> GrantCodeTaskCompletionRewardsAsync(string userId, CodeTask codeTask, CancellationToken cancellationToken = default)
    {
        ValidateUser(userId);

        if (codeTask.Id <= 0)
        {
            throw new ArgumentException("CodeTaskId must be greater than zero");
        }
        
        var progress = await _dbContext.LearnerGameProgresses
            .Include(progress => progress.Achievements)
            .SingleOrDefaultAsync(progress => progress.UserId == userId, cancellationToken);

        if (progress is null)
        {
            progress = new LearnerGameProgress
            {
                UserId = userId,
                Level = 1,
                Currency = 0,
                LastUpdated = DateTime.UtcNow,
                Achievements = []
            };
            
            _dbContext.LearnerGameProgresses.Add(progress);
        }

        var completedAchievementId = $"code-task-{codeTask.Id}-completed";
        
        var hasBeenCompleted = progress.Achievements.Any(achievement => achievement.AchievementId == completedAchievementId);
        if (!hasBeenCompleted)
        {
            const int currencyReward = 10; // TODO move to task itself or handle this from Unreal (would not be authoritatively)
            
            // TODO level 1 at 0-99, level 2 at 100-199 etc. etc.
            progress.Level = 1 + progress.Level * currencyReward / 100;
            
            progress.Achievements.Add(new Achievement
            {
                AchievementId = completedAchievementId,
                Title = $"Completed Task {codeTask.Title}",
                Description = codeTask.Description,
                GrantedAt = DateTime.UtcNow
            });
        }

        progress.LastUpdated = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LearnerGameProgressRequest
        {
            Level = progress.Level,
            Currency = progress.Currency,
            Achievements = progress.Achievements.Select(achievement => new AchievementRequest
            {
                AchievementId = achievement.AchievementId,
                Title = achievement.Title,
                Description = achievement.Description
            }).ToList()
        };

    }

    // Helper methods
    
    private static void ValidateUser(string userId)
    {
        ValidateValue(userId, nameof(userId));
    }

    private static void ValidateValue(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"The '{parameterName}' cannot be null or empty", parameterName);
        }
    }
    
}