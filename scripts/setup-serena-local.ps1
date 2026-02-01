# Creates Serena workspace. Run AFTER rename-to-onimiserablemods.ps1
#
# C:\oni-serena\
#   ONIMiserableMods  -> network (D:\ or UNC) - solution stays on network
#   indexed\          REAL directory (source/prime/reference) - Serena requires subpath, no symlinks
#   .serena\          (config)
#
# indexed must be a real subdir so Serena find_symbol relative_path works. Symlinks resolve outside project root.

$UncRoot = if (Test-Path "D:\ONIMiserableMods") { "D:\" } else { "\\192.168.1.30\development\ONI\Mods" }
$WorkspaceRoot = "C:\oni-serena"
$IndexedDir = Join-Path $WorkspaceRoot "indexed"

if (-not (Test-Path "$UncRoot\ONIMiserableMods")) {
    Write-Host "ONIMiserableMods not found at $UncRoot - run rename-to-onimiserablemods.ps1 first"
    exit 1
}

New-Item -ItemType Directory -Path $WorkspaceRoot -Force | Out-Null

# Create indexed as REAL directory (copy from C:\oni-indexed if migrating, else network)
$IndexedSource = if (Test-Path "C:\oni-indexed\source") { "C:\oni-indexed" } else { "$UncRoot\indexed" }
New-Item -ItemType Directory -Path $IndexedDir -Force | Out-Null
foreach ($tier in @("source", "prime", "reference")) {
    $destTier = Join-Path $IndexedDir $tier
    $srcTier = Join-Path $IndexedSource $tier
    New-Item -ItemType Directory -Path $destTier -Force | Out-Null
    if (Test-Path $srcTier) {
        Write-Host "Copying indexed/$tier from $IndexedSource..."
        robocopy $srcTier $destTier /E /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null
    }
}
Write-Host "Indexed (real dir): $IndexedDir"

# ONIMiserableMods -> network (solution stays on network)
$path = Join-Path $WorkspaceRoot "ONIMiserableMods"
if (Test-Path $path) {
    (Get-Item $path -Force).Delete()
    Write-Host "Removed old: ONIMiserableMods"
}
New-Item -ItemType SymbolicLink -Path $path -Target "$UncRoot\ONIMiserableMods" | Out-Null
Write-Host "Created symlink: ONIMiserableMods -> $UncRoot\ONIMiserableMods"

# Create .serena
$serenaDir = Join-Path $WorkspaceRoot ".serena"
$memoriesDir = Join-Path $serenaDir "memories"
New-Item -ItemType Directory -Path $memoriesDir -Force | Out-Null

$projectYml = @"
# Serena project - indexed on C: for speed
project_name: "ONI Mods"

languages:
- csharp

encoding: "utf-8"
ignore_all_files_in_gitignore: true

ignored_paths:
  - "**/obj/**"
  - "**/bin/**"
  - "**/obj"
  - "**/bin"
  - "ONIMiserableMods/jarvis/**"

read_only: false
excluded_tools: []
included_optional_tools: []
fixed_tools: []
base_modes:
default_modes:

initial_prompt: |
  Reference code hierarchy (use find_symbol relative_path):
  - indexed/source: Game source. Vanilla prefabs, correct references for patching.
  - indexed/prime: Trusted modded code. Correct patching, modding patterns.
  - indexed/reference: Lower priority. Duplicating a mod's capability.
  - ONIMiserableMods: Where we edit.
  - Read memory "reference-code-hierarchy" for details.
"@

$projectYml | Out-File (Join-Path $serenaDir "project.yml") -Encoding utf8
Write-Host "Created .serena/project.yml"

$memoryContent = @"
# Reference Code Hierarchy

| Task | relative_path |
|------|---------------|
| Vanilla prefabs, what to patch | indexed/source |
| Correct patching patterns | indexed/prime |
| Duplicating a mod's capability | indexed/reference |
| Our code / editing | ONIMiserableMods/... |
"@
$memoryContent | Out-File (Join-Path $memoriesDir "reference-code-hierarchy.md") -Encoding utf8
Write-Host "Created .serena/memories/reference-code-hierarchy.md"

Write-Host ""
Write-Host "Done. Serena project: $WorkspaceRoot | indexed: $IndexedDir (real dir)"
Write-Host "If ONIMiserableMods symlink failed, run as Administrator or enable Developer Mode."
Write-Host "Update .cursor/mcp.json project path to: $WorkspaceRoot"
Write-Host "Use index_repo_gui to add repos; sync-indexed-to-local.ps1 syncs network -> indexed."
