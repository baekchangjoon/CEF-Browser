# CEF Browser

Custom browser implementation using CefSharp (WinForms) for Windows.

## Features

- Full-featured browser with navigation controls
- Command-line URL support
- Custom user data directory support
- Chrome DevTools Protocol (CDP) support
- Installer with desktop shortcut

## Requirements

- .NET Framework 4.8
- .NET SDK 8.0 or later
- Windows OS
- Git (for version control)
- WiX Toolset 3.14.0 or later (for building installer)
  - CI automatically downloads and installs WiX Toolset
  - For local builds, install from: https://wixtoolset.org/releases/
  - Or use the setup script: `.\setup-build-tools.ps1`

## Building

### Using PowerShell Script (Recommended)
```powershell
.\build.ps1
```

### Using dotnet CLI
```powershell
# Restore dependencies
dotnet restore CEF-Browser.sln

# Build solution
dotnet build CEF-Browser.sln --configuration Release

# Run tests
dotnet test CEF-Browser.Tests/CEF-Browser.Tests.csproj --configuration Release

# Build installer project
dotnet build CEF-Browser.Installer/CEF-Browser.Installer.csproj --configuration Release

# Run installer to generate MSI (x86 path is correct for PlatformTarget=x86)
.\CEF-Browser.Installer\bin\x86\Release\net48\CEF-Browser.Installer.exe
```

The MSI installer will be generated in `InstallerOutput/CEF-Browser-Setup.msi`.

### Using Visual Studio
1. Open `CEF-Browser.sln` in Visual Studio
2. Restore NuGet packages
3. Build the solution (Release configuration)
4. Run tests from Test Explorer
5. Build `CEF-Browser.Installer` project
6. Run the installer executable to generate MSI

### Git Command Not Found?
If you get "git is not recognized" error after installing Git:
1. Restart PowerShell (recommended)
2. Or run: `.\refresh-git-path.ps1`
3. Or manually refresh PATH in current session

## Installation

### Creating the MSI Installer

To create the MSI installer, you need to build and run the installer project:

1. Build the `CEF-Browser.Installer` project:
   ```powershell
   dotnet build CEF-Browser.Installer/CEF-Browser.Installer.csproj --configuration Release
   ```

2. Run the installer executable to generate the MSI (x86 path is correct for PlatformTarget=x86):
   ```powershell
   .\CEF-Browser.Installer\bin\x86\Release\net48\CEF-Browser.Installer.exe
   ```

3. The MSI file will be created in `InstallerOutput/CEF-Browser-Setup.msi`

### Installing the Application

1. Run the generated `CEF-Browser-Setup.msi` installer
2. Follow the installation wizard
3. Desktop shortcut will be created automatically
4. The application will be installed to `%ProgramFiles%\CEF Browser`

## Usage

### Basic execution
```
CEF-Browser.exe
```

### Open specific URL
```
CEF-Browser.exe https://www.google.com
CEF-Browser.exe https://www.naver.com
```

### Specify user data directory
```
CEF-Browser.exe --user-data-dir C:\MyBrowserData
```

### Combined usage
```
CEF-Browser.exe --user-data-dir C:\MyBrowserData https://www.google.com
```

## Chrome DevTools Protocol (CDP)

The browser enables CDP on port 9222 by default. You can connect to it using:
- Chrome DevTools: `chrome://inspect`
- CDP client libraries
- Remote debugging tools

## Testing

Run the test project:
```powershell
dotnet test CEF-Browser.Tests/CEF-Browser.Tests.csproj --configuration Release
```

Or using the project directory:
```powershell
dotnet test CEF-Browser.Tests
```

## Project Structure

- `CEF-Browser/` - Main browser application
- `CEF-Browser.Installer/` - WiX installer project
- `CEF-Browser.Tests/` - Unit tests
