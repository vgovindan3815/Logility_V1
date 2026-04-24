param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$InstallRoot = 'C:\Logility_Freight',
    [string]$SourceOutputDir,
    [string]$DriveLetter = 'E',
    [string]$BasePath = 'C:\temp',
    [switch]$ForceRemap,
    [switch]$StartExecutable
)

$ErrorActionPreference = 'Stop'

function Copy-IfExists {
    param(
        [string]$Path,
        [string]$Destination
    )

    if (Test-Path $Path) {
        Copy-Item -Path $Path -Destination $Destination -Force
        return $true
    }

    return $false
}

if (-not $SourceOutputDir) {
    $releaseOutput = Join-Path $RepoRoot 'bin\Release'
    $debugOutput = Join-Path $RepoRoot 'bin\Debug'

    if (Test-Path (Join-Path $releaseOutput 'Logility_Freight.exe')) {
        $SourceOutputDir = $releaseOutput
    }
    elseif (Test-Path (Join-Path $debugOutput 'Logility_Freight.exe')) {
        $SourceOutputDir = $debugOutput
    }
}

$deployDir = Join-Path $RepoRoot 'deploy'
$layoutPath = Join-Path $RepoRoot 'Screenlayouts\ScreenLayouts.xml'
$substScript = Join-Path $PSScriptRoot 'Create-Tn3270Subst.ps1'

if (-not (Test-Path $deployDir)) {
    throw "Deploy directory not found: $deployDir"
}

if (-not (Test-Path $layoutPath)) {
    throw "Screen layout XML not found: $layoutPath"
}

if (-not (Test-Path $substScript)) {
    throw "Subst helper script not found: $substScript"
}

New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null

$copied = New-Object System.Collections.Generic.List[string]

if ($SourceOutputDir -and (Test-Path $SourceOutputDir)) {
    $runtimeNames = @(
        'Logility_Freight.exe',
        'Logility_Freight.exe.config',
        'FedEx.PABST.SS.SSLib.dll',
        'FedEx.PABST.SS.Exceptions.dll',
        'FedEx.PABST.SS.Screens.FXF3A.dll',
        'FedEx.PABST.SS.Screens.FXF3B.dll',
        'FedEx.PABST.SS.Screens.FXF3C.dll',
        'FedEx.PABST.SS.Screens.FXF3D.dll',
        'FedEx.PABST.SS.Screens.FXF3E.dll',
        'FedEx.PABST.SS.Screens.FXF3F.dll',
        'FedEx.PABST.SS.Screens.FXF3G.dll',
        'FedEx.PABST.SS.Screens.FXF3J.dll',
        'FedEx.PABST.SS.Screens.FXF3K.dll',
        'FedEx.PABST.SS.Screens.FXF3M.dll',
        'FedEx.PABST.SS.Screens.FXF3N.dll',
        'FedEx.PABST.SS.Screens.FXF4M.dll',
        'tn3270_dll.dll',
        'fxf3270.rsf',
        'ScreenLayouts.xml'
    )

    foreach ($name in $runtimeNames) {
        $sourcePath = Join-Path $SourceOutputDir $name
        if (Copy-IfExists -Path $sourcePath -Destination $InstallRoot) {
            $copied.Add($name)
        }
    }
}

$deployPatterns = @(
    'FedEx.PABST.SS.SSLib.dll',
    'FedEx.PABST.SS.Exceptions.dll',
    'FedEx.PABST.SS.Screens.FXF3A.dll',
    'FedEx.PABST.SS.Screens.FXF3B.dll',
    'FedEx.PABST.SS.Screens.FXF3C.dll',
    'FedEx.PABST.SS.Screens.FXF3D.dll',
    'FedEx.PABST.SS.Screens.FXF3E.dll',
    'FedEx.PABST.SS.Screens.FXF3F.dll',
    'FedEx.PABST.SS.Screens.FXF3G.dll',
    'FedEx.PABST.SS.Screens.FXF3J.dll',
    'FedEx.PABST.SS.Screens.FXF3K.dll',
    'FedEx.PABST.SS.Screens.FXF3M.dll',
    'FedEx.PABST.SS.Screens.FXF3N.dll',
    'FedEx.PABST.SS.Screens.FXF4M.dll',
    'tn3270_dll.dll',
    'fxf3270.rsf'
)

foreach ($name in $deployPatterns) {
    $sourcePath = Join-Path $deployDir $name
    if ((Test-Path $sourcePath) -and (-not (Test-Path (Join-Path $InstallRoot $name)))) {
        Copy-Item -Path $sourcePath -Destination $InstallRoot -Force
        $copied.Add($name)
    }
}

if (-not (Test-Path (Join-Path $InstallRoot 'ScreenLayouts.xml'))) {
    Copy-Item -Path $layoutPath -Destination (Join-Path $InstallRoot 'ScreenLayouts.xml') -Force
    $copied.Add('ScreenLayouts.xml')
}

& $substScript -RepoRoot $RepoRoot -BasePath $BasePath -DriveLetter $DriveLetter -ForceRemap:$ForceRemap

Write-Host "Install root: $InstallRoot"
Write-Host 'Copied runtime files:'
$copied | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" }

$exePath = Join-Path $InstallRoot 'Logility_Freight.exe'
if (Test-Path $exePath) {
    Write-Host "Executable ready at $exePath"
    if ($StartExecutable) {
        Start-Process -FilePath $exePath -WorkingDirectory $InstallRoot
    }
}
else {
    Write-Warning 'Logility_Freight.exe was not copied. Build the project first or provide -SourceOutputDir.'
}
