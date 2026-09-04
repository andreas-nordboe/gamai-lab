namespace GamAILab.Shared.Models;

public static class UserRole
{
    public const string Learner = "Learner";
    public const string Educator = "Educator";
    public const string Admin = "Admin";
    public const string Researcher = "Researcher";
    
    public static readonly string[] All =
    [
        Admin,
        Researcher,
        Educator,
        Learner
            
    ];
}