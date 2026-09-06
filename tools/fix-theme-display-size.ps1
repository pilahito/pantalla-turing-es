# Añade DISPLAY_SIZE: 3.5" a temas landscape 480x320 que no lo tienen
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root = Split-Path -Parent $Root
if (-not (Test-Path (Join-Path $Root "main.py"))) {
    $Root = "E:\turing-smart-screen-python"
}
$themesRoot = Join-Path $Root "res\themes"
$fixed = @()
$skipped = @()

Get-ChildItem $themesRoot -Directory | ForEach-Object {
    $yaml = Join-Path $_.FullName "theme.yaml"
    if (-not (Test-Path $yaml)) { return }
    $text = Get-Content $yaml -Raw
    if ($text -match '(?m)^\s*DISPLAY_SIZE\s*:') {
        return
    }
    $w = $null; $h = $null
    if ($text -match '(?m)^\s*WIDTH\s*:\s*(\d+)') { $w = [int]$Matches[1] }
    if ($text -match '(?m)^\s*HEIGHT\s*:\s*(\d+)') { $h = [int]$Matches[1] }
    $ori = $null
    if ($text -match '(?m)^\s*DISPLAY_ORIENTATION\s*:\s*(\w+)') { $ori = $Matches[1] }

    if ($w -eq 480 -and $h -eq 320 -and ($null -eq $ori -or $ori -eq 'landscape')) {
        if ($text -match '(?m)^(display:\s*\r?\n)') {
            $insert = $Matches[0] + "  DISPLAY_SIZE: 3.5`"`r`n"
            $newText = $text.Substring(0, $Matches.Index) + $insert + $text.Substring($Matches.Index + $Matches.Length)
            # Prefer UTF8 without BOM issues
            [System.IO.File]::WriteAllText($yaml, $newText)
            $fixed += $_.Name
        } else {
            $skipped += "$($_.Name) (no display: block)"
        }
    }
}

Write-Host "Fixed $($fixed.Count) themes:"
$fixed | ForEach-Object { Write-Host "  + $_" }
if ($skipped.Count -gt 0) {
    Write-Host "Skipped:"
    $skipped | ForEach-Object { Write-Host "  - $_" }
}
