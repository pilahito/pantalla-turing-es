# -*- coding: utf-8 -*-
"""Toasts nativos de Windows (esquina inferior derecha)."""
from __future__ import annotations


def toast(title: str, message: str, duration: str = "short") -> None:
    """Muestra un toast de Windows; si falla, no rompe la app."""
    try:
        from winotify import Notification, audio

        n = Notification(
            app_id="Centro Turing",
            title=title,
            msg=message,
            duration=duration,
        )
        n.set_audio(audio.Default, loop=False)
        n.show()
    except Exception:
        pass
