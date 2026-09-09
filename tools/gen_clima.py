from pathlib import Path
from PIL import Image, ImageDraw
root = Path(r"E:\turing-smart-screen-python\res\themes\ClimaES")
root.mkdir(exist_ok=True)
im = Image.new("RGB", (480, 320), (28, 78, 130))
d = ImageDraw.Draw(im)
d.rectangle((0, 180, 480, 320), fill=(46, 120, 168))
d.ellipse((300, 28, 390, 118), fill=(255, 214, 120))
d.ellipse((40, 50, 160, 110), fill=(230, 240, 248))
d.ellipse((90, 40, 210, 100), fill=(210, 228, 242))
im.save(root / "background.png")
rgb = "240, 248, 255"
(root / "theme.yaml").write_text("""author: CentroTuring
display:
  DISPLAY_SIZE: 3.5"
  DISPLAY_ORIENTATION: landscape
  DISPLAY_RGB_LED: 120, 190, 255
static_images:
  BACKGROUND:
    PATH: background.png
    X: 0
    Y: 0
    WIDTH: 480
    HEIGHT: 320
STATS:
  WEATHER:
    INTERVAL: 300
    TEMPERATURE:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 28
        Y: 70
        FONT: digital/DIGITAL-7.TTF
        FONT_SIZE: 72
        FONT_COLOR: 255, 248, 230
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
    TEMPERATURE_FELT:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 28
        Y: 160
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 18
        FONT_COLOR: 220, 236, 255
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
    WEATHER_DESCRIPTION:
      TEXT:
        SHOW: True
        X: 28
        Y: 196
        WIDTH: 260
        HEIGHT: 28
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 16
        FONT_COLOR: 255, 255, 255
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
    HUMIDITY:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 28
        Y: 230
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 16
        FONT_COLOR: 200, 230, 255
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
  DATE:
    INTERVAL: 1
    HOUR:
      TEXT:
        SHOW: True
        FORMAT: "HH:mm"
        X: 360
        Y: 18
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 24
        FONT_COLOR: 255, 248, 230
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
    DAY:
      TEXT:
        SHOW: True
        FORMAT: "dd MMM"
        X: 300
        Y: 24
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 14
        FONT_COLOR: 220, 236, 255
        BACKGROUND_IMAGE: background.png
        ALIGN: right
        ANCHOR: rt
""", encoding="utf-8")
print("theme-ok")