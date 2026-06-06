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

        public DbSet<CupAssignment> CupAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ScoreSheet>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ScoreSheet>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CupAssignment>()
                .HasOne(ca => ca.Cup)
                .WithMany(c => c.Assignments)
                .HasForeignKey(ca => ca.CupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CupAssignment>()
                .HasOne(ca => ca.User)
                .WithMany(u => u.CupAssignments)
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
