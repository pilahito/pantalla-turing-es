# -*- coding: utf-8 -*-
"""Arranque fiable del monitor Turing (mata instancia previa, elige tema, registra errores)."""
from __future__ import annotations

import os
import re
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PID_FILE = ROOT / "tmp" / "monitor.pid"
LOG = ROOT / "lanzador.log"
CFG = ROOT / "config.yaml"


def log(msg: str) -> None:
    LOG.parent.mkdir(parents=True, exist_ok=True)
    line = time.strftime("%Y-%m-%d %H:%M:%S ") + msg + "\n"
    with LOG.open("a", encoding="utf-8") as f:
        f.write(line)


def popup(msg: str) -> None:
    log("ERROR " + msg)
    try:
        import ctypes
        ctypes.windll.user32.MessageBoxW(0, msg, "Pantalla Turing", 0x10)
    except Exception:
        print(msg)


def kill_pid(pid: int) -> None:
    try:
        subprocess.run(
            ["taskkill", "/PID", str(pid), "/F", "/T"],
            capture_output=True,
            timeout=8,
        )
    except Exception:
        pass


def kill_previous() -> None:
    subprocess.run(["taskkill", "/IM", "UsbPCMonitor.exe", "/F"], capture_output=True, timeout=8)
    if PID_FILE.exists():
        try:
            old = int(PID_FILE.read_text(encoding="utf-8").strip())
            kill_pid(old)
        except Exception:
            pass
        try:
            PID_FILE.unlink()
        except Exception:
            pass
    try:
        import psutil
        for p in psutil.process_iter(["pid", "name", "cmdline"]):
            cmd = " ".join(p.info.get("cmdline") or [])
            if "turing-smart-screen-python" in cmd and "main.py" in cmd:
                kill_pid(int(p.info["pid"]))
    except Exception as e:
        log("psutil: " + str(e))


def set_theme(theme: str) -> None:
    text = CFG.read_text(encoding="utf-8")
    if not re.search(r"(?m)^(\s*THEME:\s*).+$", text):
        raise RuntimeError("No hay THEME en config.yaml")
    text = re.sub(r"(?m)^(\s*THEME:\s*).+$", r"\g<1>" + theme, text)
    text = re.sub(r"(?m)^(\s*HW_SENSORS:\s*).+$", r"\g<1>AUTO", text)
    CFG.write_text(text, encoding="utf-8")


def main() -> int:
    os.chdir(ROOT)
    theme = sys.argv[1].strip() if len(sys.argv) > 1 else ""
    log("lanzar theme=%r" % theme)
    try:
        kill_previous()
        time.sleep(0.4)
        if theme:
            theme_dir = ROOT / "res" / "themes" / theme
            if not (theme_dir / "theme.yaml").exists():
                popup("No existe el tema:\n" + theme)
                return 2
            set_theme(theme)
            log("config THEME=" + theme)
        py = ROOT / "venv" / "Scripts" / "python.exe"
        if not py.exists():
            raise RuntimeError("No hay venv\\Scripts\\python.exe")
        tmp = PID_FILE.parent
        if tmp.exists() and not tmp.is_dir():
            tmp.unlink()
        tmp.mkdir(parents=True, exist_ok=True)
        CREATE_NO_WINDOW = 0x08000000
        proc = subprocess.Popen(
            [str(py), "main.py"],
            cwd=str(ROOT),
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            creationflags=CREATE_NO_WINDOW,
        )
        PID_FILE.write_text(str(proc.pid), encoding="utf-8")
        time.sleep(2.5)
        if proc.poll() is not None:
            tail = ""
            lf = ROOT / "log.log"
            if lf.exists():
                tail = "\n".join(lf.read_text(encoding="utf-8", errors="replace").splitlines()[-12:])
            popup("El monitor se cerro al arrancar.\nRevisa log.log\n\n" + tail[:800])
            return 3
        log("ok pid=%s" % proc.pid)
        return 0
    except Exception as e:
        popup(str(e))
        return 1


if __name__ == "__main__":
    sys.exit(main())
