param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$BasePath = 'C:\temp',
    [string]$DriveLetter = 'E',
    [string]$SourceDllPath,
    [switch]$ForceRemap
)

$ErrorActionPreference = 'Stop'

function Get-SubstTarget {
    param([string]$Letter)

    $output = & subst.exe 2>$null
    foreach ($line in $output) {
        if ($line -match ('^{0}: => (.+)$' -f [regex]::Escape($Letter.ToUpperInvariant()))) {
            return $Matches[1].Trim()
        }
    }

    return $null
}

$DriveLetter = $DriveLetter.Substring(0, 1).ToUpperInvariant()
$expectedTarget = $BasePath.TrimEnd('\')
$mappedTarget = Get-SubstTarget -Letter $DriveLetter

if (-not $SourceDllPath) {
    $SourceDllPath = Join-Path $RepoRoot 'deploy\tn3270_dll.dll'
}

if (-not (Test-Path $SourceDllPath)) {
    throw "tn3270 source DLL not found: $SourceDllPath"
}

New-Item -ItemType Directory -Path $expectedTarget -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $expectedTarget 'fedex\tn3270') -Force | Out-Null

if ($mappedTarget) {
    if ($mappedTarget -ieq $expectedTarget) {
        Write-Host "$DriveLetter`: is already mapped to $expectedTarget"
    }
    elseif ($ForceRemap) {
        & subst.exe "$DriveLetter`:" /D | Out-Null
        & subst.exe "$DriveLetter`:" $expectedTarget | Out-Null
        Write-Host "Remapped $DriveLetter`: to $expectedTarget"
    }
    else {
        throw "$DriveLetter`: is already mapped to $mappedTarget. Use -ForceRemap to replace it."
    }
}
else {
    & subst.exe "$DriveLetter`:" $expectedTarget | Out-Null
    Write-Host "Mapped $DriveLetter`: to $expectedTarget"
}

$destinationDllPath = Join-Path "$DriveLetter`:\" 'fedex\tn3270\tn3270_dll.dll'
Copy-Item -Path $SourceDllPath -Destination $destinationDllPath -Force

Write-Host "Copied tn3270_dll.dll to $destinationDllPath"
Write-Host 'Validation:'
& subst.exe
Write-Host ("DLL present: {0}" -f (Test-Path $destinationDllPath))
