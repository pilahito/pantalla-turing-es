from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import re, time, subprocess

ROOT = Path(r"E:\turing-smart-screen-python")
DEST = ROOT / "res" / "themes" / "QuantumES"
DEST.mkdir(parents=True, exist_ok=True)
MONO = str(ROOT / "res" / "fonts" / "roboto-mono" / "RobotoMono-Bold.ttf")
DIGI = str(ROOT / "res" / "fonts" / "digital" / "DIGITAL-7.TTF")

def font(path, size):
    return ImageFont.truetype(path, size)

img = Image.new("RGB", (480, 320), (4, 10, 18))
dr = ImageDraw.Draw(img)

def frame(box):
    dr.rounded_rectangle(box, radius=8, outline=(180, 120, 40), width=2)

dr.line((16, 28, 150, 28), fill=(180, 120, 40), width=2)
dr.line((330, 28, 464, 28), fill=(180, 120, 40), width=2)
dr.text((168, 8), "QUANTUM UI", fill=(255, 176, 48), font=font(MONO, 16))

frame((10, 40, 300, 168))
frame((10, 176, 300, 308))
frame((308, 40, 470, 200))
frame((308, 208, 470, 308))

dr.text((22, 52), "CPU", fill=(255, 150, 40), font=font(MONO, 14))
dr.text((22, 118), "TEMP", fill=(255, 176, 70), font=font(MONO, 12))
dr.text((22, 188), "GPU", fill=(255, 150, 40), font=font(MONO, 14))
dr.text((22, 258), "TEMP", fill=(255, 176, 70), font=font(MONO, 12))
dr.text((318, 48), "RAM", fill=(160, 220, 255), font=font(MONO, 11))
dr.text((390, 48), "DISCO", fill=(255, 200, 90), font=font(MONO, 11))
dr.text((318, 216), "BAJADA", fill=(80, 220, 230), font=font(MONO, 11))
dr.text((318, 262), "SUBIDA", fill=(120, 180, 255), font=font(MONO, 11))

img.save(DEST / "background.png")

yaml = """author: \"Ayistax\"
display:
  DISPLAY_SIZE: 3.5\"
  DISPLAY_ORIENTATION: landscape
  DISPLAY_RGB_LED: 255, 140, 30

static_images:
  BACKGROUND:
    PATH: background.png
    X: 0
    Y: 0
    WIDTH: 480
    HEIGHT: 320

STATS:
  CPU:
    PERCENTAGE:
      INTERVAL: 1
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 78
        Y: 48
        WIDTH: 140
        HEIGHT: 52
        FONT: digital/DIGITAL-7.TTF
        FONT_SIZE: 52
        FONT_COLOR: 255, 160, 30
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 150
        Y: 122
        WIDTH: 130
        HEIGHT: 18
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: 255, 160, 40
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
    TEMPERATURE:
      INTERVAL: 5
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 86
        Y: 114
        WIDTH: 70
        HEIGHT: 24
        FONT: digital/DIGITAL-7.TTF
        FONT_SIZE: 22
        FONT_COLOR: 255, 186, 70
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
  GPU:
    INTERVAL: 1
    PERCENTAGE:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 78
        Y: 184
        WIDTH: 140
        HEIGHT: 52
        FONT: digital/DIGITAL-7.TTF
        FONT_SIZE: 52
        FONT_COLOR: 255, 160, 30
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 150
        Y: 262
        WIDTH: 130
        HEIGHT: 18
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: 80, 220, 230
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
    TEMPERATURE:
      INTERVAL: 5
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 86
        Y: 252
        WIDTH: 70
        HEIGHT: 24
        FONT: digital/DIGITAL-7.TTF
        FONT_SIZE: 22
        FONT_COLOR: 255, 186, 70
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
  MEMORY:
    INTERVAL: 5
    VIRTUAL:
      PERCENT_TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 318
        Y: 158
        WIDTH: 64
        HEIGHT: 22
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 14
        FONT_COLOR: 180, 230, 255
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 318
        Y: 72
        WIDTH: 28
        HEIGHT: 80
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: 140, 210, 255
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  DISK:
    INTERVAL: 10
    USED:
      PERCENT_TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 390
        Y: 158
        WIDTH: 70
        HEIGHT: 22
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 14
        FONT_COLOR: 255, 200, 90
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 392
        Y: 72
        WIDTH: 28
        HEIGHT: 80
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: 255, 190, 70
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  NET:
    INTERVAL: 1
    ETH:
      DOWNLOAD:
        TEXT:
          SHOW: True
          X: 318
          Y: 232
          WIDTH: 140
          HEIGHT: 22
          FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
          FONT_SIZE: 14
          FONT_COLOR: 80, 230, 235
          BACKGROUND_IMAGE: background.png
          ALIGN: left
          ANCHOR: lt
      UPLOAD:
        TEXT:
          SHOW: True
          X: 318
          Y: 276
          WIDTH: 140
          HEIGHT: 22
          FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
          FONT_SIZE: 14
          FONT_COLOR: 140, 190, 255
          BACKGROUND_IMAGE: background.png
          ALIGN: left
          ANCHOR: lt
  DATE:
    INTERVAL: 1
    DAY:
      TEXT:
        FORMAT: \"dd MMM\"
        SHOW: True
        X: 150
        Y: 30
        WIDTH: 90
        HEIGHT: 14
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 10
        FONT_COLOR: 180, 140, 70
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
    HOUR:
      TEXT:
        FORMAT: \"HH:mm\"
        SHOW: True
        X: 250
        Y: 28
        WIDTH: 60
        HEIGHT: 16
        FONT: digital/DIGITAL-7.TTF
        FONT_SIZE: 16
        FONT_COLOR: 255, 176, 48
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
"""
# jetbrains may be encrypted; use roboto-mono for net
yaml = yaml.replace("jetbrains-mono/JetBrainsMono-Bold.ttf", "roboto-mono/RobotoMono-Bold.ttf")
(DEST / "theme.yaml").write_text(yaml, encoding="utf-8")
print("theme ok", DEST)
