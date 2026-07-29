using GamAILab.Shared.Models.Game;

namespace GamAILab.WebApi.Services.Game;

public interface IGameService
{
    // Key learner progress data
    Task<List<LearnerGameProgress>> LoadLearnerGameProgress(string userId);
    Task SaveLearnerGameProgress(string userId, LearnerGameProgress learnerGameProgress);
    
    // Key value stores
    
    Task<List<CustomData>> ListCustomData(string userId);
    Task<CustomData?> GetCustomData(string userId, string key);
    Task SaveCustomData(string userId, CustomData customData);
    
    // Objectives (Missions/Quests/Tasks/Tutorial etc. for later but the software artefact will be smaller scale)
    
    Task<List<GameObjective>> LoadGameObjectives(string userId);
    Task<GameObjective?> GetGameObjective(string userId, string objectiveId);
    Task SaveGameObjectives(string userId, GameObjective gameObjective);
    
}