# CEF Browser Build Tools Setup Script
# This script installs necessary build tools for CEF Browser

Write-Host "Checking for required build tools..." -ForegroundColor Cyan

# Check for .NET Framework 4.8
$netFramework48 = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction SilentlyContinue
if ($netFramework48 -and $netFramework48.Release -ge 528040) {
    Write-Host "[OK] .NET Framework 4.8 is installed" -ForegroundColor Green
} else {
    Write-Host "[MISSING] .NET Framework 4.8 is required" -ForegroundColor Yellow
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet-framework/net48" -ForegroundColor Yellow
}

# Check for MSBuild
$msbuildPaths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
)

$msbuildFound = $false
foreach ($path in $msbuildPaths) {
    if (Test-Path $path) {
        Write-Host "[OK] MSBuild found at: $path" -ForegroundColor Green
        $msbuildFound = $true
        break
    }
}

if (-not $msbuildFound) {
    Write-Host "[MISSING] MSBuild is required" -ForegroundColor Yellow
    Write-Host "Download Visual Studio Build Tools from: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022" -ForegroundColor Yellow
    Write-Host "Install with: .NET desktop build tools workload" -ForegroundColor Yellow
}

# Check for NuGet
$nugetPath = "${env:ProgramFiles}\NuGet\nuget.exe"
if (Test-Path $nugetPath) {
    Write-Host "[OK] NuGet found at: $nugetPath" -ForegroundColor Green
} else {
    Write-Host "[INFO] NuGet will be downloaded automatically by MSBuild" -ForegroundColor Cyan
}

Write-Host "`nSetup check complete!" -ForegroundColor Cyan
