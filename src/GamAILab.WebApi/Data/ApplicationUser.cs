using Microsoft.AspNetCore.Identity;

namespace GamAILab.WebApi.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}