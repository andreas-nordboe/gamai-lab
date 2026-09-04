using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.Shared.Models.Game.DTOs;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;

namespace GamAILab.WebApi.Services;

public interface ICodeSubmissionService
{
    public Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, string? userId, bool updateGameProgress = true, CancellationToken cancellationToken = default);
    public Task<CodeTaskLearnerProgress?> LoadCodeTaskProgress(string? userId, int codeTaskId, CancellationToken cancellationToken = default);
}