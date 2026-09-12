# Pantalla Turing / Centro Turing (Windows ES)

Edición española del proyecto [turing-smart-screen-python](https://github.com/mathoudebine/turing-smart-screen-python) (GPLv3).  
Este fork mantiene el crédito al upstream y añade un **Centro** pensado como app de escritorio Windows.

## Qué es qué

| Acción | Archivo | Efecto |
|--------|---------|--------|
| Abrir el panel gráfico | `Centro-Turing.bat` | Solo la GUI (Centro Turing). No arranca la pantalla por sí solo. |
| Encender la pantalla en silencio | `Iniciar.ps1` | Lanza `main.py` una sola vez (idempotente: no duplica el proceso). |
| Sensores LHM (admin) | `Iniciar-Admin.ps1` | Avanzado: pide UAC y usa LibreHardwareMonitor. |

## Primeros pasos

1. Copia `config.example.yaml` → `config.yaml` y ajusta `COM_PORT`, tema y brillo.
2. Ejecuta `Instalar.ps1` si aún no tienes el `venv`.
3. Abre **`Centro-Turing.bat`** → pestaña **Panel**.
4. Pulsa **Encender** (o usa `Iniciar.ps1` para arranque silencioso).

## Panel (Centro Turing v2.2)

- Tarjetas de estado: Pantalla ON/OFF, COM, tema, brillo.
- Acciones: **Encender** / **Apagar** / **Reiniciar pantalla**.
- Autostart ON/OFF: acceso directo en Inicio de Windows que apunta a  
  `powershell -File <ROOT>\Iniciar.ps1` con ventana oculta (WorkingDirectory = ROOT).
- Ajustes: COM, brillo, modelo. Sin botón de “ejecutar como admin”.
- Temas, clima Open-Meteo, personalización (reloj 12/24, giro).

La ruta del proyecto se detecta sola (`Path(__file__)`); no hace falta hardcodear `E:\...`.

## Temas y COM

- Elige el **modelo** (p. ej. 3.5 landscape) en Ajustes para filtrar temas compatibles.
- Puerto típico 3.5": `COM3` (`USB35INCHIPSV2`). Si falla, prueba `AUTO` o lista puertos en Ajustes.

## Arranque con Windows

En el **Panel**, activa **ON**. Se crea `Centro Turing.lnk` en la carpeta Startup.  
Al iniciar sesión solo se enciende la minipantalla (sin abrir el Centro).

## Licencia

GPLv3 — ver `LICENSE` y `AUTHORS`. Basado en el trabajo de mathoudebine y contribuidores.
