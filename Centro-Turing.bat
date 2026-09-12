@echo off
REM Solo abre el Centro Turing (GUI). No arranca main.py.
cd /d "%~dp0"
if not exist "venv\Scripts\python.exe" (
  echo No hay venv. Ejecuta Instalar.ps1
  pause
  exit /b 1
)
if exist "venv\Scripts\pythonw.exe" (
  start "" "venv\Scripts\pythonw.exe" tools\centro_turing.py
) else (
  "venv\Scripts\python.exe" tools\centro_turing.py
)
