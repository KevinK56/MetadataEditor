@echo off
setlocal enabledelayedexpansion

:: Compute dynamic version: YYYY.MM.DD.CommitCount
for /f "tokens=*" %%a in ('powershell -NoProfile -Command "Get-Date -Format yyyy.MM.dd"') do set BUILD_DATE=%%a
for /f "tokens=*" %%a in ('git rev-list --count HEAD 2^>nul') do set COMMIT_COUNT=%%a
if "%COMMIT_COUNT%"=="" set COMMIT_COUNT=1

set VERSION=%BUILD_DATE%.%COMMIT_COUNT%
set TAG=v%VERSION%

echo ========================================================
echo Building Metadata Editor - Self-Contained Single-File
echo Version: %VERSION% (Tag: %TAG%)
echo ========================================================
echo.

dotnet publish src/MetadataEditor/MetadataEditor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:AssemblyVersion=%VERSION% -p:FileVersion=%VERSION% -p:InformationalVersion=%TAG% -o ./release/%TAG%/single-file

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo Build Succeeded!
    echo Standalone executable created at:
    echo   release\%TAG%\single-file\MetadataEditor.exe
    echo.
    echo This file can be run directly on any Windows 10/11 64-bit PC
    echo without needing any .NET runtime installed.
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed with error code %ERRORLEVEL%.
)

pause

