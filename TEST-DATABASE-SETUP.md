# 🧪 Test Database Setup Guide

## 📋 Overview

This guide explains how to set up and manage the test database for AirlineManager.

---

## 🎯 Quick Start

### Option 1: Automatic Setup (Recommended)
```powershell
# Run from repository root
.\Setup-TestDatabase.ps1
```

This script will:
- ✅ Create `AirlineManager-Test` database (if doesn't exist)
- ✅ Apply all migrations
- ✅ Set up database schema
- ✅ Ready for running tests

### Option 2: Manual Connection String
```powershell
cd AirlineManager.DataAccess

dotnet ef database update `
  --startup-project ..\AirlineManager\AirlineManager.csproj `
  --connection "Server=192.168.10.5;Database=AirlineManager-Test;User Id=amdbuser;Password=Ef&7HgBy0S%LcgX;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Option 3: Environment Variable
```powershell
# Set environment to Test
$env:ASPNETCORE_ENVIRONMENT="Test"

# Apply migrations (uses appsettings.Test.json)
cd AirlineManager.DataAccess
dotnet ef database update --startup-project ..\AirlineManager\AirlineManager.csproj

# Reset environment
$env:ASPNETCORE_ENVIRONMENT="Development"
```

---

## 📁 Configuration Files

### appsettings.Test.json Locations

#### 1. AirlineManager/appsettings.Test.json
**Purpose**: Used by EF migrations and main application when `ASPNETCORE_ENVIRONMENT=Test`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.10.5;Database=AirlineManager-Test;..."
  }
}
```

#### 2. AirlineManager.Tests/appsettings.Test.json
**Purpose**: Used by xUnit tests

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.10.5;Database=AirlineManager-Test;..."
  }
}
```

**⚠️ Important**: Both files should have the **same connection string** for consistency.

---

## 🔧 PowerShell Scripts

### Setup-TestDatabase.ps1
**Purpose**: Complete database setup from scratch

**What it does:**
1. Checks if database exists
2. Creates database if needed
3. Applies all migrations
4. Sets recovery mode to SIMPLE

**Usage:**
```powershell
.\Setup-TestDatabase.ps1
```

**Output:**
```
======================================
Setting Up Test Database
======================================

Target Server: 192.168.10.5
Target Database: AirlineManager-Test

Checking if database exists...
Database does not exist. Creating...
Database created successfully!

Applying migrations...
Build successful!
Applying migration: 20251022235900_InitialCreate
...
======================================
Test database setup complete!
======================================
```

### Apply-TestMigrations.ps1
**Purpose**: Apply new migrations to existing test database

**What it does:**
1. Builds DataAccess project
2. Applies pending migrations only
3. Verifies success

**Usage:**
```powershell
.\Apply-TestMigrations.ps1
```

**Use this when:**
- Database already exists
- You added new migrations
- You want to update schema only

---

## 🚀 Workflow

### Initial Setup
```powershell
# 1. Clone repository
git clone https://github.com/sebiusz76/AirlineManager
cd AirlineManager

# 2. Setup test database
.\Setup-TestDatabase.ps1

# 3. Run tests
cd AirlineManager.Tests
dotnet test
```

### After Adding New Migration
```powershell
# 1. Create migration
cd AirlineManager.DataAccess
dotnet ef migrations add YourMigrationName --startup-project ..\AirlineManager\AirlineManager.csproj

# 2. Apply to development database
dotnet ef database update --startup-project ..\AirlineManager\AirlineManager.csproj

# 3. Apply to test database
cd ..
.\Apply-TestMigrations.ps1

# 4. Run tests to verify
cd AirlineManager.Tests
dotnet test
```

---

## 🔍 Troubleshooting

### Problem: "Unable to create a 'DbContext'"
**Error:**
```
Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[...]'
```

**Cause**: EF CLI can't find startup project configuration

**Solution:**
```powershell
# Always specify --startup-project
dotnet ef database update --startup-project ..\AirlineManager\AirlineManager.csproj
```

### Problem: "Cannot open database"
**Error:**
```
Cannot open database "AirlineManager-Test" requested by the login
```

**Solutions:**

#### Option A: Create database first
```powershell
.\Setup-TestDatabase.ps1
```

#### Option B: Manual creation
```sql
sqlcmd -S 192.168.10.5 -U amdbuser -P "Ef&7HgBy0S%LcgX"
> CREATE DATABASE [AirlineManager-Test];
> GO
> ALTER DATABASE [AirlineManager-Test] SET RECOVERY SIMPLE;
> GO
```

### Problem: "Login failed"
**Error:**
```
Login failed for user 'amdbuser'
```

**Causes:**
1. Wrong password
2. User doesn't exist
3. User doesn't have permissions

**Solutions:**

#### Check user exists:
```sql
USE master;
SELECT name FROM sys.server_principals WHERE name = 'amdbuser';
```

#### Grant permissions:
```sql
USE master;
ALTER LOGIN [amdbuser] WITH PASSWORD = 'Ef&7HgBy0S%LcgX';
GO

