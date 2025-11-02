using AirlineManager.Models.Domain;
using AirlineManager.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AirlineManager.Tests.Relationships;

/// <summary>
/// Tests for ApplicationUser → UserAuditLog relationships (multiple relationships)
/// </summary>
public class UserAuditLogRelationshipTests : DatabaseTestBase
{
    [Fact]
    public async Task ForeignKey_UserAuditLogs_AspNetUsers_UserId_ShouldExist()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var foreignKeyName = "FK_UserAuditLogs_AspNetUsers_UserId";

        // Act
        var deleteBehavior = await GetForeignKeyDeleteBehaviorAsync(foreignKeyName);

        // Assert
        deleteBehavior.Should().NotBeNull("Foreign key should exist");
        deleteBehavior.Should().Be("CASCADE", "Delete behavior should be CASCADE for UserId");
    }

    [Fact]
    public async Task ForeignKey_UserAuditLogs_AspNetUsers_ModifiedBy_ShouldExist()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var foreignKeyName = "FK_UserAuditLogs_AspNetUsers_ModifiedBy";

        // Act
        var deleteBehavior = await GetForeignKeyDeleteBehaviorAsync(foreignKeyName);

        // Assert
        deleteBehavior.Should().NotBeNull("Foreign key should exist");
        deleteBehavior.Should().BeOneOf("NO_ACTION", "RESTRICT",
             "Delete behavior should be RESTRICT/NO_ACTION for ModifiedBy");
    }

    [Fact]
    public async Task EagerLoading_UserWithAuditLogs_AsSubject_ShouldWork()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user = await CreateTestUserAsync("audit.subject@test.com");
        var admin = await CreateTestUserAsync("audit.admin@test.com");

        var auditLog = new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "ProfileUpdated",
            Changes = "{\"Field\":\"Email\",\"OldValue\":\"old@test.com\",\"NewValue\":\"new@test.com\"}"
        };

        Context.UserAuditLogs.Add(auditLog);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedUser = await Context.Users
       .Include(u => u.AuditLogs)
       .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        loadedUser.Should().NotBeNull();
        loadedUser!.AuditLogs.Should().NotBeEmpty();
        loadedUser.AuditLogs.Should().HaveCount(1);
        loadedUser.AuditLogs.First().UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task EagerLoading_UserWithAuditLogs_AsModifier_ShouldWork()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user = await CreateTestUserAsync("audit.user2@test.com");
        var admin = await CreateTestUserAsync("audit.modifier@test.com");

        var auditLog = new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "PasswordReset",
            Changes = "{\"Action\":\"PasswordReset\",\"ResetBy\":\"Admin\"}"
        };

        Context.UserAuditLogs.Add(auditLog);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedAdmin = await Context.Users
                  .Include(u => u.ModifiedAuditLogs)
            .FirstOrDefaultAsync(u => u.Id == admin.Id);

        // Assert
        loadedAdmin.Should().NotBeNull();
        loadedAdmin!.ModifiedAuditLogs.Should().NotBeEmpty();
        loadedAdmin.ModifiedAuditLogs.Should().HaveCount(1);
        loadedAdmin.ModifiedAuditLogs.First().ModifiedBy.Should().Be(admin.Id);
    }

    [Fact]
    public async Task NavigationProperty_AuditLogToUser_ShouldWork()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user = await CreateTestUserAsync("audit.nav.user@test.com");
        var admin = await CreateTestUserAsync("audit.nav.admin@test.com");

        var auditLog = new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "Test"
        };

        Context.UserAuditLogs.Add(auditLog);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedAudit = await Context.UserAuditLogs
            .Include(a => a.User)
        .Include(a => a.Modifier)
    .FirstOrDefaultAsync(a => a.UserId == user.Id);

        // Assert
        loadedAudit.Should().NotBeNull();
        loadedAudit!.User.Should().NotBeNull();
        loadedAudit.Modifier.Should().NotBeNull();
        loadedAudit.User!.Id.Should().Be(user.Id);
        loadedAudit.Modifier!.Id.Should().Be(admin.Id);
    }

    [Fact]
    public async Task CascadeDelete_ForUserId_ShouldDeleteAuditLogs()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user = await CreateTestUserAsync("cascade.audit.user@test.com");
        var admin = await CreateTestUserAsync("cascade.audit.admin@test.com");

        var auditLog1 = new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow.AddDays(-1),
            Action = "Created"
        };

        var auditLog2 = new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "Updated"
        };

        Context.UserAuditLogs.AddRange(auditLog1, auditLog2);
        await Context.SaveChangesAsync();

        var logsBeforeDelete = await Context.UserAuditLogs
   .CountAsync(a => a.UserId == user.Id);

        logsBeforeDelete.Should().Be(2, "Two audit logs should exist before delete");

        // Act - Delete the USER (subject)
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var logsAfterDelete = await Context.UserAuditLogs
       .CountAsync(a => a.UserId == user.Id);

        logsAfterDelete.Should().Be(0, "Audit logs should be deleted when user is deleted (CASCADE)");
    }

    [Fact]
    public async Task RestrictDelete_ForModifiedBy_ShouldPreventDeletion()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user = await CreateTestUserAsync("restrict.audit.user@test.com");
        var admin = await CreateTestUserAsync("restrict.audit.admin@test.com");

        var auditLog = new UserAuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "AdminAction"
        };

        Context.UserAuditLogs.Add(auditLog);
        await Context.SaveChangesAsync();

        // Act - Try to delete the ADMIN (modifier)
        Context.Users.Remove(admin);
        Func<Task> act = async () => await Context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SelfAudit_UserModifiesOwnProfile_ShouldWork()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user = await CreateTestUserAsync("selfaudit@test.com");

        // Act - User modifies their own profile
        var auditLog = new UserAuditLog
        {
            UserId = user.Id,       // Subject
            UserEmail = user.Email!,
            ModifiedBy = user.Id,   // Modifier (same user!)
            ModifiedByEmail = user.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "SelfProfileUpdate",
            Changes = "{\"Field\":\"PreferredTheme\",\"OldValue\":\"auto\",\"NewValue\":\"dark\"}"
        };

        Context.UserAuditLogs.Add(auditLog);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Assert
        var loadedUser = await Context.Users
         .Include(u => u.AuditLogs)
          .Include(u => u.ModifiedAuditLogs)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        loadedUser.Should().NotBeNull();
        loadedUser!.AuditLogs.Should().HaveCount(1, "User should have audit log as subject");
        loadedUser.ModifiedAuditLogs.Should().HaveCount(1, "User should have audit log as modifier");
        loadedUser.AuditLogs.First().Id.Should().Be(loadedUser.ModifiedAuditLogs.First().Id,
   "Should be the same audit log entry");
    }

    [Fact]
    public async Task MultipleRelationships_ShouldBeIndependent()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange
        var user1 = await CreateTestUserAsync("multi.user1@test.com");
        var user2 = await CreateTestUserAsync("multi.user2@test.com");
        var admin = await CreateTestUserAsync("multi.admin@test.com");

        // Admin modifies user1
        var log1 = new UserAuditLog
        {
            UserId = user1.Id,
            UserEmail = user1.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "AdminModifiedUser1"
        };

        // Admin modifies user2
        var log2 = new UserAuditLog
        {
            UserId = user2.Id,
            UserEmail = user2.Email!,
            ModifiedBy = admin.Id,
            ModifiedByEmail = admin.Email!,
            ModifiedAt = DateTime.UtcNow,
            Action = "AdminModifiedUser2"
        };

        Context.UserAuditLogs.AddRange(log1, log2);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedAdmin = await Context.Users
               .Include(u => u.ModifiedAuditLogs)
     .FirstOrDefaultAsync(u => u.Id == admin.Id);

        var loadedUser1 = await Context.Users
           .Include(u => u.AuditLogs)
