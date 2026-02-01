<#
.SYNOPSIS
    Releases a mod to GitHub.

.DESCRIPTION
    Interactive script that:
    1. Lists all available mod projects
    2. Prompts for project selection
    3. Builds the mod in Release mode
    4. Creates release package (zip)
    5. Automatically builds and releases the "All Mods" package
    6. Updates README.md if this is a new mod
    7. Commits, pushes, and creates git tags
    8. GitHub Actions handles the actual GitHub release
    9. Waits for workflow, then pulls README updates to keep local in sync

.PARAMETER NoSync
    Skip waiting for and pulling README updates from the workflow.
.PARAMETER SyncWaitSeconds
    Seconds to wait before pulling (default 90). Increase if workflow is slow.
.NOTES
    Script lives in scripts/; solution root = parent of scripts.
    Must be on the master branch.
    No version increment happens - versions should be set during development.
    Optional -Project parameter for non-interactive use (e.g. from GUI).
#>

param([string]$Project, [switch]$Force, [switch]$NoSync, [int]$SyncWaitSeconds = 90)

$ErrorActionPreference = "Stop"

# Solution root = parent of scripts folder
$SolutionRoot = Split-Path $PSScriptRoot -Parent
if (-not $SolutionRoot) {
    $SolutionRoot = Get-Location
}

# Ensure we run from solution root for all operations
Push-Location $SolutionRoot | Out-Null
try {

Write-Host "`n=== Release Mod ===" -ForegroundColor Cyan
Write-Host "Solution root: $SolutionRoot`n"

# Check we're on master branch
$currentBranch = git rev-parse --abbrev-ref HEAD 2>$null
if ($currentBranch -ne "master") {
    Write-Host "ERROR: Must be on 'master' branch to release." -ForegroundColor Red
    Write-Host "Current branch: $currentBranch" -ForegroundColor Yellow
    Write-Host "Run: git checkout master" -ForegroundColor Yellow
    exit 1
}

# Check for uncommitted changes
$uncommittedChanges = git status --porcelain 2>$null
if ($uncommittedChanges) {
    Write-Host "WARNING: You have uncommitted changes:" -ForegroundColor Yellow
    $uncommittedChanges | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    Write-Host ""
    $confirm = if ($Force) { 'y' } else { Read-Host "Continue anyway? (y/n)" }
    if ($confirm -ne 'y') {
        Write-Host "Aborted. Commit or stash your changes first." -ForegroundColor Yellow
        exit 0
    }
}

#region Helper Functions

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

function Get-ModVersion {
    param([string]$ProjectFolder)
    
    $modInfoPath = Join-Path (Join-Path $SolutionRoot $ProjectFolder) "mod_info.yaml"
    if (-not (Test-Path $modInfoPath)) {
        return "unknown"
    }
    
    $content = Get-Content $modInfoPath -Raw
    if ($content -match 'version:\s*(\d+\.\d+\.\d+)(?:\.\d+)?') {
        return $matches[1]
    }
    return "unknown"
}

function Get-AllModsVersion {
    $versionFile = Join-Path $SolutionRoot "all-mods-version.txt"
    if (Test-Path $versionFile) {
        return (Get-Content $versionFile -Raw).Trim()
    }
    return "1.0.0"
}

function Set-AllModsVersion {
    param([string]$Version)
    $versionFile = Join-Path $SolutionRoot "all-mods-version.txt"
    Set-Content -Path $versionFile -Value $Version -NoNewline
}

function Increment-AllModsVersion {
    param([bool]$IsNewMod = $false)
    
    $currentVersion = Get-AllModsVersion
    $parts = $currentVersion -split '\.'
    
    if ($IsNewMod) {
        $parts[1] = [int]$parts[1] + 1
        $parts[2] = 0
    } else {
        $parts[2] = [int]$parts[2] + 1
    }
    
    $newVersion = "$($parts[0]).$($parts[1]).$($parts[2])"
    Set-AllModsVersion -Version $newVersion
    return $newVersion
}

function Find-MSBuild {
    $msbuildPath = $null
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($vsPath) {
            $msbuildPath = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $msbuildPath) { return $msbuildPath }
        }
    }
    $commonPaths = @(
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($path in $commonPaths) {
        if (Test-Path $path) { return $path }
    }
    return $null
}

function Is-NewMod {
    param([string]$ModName)
    $readmePath = Join-Path $SolutionRoot "README.md"
    if (-not (Test-Path $readmePath)) { return $true }
    $readmeContent = Get-Content $readmePath -Raw
    return -not ($readmeContent -match "### .*$ModName" -or $readmeContent -match "\| $ModName \|")
}

#endregion

#region Main Script

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
    $selection = Read-Host "Enter project number to release (1-$($projects.Count))"
    $index = 0
    if (-not [int]::TryParse($selection, [ref]$index) -or $index -lt 1 -or $index -gt $projects.Count) {
        Write-Host "ERROR: Invalid selection" -ForegroundColor Red
        exit 1
    }
    $selectedProject = $projects[$index - 1]
}

