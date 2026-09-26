@echo off
set VERSION=v1.0.0
echo ========================================================
echo Building Metadata Editor - Portable Framework-Dependent (%VERSION%)
echo ========================================================
echo.

dotnet publish src/MetadataEditor/MetadataEditor.csproj -c Release -o ./release/%VERSION%/portable

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo Build Succeeded!
    echo Lightweight build created at:
    echo   release\%VERSION%\portable\
    echo.
    echo Requires .NET 8 Desktop Runtime installed on the machine.
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed with error code %ERRORLEVEL%.
)

pause