.FirstOrDefaultAsync(u => u.Id == user1.Id);

        // Assert
        loadedAdmin.Should().NotBeNull();
        loadedAdmin!.ModifiedAuditLogs.Should().HaveCount(2, "Admin modified 2 users");

        loadedUser1.Should().NotBeNull();
        loadedUser1!.AuditLogs.Should().HaveCount(1, "User1 has 1 audit log");
    }

    [Fact]
    public async Task Index_OnUserId_ShouldExist()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange & Act
        var connection = Context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
 SELECT COUNT(*)
       FROM sys.indexes
WHERE object_id = OBJECT_ID('UserAuditLogs')
      AND name = 'IX_UserAuditLogs_UserId'";

            var result = await command.ExecuteScalarAsync();

            // Assert
            Convert.ToInt32(result).Should().BeGreaterThan(0, "Index on UserId should exist");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task Index_OnModifiedBy_ShouldExist()
    {
        SkipIfDatabaseNotAvailable();

        // Arrange & Act
        var connection = Context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
      SELECT COUNT(*)
 FROM sys.indexes
    WHERE object_id = OBJECT_ID('UserAuditLogs')
      AND name = 'IX_UserAuditLogs_ModifiedBy'";

            var result = await command.ExecuteScalarAsync();

            // Assert
            Convert.ToInt32(result).Should().BeGreaterThan(0, "Index on ModifiedBy should exist");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}