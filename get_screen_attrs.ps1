Set-Location "C:\Users\1647111\OneDrive - MyFedEx\Projects\FX3A_Tool"

# Load all deploy DLLs
Get-ChildItem ".\deploy\*.dll" | ForEach-Object {
    try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}

$fieldType = $null
$msgType = $null
foreach ($a in [AppDomain]::CurrentDomain.GetAssemblies()) {
    if (-not $fieldType) { $fieldType = $a.GetType('FedEx.PABST.SS.Screens.ScreenElements+ScreenField', $false) }
    if (-not $msgType)   { $msgType   = $a.GetType('FedEx.PABST.SS.Screens.ScreenElements+ScreenMessage', $false) }
    if ($fieldType -and $msgType) { break }
}

$screenDefs = @(
    @{Name='FREIGHTLOGIN'; TypeName='FedEx.PABST.SS.Screens.FREIGHTLOGINScreenAttributes'},
    @{Name='FXF3A';  TypeName='FedEx.PABST.SS.Screens.FXF3A+ScreenAttributes'},
    @{Name='FXF3B';  TypeName='FedEx.PABST.SS.Screens.FXF3B+ScreenAttributes'},
    @{Name='FXF3C';  TypeName='FedEx.PABST.SS.Screens.FXF3C+ScreenAttributes'},
    @{Name='FXF3D';  TypeName='FedEx.PABST.SS.Screens.FXF3D+ScreenAttributes'},
    @{Name='FXF3E';  TypeName='FedEx.PABST.SS.Screens.FXF3E+ScreenAttributes'},
    @{Name='FXF3F';  TypeName='FedEx.PABST.SS.Screens.FXF3F+ScreenAttributes'},
    @{Name='FXF3G';  TypeName='FedEx.PABST.SS.Screens.FXF3G+ScreenAttributes'},
    @{Name='FXF3J';  TypeName='FedEx.PABST.SS.Screens.FXF3J+ScreenAttributes'},
    @{Name='FXF3K';  TypeName='FedEx.PABST.SS.Screens.FXF3K+ScreenAttributes'},
    @{Name='FXF3M';  TypeName='FedEx.PABST.SS.Screens.FXF3M+ScreenAttributes'},
    @{Name='FXF3N';  TypeName='FedEx.PABST.SS.Screens.FXF3N+ScreenAttributes'},
    @{Name='FXF4M';  TypeName='FedEx.PABST.SS.Screens.FXF4M+ScreenAttributes'}
)

foreach ($def in $screenDefs) {
    $attrType = $null
    foreach ($a in [AppDomain]::CurrentDomain.GetAssemblies()) {
        $attrType = $a.GetType($def.TypeName, $false)
        if ($attrType) { break }
    }
    if (-not $attrType) {
        Write-Host "SCREEN $($def.Name) : type NOT FOUND ($($def.TypeName))"
        continue
    }
    Write-Host "=== $($def.Name) ==="
    $flds = $attrType.GetFields([Reflection.BindingFlags]'Instance,Public,NonPublic') | Where-Object { $_.FieldType -eq $fieldType } | Sort-Object Name
    $msgs = $attrType.GetFields([Reflection.BindingFlags]'Instance,Public,NonPublic') | Where-Object { $_.FieldType -eq $msgType   } | Sort-Object Name
    foreach ($f in $flds) { Write-Host "  FIELD $($f.Name -replace '^m_','')" }
    foreach ($m in $msgs) { Write-Host "  MSG   $($m.Name -replace '^m_','')" }
}
