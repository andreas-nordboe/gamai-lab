namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmissionRequest
{
    public int CodeTaskId  { get; set; }
    public string Code { get; set; } = string.Empty;
}