using AirlineManager.Models.Domain;
using AirlineManager.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AirlineManager.Tests.Relationships;

/// <summary>
/// Tests for ApplicationUser → UserLoginHistory relationship
/// </summary>
public class UserLoginHistoryRelationshipTests : DatabaseTestBase
{
    [Fact]
    public async Task ForeignKey_UserLoginHistories_AspNetUsers_UserId_ShouldExist()
    {
        // Arrange
        var foreignKeyName = "FK_UserLoginHistories_AspNetUsers_UserId";

        // Act
        var deleteBehavior = await GetForeignKeyDeleteBehaviorAsync(foreignKeyName);

        // Assert
        deleteBehavior.Should().NotBeNull("Foreign key should exist");
        deleteBehavior.Should().Be("CASCADE", "Delete behavior should be CASCADE");
    }

    [Fact]
    public async Task EagerLoading_UserWithLoginHistories_ShouldWork()
    {
        // Arrange
        var user = await CreateTestUserAsync("loginhistory.test@test.com");

        var loginHistory = new UserLoginHistory
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            LoginTime = DateTime.UtcNow,
            IsSuccessful = true,
            RequiredTwoFactor = false,
            IpAddress = "127.0.0.1",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            Device = "Desktop"
        };

        Context.UserLoginHistories.Add(loginHistory);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedUser = await Context.Users
            .Include(u => u.LoginHistories)
        .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        loadedUser.Should().NotBeNull();
        loadedUser!.LoginHistories.Should().NotBeEmpty();
        loadedUser.LoginHistories.Should().HaveCount(1);
        loadedUser.LoginHistories.First().UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task NavigationProperty_LoginHistoryToUser_ShouldWork()
    {
        // Arrange
        var user = await CreateTestUserAsync("loginhistory.nav@test.com");

        var loginHistory = new UserLoginHistory
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            LoginTime = DateTime.UtcNow,
            IsSuccessful = true,
            RequiredTwoFactor = false
        };

        Context.UserLoginHistories.Add(loginHistory);
        await Context.SaveChangesAsync();

        // Clear tracking
        Context.ChangeTracker.Clear();

        // Act
        var loadedHistory = await Context.UserLoginHistories
            .Include(h => h.User)
      .FirstOrDefaultAsync(h => h.UserId == user.Id);

        // Assert
        loadedHistory.Should().NotBeNull();
        loadedHistory!.User.Should().NotBeNull();
        loadedHistory.User!.Id.Should().Be(user.Id);
        loadedHistory.User.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task ForeignKeyConstraint_ShouldPreventOrphanRecords()
    {
        // Arrange
        var invalidUserId = "00000000-0000-0000-0000-000000000000";

        var loginHistory = new UserLoginHistory
        {
            UserId = invalidUserId,
            UserEmail = "invalid@test.com",
            LoginTime = DateTime.UtcNow,
            IsSuccessful = true,
            RequiredTwoFactor = false
        };

        Context.UserLoginHistories.Add(loginHistory);

        // Act
        Func<Task> act = async () => await Context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CascadeDelete_ShouldDeleteLoginHistoriesWhenUserDeleted()
    {
        // Arrange
        var user = await CreateTestUserAsync("cascade.loginhistory@test.com");

        var loginHistory1 = new UserLoginHistory
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            LoginTime = DateTime.UtcNow.AddDays(-2),
            IsSuccessful = true,
            RequiredTwoFactor = false
        };

        var loginHistory2 = new UserLoginHistory
        {
            UserId = user.Id,
            UserEmail = user.Email!,
            LoginTime = DateTime.UtcNow.AddDays(-1),
            IsSuccessful = false,
            RequiredTwoFactor = true,
            FailureReason = "Invalid password"
        };

        Context.UserLoginHistories.AddRange(loginHistory1, loginHistory2);
        await Context.SaveChangesAsync();

        var historiesBeforeDelete = await Context.UserLoginHistories
    .CountAsync(h => h.UserId == user.Id);

        historiesBeforeDelete.Should().Be(2, "Two login histories should exist before delete");

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var historiesAfterDelete = await Context.UserLoginHistories
       .CountAsync(h => h.UserId == user.Id);

        historiesAfterDelete.Should().Be(0, "All login histories should be deleted (CASCADE)");
    }

    [Fact]
    public async Task MultipleUsers_WithLoginHistories_ShouldWorkIndependently()
    {
        // Arrange
        var user1 = await CreateTestUserAsync("multi1.loginhistory@test.com");
        var user2 = await CreateTestUserAsync("multi2.loginhistory@test.com");

        for (int i = 0; i < 3; i++)
        {
            Context.UserLoginHistories.Add(new UserLoginHistory
            {
                UserId = user1.Id,
                UserEmail = user1.Email!,
                LoginTime = DateTime.UtcNow.AddHours(-i),
                IsSuccessful = true,
                RequiredTwoFactor = false
            });
        }

        for (int i = 0; i < 2; i++)
        {
            Context.UserLoginHistories.Add(new UserLoginHistory
            {
                UserId = user2.Id,
                UserEmail = user2.Email!,
                LoginTime = DateTime.UtcNow.AddHours(-i),
                IsSuccessful = true,
                RequiredTwoFactor = false
            });
        }

        await Context.SaveChangesAsync();

        // Act
        var user1Histories = await Context.UserLoginHistories
         .CountAsync(h => h.UserId == user1.Id);
        var user2Histories = await Context.UserLoginHistories
   .CountAsync(h => h.UserId == user2.Id);

        // Assert
        user1Histories.Should().Be(3);
        user2Histories.Should().Be(2);
    }

    [Fact]
    public async Task Index_OnUserId_ShouldExist()
    {
        // Arrange & Act
        var connection = Context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
  SELECT COUNT(*)
   FROM sys.indexes
   WHERE object_id = OBJECT_ID('UserLoginHistories')
           AND name = 'IX_UserLoginHistories_UserId'";

            var result = await command.ExecuteScalarAsync();

            // Assert
            Convert.ToInt32(result).Should().BeGreaterThan(0, "Index on UserId should exist");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}