using GamAILab.Shared.Models.Game;
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
    
    public async Task SaveLearnerGameProgress(string userId, LearnerGameProgress learnerGameProgress)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(learnerGameProgress);
        
        learnerGameProgress.UserId = userId;
        
        var existingGameProgress = await _dbContext.LearnerGameProgresses.SingleOrDefaultAsync(prog => prog.UserId == userId && prog.Id == learnerGameProgress.Id);

        if (existingGameProgress is null)
        {
            
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

    public async Task SaveCustomData(string userId, CustomData customData)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(customData);
        ValidateValue(customData.Key, nameof(customData.Key));
        
        customData.UserId = userId;
        
        var dataSet = _dbContext.Set<CustomData>();
        
        var existingData = _dbContext.Set<CustomData>().SingleOrDefault(data => data.UserId == userId && data.Key == customData.Key);

        if (existingData is null)
        {
            dataSet.Add(customData);
        }
        else
        {
            _dbContext.Entry(existingData).CurrentValues.SetValues(customData);
            existingData.UserId = userId;
            existingData.Key = customData.Key;
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

    public async Task SaveGameObjectives(string userId, GameObjective gameObjective)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(gameObjective);

        ValidateValue(gameObjective.ObjectiveId, nameof(gameObjective.ObjectiveId));
        gameObjective.UserId = userId;

        var objectiveSet = _dbContext.Set<GameObjective>();

        var existingObjective = await objectiveSet.SingleOrDefaultAsync(
            objective =>
                objective.UserId == userId &&
                objective.ObjectiveId ==
                gameObjective.ObjectiveId);

        if (existingObjective is null)
        {
            objectiveSet.Add(gameObjective);
        }
        else
        {
            _dbContext.Entry(existingObjective).CurrentValues.SetValues(gameObjective);

            existingObjective.UserId = userId;
            existingObjective.ObjectiveId = gameObjective.ObjectiveId;
        }

        await _dbContext.SaveChangesAsync();
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