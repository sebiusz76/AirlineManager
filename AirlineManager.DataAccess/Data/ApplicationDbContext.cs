using AirlineManager.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirlineManager.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<UserAuditLog> UserAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<ApplicationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Level).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Exception);
                entity.Property(e => e.LogEvent);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Level);
            });

            builder.Entity<UserAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ModifiedBy).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ModifiedByEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ModifiedAt).IsRequired();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Changes);
                entity.Property(e => e.OldValues);
                entity.Property(e => e.NewValues);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ModifiedBy);
                entity.HasIndex(e => e.ModifiedAt);
                entity.HasIndex(e => e.Action);
            });
        }
    }
}