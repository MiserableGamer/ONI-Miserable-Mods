@echo off
REM Run index_repo_gui.py if it exists, otherwise show message.
REM Set ONI_SOLUTION_ROOT to your mods repo path before running.
set "SCRIPT_DIR=d:\mcp\oni-serena\scripts\"
set "ROOT=%1"
if "%ROOT%"=="" set "ROOT=%ONI_SOLUTION_ROOT%"
if "%ROOT%"=="" set "ROOT=%CD%"
pushd "%ROOT%" 2>nul || cd /d "%ROOT%"
if exist "%SCRIPT_DIR%index_repo_gui.py" (
    python "%SCRIPT_DIR%index_repo_gui.py" "%ROOT%"
) else (
    echo index_repo_gui.py not found in c:\oni-serena\scripts
    echo Add it to index your ONI mods repo into Serena.
)
if errorlevel 1 pause
popd 2>nul
