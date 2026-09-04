# Turing Smart Screen 3.5" — Windows (español)

Pantalla: **COM3** (`USB35INCHIPSV2`, revision A).
Software: **turing-smart-screen-python** (GitHub, LibreHardwareMonitor).
El programa cerrado UsbPCMonitor se eliminó: bloqueaba COM3 y no tiene comunidad.

## Arrancar (escritorio, .exe — ya no hay .bat)

| Acceso | Tema |
|--------|------|
| `Turing-Iniciar.exe` / **Pantalla Turing** | el tema actual |
| `Turing-Admin.exe` | igual, con UAC (temps CPU reales) |
| `Turing-Horizonte.exe` | HorizonES (español) |
| `Turing-NocheNeon.exe` | NocheNeon (español) |
| `Turing-Terminal.exe` | TerminalES (español) |
| `Turing-Cyberdeck.exe` | Cyberdeck |
| `Turing-6Celdas.exe` | Landscape6Grid |
| `Turing-Tierra.exe` | LandscapeEarth |
| `Turing-AzulMagico.exe` | LandscapeMagicBlue |
| `Turing-CyberArasaka.exe` | CyberArasaka landscape |
| `Turing-OnePiece.exe` | One Piece |
| `Turing-Reloj.exe` | BigClock |
| `Turing-Servidor.exe` | LandscapeModernDevice35 |

Piden administrador una vez: así salen CPU/GPU/RAM/disco como en el Administrador de tareas (LibreHardwareMonitor).

Cierra cualquier UsbPCMonitor si reaparece.

## Temas horizontales 3.5" en español

HorizonES, NocheNeon y TerminalES tienen textos en español.
El resto de skins **landscape 480×320** ya cargan en la 3.5" (antes fallaban por `DISPLAY_SIZE: "3.5"` vs `3.5"`).

Si la imagen sale al revés: en `config.yaml` pon `DISPLAY_REVERSE: true`.
