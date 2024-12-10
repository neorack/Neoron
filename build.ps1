#!/usr/bin/env pwsh

# Stop on first error
$ErrorActionPreference = 'Stop'

# Clean previous artifacts
Write-Host "Cleaning solution..." -ForegroundColor Cyan
Remove-Item -Path "artifacts" -Recurse -ErrorAction SilentlyContinue
dotnet clean

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Cyan
dotnet restore

# Build solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build --configuration Release --no-restore

# Run tests with coverage
Write-Host "Running tests..." -ForegroundColor Cyan
dotnet test --no-build --configuration Release `
    --settings ./tests/Neoron.API.Tests/test.runsettings `
    --collect:"XPlat Code Coverage" `
    --results-directory "./TestResults"

# Verify test results
if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed!"
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
