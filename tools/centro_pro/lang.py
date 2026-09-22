# -*- coding: utf-8 -*-
"""i18n dinámico: JSON es/en, cambio al instante sin reiniciar."""
from __future__ import annotations
import json
from pathlib import Path

_DIR = Path(__file__).resolve().parent / "i18n"
_lang = "es"
_strings: dict = {}
_listeners: list = []


def _load(lang: str) -> dict:
    path = _DIR / f"{lang}.json"
    if not path.is_file():
        path = _DIR / "es.json"
    return json.loads(path.read_text(encoding="utf-8"))


def init(lang: str = "es") -> None:
    global _lang, _strings
    _lang = lang if lang in ("es", "en") else "es"
    _strings = _load(_lang)


def lang() -> str:
    return _lang


def t(key: str, **kwargs) -> str:
    val = _strings.get(key, key)
    if isinstance(val, dict):
        return str(val)
    if kwargs:
        try:
            return str(val).format(**kwargs)
        except Exception:
            return str(val)
    return str(val)


def tmap(group: str) -> dict:
    val = _strings.get(group) or {}
    return val if isinstance(val, dict) else {}


def on_change(cb) -> None:
    if cb not in _listeners:
        _listeners.append(cb)


def set_lang(lang: str) -> None:
    global _lang, _strings
    lang = "en" if lang == "en" else "es"
    if lang == _lang and _strings:
        return
    _lang = lang
    _strings = _load(_lang)
    for cb in list(_listeners):
        try:
            cb(_lang)
        except Exception:
            pass


init("es")
