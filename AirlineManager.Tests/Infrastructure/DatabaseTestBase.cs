using AirlineManager.DataAccess.Data;
using AirlineManager.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace AirlineManager.Tests.Infrastructure;

/// <summary>
/// Base class for database integration tests
/// Provides database context and helper methods
/// </summary>
public abstract class DatabaseTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly RoleManager<IdentityRole> RoleManager;
 private readonly string _connectionString;
    private static int _testCounter = 0;
    private static bool? _isDatabaseAvailable;
    private static readonly object _lock = new object();

    protected DatabaseTestBase()
    {
        // Read connection string from configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: true)
 .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Database=AirlineManager-Test;Trusted_Connection=True;TrustServerCertificate=True;";

        // Create DbContext
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(_connectionString);
Context = new ApplicationDbContext(optionsBuilder.Options);

  // Create UserManager and RoleManager
        var userStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<ApplicationUser>(Context);
        var roleStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<IdentityRole>(Context);

   UserManager = new UserManager<ApplicationUser>(
            userStore,
            null!,
 new PasswordHasher<ApplicationUser>(),
      null!,
       null!,
        null!,
      null!,
    null!,
   null!);

        RoleManager = new RoleManager<IdentityRole>(
  roleStore,
    null!,
        null!,
            null!,
          null!);
    }

    /// <summary>
    /// Checks if database is available for testing
    /// </summary>
    protected bool IsDatabaseAvailable()
    {
        if (_isDatabaseAvailable.HasValue)
        {
            return _isDatabaseAvailable.Value;
    }

        lock (_lock)
        {
    if (_isDatabaseAvailable.HasValue)
            {
                return _isDatabaseAvailable.Value;
      }

 try
       {
       Context.Database.CanConnect();
        _isDatabaseAvailable = true;
     }
            catch
   {
        _isDatabaseAvailable = false;
          }

         return _isDatabaseAvailable.Value;
        }
    }

    /// <summary>
    /// Skips test if database is not available
    /// </summary>
    protected void SkipIfDatabaseNotAvailable()
    {
        Xunit.Skip.IfNot(IsDatabaseAvailable(), "Database is not available. Skipping integration test.");
    }

    /// <summary>
    /// Creates a test user in the database with unique email
    /// </summary>
    protected async Task<ApplicationUser> CreateTestUserAsync(
    string email = "test@test.com",
    string password = "Test123!",
        string firstName = "Test",
        string lastName = "User")
    {
        // Add unique counter to email to avoid duplicates
  var counter = System.Threading.Interlocked.Increment(ref _testCounter);
    var timestamp = DateTime.UtcNow.Ticks;
    var uniqueEmail = email.Replace("@", $".{counter}.{timestamp}@");

        var user = new ApplicationUser
        {
  Id = Guid.NewGuid().ToString(),
            UserName = uniqueEmail,
 NormalizedUserName = uniqueEmail.ToUpper(),
Email = uniqueEmail,
            NormalizedEmail = uniqueEmail.ToUpper(),
   EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            MustChangePassword = false,
 PreferredTheme = "auto",
            SecurityStamp = Guid.NewGuid().ToString(),
         ConcurrencyStamp = Guid.NewGuid().ToString()
   };

 user.PasswordHash = UserManager.PasswordHasher.HashPassword(user, password);

    await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

   return user;
    }

    /// <summary>
    /// Executes raw SQL query and returns results
    /// </summary>
    protected async Task<List<T>> ExecuteRawSqlAsync<T>(string sql, params object[] parameters)
        where T : class
    {
        return await Context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
    }

    /// <summary>
    /// Checks if foreign key exists in database
  /// </summary>
    protected async Task<bool> ForeignKeyExistsAsync(string foreignKeyName)
    {
     var sql = @"
            SELECT COUNT(*) 
         FROM sys.foreign_keys 
      WHERE name = {0}";

        var result = await Context.Database.ExecuteSqlRawAsync(
  "SELECT @p0 = COUNT(*) FROM sys.foreign_keys WHERE name = @p1",
            foreignKeyName);

    return result > 0;
}

  /// <summary>
    /// Gets foreign key delete behavior
    /// </summary>
    protected async Task<string?> GetForeignKeyDeleteBehaviorAsync(string foreignKeyName)
 {
        DbConnection connection = Context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
       using var command = connection.CreateCommand();
         command.CommandText = @"
   SELECT delete_referential_action_desc 
    FROM sys.foreign_keys 
         WHERE name = @foreignKeyName";

      var parameter = command.CreateParameter();
            parameter.ParameterName = "@foreignKeyName";
            parameter.Value = foreignKeyName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
      return result?.ToString();
        }
   finally
    {
          await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Cleans up test data
    /// </summary>
    protected async Task CleanupTestDataAsync()
    {
  try
        {
            // First, delete audit logs where ModifiedBy references test users (RESTRICT constraint)
            var testUserIds = await Context.Users
     .Where(u => u.Email != null && u.Email.Contains("test"))
      .Select(u => u.Id)
    .ToListAsync();

            if (testUserIds.Any())
            {
             // Delete audit logs modified by test users
     var auditLogsToDelete = await Context.UserAuditLogs
      .Where(a => testUserIds.Contains(a.ModifiedBy))
        .ToListAsync();

        if (auditLogsToDelete.Any())
                {
        Context.UserAuditLogs.RemoveRange(auditLogsToDelete);
     await Context.SaveChangesAsync();
  }

  // Now delete test users (CASCADE will handle remaining relations)
           var testUsers = await Context.Users
    .Where(u => u.Email != null && u.Email.Contains("test"))
  .ToListAsync();

         if (testUsers.Any())
                {
             Context.Users.RemoveRange(testUsers);
          await Context.SaveChangesAsync();
        }
   }

      // Delete any orphan roles created during tests
            var testRoles = await Context.Roles
         .Where(r => r.Name != null && 
         (r.Name.Contains("Test") || r.Name.Contains("Cascade") || r.Name.Contains("Delete")))
     .ToListAsync();

   if (testRoles.Any())
     {
     Context.Roles.RemoveRange(testRoles);
       await Context.SaveChangesAsync();
            }
        }
 catch (Exception)
        {
            // Ignore cleanup errors - tests should still pass
            // This prevents test failures due to cleanup issues
   }
    }

    public virtual void Dispose()
    {
    Context?.Dispose();
        UserManager?.Dispose();
        RoleManager?.Dispose();
        GC.SuppressFinalize(this);
    }
}