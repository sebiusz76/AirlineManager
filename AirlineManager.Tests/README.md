# AirlineManager.Tests - Database Relationship Tests

## 📋 Overview

This test project contains **integration tests** for verifying database relationships in the AirlineManager application.

## 🎯 Test Coverage

### Relationship Tests

| Test Class | Relationships Tested | Tests Count |
|------------|---------------------|-------------|
| `UserLoginHistoryRelationshipTests` | ApplicationUser → UserLoginHistory | 7 |
| `UserSessionRelationshipTests` | ApplicationUser → UserSession | 8 |
| `UserAuditLogRelationshipTests` | ApplicationUser → UserAuditLog (x2) | 10 |
| `IdentityRoleRelationshipTests` | ApplicationUser ↔ IdentityRole | 10 |
| **TOTAL** | **5 relationships** | **35 tests** |

---

## 🧪 What is Tested

### 1. **Foreign Key Existence**
```csharp
[Fact]
public async Task ForeignKey_UserSessions_AspNetUsers_UserId_ShouldExist()
```
- Verifies FK exists in database
- Checks correct DELETE behavior (CASCADE/RESTRICT)

### 2. **Eager Loading**
```csharp
[Fact]
public async Task EagerLoading_UserWithSessions_ShouldWork()
```
- Tests `.Include()` functionality
- Verifies navigation properties work

### 3. **Navigation Properties**
```csharp
[Fact]
public async Task NavigationProperty_SessionToUser_ShouldWork()
```
- Tests bi-directional navigation
- Verifies EF Core relationship mapping

### 4. **Foreign Key Constraints**
```csharp
[Fact]
public async Task ForeignKeyConstraint_ShouldPreventOrphanRecords()
```
- Attempts to insert invalid FK
- Expects `DbUpdateException`

### 5. **CASCADE Delete**
```csharp
[Fact]
public async Task CascadeDelete_ShouldDeleteSessionsWhenUserDeleted()
```
- Deletes parent record
- Verifies child records are deleted automatically

### 6. **RESTRICT Delete**
```csharp
[Fact]
public async Task RestrictDelete_ForModifiedBy_ShouldPreventDeletion()
```
- Tests NO_ACTION/RESTRICT behavior
- Expects exception when deleting referenced record

### 7. **Multiple Relationships**
```csharp
[Fact]
public async Task MultipleRelationships_ShouldBeIndependent()
```
- Tests UserAuditLog with 2 FKs to same table
- Verifies both relationships work independently

### 8. **Indexes**
```csharp
[Fact]
public async Task Index_OnUserId_ShouldExist()
```
- Verifies database indexes exist
- Checks index properties (unique, etc.)

---

## 🔧 Setup

### Prerequisites

1. **SQL Server** (LocalDB or full instance)
2. **.NET 9 SDK**
3. **Test Database**: `AirlineManager-Test`

### Configuration

Edit `appsettings.Test.json`:
```json
{
  "ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AirlineManager-Test;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Database Setup

#### Option 1: Automatic (Recommended)
```bash
# Tests will create database automatically on first run
dotnet test
```

#### Option 2: Manual
```bash
# Create test database
cd AirlineManager.DataAccess
dotnet ef database update --connection "Server=localhost;Database=AirlineManager-Test;Trusted_Connection=True;TrustServerCertificate=True;"
```

---

## ▶️ Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~UserLoginHistoryRelationshipTests"
```

### Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~CascadeDelete_ShouldDeleteLoginHistoriesWhenUserDeleted"
```

### With Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Generate Test Report
```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
```

---

## 📊 Test Results Interpretation

### ✅ **PASS** - All tests pass
```
Passed!  - Failed:     0, Passed:    35, Skipped:     0, Total:    35
```
**Meaning**: All relationships work correctly in database

### ❌ **FAIL** - Foreign Key Not Found
```
FluentAssertions.Execution.AssertionFailedException: 
Expected deleteBehavior not to be <null> because Foreign key should exist
```
**Solution**: Run migrations or check database schema

### ❌ **FAIL** - CASCADE Delete Not Working
```
Expected historiesAfterDelete to be 0 because All login histories should be deleted (CASCADE), but found 2
```
**Solution**: Check FK DELETE behavior in database

### ❌ **FAIL** - RESTRICT Not Working
```
Expected a to throw an exception with message matching the equivalent of "*REFERENCE constraint*"
```
**Solution**: Verify RESTRICT/NO_ACTION is set on FK

---

## 🏗️ Test Structure

### Base Class: `DatabaseTestBase`
```csharp
public abstract class DatabaseTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly UserManager<ApplicationUser> UserManager;
    
    protected async Task<ApplicationUser> CreateTestUserAsync(...)
    protected async Task<bool> ForeignKeyExistsAsync(string foreignKeyName)
    protected async Task<string?> GetForeignKeyDeleteBehaviorAsync(...)
    protected async Task CleanupTestDataAsync()
}
```

**Features:**
- ✅ Automatic database context creation
- ✅ Helper methods for creating test data
- ✅ FK verification helpers
- ✅ Automatic cleanup
- ✅ UserManager/RoleManager for Identity tests

### Test Naming Convention
```csharp
[Fact]
public async Task <What>_<Scenario>_<ExpectedResult>()
```

**Examples:**
- `ForeignKey_UserSessions_AspNetUsers_UserId_ShouldExist`
- `CascadeDelete_ShouldDeleteSessionsWhenUserDeleted`
- `EagerLoading_UserWithLoginHistories_ShouldWork`

---

## 🐛 Troubleshooting

### Problem: "Cannot open database"
**Solution:**
```bash
# Check SQL Server is running
# Create database manually:
sqlcmd -S localhost -Q "CREATE DATABASE [AirlineManager-Test]"
```

### Problem: "The INSERT statement conflicted with the FOREIGN KEY constraint"
**Cause:** Migrations not applied to test database

**Solution:**
```bash
cd AirlineManager.DataAccess
dotnet ef database update --connection "Server=localhost;Database=AirlineManager-Test;..."
```

### Problem: "User 'NT AUTHORITY\SYSTEM' login failed"
**Solution:** Change connection string to use SQL Authentication:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AirlineManager-Test;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### Problem: Tests fail with "Object reference not set to an instance"
**Cause:** Context not initialized properly

**Solution:** Check `appsettings.Test.json` exists and connection string is valid

---

## 📝 Test Data Cleanup

### Automatic Cleanup
Tests use `CleanupTestDataAsync()` which:
- Removes test users (email contains "test")
- CASCADE delete removes related records automatically
- Runs after each test (in `Dispose()` or explicitly)

### Manual Cleanup
```sql
-- Delete all test data
DELETE FROM AspNetUsers WHERE Email LIKE '%test%'
-- CASCADE will handle related records
```

---

## 🎯 Best Practices

### ✅ DO:
- Run tests before committing code
- Test on clean database periodically
- Verify all tests pass before deployment
- Use meaningful test names
- Clean up test data

### ❌ DON'T:
- Run tests on production database
- Commit failing tests
- Skip database setup
- Modify `DatabaseTestBase` without updating all tests
- Leave orphan test data

---

## 📚 Test Examples

### Example 1: Simple FK Test
```csharp
[Fact]
public async Task ForeignKey_ShouldExist()
{
    var deleteBehavior = await GetForeignKeyDeleteBehaviorAsync("FK_Name");
    deleteBehavior.Should().Be("CASCADE");
}
```

### Example 2: Eager Loading Test
```csharp
[Fact]
public async Task EagerLoading_ShouldWork()
{
    var user = await CreateTestUserAsync("test@test.com");
    // ... add related data ...
    
    var loaded = await Context.Users
    .Include(u => u.RelatedCollection)
   .FirstOrDefaultAsync(u => u.Id == user.Id);
    
    loaded!.RelatedCollection.Should().NotBeEmpty();
    await CleanupTestDataAsync();
}
```

### Example 3: CASCADE Delete Test
```csharp
[Fact]
public async Task CascadeDelete_ShouldWork()
{
    var user = await CreateTestUserAsync("test@test.com");
    // ... add related data ...
    
    Context.Users.Remove(user);
    await Context.SaveChangesAsync();
    
    var orphans = await Context.RelatedTable
        .CountAsync(r => r.UserId == user.Id);
    
    orphans.Should().Be(0, "CASCADE should delete related records");
}
```

---

## 📈 Coverage Report

### Current Status
- ✅ **UserLoginHistory**: 7/7 tests (100%)
- ✅ **UserSession**: 8/8 tests (100%)
- ✅ **UserAuditLog**: 10/10 tests (100%)
- ✅ **IdentityRole**: 10/10 tests (100%)

### Total: 35 tests covering all database relationships

---

## 🚀 CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run Database Tests
  run: |
    dotnet ef database update --project AirlineManager.DataAccess --connection "${{ secrets.TEST_DB_CONNECTION }}"
    dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--logger trx --collect:"Code Coverage"'
```

---

## 📞 Support

### Issues
If tests fail:
1. Check database connection string
2. Verify migrations are applied
3. Check SQL Server is running
4. Review test output for specific error
5. Check `Docs/DatabaseRelations/Troubleshooting-*.md`

### Documentation
- Relationship docs: `Docs/DatabaseRelations/`
- SQL scripts: `Docs/DatabaseRelations/*.sql`

---

**Last Updated**: 2024-11-01  
**Test Framework**: xUnit  
**Assertion Library**: FluentAssertions  
**Database**: SQL Server  
**Coverage**: 35 tests / 5 relationships
