@echo off
rem Centro Turing 3.0 — lanzador para Windows
rem Abre el panel grafico multiplataforma (misma interfaz que en Linux).
setlocal
set "ROOT=%~dp0"

set "PY=%ROOT%venv\Scripts\pythonw.exe"
if not exist "%PY%" set "PY=%ROOT%venv\Scripts\python.exe"
if not exist "%PY%" set "PY=python"

start "" "%PY%" "%ROOT%tools\turing_center.py" %*
endlocal
