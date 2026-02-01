@echo off
cd /d "%~dp0.."
python "%~dp0index_repo_gui.py"
if errorlevel 1 pause
