# -*- coding: utf-8 -*-
"""Clona temas 3.5\" verticales a horizontal 480x320 (sufijo _H)."""
from __future__ import annotations

import copy
from pathlib import Path

import yaml
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
THEMES = ROOT / "res" / "themes"
PW, PH = 320, 480
LW, LH = 480, 320


def load(p: Path):
    with p.open(encoding="utf-8") as f:
        return yaml.safe_load(f)


def is_portrait_35(data: dict, folder: Path) -> bool:
    disp = data.get("display") or {}
    orient = str(disp.get("DISPLAY_ORIENTATION", "portrait")).lower()
    size = str(disp.get("DISPLAY_SIZE", "3.5")).replace('"', "").replace("'", "").strip()
    bg = ((data.get("static_images") or {}).get("BACKGROUND") or {})
    w, h = int(bg.get("WIDTH") or 0), int(bg.get("HEIGHT") or 0)
    if orient == "landscape":
        return False
    if size and size not in ("3.5", "3.5inch"):
        return False
    if w and h:
        return w <= 360 and h >= 400
    img = folder / str(bg.get("PATH") or "background.png")
    if img.exists():
        im = Image.open(img)
        return im.size[0] <= 360 and im.size[1] >= 400
    return False


def scale_box(node: dict):
    if not isinstance(node, dict):
        return
    if "X" in node and "Y" in node:
        try:
            x, y = int(node["X"]), int(node["Y"])
            w = int(node["WIDTH"]) if node.get("WIDTH") is not None else None
            h = int(node["HEIGHT"]) if node.get("HEIGHT") is not None else None
            node["X"] = int(round(x * LW / PW))
            node["Y"] = int(round(y * LH / PH))
            if w is not None:
                node["WIDTH"] = max(8, int(round(w * LW / PW)))
            if h is not None:
                node["HEIGHT"] = max(8, int(round(h * LH / PH)))
        except Exception:
            pass
    if "RADIUS" in node:
        try:
            node["RADIUS"] = max(6, int(round(int(node["RADIUS"]) * LW / PW)))
        except Exception:
            pass
    for v in node.values():
        if isinstance(v, dict):
            scale_box(v)
        elif isinstance(v, list):
            for i in v:
                if isinstance(i, dict):
                    scale_box(i)


def convert_one(src: Path) -> bool:
    yml = src / "theme.yaml"
    if not yml.exists():
        return False
    data = load(yml)
    if not is_portrait_35(data, src):
        return False
    name = src.name + "_H"
    dst = THEMES / name
    dst.mkdir(parents=True, exist_ok=True)
    data = copy.deepcopy(data)
    disp = data.setdefault("display", {})
    disp["DISPLAY_ORIENTATION"] = "landscape"
    disp["DISPLAY_SIZE"] = '3.5"'
    bg = (data.get("static_images") or {}).get("BACKGROUND") or {}
    path = bg.get("PATH") or "background.png"
    src_img = src / path
    if src_img.exists():
        im = Image.open(src_img).convert("RGB")
        im = im.resize((LW, LH), Image.Resampling.LANCZOS)
        im.save(dst / Path(path).name, optimize=True)
        if Path(path).name != "background.png":
            # keep same filename
            pass
        preview = im.copy()
        preview.save(dst / "preview.png", optimize=True)
    bg["X"] = 0
    bg["Y"] = 0
    bg["WIDTH"] = LW
    bg["HEIGHT"] = LH
    scale_box(data)
    # restore background size after scale_box
    bg["WIDTH"] = LW
    bg["HEIGHT"] = LH
    bg["X"] = 0
    bg["Y"] = 0
    out = dst / "theme.yaml"
    with out.open("w", encoding="utf-8") as f:
        yaml.safe_dump(data, f, allow_unicode=True, sort_keys=False)
    print("H", src.name, "->", name)
    return True


def main():
    n = 0
    for d in sorted(THEMES.iterdir()):
        if not d.is_dir() or d.name.endswith("_H") or d.name.startswith("--"):
            continue
        if convert_one(d):
            n += 1
    print("converted", n)


if __name__ == "__main__":
    main()
