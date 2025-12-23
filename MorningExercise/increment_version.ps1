# PowerShell script to increment version in mod_info.yaml, README.md, and STEAM_WORKSHOP_DESCRIPTION.md
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
$readmeFile = Join-Path $scriptDir "README.md"
$steamFile = Join-Path $scriptDir "STEAM_WORKSHOP_DESCRIPTION.md"

if (Test-Path $yamlFile) {
    $content = Get-Content $yamlFile -Raw
    
    # Match version pattern: version: X.Y.Z
    if ($content -match 'version:\s*(\d+)\.(\d+)\.(\d+)') {
        $major = [int]$matches[1]
        $minor = [int]$matches[2]
        $patch = [int]$matches[3]
        
        # Increment patch version
        $newPatch = $patch + 1
        $newVersion = "$major.$minor.$newPatch"
        $newVersionYaml = "version: $newVersion"
        
        # Replace version line in mod_info.yaml
        $content = $content -replace 'version:\s*\d+\.\d+\.\d+', $newVersionYaml
        
        # Write back to mod_info.yaml
        Set-Content -Path $yamlFile -Value $content -NoNewline
        
        Write-Host "Version incremented: $major.$minor.$patch -> $newVersion"
        
        # Update README.md if it exists
        if (Test-Path $readmeFile) {
            $readmeContent = Get-Content $readmeFile -Raw
            # Match pattern: - **X.Y.Z**: Initial release
            if ($readmeContent -match '- \*\*(\d+\.\d+\.\d+)\*\*:\s*Initial release') {
                $readmeContent = $readmeContent -replace '- \*\*\d+\.\d+\.\d+\*\*:\s*Initial release', "- **$newVersion**: Initial release"
                Set-Content -Path $readmeFile -Value $readmeContent -NoNewline
                Write-Host "Updated version in README.md"
            }
        }
        
        # Update STEAM_WORKSHOP_DESCRIPTION.md if it exists
        if (Test-Path $steamFile) {
            $steamContent = Get-Content $steamFile -Raw
            # Match pattern: [*][b]X.Y.Z[/b]: Initial release
            if ($steamContent -match '\[\*\]\[b\](\d+\.\d+\.\d+)\[/b\]:\s*Initial release') {
                $steamContent = $steamContent -replace '\[\*\]\[b\]\d+\.\d+\.\d+\[/b\]:\s*Initial release', "[*][b]$newVersion[/b]: Initial release"
                Set-Content -Path $steamFile -Value $steamContent -NoNewline
                Write-Host "Updated version in STEAM_WORKSHOP_DESCRIPTION.md"
            }
        }
    } else {
        Write-Warning "Could not find version pattern in mod_info.yaml"
    }
} else {
    Write-Warning "mod_info.yaml not found at: $yamlFile"
}

