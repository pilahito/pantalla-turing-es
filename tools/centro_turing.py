# -*- coding: utf-8 -*-
import json, re, subprocess, time, tkinter as tk
from pathlib import Path
from tkinter import messagebox

ROOT = Path(__file__).resolve().parents[1]
PY = ROOT / "venv" / "Scripts" / "python.exe"
CFG = ROOT / "config.yaml"
THEMES = ROOT / "res" / "themes"
LOG = ROOT / "log.log"
UI = ROOT / "tools" / "centro_ui.json"
INICIAR = ROOT / "Iniciar.ps1"
STARTUP = Path.home() / "AppData/Roaming/Microsoft/Windows/Start Menu/Programs/Startup/Centro Turing.lnk"

VERSION = "2.2"
BG, SIDE, CARD, FG, MUTED = "#161b22", "#12161c", "#1e2630", "#e6edf3", "#8b9bab"
CYAN, AMBER, LINE, GREEN, RED = "#3dccc7", "#e0a050", "#2a3542", "#3dd68c", "#f07178"
SKINS = {
    "dark": ("#161b22", "#12161c", "#1e2630", "#e6edf3", "#8b9bab"),
    "light": ("#f4f6f8", "#e8edf2", "#ffffff", "#1b2430", "#5c6b7a"),
    "black": ("#0c0e12", "#090b0e", "#161a20", "#f2f4f8", "#8b93a0"),
}
MODELS = (
    ("3.5 landscape", "3.5", "landscape"),
    ("3.5 portrait", "3.5", "portrait"),
    ("5 landscape", "5", "landscape"),
    ("8.8 landscape", "8.8", "landscape"),
)

def load_ui():
    try:
        d = json.loads(UI.read_text(encoding="utf-8"))
    except Exception:
        d = {}
    d.setdefault("lang", "es")
    d.setdefault("skin", "dark")
    d.setdefault("model", "3.5 landscape")
    return d

def save_ui(d):
    UI.write_text(json.dumps(d, ensure_ascii=False, indent=2), encoding="utf-8")

def read_cfg():
    text = CFG.read_text(encoding="utf-8", errors="replace") if CFG.exists() else ""
    out = {}
    for key in ("THEME", "COM_PORT", "BRIGHTNESS", "WEATHER_LATITUDE", "WEATHER_LONGITUDE",
                "WEATHER_LANGUAGE", "CLOCK_FORMAT"):
        m = re.search(r"(?m)^\s*" + key + r":\s*(.*)$", text)
        out[key] = m.group(1).strip().strip("'\"") if m else ""
    rev = re.search(r"(?m)^\s*DISPLAY_REVERSE:\s*(.+)$", text)
    out["DISPLAY_REVERSE"] = rev.group(1).strip().lower() if rev else "false"
    return out

def write_key(key, value):
    text = CFG.read_text(encoding="utf-8", errors="replace")
    pat = r"(?m)^(\s*" + key + r":\s*).+$"
    if re.search(pat, text):
        text = re.sub(pat, r"\g<1>" + str(value), text, count=1)
    elif "\ndisplay:" in text:
        text = text.replace("\ndisplay:", "\n  " + key + ": " + str(value) + "\ndisplay:", 1)
    else:
        text += "\n  " + key + ": " + str(value) + "\n"
    CFG.write_text(text, encoding="utf-8")

def write_reverse(val):
    text = CFG.read_text(encoding="utf-8", errors="replace")
    if re.search(r"(?m)^\s*DISPLAY_REVERSE:", text):
        text = re.sub(r"(?m)^(\s*DISPLAY_REVERSE:\s*).+$", r"\g<1>" + val, text, count=1)
    else:
        text = text.replace("display:\n", "display:\n  DISPLAY_REVERSE: " + val + "\n", 1)
    CFG.write_text(text, encoding="utf-8")

