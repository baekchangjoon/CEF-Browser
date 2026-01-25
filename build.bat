@echo off
echo Building CEF Browser solution...
msbuild CEF-Browser.sln /p:Configuration=Release /p:Platform="x86" /t:Rebuild
if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo Output: CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe
) else (
    echo Build failed!
    exit /b 1
)
