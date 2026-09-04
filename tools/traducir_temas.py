# -*- coding: utf-8 -*-
"""Traduce etiquetas inglesas de theme.yaml a español (solo valores TEXT)."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1] / "res" / "themes"

REPL = [
    (r'(TEXT:\s*")Used:"', r'\1Usado:"'),
    (r"(TEXT:\s*')Used:'", r"\1Usado:'"),
    (r'(TEXT:\s*")Free:"', r'\1Libre:"'),
    (r'(TEXT:\s*")Total:"', r'\1Total:"'),
    (r'(TEXT:\s*")Used    -"', r'\1Usado   -"'),
    (r'(TEXT:\s*")Free    -"', r'\1Libre   -"'),
    (r'(TEXT:\s*")ETH Upload"', r'\1Subida ETH"'),
    (r'(TEXT:\s*")WL Upload"', r'\1Subida WiFi"'),
    (r'(TEXT:\s*")MEMORY"', r'\1MEMORIA"'),
    (r'(TEXT:\s*")Disk C"', r'\1Disco C"'),
    (r'(TEXT:\s*")DISK"', r'\1DISCO"'),
    (r'(TEXT:\s*")Download"', r'\1Bajada"'),
    (r'(TEXT:\s*")Upload"', r'\1Subida"'),
    (r'(TEXT:\s*")Storage"', r'\1Almacen"'),
    (r'(TEXT:\s*")Network"', r'\1Red"'),
    (r'(TEXT:\s*")Weather"', r'\1Tiempo"'),
    (r'(TEXT:\s*")Temperature"', r'\1Temperatura"'),
    (r'(TEXT:\s*")Humidity"', r'\1Humedad"'),
    (r'(TEXT:\s*")Date"', r'\1Fecha"'),
    (r'(TEXT:\s*")Time"', r'\1Hora"'),
    (r'(TEXT:\s*")USED"', r'\1USADO"'),
    (r'(TEXT:\s*")FREE"', r'\1LIBRE"'),
]


def main():
    n = 0
    for y in ROOT.rglob("theme.yaml"):
        t = y.read_text(encoding="utf-8")
        orig = t
        for pat, rep in REPL:
            t = re.sub(pat, rep, t)
        if t != orig:
            y.write_text(t, encoding="utf-8")
            n += 1
            print("es", y.parent.name)
    print("updated", n)


if __name__ == "__main__":
    main()
