<#
.SYNOPSIS
    Promotes a mod from development branch to master branch.

.DESCRIPTION
    Interactive script that:
    1. Lists all available mod projects
    2. Prompts for project selection
    3. Commits any uncommitted changes (local only)
    4. Cherry-picks the mod's commits to master
    5. Returns to development branch

.NOTES
    Script lives in scripts/; solution root = parent of scripts.
    Must be on the development branch to start.
    Optional -Project parameter for non-interactive use (e.g. from GUI).
#>

param([string]$Project, [switch]$Force)

$ErrorActionPreference = "Stop"

# Solution root = parent of scripts folder
$SolutionRoot = Split-Path $PSScriptRoot -Parent
if (-not $SolutionRoot) {
    $SolutionRoot = Get-Location
}

# Ensure we run from solution root for git commands
Push-Location $SolutionRoot | Out-Null
try {

Write-Host "`n=== Promote Mod to Master ===" -ForegroundColor Cyan
Write-Host "Solution root: $SolutionRoot`n"

# Check we're on development branch
$currentBranch = git rev-parse --abbrev-ref HEAD 2>$null
if ($currentBranch -ne "development") {
    Write-Host "ERROR: Must be on 'development' branch to promote." -ForegroundColor Red
    Write-Host "Current branch: $currentBranch" -ForegroundColor Yellow
    Write-Host "Run: git checkout development" -ForegroundColor Yellow
    exit 1
}

# Discover all mod projects (folders with .csproj AND mod_info.yaml)
function Get-ModProjects {
    $projects = @()
    $devOnly = @('BlankProject', 'scripts', 'ElectrolyzerV2FIXED', 'ResourceLimpet', 'ResourceSensorFIXED', 'ControlledVisuals', 'ControlledLoaders', 'ControlledPerformance')
    $folders = Get-ChildItem -Path $SolutionRoot -Directory | Where-Object { 
        $_.Name -notmatch '^\.' -and 
        $_.Name -ne 'lib' -and 
        $_.Name -ne 'release' -and
        $_.Name -ne 'packages' -and
        $_.Name -ne 'infrastructure' -and
        $_.Name -notin $devOnly
    }
    
    foreach ($folder in $folders) {
        $csproj = Get-ChildItem -Path $folder.FullName -Filter "*.csproj" -File | Select-Object -First 1
        $modInfo = Join-Path $folder.FullName "mod_info.yaml"
        
        if ($csproj -and (Test-Path $modInfo)) {
            $projects += $folder.Name
        }
    }
    
    return $projects | Sort-Object
}

# Get version from mod_info.yaml
function Get-ModVersion {
    param([string]$ProjectFolder)
    
    $modInfoPath = Join-Path (Join-Path $SolutionRoot $ProjectFolder) "mod_info.yaml"
    if (-not (Test-Path $modInfoPath)) {
        return "unknown"
    }
    
    $content = Get-Content $modInfoPath -Raw
    if ($content -match 'version:\s*(\d+\.\d+\.\d+(?:\.\d+)?)') {
        return $matches[1]
    }
    return "unknown"
}

# List and select project
$projects = Get-ModProjects

if ($projects.Count -eq 0) {
    Write-Host "ERROR: No mod projects found in $SolutionRoot" -ForegroundColor Red
    exit 1
}

if ($Project) {
    if ($projects -notcontains $Project) {
        Write-Host "ERROR: Project '$Project' not found in solution." -ForegroundColor Red
        exit 1
    }
    $selectedProject = $Project
} else {
    Write-Host "Available mod projects:" -ForegroundColor Green
    for ($i = 0; $i -lt $projects.Count; $i++) {
        $version = Get-ModVersion -ProjectFolder $projects[$i]
        Write-Host "  [$($i + 1)] $($projects[$i]) (v$version)"
    }

    Write-Host ""
    $selection = Read-Host "Enter project number to promote (1-$($projects.Count))"

    # Validate selection
    $index = 0
    if (-not [int]::TryParse($selection, [ref]$index) -or $index -lt 1 -or $index -gt $projects.Count) {
        Write-Host "ERROR: Invalid selection" -ForegroundColor Red
        exit 1
    }

    $selectedProject = $projects[$index - 1]
}
$version = Get-ModVersion -ProjectFolder $selectedProject

Write-Host "`nSelected: $selectedProject v$version" -ForegroundColor Cyan

# Check for uncommitted changes in the project folder
$projectPath = Join-Path $SolutionRoot $selectedProject
$uncommitted = git status --porcelain -- $selectedProject 2>$null

if ($uncommitted) {
    Write-Host "`nUncommitted changes detected in $selectedProject" -ForegroundColor Yellow
    Write-Host $uncommitted
    
    $confirm = if ($Force) { 'y' } else { Read-Host "`nCommit these changes? (y/n)" }
    if ($confirm -ne 'y') {
        Write-Host "Aborted." -ForegroundColor Yellow
        exit 0
    }
    
    # Stage and commit changes
    Write-Host "`nCommitting changes..." -ForegroundColor Green
    git add -- $selectedProject
    $commitMessage = "development complete for $selectedProject v$version"
    git commit -m $commitMessage
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Commit failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Committed: $commitMessage" -ForegroundColor Green
}

# Get commits for this project that aren't in master
Write-Host "`nFinding commits to cherry-pick..." -ForegroundColor Cyan
$commits = git log --oneline "master..development" -- $selectedProject 2>$null

if (-not $commits) {
    Write-Host "No new commits for $selectedProject to promote." -ForegroundColor Yellow
    exit 0
}

Write-Host "Commits to promote:"
$commits | ForEach-Object { Write-Host "  $_" }

$confirm = if ($Force) { 'y' } else { Read-Host "`nProceed with cherry-pick to master? (y/n)" }
if ($confirm -ne 'y') {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 0
}

# Switch to master
Write-Host "`nSwitching to master branch..." -ForegroundColor Cyan
git checkout master

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to switch to master" -ForegroundColor Red
    exit 1
}

