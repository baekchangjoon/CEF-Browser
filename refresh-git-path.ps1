# Refresh PATH to include Git
# Run this script if git command is not recognized in your terminal

$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

Write-Host "PATH refreshed. Testing git..." -ForegroundColor Yellow
git --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "Git is now available!" -ForegroundColor Green
} else {
    Write-Host "Git is still not available. Please restart PowerShell." -ForegroundColor Red
}
