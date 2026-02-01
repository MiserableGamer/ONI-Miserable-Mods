# Launcher for index_repo_gui.py - handles UNC paths (PowerShell supports them)
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$WorkspaceRoot = Split-Path -Parent $ScriptDir

if (Test-Path "$WorkspaceRoot\venv\Scripts\Activate.ps1") {
    & "$WorkspaceRoot\venv\Scripts\Activate.ps1"
} elseif (Test-Path "$ScriptDir\venv\Scripts\Activate.ps1") {
    & "$ScriptDir\venv\Scripts\Activate.ps1"
}

$exitCode = 0
try {
    & python "$ScriptDir\index_repo_gui.py"
    $exitCode = $LASTEXITCODE
} catch {
    Write-Host $_.Exception.Message
    $exitCode = 1
}

if ($exitCode -ne 0) {
    Write-Host "`nScript exited with code $exitCode"
    Read-Host "Press Enter to close"
}
exit $exitCode
