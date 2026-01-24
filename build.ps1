# CEF Browser Build Script
# This script builds the CEF Browser project

param(
    [string]$Configuration = "Release",
    [switch]$Restore = $true,
    [switch]$Clean = $false
)

Write-Host "=== CEF Browser Build ===" -ForegroundColor Cyan
Write-Host ""

# Check for .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] .NET SDK is required. Please install from: https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] .NET SDK version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Restore NuGet packages
if ($Restore) {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore CEF-Browser\CEF-Browser.csproj -p:Configuration=$Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] NuGet package restoration failed" -ForegroundColor Red
        exit 1
    }
    dotnet restore CEF-Browser.Tests\CEF-Browser.Tests.csproj -p:Configuration=$Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[WARNING] Test package restoration failed" -ForegroundColor Yellow
    }
    Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
    Write-Host ""
}

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning solution..." -ForegroundColor Yellow
    dotnet clean CEF-Browser\CEF-Browser.csproj -c $Configuration
    dotnet clean CEF-Browser.Tests\CEF-Browser.Tests.csproj -c $Configuration
    Write-Host "[OK] Solution cleaned" -ForegroundColor Green
    Write-Host ""
}

# Build solution
Write-Host "Building solution ($Configuration)..." -ForegroundColor Yellow
dotnet build CEF-Browser\CEF-Browser.csproj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Building tests..." -ForegroundColor Yellow
dotnet build CEF-Browser.Tests\CEF-Browser.Tests.csproj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "[WARNING] Test build failed" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "[OK] Build successful!" -ForegroundColor Green
Write-Host "Output: CEF-Browser\bin\x86\$Configuration\net48\CEF-Browser.exe" -ForegroundColor Cyan
