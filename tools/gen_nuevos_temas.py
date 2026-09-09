# -*- coding: utf-8 -*-
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(r"E:\turing-smart-screen-python\res\themes")
FONT = "roboto-mono/RobotoMono-Bold.ttf"
DIGI = "digital/DIGITAL-7.TTF"

def yaml(size, ori, rgb, w, h):
    hour_x = w - 110
    return f'''author: CentroTuring
display:
  DISPLAY_SIZE: {size}
  DISPLAY_ORIENTATION: {ori}
  DISPLAY_RGB_LED: {rgb}
static_images:
  BACKGROUND:
    PATH: background.png
    X: 0
    Y: 0
    WIDTH: {w}
    HEIGHT: {h}
STATS:
  CPU:
    PERCENTAGE:
      INTERVAL: 1
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 24
        Y: 48
        FONT: {DIGI}
        FONT_SIZE: 42
        FONT_COLOR: {rgb}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 24
        Y: 100
        WIDTH: {min(180, w-48)}
        HEIGHT: 14
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: {rgb}
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  GPU:
    INTERVAL: 1
    PERCENTAGE:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 24
        Y: 150
        FONT: {DIGI}
        FONT_SIZE: 42
        FONT_COLOR: {rgb}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
  DATE:
    INTERVAL: 1
    HOUR:
      TEXT:
        SHOW: True
        FORMAT: "HH:mm"
        X: {hour_x}
        Y: 16
        FONT: {FONT}
        FONT_SIZE: 22
        FONT_COLOR: {rgb}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
'''

def save(name, size, ori, rgb, w, h, paint):
    d = ROOT / name
    if (d / "theme.yaml").exists():
        print("skip", name)
        return
    d.mkdir(parents=True, exist_ok=True)
    im = Image.new("RGB", (w, h), (8, 10, 16))
    paint(ImageDraw.Draw(im), w, h)
    im.save(d / "background.png")
    (d / "theme.yaml").write_text(yaml(size, ori, rgb, w, h), encoding="utf-8")
    print("made", name)

def vice(dr, w, h):
    dr.rectangle((0, 0, w, h), fill=(18, 8, 32))
    for x in range(0, w, 16):
        dr.line((x, 0, x, h), fill=(40, 16, 60))
    dr.rectangle((0, h-70, w, h), fill=(28, 10, 48))
    dr.rectangle((0, 0, w, 8), fill=(255, 40, 160))
    dr.rectangle((0, 8, w, 12), fill=(40, 220, 255))

def bloque(dr, w, h):
    grass, dirt, sky = (86, 168, 70), (120, 82, 48), (126, 196, 238)
    dr.rectangle((0, 0, w, h), fill=sky)
    bs = 16
    for y in range(h-80, h, bs):
        for x in range(0, w, bs):
            c = grass if y == h-80 else dirt
            dr.rectangle((x, y, x+bs-1, y+bs-1), fill=c, outline=(40, 40, 40))

save("ViceES", '3.5"', "landscape", "255, 60, 170", 480, 320, vice)
save("BloqueES", '3.5"', "landscape", "86, 200, 80", 480, 320, bloque)
save("ViceES_V", '3.5"', "portrait", "255, 60, 170", 320, 480, vice)
save("BloqueES_V", '3.5"', "portrait", "86, 200, 80", 320, 480, bloque)
print("done")