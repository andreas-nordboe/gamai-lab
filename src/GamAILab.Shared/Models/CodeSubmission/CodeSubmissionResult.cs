namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmissionResult
{
    // TODO to be displayed on frontend Status (Pass/Fail/Partial), Feedback, Progress
    // TODO separate parameters for AI personas
    //public string Status { get; set; }
    public CodeTask? CodeTask { get; set; } // TODO remove after testing output to avoid disclosing internal code task
    
}