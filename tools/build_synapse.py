# -*- coding: utf-8 -*-
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"E:\turing-smart-screen-python")
DEST = ROOT / "res" / "themes" / "SynapseES"
DEST.mkdir(parents=True, exist_ok=True)
FONT = ROOT / "res" / "fonts" / "jetbrains-mono" / "JetBrainsMono-Bold.ttf"

def fnt(size):
    return ImageFont.truetype(str(FONT), size)

img = Image.new("RGB", (480, 320), (6, 14, 28))
d = ImageDraw.Draw(img)

def panel(box, title=None, title_color=(140, 210, 255)):
    d.rounded_rectangle(box, radius=10, outline=(40, 120, 170), width=2)
    if title:
        d.text((box[0] + 10, box[1] + 6), title, fill=title_color, font=fnt(12))

# title
d.text((168, 8), "SYNAPSE OS", fill=(190, 230, 255), font=fnt(18))

panel((8, 36, 118, 188), "CPU", (0, 220, 255))
panel((8, 196, 118, 308), "RAM", (80, 255, 140))
panel((126, 36, 352, 308), "HISTORIAL", (140, 210, 255))
panel((360, 36, 472, 168), "DISCO", (140, 160, 255))
panel((360, 176, 472, 308), "RED", (0, 230, 170))

d.text((22, 52), "uso", fill=(120, 180, 200), font=fnt(11))
d.text((18, 214), "uso", fill=(120, 200, 160), font=fnt(11))
d.text((138, 58), "CPU", fill=(0, 220, 255), font=fnt(11))
d.text((188, 58), "GPU", fill=(255, 150, 60), font=fnt(11))
d.text((372, 54), "C:", fill=(180, 190, 255), font=fnt(12))
d.text((372, 194), "bajada", fill=(140, 220, 200), font=fnt(11))
d.text((372, 246), "subida", fill=(200, 180, 255), font=fnt(11))

img.save(DEST / "background.png")

yaml = r'''author: "Ayistax"
display:
  DISPLAY_SIZE: 3.5"
  DISPLAY_ORIENTATION: landscape
  DISPLAY_RGB_LED: 0, 180, 255

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
      RADIAL:
        SHOW: True
        X: 48
        Y: 108
        RADIUS: 28
        WIDTH: 7
        MIN_VALUE: 0
        MAX_VALUE: 100
        ANGLE_START: -90
        ANGLE_END: 270
        ANGLE_STEPS: 1
        ANGLE_SEP: 0
        CLOCKWISE: True
        BAR_COLOR: 0, 220, 255
        SHOW_TEXT: True
        SHOW_UNIT: True
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 13
        FONT_COLOR: 230, 250, 255
        BACKGROUND_IMAGE: background.png
      LINE_GRAPH:
        SHOW: True
        X: 138
        Y: 78
        WIDTH: 200
        HEIGHT: 150
        MIN_VALUE: 0
        MAX_VALUE: 100
        HISTORY_SIZE: 60
        AUTOSCALE: False
        LINE_COLOR: 0, 220, 255
        AXIS: False
        BACKGROUND_IMAGE: background.png
    FREQUENCY:
      INTERVAL: 2
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 16
        Y: 156
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 12
        FONT_COLOR: 180, 220, 235
        BACKGROUND_IMAGE: background.png
    TEMPERATURE:
      INTERVAL: 5
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 62
        Y: 96
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 14
        FONT_COLOR: 255, 150, 60
        BACKGROUND_IMAGE: background.png
  GPU:
    INTERVAL: 1
    PERCENTAGE:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 250
        Y: 242
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 16
        FONT_COLOR: 255, 150, 60
        BACKGROUND_IMAGE: background.png
    TEMPERATURE:
      INTERVAL: 5
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 140
        Y: 250
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 14
        FONT_COLOR: 255, 170, 80
        BACKGROUND_IMAGE: background.png
  MEMORY:
    INTERVAL: 5
    VIRTUAL:
      PERCENT_TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 18
        Y: 248
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 22
        FONT_COLOR: 80, 255, 140
        BACKGROUND_IMAGE: background.png
      GRAPH:
        SHOW: True
        X: 18
        Y: 278
        WIDTH: 88
        HEIGHT: 16
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: 80, 255, 140
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  DISK:
    INTERVAL: 10
    USED:
      PERCENT_TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 372
        Y: 78
        FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
        FONT_SIZE: 20
        FONT_COLOR: 180, 200, 255
        BACKGROUND_IMAGE: background.png
      GRAPH:
        SHOW: True
        X: 372
        Y: 128
        WIDTH: 88
        HEIGHT: 12
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: 140, 160, 255
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  NET:
    INTERVAL: 1
    ETH:
      DOWNLOAD:
        TEXT:
          SHOW: True
          X: 372
          Y: 214
          FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
          FONT_SIZE: 12
          FONT_COLOR: 0, 230, 170
          BACKGROUND_IMAGE: background.png
      UPLOAD:
        TEXT:
          SHOW: True
          X: 372
          Y: 266
          FONT: jetbrains-mono/JetBrainsMono-Bold.ttf
          FONT_SIZE: 12
          FONT_COLOR: 200, 180, 255
          BACKGROUND_IMAGE: background.png
'''
(DEST / "theme.yaml").write_text(yaml, encoding="utf-8")
print("wrote", DEST)

