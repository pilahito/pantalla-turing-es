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
| `Turing-Conil.exe` | ConilES (playa) |
| `Turing-Ember.exe` | EmberES (lava) |
| `Turing-Hielo.exe` | HieloES |
| `Turing-Atardecer.exe` | AtardecerES |
| `Turing-Violeta.exe` | VioletaES |
| `Turing-Minimal.exe` | MinimalES |
| `Turing-Circuito.exe` | CircuitoES |
| `Turing-Bosque.exe` | BosqueES |
| `Turing-Tareas.exe` | AdminES (administrador de tareas) |
| `Turing-Clasico.exe` | 3.5inchTheme2 horizontal |
| `Turing-Azul.exe` | SimpleBlue horizontal |
| `Turing-Naranja.exe` | SimpleOrange horizontal |
| `Turing-Verde.exe` | SimpleGreen horizontal |
| `Turing-Fallout.exe` | Fallout horizontal |
| `Turing-CyberpunkH.exe` | Cyberpunk horizontal |
| `Turing-TermH.exe` | Terminal original horizontal |
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

Temas ES propios: HorizonES, NocheNeon, TerminalES, ConilES, EmberES, HieloES, AtardecerES, VioletaES, MinimalES, CircuitoES, BosqueES, AdminES.
El resto de skins **landscape 480×320** ya cargan en la 3.5" (antes fallaban por `DISPLAY_SIZE: "3.5"` vs `3.5"`).

Si la imagen sale al revés: en `config.yaml` pon `DISPLAY_REVERSE: true`.
