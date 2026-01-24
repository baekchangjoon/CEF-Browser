# Complete setup and build script
# This script installs tools and builds the project

Write-Host "=== CEF Browser Complete Setup ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Install build tools
Write-Host "Step 1: Installing build tools..." -ForegroundColor Yellow
& .\install-build-tools.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build tools installation failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Build project
Write-Host "Step 2: Building project..." -ForegroundColor Yellow
& .\build.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host "You can now run the browser using:" -ForegroundColor Cyan
Write-Host "  .\run.ps1" -ForegroundColor White
Write-Host "  .\run.ps1 -Url https://www.google.com" -ForegroundColor White
Write-Host "  .\run.ps1 -UserDataDir C:\MyData -Url https://www.naver.com" -ForegroundColor White
