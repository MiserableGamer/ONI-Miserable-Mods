@echo off
setlocal
title PROMOTE ^& RELEASE - ONI Miserable Mods
color 0B
echo.
echo   ╔══════════════════════════════════════════════════╗
echo   ║   ◇  PROMOTE ^& RELEASE  ◇                        ║
echo   ║   Cherry-pick to master · Build · Tag · Push     ║
echo   ╚══════════════════════════════════════════════════╝
echo.
echo   Launching GUI...
echo.

set "SCRIPT_DIR=%~dp0"
set "ROOT=%SCRIPT_DIR%.."
cd /d "%ROOT%"

python "%SCRIPT_DIR%promote_release_gui.py"
if errorlevel 1 (
    echo.
    echo   [ERROR] Python or script failed. Make sure Python is installed.
    pause
    exit /b 1
)
exit /b 0
