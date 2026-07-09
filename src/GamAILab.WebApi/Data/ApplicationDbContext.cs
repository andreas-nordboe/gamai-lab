using GamAILab.Shared.Models;
using GamAILab.Shared.Models.CodeSubmission;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<CodeSubmission> CodeSubmissions => Set<CodeSubmission>();
    public DbSet<CodeTask> CodeTasks => Set<CodeTask>();
}