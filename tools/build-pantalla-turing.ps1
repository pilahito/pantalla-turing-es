# Compila PantallaTuring.exe (GUI español única) — sin NuGet
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root = Split-Path -Parent $Root
if (-not (Test-Path (Join-Path $Root "main.py"))) {
    $Root = "E:\turing-smart-screen-python"
}
Set-Location $Root
$src = Join-Path $Root "tools\PantallaTuringUI.cs"
$out = Join-Path $Root "PantallaTuring.exe"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) { throw "No se encuentra csc.exe (.NET Framework 4.x)" }
if (-not (Test-Path $src)) { throw "Falta $src" }
& $csc /nologo /target:winexe /platform:anycpu /optimize+ /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:$out $src
if ($LASTEXITCODE -ne 0) { throw "Compilación fallida ($LASTEXITCODE)" }
Write-Host "OK -> $out"
Get-Item $out | Format-List FullName, Length, LastWriteTime
