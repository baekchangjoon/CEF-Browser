# CEF Browser Run Script
# This script runs the CEF Browser with optional arguments

param(
    [string]$Url = "",
    [string]$UserDataDir = "",
    [string]$Configuration = "Release"
)

Write-Host "=== CEF Browser Run ===" -ForegroundColor Cyan
Write-Host ""

$exePath = "CEF-Browser\bin\x86\$Configuration\net48\CEF-Browser.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "[ERROR] CEF-Browser.exe not found at: $exePath" -ForegroundColor Red
    Write-Host "Please build the project first using: .\build.ps1" -ForegroundColor Yellow
    exit 1
}

# Build arguments
$args = @()
if ($UserDataDir) {
    $args += "--user-data-dir"
    $args += $UserDataDir
}
if ($Url) {
    $args += $Url
}

Write-Host "Starting CEF Browser..." -ForegroundColor Yellow
if ($args.Count -gt 0) {
    Write-Host "Arguments: $($args -join ' ')" -ForegroundColor Cyan
}

& $exePath $args
