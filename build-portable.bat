@echo off
setlocal enabledelayedexpansion

:: Compute dynamic version: YYYY.MM.DD.CommitCount
for /f "tokens=*" %%a in ('powershell -NoProfile -Command "Get-Date -Format yyyy.MM.dd"') do set BUILD_DATE=%%a
for /f "tokens=*" %%a in ('git rev-list --count HEAD 2^>nul') do set COMMIT_COUNT=%%a
if "%COMMIT_COUNT%"=="" set COMMIT_COUNT=1

set VERSION=%BUILD_DATE%.%COMMIT_COUNT%
set TAG=v%VERSION%

echo ========================================================
echo Building Metadata Editor - Portable Framework-Dependent
echo Version: %VERSION% (Tag: %TAG%)
echo ========================================================
echo.

dotnet publish src/MetadataEditor/MetadataEditor.csproj -c Release -p:AssemblyVersion=%VERSION% -p:FileVersion=%VERSION% -p:InformationalVersion=%TAG% -o ./release/%TAG%/portable

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo Build Succeeded!
    echo Lightweight build created at:
    echo   release\%TAG%\portable\
    echo.
    echo Requires .NET 8 Desktop Runtime installed on the machine.
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed with error code %ERRORLEVEL%.
)

pause

