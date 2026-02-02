# PowerShell script to increment version in mod_info.yaml
# This script can be called from any project directory or with a project path parameter
# Version format: major.minor.patch.build
#
# Usage:
#   .\increment_version.ps1                    # Uses current directory
#   .\increment_version.ps1 -ProjectDir "Path" # Uses specified directory

param(
    [string]$ProjectDir = ""
)

# If ProjectDir parameter is provided, use it (strip quotes if present)
if (-not [string]::IsNullOrWhiteSpace($ProjectDir)) {
    # Remove all quotes (single and double) from anywhere in the string, then trim trailing slashes
    $scriptDir = ($ProjectDir -replace '"', '' -replace "'", '').TrimEnd('\', '/')
} else {
    # Get the directory where this script is located
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    
    # If scriptDir is empty, try PSScriptRoot
    if ([string]::IsNullOrWhiteSpace($scriptDir)) {
        $scriptDir = $PSScriptRoot
    }
    
    # If still empty, use current directory
    if ([string]::IsNullOrWhiteSpace($scriptDir)) {
        $scriptDir = Get-Location
    }
}

# Ensure scriptDir doesn't end with a backslash (except for root UNC paths like \\server\share)
if ($scriptDir.Length -gt 1 -and $scriptDir.EndsWith('\') -and -not $scriptDir.StartsWith('\\')) {
    $scriptDir = $scriptDir.TrimEnd('\')
}

$yamlFile = Join-Path $scriptDir "mod_info.yaml"

# Use LiteralPath for UNC paths and paths with special characters
if (Test-Path -LiteralPath $yamlFile) {
    $content = Get-Content $yamlFile -Raw
    
    # Match version pattern: version: X.Y.Z.B (major.minor.patch.build)
    if ($content -match 'version:\s*(\d+)\.(\d+)\.(\d+)\.(\d+)') {
        $major = [int]$matches[1]
        $minor = [int]$matches[2]
        $patch = [int]$matches[3]
        $build = [int]$matches[4]
        
        # Increment build version
        $newBuild = $build + 1
        $newVersion = "$major.$minor.$patch.$newBuild"
        $newVersionYaml = "version: $newVersion"
        
        # Replace version line in mod_info.yaml
        $content = $content -replace 'version:\s*\d+\.\d+\.\d+\.\d+', $newVersionYaml
        
        # Write back to mod_info.yaml (use LiteralPath for UNC paths)
        Set-Content -LiteralPath $yamlFile -Value $content -NoNewline
        
        Write-Host "Version incremented: $major.$minor.$patch.$build -> $newVersion in $scriptDir"
    } else {
        Write-Warning "Could not find version pattern (X.Y.Z.B) in mod_info.yaml at: $yamlFile"
    }
} else {
    Write-Warning "mod_info.yaml not found at: $yamlFile"
}

