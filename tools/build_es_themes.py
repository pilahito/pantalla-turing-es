"""Build Spanish landscape 3.5\" HUD backgrounds (480x320)."""
from pathlib import Path

from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parents[1]
FONTS = ROOT / "res" / "fonts"
TEX = Path(__file__).resolve().parent / "es_textures"
THEMES = ROOT / "res" / "themes"
W, H = 480, 320


def font(rel: str, size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(FONTS / rel), size)


F_TITLE = lambda s: font("geforce/GeForce-Bold.ttf", s)
F_MONO = lambda s: font("roboto-mono/RobotoMono-Bold.ttf", s)
F_SANS = lambda s: font("roboto-mono/RobotoMono-Bold.ttf", s)
F_TERM = lambda s: font("generale-mono/GeneraleMonoA.ttf", s)


def load_tex(name: str, dark: float = 0.42) -> Image.Image:
    im = Image.open(TEX / name).convert("RGB")
    im = im.resize((W, H), Image.Resampling.LANCZOS)
    im = ImageEnhance.Brightness(im).enhance(dark)
    im = ImageEnhance.Contrast(im).enhance(1.15)
    return im


def panel(base: Image.Image, xywh, fill, outline, radius=10, width=1):
    x, y, w, h = xywh
    overlay = Image.new("RGBA", base.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(overlay)
    d.rounded_rectangle((x, y, x + w, y + h), radius=radius, fill=fill, outline=outline, width=width)
    return Image.alpha_composite(base.convert("RGBA"), overlay)


def accent_bar(draw: ImageDraw.ImageDraw, x, y, w, color):
    draw.rectangle((x, y, x + w, y + 2), fill=color)


def text(draw, xy, s, fnt, fill, anchor="lt"):
    draw.text(xy, s, font=fnt, fill=fill, anchor=anchor)


def save_theme(name: str, bg: Image.Image, preview: Image.Image):
    out = THEMES / name
    out.mkdir(parents=True, exist_ok=True)
    bg.convert("RGB").save(out / "background.png", optimize=True)
    preview.convert("RGB").save(out / "preview.png", optimize=True)
    print(f"wrote {out}")


def horizon():
    bg = load_tex("carbon.jpg", 0.38)
    amber = (255, 176, 48, 230)
    cyan = (64, 210, 255, 230)
    white = (240, 244, 255, 255)
    mute = (170, 186, 210, 220)
    glass = (6, 10, 18, 168)
    line = (255, 176, 48, 90)

    bg = panel(bg, (6, 6, 468, 30), (6, 10, 18, 190), line, 8)
    bg = panel(bg, (6, 42, 230, 168), glass, line, 12)
    bg = panel(bg, (244, 42, 230, 168), glass, (64, 210, 255, 90), 12)
    bg = panel(bg, (6, 218, 152, 94), glass, line, 10)
    bg = panel(bg, (164, 218, 152, 94), glass, line, 10)
    bg = panel(bg, (322, 218, 152, 94), glass, (64, 210, 255, 90), 10)
    bg = bg.filter(ImageFilter.SMOOTH)

    d = ImageDraw.Draw(bg)
    accent_bar(d, 16, 42, 86, amber)
    accent_bar(d, 254, 42, 86, cyan)
    accent_bar(d, 16, 218, 54, amber)
    accent_bar(d, 174, 218, 62, amber)
    accent_bar(d, 332, 218, 48, cyan)

    text(d, (16, 12), "HORIZONTE", F_TITLE(16), amber)
    text(d, (148, 16), "PC 3.5\"", F_SANS(11), mute)
    text(d, (16, 50), "CPU  i5-12400F", F_SANS(13), white)
    text(d, (254, 50), "GPU  RTX 3060", F_SANS(13), white)
    text(d, (16, 226), "RAM", F_SANS(13), white)
    text(d, (174, 226), "DISCO", F_SANS(13), white)
    text(d, (332, 226), "RED", F_SANS(13), white)
    text(d, (16, 96), "USO", F_MONO(9), mute)
    text(d, (150, 96), "TEMP", F_MONO(9), mute)
    text(d, (254, 96), "USO", F_MONO(9), mute)
    text(d, (388, 96), "TEMP", F_MONO(9), mute)
    text(d, (254, 168), "VRAM", F_MONO(9), mute)
    text(d, (332, 250), "BAJADA", F_MONO(8), mute)
    text(d, (332, 278), "SUBIDA", F_MONO(8), mute)

    preview = bg.copy()
    pd = ImageDraw.Draw(preview)
    text(pd, (392, 12), "21:47", F_MONO(16), white)
    text(pd, (292, 14), "30 ago", F_SANS(12), mute)
    text(pd, (20, 108), "18%", F_MONO(34), amber)
    text(pd, (148, 114), "47C", F_MONO(20), white)
    pd.rectangle((16, 154, 220, 164), fill=(28, 34, 48, 255))
    pd.rectangle((16, 154, 52, 164), fill=amber)
    text(pd, (258, 108), "6%", F_MONO(34), cyan)
    text(pd, (386, 114), "41C", F_MONO(20), white)
    pd.rectangle((254, 154, 458, 164), fill=(28, 34, 48, 255))
    pd.rectangle((254, 154, 268, 164), fill=cyan)
    text(pd, (300, 166), "12%", F_MONO(12), cyan)
    pd.rectangle((360, 170, 458, 178), fill=(28, 34, 48, 255))
    pd.rectangle((360, 170, 372, 178), fill=cyan)
    text(pd, (16, 250), "24%", F_MONO(22), amber)
    pd.rectangle((16, 286, 142, 296), fill=(28, 34, 48, 255))
    pd.rectangle((16, 286, 50, 296), fill=amber)
    text(pd, (174, 250), "61%", F_MONO(22), amber)
    pd.rectangle((174, 286, 300, 296), fill=(28, 34, 48, 255))
    pd.rectangle((174, 286, 250, 296), fill=amber)
    text(pd, (400, 248), "1.2M", F_MONO(13), cyan)
    text(pd, (400, 276), "0.1M", F_MONO(13), white)

    save_theme("HorizonES", bg, preview)


def noche():
    bg = load_tex("neon.jpg", 0.48)
    mag = (255, 70, 190, 230)
    cyan = (40, 245, 255, 230)
    white = (245, 245, 255, 255)
    mute = (186, 170, 210, 220)
    glass = (12, 2, 22, 150)
    line = (255, 70, 190, 110)

    bg = panel(bg, (8, 8, 304, 72), glass, line, 14, 2)
    bg = panel(bg, (320, 8, 152, 72), glass, (40, 245, 255, 120), 14, 2)
    bg = panel(bg, (8, 88, 228, 132), glass, line, 14, 2)
    bg = panel(bg, (244, 88, 228, 132), glass, (40, 245, 255, 110), 14, 2)
    bg = panel(bg, (8, 228, 150, 84), glass, line, 12)
    bg = panel(bg, (166, 228, 150, 84), glass, (40, 245, 255, 110), 12)
    bg = panel(bg, (324, 228, 148, 84), glass, line, 12)

    d = ImageDraw.Draw(bg)
    text(d, (20, 16), "NOCHE NEON", F_TITLE(18), mag)
    text(d, (20, 42), "CIUDAD / PC", F_SANS(12), mute)
    text(d, (332, 16), "HORA", F_MONO(10), cyan)
    text(d, (20, 96), "PROCESADOR", F_SANS(12), mag)
    text(d, (256, 96), "GRAFICA", F_SANS(12), cyan)  # sin tilde: mas legible a 12px
    text(d, (20, 236), "MEMORIA", F_SANS(11), mag)
    text(d, (178, 236), "ALMACEN", F_SANS(11), cyan)
    text(d, (336, 236), "RED", F_SANS(11), mag)
    text(d, (20, 168), "CARGA", F_MONO(9), mute)
    text(d, (256, 168), "CARGA", F_MONO(9), mute)

    preview = bg.copy()
    pd = ImageDraw.Draw(preview)
    text(pd, (332, 32), "21:47", F_MONO(22), white)
    text(pd, (332, 58), "dom 30 ago", F_SANS(11), mute)
    text(pd, (20, 118), "18%", F_MONO(36), mag)
    text(pd, (140, 128), "47C", F_MONO(20), white)
    pd.rectangle((20, 186, 220, 198), fill=(40, 10, 40, 255))
    pd.rectangle((20, 186, 60, 198), fill=mag)
    text(pd, (256, 118), "6%", F_MONO(36), cyan)
    text(pd, (376, 128), "41C", F_MONO(20), white)
    pd.rectangle((256, 186, 456, 198), fill=(10, 40, 50, 255))
    pd.rectangle((256, 186, 272, 198), fill=cyan)
    text(pd, (20, 256), "24%", F_MONO(20), white)
    pd.rectangle((20, 288, 142, 298), fill=(40, 10, 40, 255))
    pd.rectangle((20, 288, 54, 298), fill=mag)
    text(pd, (178, 256), "61%", F_MONO(20), white)
    pd.rectangle((178, 288, 300, 298), fill=(10, 40, 50, 255))
    pd.rectangle((178, 288, 254, 298), fill=cyan)
    text(pd, (336, 256), "1.2M", F_MONO(14), white)
    text(pd, (336, 278), "0.1M", F_MONO(14), mute)

    save_theme("NocheNeon", bg, preview)


def terminal():
    bg = load_tex("crt.jpg", 0.55)
    green = (110, 255, 140, 240)
    dim = (70, 160, 90, 220)
    white = (210, 255, 220, 255)
    glass = (0, 12, 4, 175)
    line = (80, 220, 110, 100)

    bg = panel(bg, (10, 10, 460, 300), glass, line, 8, 2)
    d = ImageDraw.Draw(bg)
    text(d, (22, 18), "root@ayistax:~$", F_MONO(14), green)
    text(d, (210, 18), "monitor --lcd 3.5", F_MONO(13), dim)
    d.rectangle((22, 42, 458, 43), fill=line)
    text(d, (22, 52), "CPU   i5-12400F", F_TERM(13), green)
    text(d, (250, 52), "TEMP", F_TERM(13), dim)
    text(d, (22, 108), "GPU   RTX 3060", F_TERM(13), green)
    text(d, (250, 108), "TEMP", F_TERM(13), dim)
    text(d, (22, 164), "RAM   CORS 32G", F_TERM(13), green)
    text(d, (22, 204), "DISCO  WD SN770", F_TERM(13), green)
    text(d, (22, 244), "RED   ethernet", F_TERM(13), green)
    text(d, (22, 280), "HORA", F_TERM(13), dim)
    text(d, (250, 280), "FECHA", F_TERM(13), dim)

    preview = bg.copy()
    pd = ImageDraw.Draw(preview)
    text(pd, (22, 72), "18%", F_TERM(22), white)
    pd.rectangle((110, 80, 330, 90), outline=green, width=1)
    pd.rectangle((112, 82, 150, 88), fill=green)
    text(pd, (250, 72), "47C", F_TERM(22), green)
    text(pd, (22, 128), "6%", F_TERM(22), white)
    pd.rectangle((110, 136, 330, 146), outline=green, width=1)
    pd.rectangle((112, 138, 124, 144), fill=green)
    text(pd, (250, 128), "41C", F_TERM(22), green)
    text(pd, (250, 164), "24%", F_TERM(16), white)
    pd.rectangle((330, 172, 454, 180), outline=green, width=1)
    pd.rectangle((331, 173, 360, 179), fill=green)
    text(pd, (250, 204), "61%", F_TERM(16), white)
    pd.rectangle((330, 212, 454, 220), outline=green, width=1)
    pd.rectangle((331, 213, 406, 219), fill=green)
    text(pd, (250, 244), "dn 1.2M", F_TERM(14), white)
    text(pd, (360, 244), "up 0.1M", F_TERM(14), dim)
    text(pd, (80, 280), "21:47:03", F_TERM(14), white)
    text(pd, (320, 280), "30/08/2026", F_TERM(14), white)

    save_theme("TerminalES", bg, preview)


def rgb(c):
    return f"{c[0]}, {c[1]}, {c[2]}"


def write_horizon_yaml(name: str, accent, accent2, white, mute, led=None):
    """Misma plantilla 480x320 que HorizonES (stats fiables)."""
    a, b = accent[:3], accent2[:3]
    w, m = white[:3], mute[:3]
    led = led or a
    yaml = f"""author: "Ayistax"
display:
  DISPLAY_SIZE: 3.5"
  DISPLAY_ORIENTATION: landscape
  DISPLAY_RGB_LED: {rgb(led)}

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
        X: 16
        Y: 108
        WIDTH: 120
        HEIGHT: 40
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 34
        FONT_COLOR: {rgb(a)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 16
        Y: 154
        WIDTH: 204
        HEIGHT: 10
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: {rgb(a)}
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
    TEMPERATURE:
      INTERVAL: 5
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 148
        Y: 114
        WIDTH: 78
        HEIGHT: 28
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 20
        FONT_COLOR: {rgb(w)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
  GPU:
    INTERVAL: 1
    PERCENTAGE:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 254
        Y: 108
        WIDTH: 120
        HEIGHT: 40
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 34
        FONT_COLOR: {rgb(b)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 254
        Y: 154
        WIDTH: 204
        HEIGHT: 10
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: {rgb(b)}
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
    TEMPERATURE:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 386
        Y: 114
        WIDTH: 78
        HEIGHT: 28
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 20
        FONT_COLOR: {rgb(w)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
    MEMORY_PERCENT:
      TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 300
        Y: 166
        WIDTH: 56
        HEIGHT: 16
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 12
        FONT_COLOR: {rgb(b)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 360
        Y: 170
        WIDTH: 98
        HEIGHT: 8
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: {rgb(b)}
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  MEMORY:
    INTERVAL: 5
    VIRTUAL:
      PERCENT_TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 16
        Y: 250
        WIDTH: 90
        HEIGHT: 28
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 22
        FONT_COLOR: {rgb(a)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 16
        Y: 286
        WIDTH: 126
        HEIGHT: 10
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: {rgb(a)}
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  DISK:
    INTERVAL: 10
    USED:
      PERCENT_TEXT:
        SHOW: True
        SHOW_UNIT: True
        X: 174
        Y: 250
        WIDTH: 90
        HEIGHT: 28
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 22
        FONT_COLOR: {rgb(a)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
      GRAPH:
        SHOW: True
        X: 174
        Y: 286
        WIDTH: 126
        HEIGHT: 10
        MIN_VALUE: 0
        MAX_VALUE: 100
        BAR_COLOR: {rgb(a)}
        BAR_OUTLINE: False
        BACKGROUND_IMAGE: background.png
  NET:
    INTERVAL: 1
    ETH:
      DOWNLOAD:
        TEXT:
          SHOW: True
          X: 390
          Y: 248
          WIDTH: 74
          HEIGHT: 18
          FONT: roboto-mono/RobotoMono-Bold.ttf
          FONT_SIZE: 13
          FONT_COLOR: {rgb(b)}
          BACKGROUND_IMAGE: background.png
          ALIGN: right
          ANCHOR: rt
      UPLOAD:
        TEXT:
          SHOW: True
          X: 390
          Y: 276
          WIDTH: 74
          HEIGHT: 18
          FONT: roboto-mono/RobotoMono-Bold.ttf
          FONT_SIZE: 13
          FONT_COLOR: {rgb(w)}
          BACKGROUND_IMAGE: background.png
          ALIGN: right
          ANCHOR: rt
  DATE:
    INTERVAL: 1
    DAY:
      TEXT:
        FORMAT: "dd MMM"
        SHOW: True
        X: 292
        Y: 14
        WIDTH: 88
        HEIGHT: 18
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 12
        FONT_COLOR: {rgb(m)}
        BACKGROUND_IMAGE: background.png
        ALIGN: right
        ANCHOR: rt
    HOUR:
      TEXT:
        FORMAT: "HH:mm"
        SHOW: True
        X: 392
        Y: 12
        WIDTH: 72
        HEIGHT: 22
        FONT: roboto-mono/RobotoMono-Bold.ttf
        FONT_SIZE: 16
        FONT_COLOR: {rgb(w)}
        BACKGROUND_IMAGE: background.png
        ALIGN: left
        ANCHOR: lt
"""
    out = THEMES / name
    out.mkdir(parents=True, exist_ok=True)
    (out / "theme.yaml").write_text(yaml, encoding="utf-8")


def tint_tex(name: str, dark: float, rgb_mul):
    im = load_tex(name, dark).convert("RGB")
    px = im.load()
    r, g, b = rgb_mul
    for y in range(H):
        for x in range(W):
            pr, pg, pb = px[x, y]
            px[x, y] = (
                min(255, int(pr * r)),
                min(255, int(pg * g)),
                min(255, int(pb * b)),
            )
    return im


def gradient_bg(c1, c2):
    im = Image.new("RGB", (W, H))
    px = im.load()
    for y in range(H):
        t = y / (H - 1)
        col = tuple(int(c1[i] * (1 - t) + c2[i] * t) for i in range(3))
        for x in range(W):
            px[x, y] = col
    return im


def hud_horizon(bg, title, a, b, white, mute, cpu_lbl="CPU", gpu_lbl="GPU"):
    glass = (6, 10, 18, 168)
    line = a[:3] + (90,)
    bg = panel(bg, (6, 6, 468, 30), (6, 10, 18, 190), line, 8)
    bg = panel(bg, (6, 42, 230, 168), glass, line, 12)
    bg = panel(bg, (244, 42, 230, 168), glass, b[:3] + (90,), 12)
    bg = panel(bg, (6, 218, 152, 94), glass, line, 10)
    bg = panel(bg, (164, 218, 152, 94), glass, line, 10)
    bg = panel(bg, (322, 218, 152, 94), glass, b[:3] + (90,), 10)
    bg = bg.filter(ImageFilter.SMOOTH)
    d = ImageDraw.Draw(bg)
    accent_bar(d, 16, 42, 86, a)
    accent_bar(d, 254, 42, 86, b)
    accent_bar(d, 16, 218, 54, a)
    accent_bar(d, 174, 218, 62, a)
    accent_bar(d, 332, 218, 48, b)
    text(d, (16, 12), title, F_TITLE(16), a)
    text(d, (16, 50), cpu_lbl, F_SANS(13), white)
    text(d, (254, 50), gpu_lbl, F_SANS(13), white)
    text(d, (16, 226), "RAM", F_SANS(13), white)
    text(d, (174, 226), "DISCO", F_SANS(13), white)
    text(d, (332, 226), "RED", F_SANS(13), white)
    text(d, (16, 96), "USO", F_MONO(9), mute)
    text(d, (150, 96), "TEMP", F_MONO(9), mute)
    text(d, (254, 96), "USO", F_MONO(9), mute)
    text(d, (388, 96), "TEMP", F_MONO(9), mute)
    text(d, (254, 168), "VRAM", F_MONO(9), mute)
    text(d, (332, 250), "BAJADA", F_MONO(8), mute)
    text(d, (332, 278), "SUBIDA", F_MONO(8), mute)
    preview = bg.copy()
    pd = ImageDraw.Draw(preview)
    text(pd, (392, 12), "21:47", F_MONO(16), white)
    text(pd, (20, 108), "18%", F_MONO(34), a)
    text(pd, (258, 108), "6%", F_MONO(34), b)
    return bg, preview


def pack(name, title, bg, a, b, white=(240, 244, 255, 255), mute=(170, 186, 210, 220), cpu="PROCESADOR", gpu="GRAFICA"):
    bg, preview = hud_horizon(bg, title, a, b, white, mute, cpu, gpu)
    save_theme(name, bg, preview)
    write_horizon_yaml(name, a, b, white, mute)
    print("pack", name)


def extra_themes():
    w = (240, 244, 255, 255)
    pack(
        "ConilES", "CONIL",
        tint_tex("neon.jpg", 0.42, (0.55, 0.85, 1.25)),
        (255, 196, 92, 230), (64, 210, 220, 230), w, (180, 200, 210, 220),
        "CPU  PLAYA", "GPU  SOL",
    )
    pack(
        "EmberES", "EMBER",
        tint_tex("carbon.jpg", 0.36, (1.35, 0.45, 0.25)),
        (255, 92, 40, 230), (255, 176, 48, 230), w, (210, 160, 140, 220),
        "CPU  FUEGO", "GPU  LAVA",
    )
    pack(
        "HieloES", "HIELO",
        gradient_bg((8, 18, 36), (40, 90, 130)),
        (180, 230, 255, 230), (120, 200, 255, 230), w, (160, 190, 210, 220),
        "CPU  FRIO", "GPU  NIEVE",
    )
    pack(
        "AtardecerES", "ATARDECER",
        gradient_bg((48, 12, 40), (255, 110, 40)),
        (255, 140, 70, 230), (220, 80, 160, 230), w, (220, 180, 170, 220),
        "CPU  OCASO", "GPU  CIELO",
    )
    pack(
        "VioletaES", "VIOLETA",
        tint_tex("neon.jpg", 0.40, (1.1, 0.45, 1.35)),
        (190, 90, 255, 230), (255, 90, 200, 230), w, (200, 170, 220, 220),
        "CPU  NEON", "GPU  ROSA",
    )
    pack(
        "MinimalES", "MINIMAL",
        gradient_bg((12, 12, 14), (28, 28, 32)),
        (230, 230, 230, 230), (140, 200, 255, 230), w, (150, 150, 155, 220),
        "CPU", "GPU",
    )
    pack(
        "CircuitoES", "CIRCUITO",
        tint_tex("carbon.jpg", 0.34, (1.2, 0.35, 0.35)),
        (255, 40, 40, 230), (255, 210, 40, 230), w, (210, 180, 140, 220),
        "CPU  MOTOR", "GPU  PISTA",
    )
    pack(
        "BosqueES", "BOSQUE",
        tint_tex("crt.jpg", 0.40, (0.45, 1.15, 0.55)),
        (90, 220, 110, 230), (180, 230, 90, 230), w, (160, 200, 160, 220),
        "CPU  HOJA", "GPU  MUSGO",
    )
    pack(
        "AdminES", "ADMIN TAREAS",
        gradient_bg((16, 22, 32), (24, 40, 64)),
        (80, 170, 255, 230), (40, 220, 160, 230), w, (160, 180, 200, 220),
        "CPU  NUCLEO", "GPU  RENDER",
    )


if __name__ == "__main__":
    horizon()
    noche()
    terminal()
    extra_themes()
    print("ok")

