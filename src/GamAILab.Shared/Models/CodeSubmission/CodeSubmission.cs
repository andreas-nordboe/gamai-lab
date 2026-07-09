using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmission
{

    [Key]
    public int Id { get; set; } // CodeSubmissionId
    public string UserId { get; set; }
    public int CodeTaskId  { get; set; }
    public string Code { get; set; }
    public int Attempts { get; set; }
    public DateTime LastSubmit { get; set; }
    public DateTime InitialSubmit { get; set; }
    // TODO possibly add submit frequency here
}