using Azure;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.AdModels;
using KombfuscaWebManager.Models.CertificateModels;
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

        public DbSet<CupResult> CupResults { get; set; }

        public DbSet<AdRequest> AdRequests { get; set; }

        public DbSet<AdCategory> AdCategories { get; set; }
        
        public DbSet<AdSubscriptionPeriod> AdSubscriptionPeriods { get; set; }

        public DbSet<AuctionBid> AuctionBids { get; set; }

        public DbSet<Certificate> Certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Preserve the key sizes used by the existing Identity schema.
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(entity =>
            {
                entity.Property(e => e.LoginProvider).HasMaxLength(128);
                entity.Property(e => e.ProviderKey).HasMaxLength(128);
            });
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(entity =>
            {
                entity.Property(e => e.LoginProvider).HasMaxLength(128);
                entity.Property(e => e.Name).HasMaxLength(128);
            });

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

            builder.Entity<Certificate>()
                .HasIndex(c => c.ValidationCode)
                .IsUnique();

            builder.Entity<Certificate>()
                .HasIndex(c => new { c.CupId, c.UserId })
                .IsUnique();

            builder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Certificate>()
                .HasOne(c => c.Cup)
                .WithMany()
                .HasForeignKey(c => c.CupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
