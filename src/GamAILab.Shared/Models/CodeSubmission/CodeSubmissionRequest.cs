namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmissionRequest
{
    public int CodeTaskId  { get; set; }
    public string CodeAttempt { get; set; } = string.Empty;
}