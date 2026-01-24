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

## Building

### Using PowerShell Script (Recommended)
```powershell
.\build.ps1
```

### Using dotnet CLI
```powershell
dotnet build CEF-Browser\CEF-Browser.csproj -c Release
```

### Using Visual Studio
1. Open `CEF-Browser.sln` in Visual Studio
2. Restore NuGet packages
3. Build the solution (Release configuration)

### Git Command Not Found?
If you get "git is not recognized" error after installing Git:
1. Restart PowerShell (recommended)
2. Or run: `.\refresh-git-path.ps1`
3. Or manually refresh PATH in current session

## Installation

Run the installer project to create an MSI installer:
1. Build `CEF-Browser.Installer` project
2. Run the generated MSI installer
3. Desktop shortcut will be created automatically

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
```
dotnet test CEF-Browser.Tests
```

## Project Structure

- `CEF-Browser/` - Main browser application
- `CEF-Browser.Installer/` - WiX installer project
- `CEF-Browser.Tests/` - Unit tests
