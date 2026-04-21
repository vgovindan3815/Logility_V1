$d = "C:\Users\1647111\OneDrive - MyFedEx\Projects\FX3A_Tool\bin\Debug"
[System.Reflection.Assembly]::LoadFrom("$d\FedEx.PABST.SS.SSLib.dll") | Out-Null
$flType = $null
foreach ($a in [AppDomain]::CurrentDomain.GetAssemblies()) {
    $t = $a.GetType("FedEx.PABST.SS.Screens.FREIGHTLOGIN", $false)
    if ($t) { $flType = $t; break }
}
$flags = [System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static"
foreach ($mname in @("Initialize","gotoLogility","gotoNextScreen")) {
    $m = $flType.GetMethod($mname, $flags)
    if (-not $m) { "Method $mname not found"; continue }
    "=== $mname IL strings ==="
    $il = $m.GetMethodBody().GetILAsByteArray()
    $module = $flType.Module
    $idx = 0
    for ($i = 0; $i -lt $il.Length; $i++) {
        if ($il[$i] -eq 0x72 -and ($i + 4) -lt $il.Length) {
            $token = [BitConverter]::ToInt32($il, $i + 1)
            try { "  [$idx] " + $module.ResolveString($token); $idx++ } catch {}
            $i += 4
        }
    }
}