USE [AirlineManager-Test];
ALTER ROLE db_owner ADD MEMBER [amdbuser];
GO
```

### Problem: Script execution policy
**Error:**
```
cannot be loaded because running scripts is disabled on this system
```

**Solution:**
```powershell
# Allow script execution for current session
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process

# Then run script
.\Setup-TestDatabase.ps1
```

### Problem: SqlServer module not found
**Warning:**
```
SqlServer module not found, using sqlcmd
```

**Solution (optional):**
```powershell
# Install SQL Server PowerShell module
Install-Module -Name SqlServer -AllowClobber -Force
```

**Note**: Scripts work without this module (fallback to `sqlcmd`)

---

## 📊 Database Comparison

| Database | Purpose | Connection String |
|----------|---------|-------------------|
| **AirlineManager-Dev** | Development | `Server=192.168.10.5;Database=AirlineManager-Dev;...` |
| **AirlineManager-Test** | Testing | `Server=192.168.10.5;Database=AirlineManager-Test;...` |
| **AirlineManager** | Production | `Server=prod;Database=AirlineManager;...` |

---

## 🔐 Security Notes

### Connection String Security

**⚠️ Important**: The test database password is stored in:
- `appsettings.Test.json` files
- PowerShell scripts

**Best Practices:**

1. **Don't commit real passwords**
   ```gitignore
   # .gitignore
appsettings.Test.json
   appsettings.*.json
   !appsettings.Development.json
   ```

2. **Use environment variables in CI/CD**
   ```yaml
   # GitHub Actions / Azure DevOps
   - name: Apply Migrations
     env:
    TEST_DB_CONNECTION: ${{ secrets.TEST_DB_CONNECTION }}
     run: |
       dotnet ef database update --connection "$TEST_DB_CONNECTION"
   ```

3. **Use different passwords for each environment**
   - Dev: One password
   - Test: Different password
   - Prod: Completely different credentials

---

## 🧪 Running Tests

After database setup:

```powershell
# Navigate to test project
cd AirlineManager.Tests

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~UserLoginHistoryRelationshipTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Generate test report
dotnet test --logger "trx;LogFileName=test-results.trx"
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:    35, Skipped:     0, Total:    35
Duration: 15-30 seconds
```

---

## 📝 FAQ

### Q: Do I need to run Setup-TestDatabase.ps1 every time?
**A**: No, only once initially. After that, use `Apply-TestMigrations.ps1` for updates.

### Q: Can I use Windows Authentication instead of SQL Authentication?
**A**: Yes, change connection string to:
```json
"DefaultConnection": "Server=192.168.10.5;Database=AirlineManager-Test;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Q: How do I reset test database?
**A**: Drop and recreate:
```sql
USE master;
DROP DATABASE [AirlineManager-Test];
GO
```
Then run `Setup-TestDatabase.ps1` again.

### Q: Can tests run without applying migrations manually?
**A**: Partially. Tests will fail if schema doesn't match. Best practice: always apply migrations before running tests.

### Q: What's the difference between --connection and environment variable?
**A**: 
- `--connection`: Overrides any config, always works
- `ASPNETCORE_ENVIRONMENT=Test`: Uses `appsettings.Test.json`, requires file to exist

---

## 🎯 Summary

### Quick Reference

```powershell
# Initial setup
.\Setup-TestDatabase.ps1

# After new migration
.\Apply-TestMigrations.ps1

# Run tests
cd AirlineManager.Tests
dotnet test

# Manual migration (alternative)
cd AirlineManager.DataAccess
dotnet ef database update `
  --startup-project ..\AirlineManager\AirlineManager.csproj `
  --connection "Server=192.168.10.5;Database=AirlineManager-Test;..."
```

### Files to Maintain

| File | Purpose | Keep in Git? |
|------|---------|--------------|
| `Setup-TestDatabase.ps1` | Database setup script | ✅ Yes |
| `Apply-TestMigrations.ps1` | Migration script | ✅ Yes |
| `AirlineManager/appsettings.Test.json` | Test config (main app) | ⚠️ Template only |
| `AirlineManager.Tests/appsettings.Test.json` | Test config (tests) | ⚠️ Template only |
| `TEST-DATABASE-SETUP.md` | This guide | ✅ Yes |

---

**Last Updated**: 2024-11-01  
**Tested With**: .NET 9, SQL Server 2019+, PowerShell 7+
