# Turing Smart Screen 3.5" — Windows (español)

Pantalla típica: COM3 (`USB35INCHIPSV2`, revision A).
Software: **turing-smart-screen-python** + LibreHardwareMonitor.

## Arrancar (un solo .exe)

Usa **`PantallaTuring.exe`** (menú gráfico en español):

- Lista temas ES / horizontales
- Muestra tema actual y puerto COM
- **Iniciar** / **Iniciar como administrador** (UAC → temps reales)
- **Detener**, **Aplicar tema**, **Abrir carpeta**
- Checkbox **Invertir pantalla** (`DISPLAY_REVERSE`)

CLI (sin GUI, compatibilidad):

```text
PantallaTuring.exe ConilES
PantallaTuring.exe HorizonES
```

Compilar de nuevo:

```powershell
powershell -ExecutionPolicy Bypass -File tools\build-pantalla-turing.ps1
```

Fuente: `tools\PantallaTuringUI.cs` (única fuente de verdad del lanzador).
Los antiguos `Turing-*.exe` quedan **obsoletos** (opcional en `dist-lanzadores/`).

## Temas horizontales ES

HorizonES, NocheNeon, TerminalES, ConilES, EmberES, HieloES, AtardecerES, VioletaES, MinimalES, CircuitoES, BosqueES, AdminES + variantes `*_H` / Landscape.

## Sensores más exactos

Ver `docs/SENSORES-ES.md`. En Windows: **Iniciar como admin**. Copia `config.example.yaml` → `config.yaml` y pon tu `WEATHER_API_KEY` (no la subas a git).

## Scripts PowerShell (legacy)

`Iniciar.ps1`, `Iniciar-Admin.ps1`, `Cambiar-Tema.ps1`, `Instalar.ps1`.
