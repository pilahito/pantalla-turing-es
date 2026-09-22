# -*- coding: utf-8 -*-
"""Icono en la bandeja del sistema (junto al reloj)."""
from __future__ import annotations
import threading
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ICON_CANDIDATES = [
    ROOT / "res" / "icons" / "monitor-icon-17865" / "icon.ico",
    ROOT / "res" / "icons" / "icon.ico",
    ROOT / "icon.ico",
]


def _load_icon():
    from PIL import Image

    for p in ICON_CANDIDATES:
        if p.is_file():
            try:
                return Image.open(p)
            except Exception:
                pass
    # fallback: teal square
    return Image.new("RGB", (64, 64), color=(61, 204, 199))


class SystemTray:
    def __init__(self, app):
        self.app = app
        self._icon = None
        self._thread = None

    def start(self):
        if self._thread and self._thread.is_alive():
            return

        def run():
            import pystray
            from pystray import MenuItem as Item
            from . import lang as I

            menu = pystray.Menu(
                Item(I.t("tray_show"), self._show, default=True),
                Item(I.t("tray_on"), self._on),
                Item(I.t("tray_off"), self._off),
                Item(I.t("tray_quit"), self._quit),
            )
            self._icon = pystray.Icon(
                "centro_turing",
                _load_icon(),
                "Centro Turing",
                menu,
            )
            self._icon.run()

        self._thread = threading.Thread(target=run, daemon=True)
        self._thread.start()

    def stop(self):
        try:
            if self._icon:
                self._icon.stop()
        except Exception:
            pass

    def _show(self, icon=None, item=None):
        self.app.after(0, self.app.restore_from_tray)

    def _on(self, icon=None, item=None):
        self.app.after(0, self.app.tray_start_monitor)

    def _off(self, icon=None, item=None):
        self.app.after(0, self.app.tray_stop_monitor)

    def _quit(self, icon=None, item=None):
        self.app.after(0, self.app.quit_app)
