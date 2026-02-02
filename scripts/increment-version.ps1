# Increment version in a mod project's mod_info.yaml (development + Debug builds).
# Called from Directory.Build.targets with -ProjectDir only; extra args (e.g. mod_info.yaml)
# may be passed by the host and are absorbed via ValueFromRemainingArguments.

param(
    [string]$ProjectDir,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Remaining = @()
)

$ErrorActionPreference = 'Stop'
try {
    if (-not $ProjectDir -or -not (Test-Path -LiteralPath $ProjectDir -PathType Container)) {
        Write-Warning "increment-version.ps1: ProjectDir missing or not a directory: '$ProjectDir'"
        exit 0
    }

    $modInfoPath = Join-Path -Path $ProjectDir -ChildPath 'mod_info.yaml'
    if (-not (Test-Path -LiteralPath $modInfoPath -PathType Leaf)) {
        Write-Warning "increment-version.ps1: mod_info.yaml not found: $modInfoPath"
        exit 0
    }

    $content = Get-Content -LiteralPath $modInfoPath -Raw -Encoding UTF8
    if ($content -match 'version:\s*(\d+)\.(\d+)\.(\d+)\.(\d+)') {
        $last = [int]$Matches[4]
        $newVersion = "$($Matches[1]).$($Matches[2]).$($Matches[3]).$($last + 1)"
        $prefix = $Matches[0] -replace '\d+\.\d+\.\d+\.\d+$', ''   # "version: " (keep prefix only)
        $newContent = $content -replace '(version:\s*)\d+\.\d+\.\d+\.\d+', ($prefix + $newVersion)
        Set-Content -LiteralPath $modInfoPath -Value $newContent -NoNewline -Encoding UTF8
        Write-Host "increment-version.ps1: $modInfoPath -> version $newVersion"
    } else {
        Write-Warning "increment-version.ps1: No version line match in $modInfoPath"
    }
} catch {
    Write-Warning "increment-version.ps1 failed (build continues): $_"
    exit 0
}
exit 0