def patch_clock(theme, fmt):
    y = THEMES / theme / "theme.yaml"
    if not y.is_file():
        return
    text = y.read_text(encoding="utf-8", errors="replace")
    form = '"hh:mm"' if str(fmt) == "12" else '"HH:mm"'
    if "HOUR:" in text and "FORMAT:" in text:
        text = re.sub(r'(HOUR:[\s\S]*?FORMAT:\s*)"[^"]+"', r"\1" + form, text, count=1)
        y.write_text(text, encoding="utf-8")

def kill_monitor():
    ps = (
        "Get-CimInstance Win32_Process | Where-Object { "
        "$_.CommandLine -and $_.CommandLine -like '*turing-smart-screen-python*main.py*' "
        "} | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }"
    )
    subprocess.run(["powershell", "-NoProfile", "-Command", ps], capture_output=True)
    time.sleep(1.0)

def start_monitor():
    ps = (
        "Get-CimInstance Win32_Process | Where-Object { "
        "$_.CommandLine -and $_.CommandLine -like '*main.py*' -and "
        "$_.CommandLine -like '*turing-smart-screen-python*' "
        "} | Select-Object -First 1"
    )
    r = subprocess.run(["powershell", "-NoProfile", "-Command", ps], capture_output=True, text=True)
    if (r.stdout or "").strip():
        return
    subprocess.Popen([str(PY), "main.py"], cwd=str(ROOT), creationflags=0x08000000,
                     stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)


def reload_screen():
    kill_monitor()
    start_monitor()

def com_ports():
    try:
        out = subprocess.check_output([str(PY), "-c",
            "from serial.tools.list_ports import comports\nprint('\\n'.join(p.device for p in comports()))"],
            cwd=str(ROOT), text=True, timeout=12)
        return [x for x in out.splitlines() if x.strip()]
    except Exception:
        return []

def set_autostart(on):
    # Arranque diario: Iniciar.ps1 silencioso (sin abrir el centro). ROOT portatil.
    if on:
        iniciar = str(INICIAR)
        root = str(ROOT)
        shortcut = str(STARTUP)
        ps2 = (
            "$lnk = '" + shortcut.replace("'", "''") + "';"
            "$ini = '" + iniciar.replace("'", "''") + "';"
            "$wd = '" + root.replace("'", "''") + "';"
            "$s = (New-Object -ComObject WScript.Shell).CreateShortcut($lnk);"
            "$s.TargetPath = 'powershell.exe';"
            "$s.Arguments = '-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"' + $ini + '\"';"
            "$s.WorkingDirectory = $wd;"
            "$s.WindowStyle = 7;"
            "$s.Description = 'Pantalla Turing (arranque silencioso)';"
            "$s.Save()"
        )
        subprocess.run(["powershell", "-NoProfile", "-Command", ps2], capture_output=True)
    elif STARTUP.exists():
        STARTUP.unlink()


def autostart_enabled():
    return STARTUP.exists()


def monitor_running():
    ps = (
        "Get-CimInstance Win32_Process | Where-Object { "
        "$_.CommandLine -and $_.CommandLine -like '*main.py*' -and "
        "$_.CommandLine -like '*turing-smart-screen-python*' "
        "} | Select-Object -ExpandProperty ProcessId"
    )
    try:
        r = subprocess.run(["powershell", "-NoProfile", "-Command", ps],
                           capture_output=True, text=True, timeout=8)
        ids = [x.strip() for x in (r.stdout or "").splitlines() if x.strip().isdigit()]
        return len(ids) > 0, ids
    except Exception:
        return False, []



