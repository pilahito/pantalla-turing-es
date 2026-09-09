from pathlib import Path
import shutil
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"E:\turing-smart-screen-python")
THEMES = ROOT / "res" / "themes"
FONTS = ROOT / "res" / "fonts"
SRC = THEMES / "ConilES"
W, H = 480, 320

def font(size):
    return ImageFont.truetype(str(FONTS / "roboto-mono" / "RobotoMono-Bold.ttf"), size)

def label(d, text, xy, fill, size=12):
    d.text(xy, text, font=font(size), fill=fill)

def save_theme(name, img, yaml_text):
    dest = THEMES / name
    if dest.exists():
        shutil.rmtree(dest)
    shutil.copytree(SRC, dest)
    img.convert("RGB").save(dest / "background.png")
    img.convert("RGB").save(dest / "preview.png")
    (dest / "theme.yaml").write_text(yaml_text, encoding="utf-8")
    print("wrote", name)

base = (SRC / "theme.yaml").read_text(encoding="utf-8")

def paint_marea():
    im = Image.new("RGB", (W, H), (8, 22, 32))
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, W, 44), fill=(6, 40, 48))
    d.rectangle((0, 44, W, 46), fill=(64, 210, 200))
    d.rectangle((8, 58, 232, 188), outline=(40, 120, 130), width=1)
    d.rectangle((246, 58, 470, 188), outline=(40, 120, 130), width=1)
    d.rectangle((8, 200, 232, 308), outline=(40, 120, 130), width=1)
    d.rectangle((246, 200, 470, 308), outline=(40, 120, 130), width=1)
    for i in range(8):
        y = 70 + i * 14
        d.arc((20, y, 220, y + 40), 200, 340, fill=(20, 70, 80))
    c = (120, 220, 210)
    label(d, "MAREA", (16, 12), c, 16)
    label(d, "CPU", (16, 64), c, 11)
    label(d, "GPU", (254, 64), (180, 240, 230), 11)
    label(d, "RAM", (16, 208), c, 11)
    label(d, "DISCO", (174, 208), c, 11)
    label(d, "BAJADA", (300, 214), (140, 200, 190), 10)
    label(d, "SUBIDA", (300, 258), (140, 200, 190), 10)
    label(d, "TIEMPO", (16, 14), (180, 210, 200), 10)
    return im

def paint_forja():
    im = Image.new("RGB", (W, H), (28, 18, 14))
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, W, 8), fill=(210, 120, 40))
    d.rectangle((0, 312, W, 320), fill=(210, 120, 40))
    tiles = [(10, 52, 230, 150), (248, 52, 468, 150), (10, 168, 230, 300), (248, 168, 468, 300)]
    for x1,y1,x2,y2 in tiles:
        d.rectangle((x1, y1, x2, y2), fill=(42, 26, 18), outline=(180, 90, 36), width=2)
        d.ellipse((x1+6, y1+6, x1+12, y1+12), fill=(120, 70, 30))
        d.ellipse((x2-12, y1+6, x2-6, y1+12), fill=(120, 70, 30))
    c = (255, 170, 70)
    label(d, "FORJA", (16, 16), c, 16)
    label(d, "CPU", (18, 58), c, 11)
    label(d, "GPU", (256, 58), (255, 210, 140), 11)
    label(d, "RAM", (18, 176), c, 11)
    label(d, "DISCO / RED", (256, 176), c, 11)
    return im

def paint_radar():
    im = Image.new("RGB", (W, H), (6, 14, 10))
    d = ImageDraw.Draw(im)
    cx, cy, r = 240, 168, 70
    for rr, col in ((92, (18, 50, 36)), (70, (24, 80, 52)), (46, (30, 110, 70))):
        d.ellipse((cx-rr, cy-rr, cx+rr, cy+rr), outline=col, width=1)
    d.line((cx-92, cy, cx+92, cy), fill=(30, 90, 55))
    d.line((cx, cy-92, cx, cy+92), fill=(30, 90, 55))
    d.pieslice((cx-90, cy-90, cx+90, cy+90), 250, 310, fill=(16, 48, 34))
    c = (120, 255, 140)
    label(d, "RADAR", (16, 10), c, 16)
    label(d, "CPU", (16, 46), c, 11)
    label(d, "GPU", (330, 46), (170, 255, 190), 11)
    label(d, "RAM", (16, 228), c, 11)
    label(d, "DISCO", (150, 270), c, 11)
    label(d, "RED", (360, 228), (170, 255, 190), 11)
    return im

def recolor(text, led, pairs):
    text = text.replace("author: \"Ayistax\"", "author: \"Ayistax\"")
    text = text.replace("DISPLAY_RGB_LED: 255, 196, 92", "DISPLAY_RGB_LED: " + led)
    for a, b in pairs:
        text = text.replace(a, b)
    return text

marea_yaml = recolor(base, "64, 210, 200", [
    ("255, 196, 92", "120, 220, 210"),
    ("64, 210, 220", "180, 240, 230"),
])
save_theme("MareaES", paint_marea(), marea_yaml)

forja_yaml = recolor(base, "255, 150, 50", [
    ("255, 196, 92", "255, 170, 70"),
    ("64, 210, 220", "255, 210, 140"),
])
save_theme("ForjaES", paint_forja(), forja_yaml)

# Radar: different slots
radar = base
radar = radar.replace("DISPLAY_RGB_LED: 255, 196, 92", "DISPLAY_RGB_LED: 80, 255, 140")
radar = radar.replace("255, 196, 92", "120, 255, 140")
radar = radar.replace("64, 210, 220", "170, 255, 190")
# move CPU text/graph up-left already close; shift GPU to top-right
radar = radar.replace("X: 254\n        Y: 108", "X: 330\n        Y: 64", 1)
radar = radar.replace("X: 254\n        Y: 154", "X: 330\n        Y: 108", 1)
radar = radar.replace("X: 16\n        Y: 108", "X: 16\n        Y: 64", 1)
radar = radar.replace("X: 16\n        Y: 154", "X: 16\n        Y: 108", 1)
radar = radar.replace("X: 148\n        Y: 114", "X: 140\n        Y: 68", 1)
radar = radar.replace("X: 386\n        Y: 114", "X: 400\n        Y: 68", 1)
radar = radar.replace("X: 16\n        Y: 250", "X: 16\n        Y: 248", 1)
radar = radar.replace("X: 16\n        Y: 286", "X: 16\n        Y: 292", 1)
save_theme("RadarES", paint_radar(), radar)
print("ok")