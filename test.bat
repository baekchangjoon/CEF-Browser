@echo off
echo Running tests...
msbuild CEF-Browser.Tests\CEF-Browser.Tests.csproj /p:Configuration=Release /t:Build
if %ERRORLEVEL% NEQ 0 (
    echo Test project build failed!
    exit /b 1
)

echo Tests completed. Use Visual Studio Test Explorer or NUnit console runner to execute tests.
