namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmission
{

    public int CodeSubmissionId { get; set; }
    public string UserId { get; set; }
    public string CodeTaskId  { get; set; }
    public string Code { get; set; }
    public int Attempts { get; set; }
    public DateTime LastSubmit { get; set; }
    public DateTime InitialSubmit { get; set; }
    // TODO possibly add submit frequency here
}