# Cherry-pick each commit (oldest first)
$commitHashes = git log --reverse --format="%H" "master..development" -- $selectedProject 2>$null

foreach ($hash in $commitHashes) {
    Write-Host "Cherry-picking $hash..." -ForegroundColor Green
    git cherry-pick $hash
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Cherry-pick failed. Resolve conflicts and run:" -ForegroundColor Red
        Write-Host "  git cherry-pick --continue" -ForegroundColor Yellow
        Write-Host "  # Then switch back to development:" -ForegroundColor Yellow
        Write-Host "  git checkout development" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "`nSuccessfully promoted $selectedProject to master!" -ForegroundColor Green

# Sync .cursor/mcp.json from development (MCP config shared across branches)
$mcpPath = ".cursor/mcp.json"
$devMcp = git show "development:.cursor/mcp.json" 2>$null
if ($devMcp) {
    $masterMcp = git show "HEAD:.cursor/mcp.json" 2>$null
    if (-not $masterMcp -or $devMcp.Trim() -ne $masterMcp.Trim()) {
        Write-Host "`nSyncing .cursor/mcp.json from development..." -ForegroundColor Cyan
        git checkout development -- $mcpPath
        git add $mcpPath
        git commit -m "Sync .cursor/mcp.json from development"
        Write-Host "Committed .cursor/mcp.json" -ForegroundColor Green
    }
}

# Switch back to development
Write-Host "`nSwitching back to development branch..." -ForegroundColor Cyan
git checkout development

Write-Host "`n=== Promotion Complete ===" -ForegroundColor Cyan
Write-Host "Next steps:"
Write-Host "  1. Switch to master: git checkout master"
Write-Host "  2. Build in Release mode to verify"
Write-Host "  3. Run .\scripts\release-mod.ps1 when ready to release"

} finally {
    Pop-Location | Out-Null
}
