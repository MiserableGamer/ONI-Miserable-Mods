<#
.SYNOPSIS
    Generates ONIMiserableMods.slnx from all .csproj projects in the solution root.
    Run from development branch, then commit the slnx so it persists when switching branches.
#>
$SolutionRoot = Split-Path $PSScriptRoot -Parent
$exclude = @('scripts','lib','packages','release','Shared','.git','BlankProject','infrastructure')
$projects = Get-ChildItem -Path $SolutionRoot -Directory | Where-Object {
    $_.Name -notin $exclude -and $_.Name -notmatch '^\.'
} | ForEach-Object {
    Get-ChildItem -Path $_.FullName -Filter "*.csproj" -File | Select-Object -First 1
} | Where-Object { $_ } | ForEach-Object {
    $rel = $_.FullName.Substring($SolutionRoot.Length + 1).Replace('\','/')
    "  <Project Path=`"$rel`" />"
}
$xml = @"
<Solution>
$($projects -join "`n")
</Solution>
"@
$outPath = Join-Path $SolutionRoot "ONIMiserableMods.slnx"
$xml | Set-Content -Path $outPath -Encoding UTF8 -NoNewline
Write-Host "Generated: $outPath" -ForegroundColor Green
Write-Host "Projects: $($projects.Count)" -ForegroundColor Gray
Write-Host "`nCommit to development: git add `"ONIMiserableMods.slnx`"; git commit -m `"Add solution file`"" -ForegroundColor Yellow
