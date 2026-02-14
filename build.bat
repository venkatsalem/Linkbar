@echo off
REM Simple Build Script for Linkbar
REM Builds Release configuration (Win64)

echo ========================================
echo   Linkbar Build Script
echo ========================================
echo.

REM Check if we're in correct directory
if not exist "src\Linkbar.dpr" (
    echo ERROR: Linkbar.dpr not found
    echo Please run this script from the Linkbar directory
    pause
    exit /b 1
)

echo Building Linkbar (Release, Win64)...
echo.

REM Try to find and use Delphi compiler
set DELPHI_PATH=
if exist "C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" (
    set DELPHI_PATH=C:\Program Files (x86)\Embarcadero\Studio\23.0\bin
    echo Found: Delphi 11 (Athens)
)
if exist "C:\Program Files (x86)\Embarcadero\Studio\22.0\bin\dcc64.exe" (
    set DELPHI_PATH=C:\Program Files (x86)\Embarcadero\Studio\22.0\bin
    echo Found: Delphi 10.4 (Sydney)
)
if exist "C:\Program Files (x86)\Embarcadero\Studio\21.0\bin\dcc64.exe" (
    set DELPHI_PATH=C:\Program Files (x86)\Embarcadero\Studio\21.0\bin
    echo Found: Delphi 10.3 (Rio)
)

if "%DELPHI_PATH%"=="" (
    echo.
    echo ERROR: Delphi compiler not found!
    echo.
    echo Please install Embarcadero Delphi 10.3 or higher
    echo Download: https://www.embarcadero.com/products/delphi
    echo.
    pause
    exit /b 1
)

echo.
echo Building...
"%DELPHI_PATH%\dcc64.exe" src\Linkbar.dpr -B -DRELEASE -$O+ -$L- -$C- -$Q+ -NSSystem;Vcl;Winapi

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above.
    pause
    exit /b 1
)

echo.
echo Build successful!
echo.

if exist "exe\LinkbarWin64.exe" (
    for %%A in ("exe\LinkbarWin64.exe") do set SIZE=%%~zA
    set /a SIZEMB=%SIZE% / 1048576
    echo Output: exe\LinkbarWin64.exe
    echo Size: %SIZE% bytes (~%SIZEMB% MB)
) else (
    echo WARNING: Output file not found
)

echo.
echo ========================================
echo Build complete!
echo.
echo To run: cd exe && LinkbarWin64.exe
echo ========================================
pause
