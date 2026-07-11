using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;

namespace GamAILab.WebApi.Services;

public interface ICodeSubmissionService
{
    public Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, string? userId);
}