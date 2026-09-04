# Arranca el monitor Turing 3.5" (no necesita administrador).
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $Root

$py = Join-Path $Root "venv\Scripts\python.exe"
if (-not (Test-Path $py)) {
    Write-Host "No hay venv. Ejecuta primero Instalar.ps1"
    pause
    exit 1
}

Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -and $_.CommandLine -match 'turing-smart-screen-python\\main\.py'
} | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }

Start-Process -FilePath $py -ArgumentList "main.py" -WorkingDirectory $Root -WindowStyle Hidden
Write-Host "Pantalla Turing en marcha (icono en la bandeja)."
Write-Host "Tema: HorizonES  |  Puerto: COM3  |  3.5 pulgadas horizontal"
