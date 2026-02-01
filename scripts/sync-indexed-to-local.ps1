# Sync indexed from network to C:\oni-serena\indexed (e.g. after manually adding repos on network)
# Run when you've added content to \\...\indexed and want it in Serena's indexed folder

$UncRoot = if (Test-Path "D:\indexed") { "D:\" } else { "\\192.168.1.30\development\ONI\Mods" }
$IndexedDir = "C:\oni-serena\indexed"

New-Item -ItemType Directory -Path $IndexedDir -Force | Out-Null
foreach ($tier in @("source", "prime", "reference")) {
    $destTier = Join-Path $IndexedDir $tier
    $remoteTier = Join-Path "$UncRoot\indexed" $tier
    New-Item -ItemType Directory -Path $destTier -Force | Out-Null
    if (Test-Path $remoteTier) {
        Write-Host "Syncing indexed/$tier..."
        robocopy $remoteTier $destTier /E /MIR /NFL /NDL /NJH /NJS /nc /ns /np
    }
}
Write-Host "Done. Indexed: $IndexedDir"