$modVersion = Get-ModVersion -ProjectFolder $selectedProject
$isNewMod = Is-NewMod -ModName $selectedProject
Write-Host "`nSelected: $selectedProject v$modVersion" -ForegroundColor Cyan
if ($isNewMod) {
    Write-Host "This appears to be a NEW mod (not in README)" -ForegroundColor Yellow
}

$confirm = if ($Force) { 'y' } else { Read-Host "`nProceed with release? (y/n)" }
if ($confirm -ne 'y') {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 0
}

$msbuildPath = Find-MSBuild
if (-not $msbuildPath) {
    Write-Host "ERROR: MSBuild not found. Please install Visual Studio." -ForegroundColor Red
    exit 1
}
Write-Host "`nUsing MSBuild: $msbuildPath" -ForegroundColor Gray

#region Step 1: Build
Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "Step 1: Building $selectedProject in Release mode" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan

$projectFile = Join-Path (Join-Path $SolutionRoot $selectedProject) "$selectedProject.csproj"
if (-not (Test-Path $projectFile)) {
    $projectFile = Get-ChildItem -Path (Join-Path $SolutionRoot $selectedProject) -Filter "*.csproj" | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $projectFile -or -not (Test-Path $projectFile)) {
    Write-Host "ERROR: Project file not found for $selectedProject" -ForegroundColor Red
    exit 1
}

Push-Location $SolutionRoot
try {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore $projectFile
    if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: NuGet restore failed" -ForegroundColor Red; exit 1 }
    Write-Host "Building in Release mode..." -ForegroundColor Yellow
    & $msbuildPath $projectFile /p:Configuration=Release /p:Platform=AnyCPU /t:Rebuild /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: Build failed" -ForegroundColor Red; exit 1 }
    Write-Host "Build successful!" -ForegroundColor Green
} finally { Pop-Location }
#endregion

#region Step 2: Create release package
Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "Step 2: Creating release package" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan

$releaseDir = Join-Path (Join-Path $SolutionRoot "release") $selectedProject
$zipPath = Join-Path (Join-Path $SolutionRoot "release") "$selectedProject-v$modVersion.zip"
$outputPath = Join-Path (Join-Path (Join-Path $SolutionRoot $selectedProject) "bin") "Release"

if (Test-Path $releaseDir) { Remove-Item $releaseDir -Recurse -Force }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

# Prefer the mod's main DLL (e.g. ControlledStorage.dll); exclude dependencies
$excludePattern = "PLib|CommonLib|AsmResolver|Newtonsoft|0Harmony"
$allDlls = Get-ChildItem -Path $outputPath -Filter "*.dll" -ErrorAction SilentlyContinue | Where-Object { $_.Name -notmatch $excludePattern }
$dllPath = $allDlls | Where-Object { $_.BaseName -eq $selectedProject } | Select-Object -First 1
if (-not $dllPath) { $dllPath = $allDlls | Select-Object -First 1 }
if ($dllPath) {
    $destPath = Join-Path $releaseDir $dllPath.Name
    $copied = $false
    foreach ($attempt in 1..5) {
        try {
            Copy-Item $dllPath.FullName -Destination $destPath -Force
            $copied = $true
            break
        } catch {
            try {
                [System.IO.File]::Copy($dllPath.FullName, $destPath, $true)
                $copied = $true
                break
            } catch {
                if ($attempt -lt 5) { Start-Sleep -Milliseconds 500 }
                else { throw }
            }
        }
    }
    Write-Host "  Copied: $($dllPath.Name)"
} else {
    Write-Host "ERROR: No DLL found in $outputPath" -ForegroundColor Red
    exit 1
}

$modFolder = Join-Path $SolutionRoot $selectedProject
@("mod.yaml", "mod_info.yaml", "preview.png") | ForEach-Object {
    $filePath = Join-Path $modFolder $_
    if (Test-Path $filePath) { Copy-Item $filePath -Destination $releaseDir; Write-Host "  Copied: $_" }
}
$animPath = Join-Path $modFolder "anim"
if (Test-Path $animPath) { Copy-Item $animPath -Destination $releaseDir -Recurse; Write-Host "  Copied: anim folder" }

Push-Location (Join-Path $SolutionRoot "release")
try { Compress-Archive -Path $selectedProject -DestinationPath "$selectedProject-v$modVersion.zip" -Force }
finally { Pop-Location }

if (Test-Path $zipPath) {
    Write-Host "Created: $selectedProject-v$modVersion.zip" -ForegroundColor Green
} else {
    Write-Host "ERROR: Failed to create zip file" -ForegroundColor Red
    exit 1
}
#endregion

#region Step 3: All Mods package
Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "Step 3: Building All Mods package" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan

