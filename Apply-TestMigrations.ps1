# Apply migrations to Test database
# Usage: .\Apply-TestMigrations.ps1

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Applying Migrations to Test Database" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Test database connection string
$connectionString = "Server=192.168.10.5;Database=AirlineManager-Test;User Id=amdbuser;Password=Ef&7HgBy0S%LcgX;TrustServerCertificate=True;MultipleActiveResultSets=true"

Write-Host "Target Database: AirlineManager-Test" -ForegroundColor Yellow
Write-Host "Server: 192.168.10.5" -ForegroundColor Yellow
Write-Host ""

# Navigate to DataAccess project
Push-Location -Path "$PSScriptRoot\AirlineManager.DataAccess"

try {
    Write-Host "Building project..." -ForegroundColor Green
    dotnet build --nologo --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
  Write-Host "Build failed!" -ForegroundColor Red
    exit 1
    }
    
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Applying migrations..." -ForegroundColor Green
    dotnet ef database update `
        --startup-project ..\AirlineManager\AirlineManager.csproj `
        --connection $connectionString `
    --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "======================================" -ForegroundColor Green
 Write-Host "Migrations applied successfully!" -ForegroundColor Green
      Write-Host "======================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "You can now run tests:" -ForegroundColor Cyan
        Write-Host "  cd AirlineManager.Tests" -ForegroundColor White
        Write-Host "  dotnet test" -ForegroundColor White
  } else {
        Write-Host ""
 Write-Host "======================================" -ForegroundColor Red
        Write-Host "Migration failed!" -ForegroundColor Red
 Write-Host "======================================" -ForegroundColor Red
        exit 1
    }
}
finally {
    Pop-Location
}
