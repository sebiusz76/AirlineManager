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
        public DbSet<AppConfiguration> AppConfigurations { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

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

            builder.Entity<AppConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsEncrypted).IsRequired();
                entity.Property(e => e.LastModified).IsRequired();
                entity.Property(e => e.LastModifiedBy).HasMaxLength(256);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.Category);
            });

            builder.Entity<UserLoginHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.LoginTime).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Browser).HasMaxLength(100);
                entity.Property(e => e.OperatingSystem).HasMaxLength(100);
                entity.Property(e => e.Device).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.IsSuccessful).IsRequired();
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.RequiredTwoFactor).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.LoginTime);
                entity.HasIndex(e => e.IsSuccessful);
            });

            builder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastActivityAt).IsRequired();
                entity.Property(e => e.ExpiresAt);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Browser).HasMaxLength(100);
                entity.Property(e => e.OperatingSystem).HasMaxLength(100);
                entity.Property(e => e.Device).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.IsPersistent).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ExpiresAt);
            });

            // Seed default SMTP configuration
            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<AppConfiguration>().HasData(
                new AppConfiguration
                {
                    Id = 1,
                    Key = "SMTP_Host",
                    Value = "smtp.gmail.com",
                    Description = "SMTP server hostname",
                    Category = "SMTP",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 2,
                    Key = "SMTP_Port",
                    Value = "587",
                    Description = "SMTP server port",
                    Category = "SMTP",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 3,
                    Key = "SMTP_Username",
                    Value = "",
                    Description = "SMTP authentication username",
                    Category = "SMTP",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 4,
                    Key = "SMTP_Password",
                    Value = "",
                    Description = "SMTP authentication password",
                    Category = "SMTP",
                    IsEncrypted = true,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 5,
                    Key = "SMTP_FromEmail",
                    Value = "noreply@example.com",
                    Description = "Sender email address",
                    Category = "SMTP",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 6,
                    Key = "SMTP_FromName",
                    Value = "Airline Manager",
                    Description = "Sender display name",
                    Category = "SMTP",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 7,
                    Key = "SMTP_EnableSSL",
                    Value = "true",
                    Description = "Enable SSL/TLS encryption",
                    Category = "SMTP",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 8,
                    Key = "Security_PasswordExpirationDays",
                    Value = "90",
                    Description = "Number of days before password expires (0 = never expires)",
                    Category = "Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 9,
                    Key = "Security_MaxFailedLoginAttempts",
                    Value = "5",
                    Description = "Maximum number of failed login attempts before account lockout",
                    Category = "Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 10,
                    Key = "Security_LockoutDurationMinutes",
                    Value = "30",
                    Description = "Duration of account lockout in minutes after max failed attempts",
                    Category = "Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 11,
                    Key = "Security_Password_RequireDigit",
                    Value = "true",
                    Description = "Password must contain at least one digit (0-9)",
                    Category = "Password Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 12,
                    Key = "Security_Password_RequireLowercase",
                    Value = "true",
                    Description = "Password must contain at least one lowercase letter (a-z)",
                    Category = "Password Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 13,
                    Key = "Security_Password_RequireUppercase",
                    Value = "true",
                    Description = "Password must contain at least one uppercase letter (A-Z)",
                    Category = "Password Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 14,
                    Key = "Security_Password_RequireNonAlphanumeric",
                    Value = "false",
                    Description = "Password must contain at least one special character (!@#$%^&* etc.)",
                    Category = "Password Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 15,
                    Key = "Security_Password_RequiredLength",
                    Value = "8",
                    Description = "Minimum password length",
                    Category = "Password Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                },
                new AppConfiguration
                {
                    Id = 16,
                    Key = "Security_Password_RequiredUniqueChars",
                    Value = "1",
                    Description = "Minimum number of unique characters in password",
                    Category = "Password Security",
                    IsEncrypted = false,
                    LastModified = seedDate,
                    LastModifiedBy = "System"
                }
            );
        }
    }
}