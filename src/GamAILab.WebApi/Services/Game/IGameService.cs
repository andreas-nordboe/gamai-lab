using GamAILab.Shared.Models.Game;
using GamAILab.Shared.Models.Game.DTOs;

namespace GamAILab.WebApi.Services.Game;

// TODO replace models with DTOs

public interface IGameService
{
    // Key learner progress data
    Task<List<LearnerGameProgress>> LoadLearnerGameProgress(string userId);
    Task SaveLearnerGameProgress(string userId, LearnerGameProgressRequest learnerGameProgress);
    
    // Key value stores
    
    Task<List<CustomData>> ListCustomData(string userId);
    Task<CustomData?> GetCustomData(string userId, string key);
    Task SaveCustomData(string userId, CustomDataRequest customData);
    
    // Objectives (Missions/Quests/Tasks/Tutorial etc. for later but the software artefact will be smaller scale)
    
    Task<List<GameObjective>> LoadGameObjectives(string userId);
    Task<GameObjective?> GetGameObjective(string userId, string objectiveId);
    Task SaveGameObjectives(string userId, GameObjectiveRequest gameObjective);
    
}