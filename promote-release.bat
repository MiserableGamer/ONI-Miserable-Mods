@echo off
title PROMOTE ^& RELEASE - ONI Miserable Mods
color 0B
echo.
echo   +==================================================+
echo   ^|   PROMOTE ^& RELEASE - ONI Miserable Mods          ^|
echo   ^|   Cherry-pick to master - Build - Tag - Push      ^|
echo   +==================================================+
echo.
echo   Launching GUI (scripts from c:\oni-serena\scripts)...
echo.

set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
pushd "%ROOT%" 2>nul || cd /d "%ROOT%"

python "c:\oni-serena\scripts\promote_release_gui.py" "%ROOT%"
if errorlevel 1 (
    echo.
    echo   [ERROR] Python or script failed. Ensure Python is installed and c:\oni-serena\scripts exists.
    pause
    popd 2>nul
    exit /b 1
)
popd 2>nul
exit /b 0
