@echo off
REM Create English-only version of Linkbar
REM Removes all non-English localization files to reduce size

echo ========================================
echo   Linkbar English-Only Build
echo ========================================
echo.

if not exist "exe\Locales\en-US.ini" (
    echo ERROR: en-US.ini not found
    pause
    exit /b 1
)

echo This script will remove all non-English localization files.
echo This reduces installation size by ~200-300 KB.
echo.
echo Backing up to Locales\Backup folder...
if not exist "exe\Locales\Backup" mkdir "exe\Locales\Backup"

cd exe\Locales

echo.
echo Backing up all localization files...
copy /Y *.ini Backup\ >nul 2>&1

echo.
echo Removing non-English files...
for %%f in (zh-CN.ini ru-RU.ini pt-BR.ini pl-PL.ini ko-KR.ini ja-JP.ini it-IT.ini id-ID.ini fr-FR.ini es-ES.ini el-GR.ini de-DE.ini) do (
    if exist "%%f" (
        del "%%f"
        echo Removed: %%f
    )
)

echo.
echo Keeping English:
dir /B en-US.ini

echo.
echo Size comparison:
for %%A in (Backup\*) do set SIZE_BACKUP=%%~zA
for %%A in (en-US.ini) do set SIZE_EN=%%~zA
set /a SIZE_SAVED=%SIZE_BACKUP%-%SIZE_EN%
set /a PERCENT_SAVED=%SIZE_SAVED%*100/%SIZE_BACKUP%

echo Original (all languages): %SIZE_BACKUP% bytes
echo English-only: %SIZE_EN% bytes
echo Saved: %SIZE_SAVED% bytes (%PERCENT_SAVED%%%)

echo.
echo ========================================
echo English-only build complete!
echo 
echo To restore all languages, run:
echo   copy /Y Backup\*.ini .
echo ========================================
echo.

cd ..\..
pause