$oldAllVersion = Get-AllModsVersion
$newAllVersion = Increment-AllModsVersion -IsNewMod $isNewMod
Write-Host "All Mods version: $oldAllVersion -> $newAllVersion" -ForegroundColor Yellow

$releaseRoot = Join-Path $SolutionRoot "release"
$modFolders = Get-ChildItem -Path $releaseRoot -Directory | Where-Object { $_.Name -ne "MiserableGamersMods" }

if ($modFolders.Count -gt 0) {
    $combinedDir = Join-Path $releaseRoot "MiserableGamersMods"
    $combinedZip = Join-Path $releaseRoot "MiserableGamersMods.zip"
    if (Test-Path $combinedDir) { Remove-Item $combinedDir -Recurse -Force }
    if (Test-Path $combinedZip) { Remove-Item $combinedZip -Force }
    New-Item -ItemType Directory -Force -Path $combinedDir | Out-Null
    foreach ($folder in $modFolders) {
        Copy-Item $folder.FullName -Destination $combinedDir -Recurse
        Write-Host "  Added: $($folder.Name)"
    }
    Push-Location $releaseRoot
    try { Compress-Archive -Path "MiserableGamersMods" -DestinationPath "MiserableGamersMods.zip" -Force }
    finally { Pop-Location }
    Write-Host "Created: MiserableGamersMods.zip" -ForegroundColor Green
}
#endregion

#region Step 4: Git operations
Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "Step 4: Git operations" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan

Push-Location $SolutionRoot
try {
    git add -f "release/$selectedProject-v$modVersion.zip"
    git add -f "release/MiserableGamersMods.zip"
    git add "all-mods-version.txt"
    git commit -m "Release: $selectedProject v$modVersion"
    if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: Commit failed" -ForegroundColor Red; exit 1 }
    git pull --rebase origin master
    git push origin master
    if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: Push failed" -ForegroundColor Red; exit 1 }

    $modTag = "$selectedProject-v$modVersion"
    $allTag = "MiserableGamersMods-v$newAllVersion"
    foreach ($tag in @($modTag, $allTag)) {
        if (git tag -l $tag 2>$null) { git tag -d $tag 2>$null | Out-Null }
        if (git ls-remote --tags origin $tag 2>$null) {
            $delOut = [System.IO.Path]::GetTempFileName()
            $delErr = [System.IO.Path]::GetTempFileName()
            $null = Start-Process -FilePath "git" -ArgumentList "-C",$SolutionRoot,"push","origin",":refs/tags/$tag" -NoNewWindow -Wait -PassThru -RedirectStandardOutput $delOut -RedirectStandardError $delErr
            Remove-Item $delOut, $delErr -Force -ErrorAction SilentlyContinue
        }
    }
    git tag $modTag
    git tag $allTag
    foreach ($tag in @($modTag, $allTag)) {
        $outFile = [System.IO.Path]::GetTempFileName()
        $errFile = [System.IO.Path]::GetTempFileName()
        $p = Start-Process -FilePath "git" -ArgumentList "-C",$SolutionRoot,"push","origin",$tag -NoNewWindow -Wait -PassThru -RedirectStandardOutput $outFile -RedirectStandardError $errFile
        Get-Content $outFile, $errFile -ErrorAction SilentlyContinue | Where-Object { $_ } | Out-Host
        Remove-Item $outFile, $errFile -Force -ErrorAction SilentlyContinue
        if ($p.ExitCode -ne 0) {
            Write-Host "ERROR: Failed to push tag $tag (it may already exist from a previous release)" -ForegroundColor Red
            Write-Host "  To re-release, delete the remote tag first: git push origin :refs/tags/$tag" -ForegroundColor Yellow
            exit 1
        }
    }

    Write-Host "`nRelease completed successfully!" -ForegroundColor Green
    Write-Host "Tags: $modTag, $allTag" -ForegroundColor Cyan
    Write-Host "GitHub Actions will create the releases."

    # Step 5: Sync README after workflow updates it
    if (-not $NoSync) {
        Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
        Write-Host "Step 5: Syncing README from workflow" -ForegroundColor Cyan
        Write-Host ("=" * 60) -ForegroundColor Cyan
        Write-Host "Waiting $SyncWaitSeconds seconds for GitHub Actions to update README..." -ForegroundColor Yellow
        Start-Sleep -Seconds $SyncWaitSeconds
        git pull origin master
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Synced README and other workflow changes." -ForegroundColor Green
        } else {
            Write-Host "Pull failed - run 'git pull origin master' manually when workflow completes." -ForegroundColor Yellow
        }
    } else {
        Write-Host "`nRun 'git pull origin master' after the workflow completes to sync README changes." -ForegroundColor Yellow
    }
} finally { Pop-Location }
#endregion

} finally {
    Pop-Location | Out-Null
}
