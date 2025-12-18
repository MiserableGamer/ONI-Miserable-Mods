# PowerShell script to increment version in mod_info.yaml
# This script should be in the same directory as mod_info.yaml

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

$yamlFile = Join-Path $scriptDir "mod_info.yaml"

if (Test-Path $yamlFile) {
    $content = Get-Content $yamlFile -Raw
    
    # Match version pattern: version: X.Y.Z
    if ($content -match 'version:\s*(\d+)\.(\d+)\.(\d+)') {
        $major = [int]$matches[1]
        $minor = [int]$matches[2]
        $patch = [int]$matches[3]
        
        # Increment patch version
        $newPatch = $patch + 1
        $newVersion = "version: $major.$minor.$newPatch"
        
        # Replace version line
        $content = $content -replace 'version:\s*\d+\.\d+\.\d+', $newVersion
        
        # Write back to file
        Set-Content -Path $yamlFile -Value $content -NoNewline
        
        Write-Host "Version incremented: $major.$minor.$patch -> $major.$minor.$newPatch"
    } else {
        Write-Warning "Could not find version pattern in mod_info.yaml"
    }
} else {
    Write-Warning "mod_info.yaml not found at: $yamlFile"
}

