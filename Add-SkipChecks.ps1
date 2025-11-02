# Script to add SkipIfDatabaseNotAvailable() to all test methods

$testFiles = @(
    "AirlineManager.Tests\Relationships\UserSessionRelationshipTests.cs",
    "AirlineManager.Tests\Relationships\UserAuditLogRelationshipTests.cs",
    "AirlineManager.Tests\Relationships\IdentityRoleRelationshipTests.cs"
)

foreach ($file in $testFiles) {
    Write-Host "Processing: $file" -ForegroundColor Cyan
    
    $content = Get-Content $file -Raw
    
    # Add skip check after [Fact] attribute
   $content = $content -replace '(\[Fact\]\s+public async Task [^{]+\{)', "`$1`r`n        SkipIfDatabaseNotAvailable();`r`n "
    
    # Save the file
    Set-Content $file -Value $content -NoNewline
    
    Write-Host "✅ Updated: $file" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All test files updated successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
