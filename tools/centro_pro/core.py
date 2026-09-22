# -*- coding: utf-8 -*-
"""Lógica compartida: config, monitor, temas."""
from __future__ import annotations
import json, re, subprocess, time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PY = ROOT / "venv" / "Scripts" / "python.exe"
CFG = ROOT / "config.yaml"
THEMES = ROOT / "res" / "themes"
UI = ROOT / "tools" / "centro_ui.json"
INICIAR = ROOT / "Iniciar.ps1"
STARTUP = Path.home() / "AppData/Roaming/Microsoft/Windows/Start Menu/Programs/Startup/Centro Turing.lnk"
GH_REPO = "pilahito/pantalla-turing-es"
VERSION = "2.3.0"

CREATE_NO_WINDOW = 0x08000000


def _run_hidden(cmd, **kwargs):
    kwargs.setdefault("capture_output", True)
    kwargs.setdefault("text", True)
    kwargs.setdefault("creationflags", CREATE_NO_WINDOW)
    return subprocess.run(cmd, **kwargs)


def read_cfg() -> dict:
    text = CFG.read_text(encoding="utf-8", errors="replace") if CFG.exists() else ""
    out = {}
    for key in ("THEME", "COM_PORT", "BRIGHTNESS", "WEATHER_LATITUDE", "WEATHER_LONGITUDE",
                "WEATHER_LANGUAGE", "CLOCK_FORMAT"):
        m = re.search(r"(?m)^\s*" + key + r":\s*(.*)$", text)
        out[key] = m.group(1).strip().strip("'\"") if m else ""
    rev = re.search(r"(?m)^\s*DISPLAY_REVERSE:\s*(.+)$", text)
    out["DISPLAY_REVERSE"] = rev.group(1).strip().lower() if rev else "false"
    return out


def write_key(key: str, value) -> None:
    text = CFG.read_text(encoding="utf-8", errors="replace") if CFG.exists() else "config:\n"
    pat = r"(?m)^(\s*" + key + r":\s*).+$"
    if re.search(pat, text):
        text = re.sub(pat, r"\g<1>" + str(value), text, count=1)
    elif "\ndisplay:" in text:
        text = text.replace("\ndisplay:", "\n  " + key + ": " + str(value) + "\ndisplay:", 1)
    else:
        text += "\n  " + key + ": " + str(value) + "\n"
    CFG.write_text(text, encoding="utf-8")


def monitor_running() -> bool:
    ps = (
        "Get-CimInstance Win32_Process | Where-Object { "
        "$_.CommandLine -and $_.CommandLine -like '*main.py*' -and "
        "$_.CommandLine -like '*turing-smart-screen-python*' "
        "} | Select-Object -First 1"
    )
    r = _run_hidden(["powershell", "-NoProfile", "-WindowStyle", "Hidden", "-Command", ps])
    return bool((r.stdout or "").strip())


def kill_monitor() -> None:
    ps = (
        "Get-CimInstance Win32_Process | Where-Object { "
        "$_.CommandLine -and $_.CommandLine -like '*turing-smart-screen-python*main.py*' "
        "} | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }"
    )
    _run_hidden(["powershell", "-NoProfile", "-WindowStyle", "Hidden", "-Command", ps])
    time.sleep(0.8)


def start_monitor() -> None:
    if monitor_running():
        return
    if not PY.exists():
        raise FileNotFoundError("No hay venv/python.exe")
    subprocess.Popen(
        [str(PY), "main.py"],
        cwd=str(ROOT),
        creationflags=CREATE_NO_WINDOW,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )


def reload_screen() -> None:
    kill_monitor()
    start_monitor()


def list_themes() -> list[dict]:
    items = []
    if not THEMES.is_dir():
        return items
    for d in sorted(THEMES.iterdir()):
        if not d.is_dir() or d.name.startswith(".") or d.name.startswith("--"):
            continue
        bg = None
        for name in ("background.png", "background.jpg", "background.gif"):
            p = d / name
            if p.is_file():
                bg = p
                break
        if bg is None:
            for p in d.glob("*.png"):
                bg = p
                break
        items.append({"id": d.name, "path": d, "preview": bg})
    return items


def theme_preview_path(theme_id: str) -> Path | None:
    for t in list_themes():
        if t["id"] == theme_id:
            return t.get("preview")
    return None


def com_ports() -> list[str]:
    try:
        code = "from serial.tools.list_ports import comports\nprint('\\n'.join(p.device for p in comports()))"
        out = subprocess.check_output(
            [str(PY), "-c", code],
            cwd=str(ROOT),
            text=True,
            timeout=12,
            creationflags=CREATE_NO_WINDOW,
        )
        return [x for x in out.splitlines() if x.strip()]
    except Exception:
        return []