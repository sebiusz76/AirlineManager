# Setup Test Database
# Creates test database and applies all migrations
# Usage: .\Setup-TestDatabase.ps1

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Setting Up Test Database" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Database connection parameters
$server = "192.168.10.5"
$database = "AirlineManager-Test"
$userId = "amdbuser"
$password = "Ef&7HgBy0S%LcgX"

$masterConnectionString = "Server=$server;Database=master;User Id=$userId;Password=$password;TrustServerCertificate=True;"
$testConnectionString = "Server=$server;Database=$database;User Id=$userId;Password=$password;TrustServerCertificate=True;MultipleActiveResultSets=true"

Write-Host "Target Server: $server" -ForegroundColor Yellow
Write-Host "Target Database: $database" -ForegroundColor Yellow
Write-Host ""

# Check if SQL Server module is available
try {
    Import-Module SqlServer -ErrorAction Stop
    $useSqlModule = $true
    Write-Host "Using SqlServer PowerShell module" -ForegroundColor Green
} catch {
    $useSqlModule = $false
    Write-Host "SqlServer module not found, using sqlcmd" -ForegroundColor Yellow
}

Write-Host ""

# Function to execute SQL using PowerShell module
function Invoke-SqlQuery {
    param (
        [string]$ConnectionString,
  [string]$Query
    )
    
    if ($useSqlModule) {
        Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $Query -ErrorAction Stop
    } else {
  # Fallback to sqlcmd
    $tempFile = [System.IO.Path]::GetTempFileName()
        $Query | Out-File -FilePath $tempFile -Encoding UTF8
     sqlcmd -S $server -U $userId -P $password -i $tempFile
 Remove-Item $tempFile
    }
}

try {
    # Check if database exists
    Write-Host "Checking if database exists..." -ForegroundColor Green
    
    $checkDbQuery = @"
IF EXISTS (SELECT name FROM sys.databases WHERE name = '$database')
    SELECT 1 AS DbExists
ELSE
    SELECT 0 AS DbExists
"@
    
    $result = Invoke-SqlQuery -ConnectionString $masterConnectionString -Query $checkDbQuery
    
    if ($result.DbExists -eq 0) {
        Write-Host "Database does not exist. Creating..." -ForegroundColor Yellow
        
        $createDbQuery = @"
CREATE DATABASE [$database];
ALTER DATABASE [$database] SET RECOVERY SIMPLE;
"@
      
        Invoke-SqlQuery -ConnectionString $masterConnectionString -Query $createDbQuery
        Write-Host "Database created successfully!" -ForegroundColor Green
  } else {
 Write-Host "Database already exists" -ForegroundColor Green
    }
    
    Write-Host ""
    
    # Apply migrations
    Write-Host "Applying migrations..." -ForegroundColor Green
    
    Push-Location -Path "$PSScriptRoot\AirlineManager.DataAccess"
    
    try {
     dotnet build --nologo --verbosity quiet
        
   if ($LASTEXITCODE -ne 0) {
   Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
        
        dotnet ef database update `
  --startup-project ..\AirlineManager\AirlineManager.csproj `
    --connection $testConnectionString `
            --verbose
        
        if ($LASTEXITCODE -eq 0) {
     Write-Host ""
        Write-Host "======================================" -ForegroundColor Green
            Write-Host "Test database setup complete!" -ForegroundColor Green
            Write-Host "======================================" -ForegroundColor Green
       Write-Host ""
            Write-Host "Database: $database" -ForegroundColor Cyan
     Write-Host "Server: $server" -ForegroundColor Cyan
            Write-Host ""
       Write-Host "You can now run tests:" -ForegroundColor Cyan
Write-Host "  cd AirlineManager.Tests" -ForegroundColor White
Write-Host "  dotnet test" -ForegroundColor White
        } else {
            Write-Host ""
      Write-Host "Migration failed!" -ForegroundColor Red
   exit 1
    }
    }
    finally {
      Pop-Location
    }
}
catch {
 Write-Host ""
  Write-Host "======================================" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "======================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Check if SQL Server is running" -ForegroundColor White
    Write-Host "2. Verify connection credentials" -ForegroundColor White
    Write-Host "3. Ensure user has permission to create databases" -ForegroundColor White
    Write-Host "4. Check firewall settings" -ForegroundColor White
    exit 1
}
