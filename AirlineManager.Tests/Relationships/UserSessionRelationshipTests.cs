using AirlineManager.Models.Domain;
using AirlineManager.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AirlineManager.Tests.Relationships;

/// <summary>
/// Tests for ApplicationUser → UserSession relationship
/// </summary>
public class UserSessionRelationshipTests : DatabaseTestBase
{
    [Fact]
    public async Task ForeignKey_UserSessions_AspNetUsers_UserId_ShouldExist()
    {
        // Arrange
        var foreignKeyName = "FK_UserSessions_AspNetUsers_UserId";

        // Act
        var deleteBehavior = await GetForeignKeyDeleteBehaviorAsync(foreignKeyName);

        // Assert
        deleteBehavior.Should().NotBeNull("Foreign key should exist");
        deleteBehavior.Should().Be("CASCADE", "Delete behavior should be CASCADE");
    }

    [Fact]
    public async Task EagerLoading_UserWithSessions_ShouldWork()
    {
        // Arrange
        var user = await CreateTestUserAsync("session.test@test.com");

        var session = new UserSession
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            SessionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = false,
            IpAddress = "192.168.1.1",
            Browser = "Firefox",
            OperatingSystem = "Linux",
            Device = "Desktop"
        };

        Context.UserSessions.Add(session);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedUser = await Context.Users
            .Include(u => u.Sessions)
       .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        loadedUser.Should().NotBeNull();
        loadedUser!.Sessions.Should().NotBeEmpty();
        loadedUser.Sessions.Should().HaveCount(1);
        loadedUser.Sessions.First().UserId.Should().Be(user.Id);

        // Cleanup
        await CleanupTestDataAsync();
    }

    [Fact]
    public async Task NavigationProperty_SessionToUser_ShouldWork()
    {
        // Arrange
        var user = await CreateTestUserAsync("session.nav@test.com");

        var session = new UserSession
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            SessionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = true
        };

        Context.UserSessions.Add(session);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedSession = await Context.UserSessions
        .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        // Assert
        loadedSession.Should().NotBeNull();
        loadedSession!.User.Should().NotBeNull();
        loadedSession.User!.Id.Should().Be(user.Id);
        loadedSession.User.Email.Should().Be(user.Email);

        // Cleanup
        await CleanupTestDataAsync();
    }

    [Fact]
    public async Task ForeignKeyConstraint_ShouldPreventOrphanRecords()
    {
        // Arrange
        var invalidUserId = "00000000-0000-0000-0000-000000000000";

        var session = new UserSession
        {
            UserId = invalidUserId,
            UserEmail = "invalid@test.com",
            SessionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = false
        };

        Context.UserSessions.Add(session);

        // Act
        Func<Task> act = async () => await Context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>()
                .WithMessage("*FOREIGN KEY constraint*");
    }

    [Fact]
    public async Task CascadeDelete_ShouldDeleteSessionsWhenUserDeleted()
    {
        // Arrange
        var user = await CreateTestUserAsync("cascade.session@test.com");

        var session1 = new UserSession
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            SessionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            LastActivityAt = DateTime.UtcNow.AddHours(-1),
            IsActive = true,
            IsPersistent = false
        };

        var session2 = new UserSession
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            SessionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = true
        };

        Context.UserSessions.AddRange(session1, session2);
        await Context.SaveChangesAsync();

        var sessionsBeforeDelete = await Context.UserSessions
        .CountAsync(s => s.UserId == user.Id);

        sessionsBeforeDelete.Should().Be(2, "Two sessions should exist before delete");

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var sessionsAfterDelete = await Context.UserSessions
        .CountAsync(s => s.UserId == user.Id);

        sessionsAfterDelete.Should().Be(0, "All sessions should be deleted (CASCADE)");
    }

    [Fact]
    public async Task SessionId_ShouldBeUnique()
    {
        // Arrange
        var user = await CreateTestUserAsync("unique.session@test.com");
        var sessionId = Guid.NewGuid().ToString();

        var session1 = new UserSession
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = false
        };

        Context.UserSessions.Add(session1);
        await Context.SaveChangesAsync();

        var session2 = new UserSession
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            SessionId = sessionId, // Same SessionId!
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = false
        };

        Context.UserSessions.Add(session2);

        // Act
        Func<Task> act = async () => await Context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>()
  .WithMessage("*duplicate*");

        // Cleanup
        await CleanupTestDataAsync();
    }

    [Fact]
    public async Task FilterActiveSessions_ShouldWork()
    {
        // Arrange
        var user = await CreateTestUserAsync("filter.session@test.com");

        for (int i = 0; i < 3; i++)
        {
            Context.UserSessions.Add(new UserSession
            {
                UserId = user.Id,
                UserEmail = user.Email!,
                SessionId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                LastActivityAt = DateTime.UtcNow.AddHours(-i),
                IsActive = i < 2, // First 2 active, last inactive
                IsPersistent = false
            });
        }

        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedUser = await Context.Users
        .Include(u => u.Sessions.Where(s => s.IsActive))
           .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        loadedUser.Should().NotBeNull();
        loadedUser!.Sessions.Should().HaveCount(2, "Only active sessions should be loaded");
        loadedUser.Sessions.Should().OnlyContain(s => s.IsActive);

        // Cleanup
        await CleanupTestDataAsync();
    }

    [Fact]
    public async Task Index_OnSessionId_ShouldBeUnique()
    {
        // Arrange & Act
        var connection = Context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
         SELECT is_unique
     FROM sys.indexes
             WHERE object_id = OBJECT_ID('UserSessions')
          AND name = 'IX_UserSessions_SessionId'";

            var result = await command.ExecuteScalarAsync();

            // Assert
            Convert.ToBoolean(result).Should().BeTrue("SessionId index should be unique");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}