class App(tk.Tk):
    def __init__(self):
        super().__init__()
        self.ui = load_ui()
        self.lang = self.ui["lang"] if self.ui["lang"] in ("es", "en") else "es"
        self.mode = "panel"
        self.apply_skin(self.ui.get("skin", "dark"))
        self.title("Centro Turing")
        self.geometry("1100x700")
        self.minsize(980, 600)
        self.configure(bg=self.bg)
        self.top = tk.Frame(self, bg=self.side, height=42)
        self.top.pack(fill="x")
        self.title_lbl = tk.Label(self.top, text="  Centro Turing   v" + VERSION,
                                  bg=self.side, fg=self.fg, font=("Segoe UI", 11, "bold"))
        self.title_lbl.pack(side="left", pady=8)
        langs = tk.Frame(self.top, bg=self.side)
        langs.pack(side="right", padx=10)
        self.btn_es = tk.Button(langs, text="ES", command=lambda: self.set_lang("es"), relief="flat", padx=8)
        self.btn_en = tk.Button(langs, text="EN", command=lambda: self.set_lang("en"), relief="flat", padx=8)
        self.btn_es.pack(side="left", padx=2)
        self.btn_en.pack(side="left", padx=2)
        body = tk.Frame(self, bg=self.bg)
        body.pack(fill="both", expand=True)
        self.nav = tk.Frame(body, bg=self.side, width=210)
        self.nav.pack(side="left", fill="y")
        self.nav.pack_propagate(False)
        self.main = tk.Frame(body, bg=self.bg)
        self.main.pack(side="left", fill="both", expand=True)
        self.buttons = {}
        items = (("panel","Panel"),("tema","Tema"),("estudio","Crear tema"),
                 ("tiempo","Tiempo"),("personal","Personalizar"),("ajustes","Ajustes"))
        if self.lang == "en":
            items = (("panel","Dashboard"),("tema","Theme"),("estudio","Make theme"),
                     ("tiempo","Weather"),("personal","Customize"),("ajustes","Settings"))
        for key, label in items:
            b = tk.Button(self.nav, text="   " + label, command=lambda k=key: self.show(k),
                          bg=self.side, fg=self.fg, relief="flat", anchor="w",
                          font=("Segoe UI", 11), padx=8, pady=8)
            b.pack(fill="x", padx=8, pady=2)
            self.buttons[key] = b
        self.foot = tk.StringVar(value="")
        tk.Label(self, textvariable=self.foot, bg=self.side, fg=self.muted, anchor="w",
                 font=("Segoe UI", 9)).pack(fill="x")
        self.paint_lang()
        self.show("panel")

    def apply_skin(self, name):
        c = SKINS.get(name, SKINS["dark"])
        self.bg, self.side, self.card, self.fg, self.muted = c
        self.ui["skin"] = name
        save_ui(self.ui)

    def set_lang(self, code):
        self.lang = code
        self.ui["lang"] = code
        save_ui(self.ui)
        write_key("WEATHER_LANGUAGE", code)
        self.paint_lang()
        self.show(self.mode)

    def paint_lang(self):
        es = self.lang == "es"
        labels = {
            "panel": "Panel", "tema": "Tema" if es else "Theme",
            "estudio": "Crear tema" if es else "Make theme",
            "tiempo": "Tiempo" if es else "Weather",
            "personal": "Personalizar" if es else "Customize",
            "ajustes": "Ajustes" if es else "Settings",
        }
        for k, b in self.buttons.items():
            b.configure(text="   " + labels[k], bg="#1c3140" if k == self.mode else self.side,
                        fg=CYAN if k == self.mode else self.fg)
        self.btn_es.configure(bg=CYAN if es else self.card, fg="#071014" if es else self.fg)
        self.btn_en.configure(bg=CYAN if not es else self.card, fg="#071014" if not es else self.fg)

    def tr(self, a, b):
        return a if self.lang == "es" else b

    def show(self, mode):
        self.mode = mode
        self.paint_lang()
        for w in self.main.winfo_children():
            w.destroy()
        cfg = read_cfg()
        {"panel": self.page_panel, "tema": self.page_tema, "estudio": self.page_estudio,
         "tiempo": self.page_tiempo, "personal": self.page_personal, "ajustes": self.page_ajustes}[mode](cfg)

    def h1(self, text):
        tk.Label(self.main, text=text, bg=self.bg, fg=self.fg, font=("Segoe UI", 18, "bold")).pack(anchor="w", padx=24, pady=(16, 4))

    def hint(self, text):
        tk.Label(self.main, text=text, bg=self.bg, fg=self.muted).pack(anchor="w", padx=24)

    def field(self, parent, row, label, value, width=16):
        tk.Label(parent, text=label, bg=self.bg, fg=self.muted, font=("Segoe UI", 10)).grid(row=row, column=0, sticky="w", pady=8)
        e = tk.Entry(parent, bg=self.card, fg=self.fg, insertbackground=self.fg, relief="flat", font=("Segoe UI", 12), width=width)
        e.insert(0, value)
        e.grid(row=row, column=1, padx=12, sticky="w", ipady=6)
        return e

    def gold(self, text, command):
        tk.Button(self.main, text=text, command=command, bg=AMBER, fg="#1a1208",
                  relief="flat", font=("Segoe UI", 11, "bold"), padx=16, pady=8).pack(anchor="w", padx=24, pady=12)


    def page_panel(self, cfg):
        self.h1(self.tr("Panel", "Dashboard"))
        self.hint(self.tr("Estado de la minipantalla y acciones principales.",
                          "Screen status and primary actions."))
        on, _ = monitor_running()
        status_txt = self.tr("ENCENDIDA", "ON") if on else self.tr("APAGADA", "OFF")
        status_color = GREEN if on else RED
        row = tk.Frame(self.main, bg=self.bg)
        row.pack(anchor="w", padx=24, pady=12)
        bits = (
            (self.tr("Pantalla", "Screen"), status_txt, status_color),
            ("COM", cfg.get("COM_PORT") or "AUTO", CYAN),
            (self.tr("Tema", "Theme"), (cfg.get("THEME") or "-")[:16], CYAN),
            (self.tr("Brillo", "Brightness"), (cfg.get("BRIGHTNESS") or "25") + " %", CYAN),
        )
        for title, value, accent in bits:
            f = tk.Frame(row, bg=self.card, highlightbackground=LINE, highlightthickness=1)
            f.pack(side="left", padx=6)
            tk.Label(f, text=title, bg=self.card, fg=self.muted, font=("Segoe UI", 9)).pack(anchor="w", padx=12, pady=(10, 0))
            tk.Label(f, text=value, bg=self.card, fg=accent, font=("Segoe UI", 15, "bold"), width=12).pack(anchor="w", padx=12, pady=(4, 12))
        tk.Label(self.main, text=self.tr("Acciones", "Actions"), bg=self.bg, fg=CYAN,
                 font=("Segoe UI", 10, "bold")).pack(anchor="w", padx=24, pady=(8, 4))
        actions = tk.Frame(self.main, bg=self.bg)
        actions.pack(anchor="w", padx=24, pady=6)
        def do_on():
            try:
                start_monitor(); time.sleep(0.5); self.show("panel")
                self.foot.set(self.tr("Pantalla encendida.", "Screen started."))
            except Exception as e:
                messagebox.showerror("Centro Turing", str(e))
        def do_off():
            kill_monitor(); self.show("panel")
            self.foot.set(self.tr("Pantalla apagada.", "Screen stopped."))
        def do_restart():
            reload_screen(); self.show("panel")
            self.foot.set(self.tr("Pantalla reiniciada.", "Screen restarted."))
        tk.Button(actions, text=self.tr("Encender", "Start"), command=do_on, bg=GREEN, fg="#071014",
                  relief="flat", font=("Segoe UI", 11, "bold"), padx=16, pady=10).pack(side="left", padx=4)
        tk.Button(actions, text=self.tr("Apagar", "Stop"), command=do_off, bg=RED, fg="#071014",
                  relief="flat", font=("Segoe UI", 11, "bold"), padx=16, pady=10).pack(side="left", padx=4)
        tk.Button(actions, text=self.tr("Reiniciar pantalla", "Restart screen"), command=do_restart,
                  bg=AMBER, fg="#1a1208", relief="flat", font=("Segoe UI", 11, "bold"), padx=16, pady=10).pack(side="left", padx=4)
        tk.Label(self.main, text=self.tr("Arranque con Windows", "Start with Windows"), bg=self.bg, fg=CYAN,
                 font=("Segoe UI", 10, "bold")).pack(anchor="w", padx=24, pady=(14, 4))
        boot_row = tk.Frame(self.main, bg=self.bg)
        boot_row.pack(anchor="w", padx=24, pady=4)
        enabled = autostart_enabled()
        tk.Label(boot_row,
                 text=(self.tr("Activo (Iniciar.ps1 silencioso)", "On (silent Iniciar.ps1)") if enabled
                       else self.tr("Desactivado", "Off")),
                 bg=self.bg, fg=GREEN if enabled else self.muted, font=("Segoe UI", 10, "bold")).pack(side="left", padx=(0, 12))
        def boot_on():
            set_autostart(True); self.show("panel")
            self.foot.set(self.tr("Autostart ON -> Iniciar.ps1 (sin centro).", "Autostart ON -> Iniciar.ps1 (no center)."))
        def boot_off():
            set_autostart(False); self.show("panel")
            self.foot.set(self.tr("Autostart OFF.", "Autostart OFF."))
        tk.Button(boot_row, text="ON", command=boot_on, bg=self.card, fg=self.fg, relief="flat", padx=12, pady=6).pack(side="left", padx=3)
        tk.Button(boot_row, text="OFF", command=boot_off, bg=self.card, fg=self.fg, relief="flat", padx=12, pady=6).pack(side="left", padx=3)
        tk.Label(self.main, text=self.tr("Atajo: powershell -File Iniciar.ps1 (ventana oculta).",
                                         "Shortcut: powershell -File Iniciar.ps1 (hidden)."),
                 bg=self.bg, fg=self.muted, font=("Segoe UI", 9)).pack(anchor="w", padx=24, pady=(4, 0))
        self.foot.set(self.tr("Modelo: ", "Model: ") + self.ui.get("model", "3.5 landscape")
                      + "  |  " + (self.tr("Pantalla ON", "Screen ON") if on else self.tr("Pantalla OFF", "Screen OFF")))

    def page_tema(self, cfg):
        self.h1(self.tr("Tema", "Theme"))
        size, ori = "3.5", "landscape"
        for name, s, o in MODELS:
            if name == self.ui.get("model"):
                size, ori = s, o
        names = []
        for d in sorted(THEMES.iterdir(), key=lambda p: p.name.lower()) if THEMES.is_dir() else []:
            y = d / "theme.yaml"
            if not y.is_file():
                continue
            t = y.read_text(encoding="utf-8", errors="replace")
            if d.name == "bash-dark-green-gpu_H":
                continue
            low = t.lower()
            if ('display_size: ' + size) not in low:
                continue
            portrait = "portrait" in low
            landscape = "landscape" in low
            if ori == "landscape" and not landscape:
                continue
            if ori == "portrait" and not portrait:
                continue
            if ori == "landscape" and portrait and not landscape:
                continue
            names.append(d.name)
        lb = tk.Listbox(self.main, bg=self.card, fg=self.fg, selectbackground=CYAN, selectforeground="#071014",
                        font=("Segoe UI", 12), relief="flat")
        lb.pack(fill="both", expand=True, padx=24, pady=8)
        for n in names:
            lb.insert("end", n)
            if n == cfg.get("THEME"):
                lb.selection_set(names.index(n))
        def go():
            if not lb.curselection():
                return
            apply = names[lb.curselection()[0]]
            write_key("THEME", apply)
            reload_screen()
            messagebox.showinfo("Centro Turing", apply)
        tk.Button(self.main, text=self.tr("Poner en la pantalla", "Apply"), command=go,
                  bg=CYAN, fg="#071014", relief="flat", font=("Segoe UI", 11, "bold"), padx=14, pady=8).pack(anchor="w", padx=24)
        self.foot.set(self.ui.get("model", "3.5 landscape"))

    def page_estudio(self, cfg):
        self.h1(self.tr("Crear tema", "Make theme"))
        self.hint(self.tr("Colores y widgets. Avanzado: YAML del tema, no codigo del sistema.",
                          "Colors and widgets. Advanced: theme YAML, not system code."))
        form = tk.Frame(self.main, bg=self.bg)
        form.pack(anchor="w", padx=24)
        name = self.field(form, 0, self.tr("Nombre", "Name"), "MiTema")
        color = self.field(form, 1, "R,G,B", "80, 200, 190")
        row = tk.Frame(self.main, bg=self.bg)
        row.pack(anchor="w", padx=24, pady=6)
        cpu = tk.IntVar(value=1); gpu = tk.IntVar(value=1); clk = tk.IntVar(value=1); wet = tk.IntVar(value=0)
        for text, var in (("CPU", cpu), ("GPU", gpu), (self.tr("Reloj", "Clock"), clk), (self.tr("Clima", "Weather"), wet)):
            tk.Checkbutton(row, text=text, variable=var, bg=self.bg, fg=self.fg, selectcolor=self.card,
                           activebackground=self.bg, activeforeground=self.fg).pack(side="left", padx=6)
        box = tk.Text(self.main, bg=self.card, fg=self.fg, height=8, relief="flat", font=("Consolas", 9))
        box.pack(fill="x", padx=24, pady=6)
        def make(advanced=False):
            n = re.sub(r"[^A-Za-z0-9_]", "", name.get().strip()) or "MiTema"
            rgb = color.get().strip() or "80, 200, 190"
            folder = THEMES / n
            folder.mkdir(exist_ok=True)
            parts = ["author: CentroTuring", "display:", "  DISPLAY_SIZE: 3.5\"",
                     "  DISPLAY_ORIENTATION: landscape", "  DISPLAY_RGB_LED: " + rgb, "STATS:"]
            if cpu.get():
                parts += ["  CPU:", "    PERCENTAGE:", "      INTERVAL: 1", "      TEXT:",
                          "        SHOW: True", "        X: 20", "        Y: 40", "        FONT: digital/DIGITAL-7.TTF",
                          "        FONT_SIZE: 42", "        FONT_COLOR: " + rgb, "        SHOW_UNIT: True"]
            if gpu.get():
                parts += ["  GPU:", "    INTERVAL: 1", "    PERCENTAGE:", "      TEXT:",
                          "        SHOW: True", "        X: 20", "        Y: 140", "        FONT: digital/DIGITAL-7.TTF",
                          "        FONT_SIZE: 42", "        FONT_COLOR: " + rgb, "        SHOW_UNIT: True"]
            if clk.get():
                parts += ["  DATE:", "    INTERVAL: 1", "    HOUR:", "      TEXT:",
                          "        SHOW: True", "        FORMAT: \"HH:mm\"", "        X: 360", "        Y: 16",
                          "        FONT: roboto-mono/RobotoMono-Bold.ttf", "        FONT_SIZE: 22", "        FONT_COLOR: " + rgb]
            if wet.get():
                parts += ["  WEATHER:", "    INTERVAL: 300", "    TEMPERATURE:", "      TEXT:",
                          "        SHOW: True", "        X: 20", "        Y: 240", "        FONT: roboto-mono/RobotoMono-Bold.ttf",
                          "        FONT_SIZE: 18", "        FONT_COLOR: " + rgb]
            text = "\n".join(parts) + "\n"
            if advanced and box.get("1.0", "end").strip():
                text = box.get("1.0", "end")
            (folder / "theme.yaml").write_text(text, encoding="utf-8")
            try:
                from PIL import Image, ImageDraw
                im = Image.new("RGB", (480, 320), (12, 16, 22))
                dr = ImageDraw.Draw(im)
                r, g, b = [int(x.strip()) for x in rgb.split(",")]
                dr.rectangle((0, 0, 479, 8), fill=(r, g, b))
                im.save(folder / "background.png")
            except Exception:
                pass
            box.delete("1.0", "end")
            box.insert("1.0", text)
            self.foot.set(n)
        self.gold(self.tr("Crear con widgets", "Create with widgets"), lambda: make(False))
        tk.Button(self.main, text=self.tr("Avanzado: guardar YAML", "Advanced: save YAML"),
                  command=lambda: make(True), bg=self.card, fg=self.fg, relief="flat",
                  font=("Segoe UI", 10), padx=12, pady=6).pack(anchor="w", padx=24)

    def page_tiempo(self, cfg):
        self.h1(self.tr("Tiempo", "Weather"))
        form = tk.Frame(self.main, bg=self.bg)
        form.pack(anchor="w", padx=24)
        lat = self.field(form, 0, "Lat", cfg.get("WEATHER_LATITUDE") or "36.277")
        lon = self.field(form, 1, "Lon", cfg.get("WEATHER_LONGITUDE") or "-6.088")
        def save():
            write_key("WEATHER_LATITUDE", lat.get().strip() or "0.0")
            write_key("WEATHER_LONGITUDE", lon.get().strip() or "0.0")
            write_key("WEATHER_LANGUAGE", self.lang)
            reload_screen()
            messagebox.showinfo("Centro Turing", self.tr("Guardado y recargado.", "Saved and reloaded."))
        self.gold(self.tr("Guardar y aplicar", "Save and apply"), save)

    def page_personal(self, cfg):
        self.h1(self.tr("Personalizar", "Customize"))
        self.hint(self.tr("Hora y giro se aplican al guardar. Colores de esta ventana abajo.",
                          "Clock and flip apply on save. Window colors below."))
        form = tk.Frame(self.main, bg=self.bg)
        form.pack(anchor="w", padx=24)
        clock = self.field(form, 0, self.tr("Reloj 12 o 24", "Clock 12 or 24"), cfg.get("CLOCK_FORMAT") or "24", 8)
        flip = self.field(form, 1, self.tr("Girar true/false", "Flip true/false"), cfg.get("DISPLAY_REVERSE") or "false", 10)
        row = tk.Frame(self.main, bg=self.bg)
        row.pack(anchor="w", padx=24, pady=8)
        def skin(name):
            self.apply_skin(name)
            messagebox.showinfo("Centro Turing", self.tr("Cierra y abre el centro para el color de ventana.",
                                                         "Close and reopen the center for the window color."))
        for name, label in (("dark", self.tr("Oscuro", "Dark")), ("light", self.tr("Claro", "Light")), ("black", self.tr("Negro", "Black"))):
            tk.Button(row, text=label, command=lambda n=name: skin(n), bg=self.card, fg=self.fg,
                      relief="flat", padx=10, pady=6).pack(side="left", padx=4)
        def save():
            fmt = clock.get().strip()
            if fmt not in ("12", "24"):
                fmt = "24"
            val = flip.get().strip().lower()
            if val not in ("true", "false"):
                val = "false"
            write_key("CLOCK_FORMAT", fmt)
            write_reverse(val)
            if cfg.get("THEME"):
                patch_clock(cfg["THEME"], fmt)
            reload_screen()
            messagebox.showinfo("Centro Turing", self.tr("Hora y giro aplicados.", "Clock and flip applied."))
        self.gold(self.tr("Guardar y aplicar", "Save and apply"), save)

    def page_ajustes(self, cfg):
        self.h1(self.tr("Ajustes", "Settings"))
        ports = ", ".join(com_ports()) or "COM3"
        self.hint(self.tr("Analisis: pantalla 3.5 landscape, puerto " + (cfg.get("COM_PORT") or "COM3") + ". Vistos: " + ports,
                          "Scan: 3.5 landscape screen, port " + (cfg.get("COM_PORT") or "COM3") + ". Seen: " + ports))
        form = tk.Frame(self.main, bg=self.bg)
        form.pack(anchor="w", padx=24)
        com = self.field(form, 0, "COM", cfg.get("COM_PORT") or "COM3")
        br = self.field(form, 1, self.tr("Brillo", "Brightness"), cfg.get("BRIGHTNESS") or "25", 8)
        row = tk.Frame(self.main, bg=self.bg)
        row.pack(anchor="w", padx=24, pady=8)
        def pick(model):
            self.ui["model"] = model
            save_ui(self.ui)
            self.foot.set(model)
        for name, _, _ in MODELS:
            tk.Button(row, text=name, command=lambda n=name: pick(n), bg=self.card, fg=self.fg,
                      relief="flat", padx=8, pady=6).pack(side="left", padx=3)
        def save():
            write_key("COM_PORT", com.get().strip() or "COM3")
            write_key("BRIGHTNESS", br.get().strip() or "25")
            reload_screen()
            messagebox.showinfo("Centro Turing", self.tr("Guardado.", "Saved."))
        self.gold(self.tr("Guardar", "Save"), save)
        tk.Label(self.main, text=self.tr(
            "Nota: sensores LHM (temps reales) necesitan admin via Iniciar-Admin.ps1 (avanzado).",
            "Note: LHM sensors (real temps) need admin via Iniciar-Admin.ps1 (advanced).",
        ), bg=self.bg, fg=self.muted, font=("Segoe UI", 9), wraplength=720, justify="left").pack(anchor="w", padx=24, pady=(4, 8))
        # Arranque ON/OFF esta en Panel
        def _boot_legacy():
            set_autostart(True)
            self.foot.set(self.tr("Usa Panel para ON/OFF de autostart.", "Use Panel for autostart ON/OFF."))
        extra = tk.Frame(self.main, bg=self.bg)
        extra.pack(anchor="w", padx=24)
        tk.Button(extra, text=self.tr("Autostart (ver Panel)", "Autostart (see Panel)"), command=_boot_legacy,
                  bg=self.card, fg=self.fg, relief="flat", padx=10, pady=6).pack(side="left", padx=4)
        tk.Label(self.main, text=self.tr("Avanzado. YAML del tema actual. No toca el codigo del programa.",
                                         "Advanced. YAML of the current theme. Does not touch program code."),
                 bg=self.bg, fg=self.muted).pack(anchor="w", padx=24, pady=(10, 0))
        box = tk.Text(self.main, bg=self.card, fg=self.fg, height=10, relief="flat", font=("Consolas", 9))
        box.pack(fill="both", expand=True, padx=24, pady=6)
        theme = cfg.get("THEME") or ""
        ypath = THEMES / theme / "theme.yaml" if theme else None
        if ypath and ypath.is_file():
            box.insert("1.0", ypath.read_text(encoding="utf-8", errors="replace")[:4000])
        def save_yaml():
            if not ypath or not ypath.is_file():
                messagebox.showerror("Centro Turing", self.tr("No hay tema abierto.", "No theme open."))
                return
            ypath.write_text(box.get("1.0", "end"), encoding="utf-8")
            reload_screen()
            messagebox.showinfo("Centro Turing", self.tr("YAML guardado.", "YAML saved."))
        tk.Button(self.main, text=self.tr("Guardar YAML", "Save YAML"), command=save_yaml,
                  bg=self.card, fg=self.fg, relief="flat", padx=12, pady=6).pack(anchor="w", padx=24, pady=(0, 8))
        self.foot.set(self.tr("Ruta: ", "Path: ") + str(ROOT))

if __name__ == "__main__":
    App().mainloop()