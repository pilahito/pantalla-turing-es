# Arranca la pantalla con main.py. El centro nuevo es Centro-Turing.bat.
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $Root
$py = Join-Path $Root "venv\Scripts\python.exe"
if (-not (Test-Path $py)) { Write-Host "No hay venv. Ejecuta Instalar.ps1"; exit 1 }
$running = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -and $_.CommandLine -match "turing-smart-screen-python\\main\.py"
}
if (-not $running) {
    Start-Process -FilePath $py -ArgumentList "main.py" -WorkingDirectory $Root -WindowStyle Hidden
}
Write-Host "Pantalla en marcha con main.py. Centro: Centro-Turing.bat"
