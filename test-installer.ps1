# Test Installer Script
# This script tests the MSI installer

param(
    [switch]$Install = $false,
    [switch]$Uninstall = $false
)

$msiPath = "InstallerOutput\CEF-Browser-Setup.msi"
$productName = "CEF Browser"

if (-not (Test-Path $msiPath)) {
    Write-Host "[ERROR] MSI file not found: $msiPath" -ForegroundColor Red
    Write-Host "Please build the installer first: dotnet build CEF-Browser.Installer\CEF-Browser.Installer.csproj -c Release" -ForegroundColor Yellow
    exit 1
}

if ($Uninstall) {
    Write-Host "Uninstalling $productName..." -ForegroundColor Yellow
    $product = Get-WmiObject -Class Win32_Product | Where-Object { $_.Name -eq $productName }
    if ($product) {
        $product.Uninstall()
        Write-Host "[OK] Uninstalled" -ForegroundColor Green
    } else {
        Write-Host "[INFO] Product not found" -ForegroundColor Cyan
    }
    exit 0
}

if ($Install) {
    Write-Host "Installing $productName..." -ForegroundColor Yellow
    Write-Host "MSI Path: $msiPath" -ForegroundColor Cyan
    
    $msiFullPath = Resolve-Path $msiPath
    Start-Process msiexec.exe -ArgumentList "/i `"$msiFullPath`" /qb" -Wait -Verb RunAs
    
    Write-Host "[OK] Installation completed" -ForegroundColor Green
    Write-Host ""
    Write-Host "Checking installation..." -ForegroundColor Yellow
    
    # Check if installed
    $installedPath = "${env:ProgramFiles}\CEF Browser\CEF-Browser.exe"
    if (Test-Path $installedPath) {
        Write-Host "[OK] Installed at: $installedPath" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Installation path not found" -ForegroundColor Yellow
    }
    
    # Check desktop shortcut
    $desktopShortcut = "$env:USERPROFILE\Desktop\CEF Browser.lnk"
    if (Test-Path $desktopShortcut) {
        Write-Host "[OK] Desktop shortcut found" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Desktop shortcut not found" -ForegroundColor Yellow
    }
    
    exit 0
}

Write-Host "Usage:" -ForegroundColor Cyan
Write-Host "  .\test-installer.ps1 -Install    # Install the application" -ForegroundColor White
Write-Host "  .\test-installer.ps1 -Uninstall  # Uninstall the application" -ForegroundColor White
