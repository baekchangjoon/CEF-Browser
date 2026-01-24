# CEF Browser Build Tools Installation Script
# This script installs necessary build tools for CEF Browser

Write-Host "=== CEF Browser Build Tools Installation ===" -ForegroundColor Cyan
Write-Host ""

# Check for .NET Framework 4.8
Write-Host "Checking .NET Framework 4.8..." -ForegroundColor Yellow
$netFramework48 = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction SilentlyContinue
if ($netFramework48 -and $netFramework48.Release -ge 528040) {
    Write-Host "[OK] .NET Framework 4.8 is installed" -ForegroundColor Green
} else {
    Write-Host "[MISSING] .NET Framework 4.8 is required" -ForegroundColor Red
    Write-Host "Please download and install from: https://dotnet.microsoft.com/download/dotnet-framework/net48" -ForegroundColor Yellow
    Write-Host "After installation, run this script again." -ForegroundColor Yellow
    exit 1
}

# Find MSBuild
Write-Host "`nSearching for MSBuild..." -ForegroundColor Yellow
$msbuildPaths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
)

$msbuildPath = $null
foreach ($path in $msbuildPaths) {
    if (Test-Path $path) {
        $msbuildPath = $path
        Write-Host "[OK] MSBuild found at: $path" -ForegroundColor Green
        break
    }
}

if (-not $msbuildPath) {
    Write-Host "[MISSING] MSBuild is required" -ForegroundColor Red
    Write-Host "Please install Visual Studio Build Tools:" -ForegroundColor Yellow
    Write-Host "1. Download from: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022" -ForegroundColor Yellow
    Write-Host "2. Install with: .NET desktop build tools workload" -ForegroundColor Yellow
    Write-Host "3. Run this script again after installation." -ForegroundColor Yellow
    exit 1
}

# Download NuGet if not exists
Write-Host "`nChecking NuGet..." -ForegroundColor Yellow
$nugetPath = "${env:ProgramFiles}\NuGet\nuget.exe"
if (-not (Test-Path $nugetPath)) {
    $nugetDir = "${env:ProgramFiles}\NuGet"
    if (-not (Test-Path $nugetDir)) {
        New-Item -ItemType Directory -Path $nugetDir -Force | Out-Null
    }
    
    Write-Host "Downloading NuGet..." -ForegroundColor Yellow
    $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    try {
        Invoke-WebRequest -Uri $nugetUrl -OutFile $nugetPath -UseBasicParsing
        Write-Host "[OK] NuGet downloaded to: $nugetPath" -ForegroundColor Green
    } catch {
        Write-Host "[ERROR] Failed to download NuGet: $_" -ForegroundColor Red
        Write-Host "Please download manually from: https://www.nuget.org/downloads" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "[OK] NuGet found at: $nugetPath" -ForegroundColor Green
}

# Save paths to config file
$config = @{
    MSBuildPath = $msbuildPath
    NuGetPath = $nugetPath
} | ConvertTo-Json

$configPath = "build-config.json"
$config | Out-File -FilePath $configPath -Encoding UTF8
Write-Host "`n[OK] Build configuration saved to: $configPath" -ForegroundColor Green

Write-Host "`n=== Installation Complete ===" -ForegroundColor Cyan
Write-Host "You can now run: .\build.ps1" -ForegroundColor Green
