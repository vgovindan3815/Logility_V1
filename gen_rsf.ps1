Set-Location "C:\Users\1647111\OneDrive - MyFedEx\Projects\FX3A_Tool"

# Load all deploy DLLs
Get-ChildItem ".\deploy\*.dll" | ForEach-Object {
    try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}

$fieldType = $null; $msgType = $null
foreach ($a in [AppDomain]::CurrentDomain.GetAssemblies()) {
    if (-not $fieldType) { $fieldType = $a.GetType('FedEx.PABST.SS.Screens.ScreenElements+ScreenField', $false) }
    if (-not $msgType)   { $msgType   = $a.GetType('FedEx.PABST.SS.Screens.ScreenElements+ScreenMessage', $false) }
    if ($fieldType -and $msgType) { break }
}

# Screen definitions: name -> ScreenAttributes type name
$screenDefs = @(
    [pscustomobject]@{Name='FREIGHTLOGIN'; TypeName='FedEx.PABST.SS.Screens.FREIGHTLOGINScreenAttributes'},
    [pscustomobject]@{Name='FXF3A';  TypeName='FedEx.PABST.SS.Screens.FXF3A+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3B';  TypeName='FedEx.PABST.SS.Screens.FXF3B+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3C';  TypeName='FedEx.PABST.SS.Screens.FXF3C+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3D';  TypeName='FedEx.PABST.SS.Screens.FXF3D+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3E';  TypeName='FedEx.PABST.SS.Screens.FXF3E+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3F';  TypeName='FedEx.PABST.SS.Screens.FXF3F+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3G';  TypeName='FedEx.PABST.SS.Screens.FXF3G+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3J';  TypeName='FedEx.PABST.SS.Screens.FXF3J+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3K';  TypeName='FedEx.PABST.SS.Screens.FXF3K+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3M';  TypeName='FedEx.PABST.SS.Screens.FXF3M+ScreenAttributes'},
    [pscustomobject]@{Name='FXF3N';  TypeName='FedEx.PABST.SS.Screens.FXF3N+ScreenAttributes'},
    [pscustomobject]@{Name='FXF4M';  TypeName='FedEx.PABST.SS.Screens.FXF4M+ScreenAttributes'}
)

# Known message text values
$msgTexts = @{
    'LOGILITY_PRIMARY_MENU'      = 'LOGILITY PRIMARY MENU'
    'PRICING_PRIMARY_MENU'       = 'PRICING PRIMARY MENU'
    'PRICING_PRIMARY_MENU1'      = 'PRICING PRIMARY MENU'
    'PRIMARY_PASSTHROUGH_SCREEN' = 'PRIMARY PASSTHROUGH SCREEN'
    'LOGOFF_STRING'              = 'LOGOFF'
    'ADD_SUCCESS'                = 'RECORD ADDED'
    'CHANGE_SUCCESS'             = 'RECORD CHANGED'
    'DELETE_SUCCESS'             = 'RECORD DELETED'
    'INQUIRE_SUCCESS'            = 'RECORD INQUIRY'
    'ACCOUNT_NOT_FOUND'          = 'ACCOUNT NOT FOUND'
    'NO_DISC_RECORDS'            = 'NO DISCOUNT RECORDS'
    'END_OF_ACCOUNTS'            = 'END OF ACCOUNTS'
    'ITEM_NOT_RELEASED'          = 'ITEM NOT RELEASED'
    'RELEASE_SUCCESS'            = 'ITEM RELEASED'
    'SUCCESS_INQUIRY'            = 'INQUIRY SUCCESSFUL'
    'RECORD_NOT_FOUND'           = 'RECORD NOT FOUND'
    'COPY01_SUCCESS'             = 'COPY SUCCESSFUL'
    'COPY01_ERR1'                = 'COPY ERROR 1'
    'COPY01_ERR2'                = 'COPY ERROR 2'
    'COPY01_ERR3'                = 'COPY ERROR 3'
    'COPY01_ERR4'                = 'COPY ERROR 4'
    'ERR_300'                    = 'ERROR 300'
    'ERR_ADD_RECORD_EXISTS'      = 'RECORD ALREADY EXISTS'
}

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
[void]$sb.AppendLine('<layout>')

foreach ($def in $screenDefs) {
    $attrType = $null
    foreach ($a in [AppDomain]::CurrentDomain.GetAssemblies()) {
        $attrType = $a.GetType($def.TypeName, $false)
        if ($attrType) { break }
    }
    if (-not $attrType) {
        Write-Warning "Type not found: $($def.TypeName)"
        continue
    }

    [void]$sb.AppendLine("  <screen name=""$($def.Name)"">")

    $flds = $attrType.GetFields([Reflection.BindingFlags]'Instance,Public,NonPublic') |
            Where-Object { $_.FieldType -eq $fieldType } |
            Sort-Object Name

    $msgs = $attrType.GetFields([Reflection.BindingFlags]'Instance,Public,NonPublic') |
            Where-Object { $_.FieldType -eq $msgType } |
            Sort-Object Name

    foreach ($f in $flds) {
        $propName = $f.Name -replace '^m_', ''
        [void]$sb.AppendLine("    <ScreenField name=""$propName"" x=""0"" y=""0"" length=""8"" matchText=""""/>")
    }

    foreach ($m in $msgs) {
        $propName = $m.Name -replace '^m_', ''
        $txt = if ($msgTexts.ContainsKey($propName)) { $msgTexts[$propName] } else { $propName }
        [void]$sb.AppendLine("    <ScreenMessage name=""$propName"" msg=""$txt""/>")
    }

    [void]$sb.AppendLine("  </screen>")
}

[void]$sb.AppendLine('</layout>')

$content = $sb.ToString()

# Write to both locations with UTF-8 no BOM
$enc = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText("C:\Users\1647111\OneDrive - MyFedEx\Projects\FX3A_Tool\deploy\fxf3270.rsf", $content, $enc)
[System.IO.File]::WriteAllText("C:\Users\1647111\OneDrive - MyFedEx\Projects\FX3A_Tool\bin\Debug\deploy\fxf3270.rsf", $content, $enc)

Write-Host "Written $($content.Split("`n").Count) lines"
Write-Host "FREIGHTLOGIN check: $(Select-String -Path 'deploy\fxf3270.rsf' -Pattern '<screen name=""FREIGHTLOGIN"">' | Select-Object -ExpandProperty LineNumber)"

# This script is deprecated. The `fxf3270.rsf` file is no longer used.
