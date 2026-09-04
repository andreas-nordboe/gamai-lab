namespace GamAILab.Shared.Models.AICodeEvaluation;

public enum CodeEvaluationStep
{
    SubmissionInitiated,
    ExecutingCode,
    GeneratingAIFeedback,
    RunningHallucinationChecker,
    UpdatingGameProgress,
    Finished
}