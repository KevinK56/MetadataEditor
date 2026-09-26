@echo off
set VERSION=v1.0.0
echo ========================================================
echo Building Metadata Editor - Self-Contained Single-File (%VERSION%)
echo ========================================================
echo.

dotnet publish src/MetadataEditor/MetadataEditor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./release/%VERSION%/single-file

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo Build Succeeded!
    echo Standalone executable created at:
    echo   release\%VERSION%\single-file\MetadataEditor.exe
    echo.
    echo This file can be run directly on any Windows 10/11 64-bit PC
    echo without needing any .NET runtime installed.
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed with error code %ERRORLEVEL%.
)

pause
