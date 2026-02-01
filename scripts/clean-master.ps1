<#
.SYNOPSIS
    Removes dev-only project folders from master branch.
    Run this ON master when dev-only projects have incorrectly been committed.

.DESCRIPTION
    Dev-only (never promoted): BlankProject, ElectrolyzerV2FIXED, ResourceLimpet,
    ResourceSensorFIXED, ControlledVisuals, ControlledLoaders, ControlledPerformance.
    Dev-only files: HeatingSystemTroubleshooting.html.
    Also removes "ONI Miserable Mods.slnx" if it exists (use ONIMiserableMods.slnx only).
#>
$ErrorActionPreference = "Stop"
$SolutionRoot = Split-Path $PSScriptRoot -Parent
Push-Location $SolutionRoot | Out-Null
try {
    $branch = git rev-parse --abbrev-ref HEAD
    if ($branch -ne "master") {
        Write-Host "ERROR: Must be on master branch. Current: $branch" -ForegroundColor Red
        exit 1
    }
    $devOnly = @('BlankProject', 'ElectrolyzerV2FIXED', 'ResourceLimpet', 'ResourceSensorFIXED', 'ControlledVisuals', 'ControlledLoaders', 'ControlledPerformance')
    $devOnlyFiles = @('HeatingSystemTroubleshooting.html')
    $toRemove = @()
    foreach ($dir in $devOnly) {
        $path = Join-Path $SolutionRoot $dir
        if (Test-Path $path) {
            $toRemove += $dir
        }
    }
    foreach ($f in $devOnlyFiles) {
        $path = Join-Path $SolutionRoot $f
        if (Test-Path $path) { $toRemove += $f }
    }
    $slnx = Join-Path $SolutionRoot "ONI Miserable Mods.slnx"
    if (Test-Path $slnx) { $toRemove += "ONI Miserable Mods.slnx" }
    if ($toRemove.Count -eq 0) {
        Write-Host "No dev-only items found on master." -ForegroundColor Green
        exit 0
    }
    Write-Host "Removing from master: $($toRemove -join ', ')" -ForegroundColor Yellow
    foreach ($item in $toRemove) {
        $full = Join-Path $SolutionRoot $item
        if (Test-Path $full) {
            if ((Test-Path $full -PathType Leaf) -or $item -like "*.slnx" -or $item -like "*.html") {
                git rm -f --cached $item 2>$null
            } else {
                git rm -r -f --cached $item 2>$null
            }
            if (Test-Path $full) { Remove-Item $full -Recurse -Force -ErrorAction SilentlyContinue }
        }
    }
    git status
    Write-Host "`nRun: git commit -m 'Remove dev-only projects from master'" -ForegroundColor Cyan
} finally { Pop-Location | Out-Null }
