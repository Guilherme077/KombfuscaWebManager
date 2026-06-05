using Azure;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.CupModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KombfuscaWebManager.Data
{
    public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cup> Cups { get; set; }

        public DbSet<Period> Periods { get; set; }

        public DbSet<ScoreSheet> ScoreSheets { get; set; }

        public DbSet<Participation> Participations { get; set; }
    }
}
