# Turing Smart Screen en Linux (Ubuntu y Arch)

El `.exe` es solo Windows. En Linux usa `iniciar.sh`.

## Ubuntu / Debian

```bash
cd turing-smart-screen-python
chmod +x iniciar.sh
./iniciar.sh --install
./iniciar.sh ConilES
```

Añade tu usuario al grupo `dialout` para el puerto serie:

```bash
sudo usermod -aG dialout "$USER"
# cierra sesión y vuelve a entrar
```

## Arch Linux

```bash
cd turing-smart-screen-python
chmod +x iniciar.sh
./iniciar.sh --install
./iniciar.sh ConilES
```

Grupo serie en Arch:

```bash
sudo usermod -aG uucp "$USER"
```

## Temas

```bash
./iniciar.sh HorizonES
./iniciar.sh NocheNeon
./iniciar.sh EmberES
./iniciar.sh HieloES
```

En Linux los sensores son Python (no LibreHardwareMonitor). El puerto se detecta solo (`/dev/ttyACM0` o similar).
