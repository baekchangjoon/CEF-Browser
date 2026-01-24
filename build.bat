@echo off
echo Building CEF Browser solution...
msbuild CEF-Browser.sln /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild
if %ERRORLEVEL% EQU 0 (
    echo Build successful!
) else (
    echo Build failed!
    exit /b 1
)
