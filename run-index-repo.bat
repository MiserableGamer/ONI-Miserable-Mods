@echo off
set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
call "c:\oni-serena\scripts\run-index-repo.bat" "%ROOT%"
