@echo off
REM Linkbar Optimized Build Script
REM Builds Linkbar with maximum size and performance optimizations

echo ========================================
echo   Linkbar Optimized Build Script
echo ========================================
echo.

REM Check if we're in the correct directory
if not exist "src\Linkbar.dpr" (
    echo ERROR: Linkbar.dpr not found in src directory
    echo Please run this script from the Linkbar root directory
    pause
    exit /b 1
)

echo [1/5] Cleaning build artifacts...
if exist "src\out" rmdir /s /q "src\out"
if exist "exe\*.exe" del /q "exe\*.exe"
if exist "*.map" del /q "*.map"
if exist "*.drc" del /q "*.drc"
echo Cleanup complete.
echo.

echo [2/5] Building Release configuration (Win64)...
echo This will take a moment...
echo.

REM Try to find Delphi compiler
set DELPHI_BIN=
if exist "C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" set DELPHI_BIN=C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe
if exist "C:\Program Files (x86)\Embarcadero\Studio\22.0\bin\dcc64.exe" set DELPHI_BIN=C:\Program Files (x86)\Embarcadero\Studio\22.0\bin\dcc64.exe
if exist "C:\Program Files (x86)\Embarcadero\Studio\21.0\bin\dcc64.exe" set DELPHI_BIN=C:\Program Files (x86)\Embarcadero\Studio\21.0\bin\dcc64.exe

if "%DELPHI_BIN%"=="" (
    echo WARNING: Delphi compiler not found in default locations
    echo Please ensure Delphi is installed or update this script
    echo.
    echo Attempting to use MSBuild...
    msbuild src\Linkbar.dproj /p:Config=Release /p:Platform=Win64 /t:Build /v:minimal
) else (
    "%DELPHI_BIN%" src\Linkbar.dpr -$D- -$L- -$C- -$O+ -$Q+ -B -NSVcl;System;Winapi
)

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Check the compiler output above for errors.
    pause
    exit /b 1
)

echo.
echo [3/5] Verifying build output...
if not exist "exe\LinkbarWin64.exe" (
    echo ERROR: LinkbarWin64.exe not found in exe directory
    pause
    exit /b 1
)

for %%A in ("exe\LinkbarWin64.exe") do set SIZE=%%~zA
set /a SIZEMB=%SIZE% / 1048576
echo Build successful!
echo File size: %SIZE% bytes (~%SIZEMB% MB)
echo.

echo [4/5] Removing debug files...
if exist "src\*.dcu" del /q "src\*.dcu"
if exist "src\*.~*" del /q "src\*.~*"
if exist "exe\*.map" del /q "exe\*.map"
if exist "exe\*.drc" del /q "exe\*.drc"
echo Cleanup complete.
echo.

echo [5/5] Build Summary
echo ========================================
echo Configuration: Release
echo Platform: Win64
echo Optimizations: Enabled (Maximum)
echo Output: exe\LinkbarWin64.exe
echo Size: %SIZE% bytes
echo ========================================
echo.

echo Optional: Apply UPX compression for even smaller size?
echo This will add ~50ms to startup time.
echo.
set /p APPLY_UPX="Compress with UPX? (y/N): "
if /i "%APPLY_UPX%"=="y" (
    echo.
    echo Checking for UPX...
    where upx >nul 2>&1
    if errorlevel 1 (
        echo UPX not found. Download from: https://upx.github.io/
        echo Skipping compression.
    ) else (
        echo Compressing...
        upx --best --lzma exe\LinkbarWin64.exe
        if errorlevel 1 (
            echo WARNING: UPX compression failed
        ) else (
            echo Compression successful!
            for %%A in ("exe\LinkbarWin64.exe") do set SIZE2=%%~zA
            set /a SIZEMB2=%SIZE2% / 1048576
            echo New size: %SIZE2% bytes (~%SIZEMB2% MB)
            set /a SAVED=(%SIZE%-%SIZE2%)*100/%SIZE%
            echo Space saved: %SAVED%%%
        )
    )
)

echo.
echo ========================================
echo Build complete!
echo 
echo Executable location: exe\LinkbarWin64.exe
echo To run: cd exe && LinkbarWin64.exe
echo ========================================
echo.
pause
