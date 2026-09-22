# -*- coding: utf-8 -*-
"""Paleta y tipografía — oscuro (default) y claro."""
from __future__ import annotations

FONT = "Segoe UI"
FONT_MONO = "Consolas"
COLOR_THEME = "dark-blue"
APPEARANCE = "dark"
MODE = "dark"  # dark | light

_DARK = dict(
    BG="#0B0F14",
    SURFACE="#121821",
    SURFACE_2="#182230",
    CARD="#1A2330",
    BORDER="#2A3545",
    PRIMARY="#3DCCC7",
    PRIMARY_DIM="#2A9E9A",
    AMBER="#E0A050",
    SUCCESS="#3DD68C",
    DANGER="#F07178",
    TEXT="#E8EEF5",
    MUTED="#8B9BAB",
    PRO_GOLD="#F5C542",
)

_LIGHT = dict(
    BG="#F4F7FB",
    SURFACE="#FFFFFF",
    SURFACE_2="#E8EEF5",
    CARD="#FFFFFF",
    BORDER="#D0D8E4",
    PRIMARY="#1FA8A3",
    PRIMARY_DIM="#17807C",
    AMBER="#C47E20",
    SUCCESS="#1FA86A",
    DANGER="#D64550",
    TEXT="#0F1720",
    MUTED="#5A6A7A",
    PRO_GOLD="#B8860B",
)


def apply(mode: str = "dark") -> None:
    global MODE, APPEARANCE, BG, SURFACE, SURFACE_2, CARD, BORDER
    global PRIMARY, PRIMARY_DIM, AMBER, SUCCESS, DANGER, TEXT, MUTED, PRO_GOLD
    MODE = "light" if mode == "light" else "dark"
    APPEARANCE = MODE
    pal = _LIGHT if MODE == "light" else _DARK
    for k, v in pal.items():
        globals()[k] = v


apply("dark")

MOTTO = "Software libre y gratuito para pantallas Turing"
