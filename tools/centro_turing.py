# -*- coding: utf-8 -*-
import re, subprocess, time, tkinter as tk
from pathlib import Path
from tkinter import messagebox

ROOT = Path(r"E:\turing-smart-screen-python")
PY = ROOT / "venv" / "Scripts" / "python.exe"
CFG = ROOT / "config.yaml"
THEMES = ROOT / "res" / "themes"
LOG = ROOT / "log.log"

BG = "#161b22"
SIDE = "#12161c"
CARD = "#1e2630"
FG = "#e6edf3"
MUTED = "#8b9bab"
CYAN = "#3dccc7"
AMBER = "#e0a050"
LINE = "#2a3542"

def read_cfg():
    text = CFG.read_text(encoding="utf-8", errors="replace") if CFG.exists() else ""
    out = {}
    for key in ("THEME", "COM_PORT", "BRIGHTNESS"):
        m = re.search(r"(?m)^\s*" + key + r":\s*(.+)$", text)
        out[key] = m.group(1).strip().strip('"') if m else ""
    return out

def write_key(key, value):
    text = CFG.read_text(encoding="utf-8", errors="replace")
    pat = r"(?m)^(\s*" + key + r":\s*).+$"
    if re.search(pat, text):
        text = re.sub(pat, r"\g<1>" + str(value), text, count=1)
    else:
        text += "\n" + key + ": " + str(value) + "\n"
    CFG.write_text(text, encoding="utf-8")

def list_themes():
    names = []
    if not THEMES.is_dir():
        return names
    for d in sorted(THEMES.iterdir(), key=lambda p: p.name.lower()):
        y = d / "theme.yaml"
        if not y.is_file():
            continue
        t = y.read_text(encoding="utf-8", errors="replace")
        if "3.5" in t and "landscape" in t.lower():
            names.append(d.name)
    return names

def kill_monitor():
    ps = (
        "Get-CimInstance Win32_Process | Where-Object { "
        "$_.CommandLine -and $_.CommandLine -like '*turing-smart-screen-python*main.py*' "
        "} | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }"
    )
    subprocess.run(["powershell", "-NoProfile", "-Command", ps], capture_output=True)
    time.sleep(1.2)

def start_monitor():
    subprocess.Popen([str(PY), "main.py"], cwd=str(ROOT), creationflags=0x08000000,
                     stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

def apply_theme(name):
    write_key("THEME", name)
    kill_monitor()
    start_monitor()

class App(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Turing Screen Control Center")
        self.geometry("1100x640")
        self.minsize(980, 560)
        self.configure(bg=BG)
        self.nav = []
        top = tk.Frame(self, bg=SIDE, height=42)
        top.pack(fill="x")
        tk.Label(top, text="  Turing Screen Control Center   v2", bg=SIDE, fg=FG,
                 font=("Segoe UI", 11, "bold")).pack(side="left", pady=8)
        tk.Label(top, text="pantalla 3.5   COM3", bg=SIDE, fg=MUTED,
                 font=("Segoe UI", 9)).pack(side="right", padx=16)
        body = tk.Frame(self, bg=BG)
        body.pack(fill="both", expand=True)
        self.side = tk.Frame(body, bg=SIDE, width=210)
        self.side.pack(side="left", fill="y")
        self.side.pack_propagate(False)
        self.main = tk.Frame(body, bg=BG)
        self.main.pack(side="left", fill="both", expand=True)
        self.buttons = {}
        for key, label in (
            ("panel", "Panel"),
            ("temas", "Tema"),
            ("sensores", "Sensores"),
            ("ajustes", "Ajustes"),
        ):
            b = tk.Button(self.side, text="   " + label, command=lambda k=key: self.show(k),
                          bg=SIDE, fg=FG, activebackground="#1c2836", activeforeground=CYAN,
                          relief="flat", anchor="w", font=("Segoe UI", 11), padx=8, pady=10)
            b.pack(fill="x", padx=8, pady=2)
            self.buttons[key] = b
        self.foot = tk.StringVar(value="")
        tk.Label(self, textvariable=self.foot, bg=SIDE, fg=MUTED, anchor="w",
                 font=("Segoe UI", 9)).pack(fill="x")
        self.show("panel")

    def mark(self, key):
        for k, b in self.buttons.items():
            b.configure(bg="#1c3140" if k == key else SIDE, fg=CYAN if k == key else FG)

    def clear(self):
        for w in self.main.winfo_children():
            w.destroy()

    def show(self, mode):
        self.mark(mode)
        self.clear()
        cfg = read_cfg()
        {"panel": self.page_panel, "temas": self.page_temas,
         "sensores": self.page_sensores, "ajustes": self.page_ajustes}[mode](cfg)

    def card(self, parent, title, value, x, y):
        f = tk.Frame(parent, bg=CARD, highlightbackground=LINE, highlightthickness=1)
        f.place(x=x, y=y, width=250, height=110)
        tk.Label(f, text=title, bg=CARD, fg=MUTED, font=("Segoe UI", 9)).pack(anchor="w", padx=12, pady=(10, 0))
        tk.Label(f, text=value, bg=CARD, fg=CYAN, font=("Segoe UI", 16, "bold")).pack(anchor="w", padx=12, pady=(6, 0))
        return f

    def page_panel(self, cfg):
        tk.Label(self.main, text="Panel", bg=BG, fg=FG, font=("Segoe UI", 18, "bold")).place(x=24, y=16)
        self.card(self.main, "TEMA", cfg.get("THEME") or "-", 24, 64)
        self.card(self.main, "PUERTO", cfg.get("COM_PORT") or "COM3", 290, 64)
        self.card(self.main, "BRILLO", (cfg.get("BRIGHTNESS") or "25") + " %", 556, 64)
        box = tk.Text(self.main, bg=CARD, fg=FG, relief="flat", font=("Consolas", 10),
                      highlightbackground=LINE, highlightthickness=1)
        box.place(x=24, y=196, width=820, height=340)
        if LOG.exists():
            box.insert("1.0", "\n".join(LOG.read_text(encoding="utf-8", errors="replace").splitlines()[-16:]))
        box.configure(state="disabled")
        self.foot.set("Modo panel. La pantalla usa main.py, un solo proceso.")

    def page_temas(self, cfg):
        tk.Label(self.main, text="Tema", bg=BG, fg=FG, font=("Segoe UI", 18, "bold")).pack(anchor="w", padx=24, pady=(16, 4))
        tk.Label(self.main, text="Ahora: " + (cfg.get("THEME") or "-"), bg=BG, fg=AMBER).pack(anchor="w", padx=24)
        wrap = tk.Frame(self.main, bg=BG)
        wrap.pack(fill="both", expand=True, padx=24, pady=12)
        lb = tk.Listbox(wrap, bg=CARD, fg=FG, selectbackground=CYAN, selectforeground="#071014",
                        font=("Segoe UI", 12), activestyle="none", relief="flat", highlightthickness=0)
        lb.pack(side="left", fill="both", expand=True)
        names = list_themes()
        cur = cfg.get("THEME")
        for n in names:
            lb.insert("end", n)
            if n == cur:
                lb.selection_set(names.index(n))
        def go():
            sel = lb.curselection()
            if not sel:
                return
            name = names[sel[0]]
            try:
                apply_theme(name)
                self.foot.set("Tema aplicado: " + name)
                messagebox.showinfo("Centro Turing", "Tema puesto en la pantalla:\n" + name)
                self.show("temas")
            except Exception as e:
                messagebox.showerror("Centro Turing", str(e))
        tk.Button(wrap, text="Poner en la pantalla", command=go, bg=CYAN, fg="#071014",
                  relief="flat", font=("Segoe UI", 11, "bold"), padx=14, pady=10).pack(side="left", padx=14, anchor="n")
        self.foot.set("Elegir tema reinicia el monitor una vez.")

    def page_sensores(self, cfg):
        tk.Label(self.main, text="Sensores", bg=BG, fg=FG, font=("Segoe UI", 18, "bold")).pack(anchor="w", padx=24, pady=(16, 4))
        tk.Label(self.main, text="Ultimas lineas del monitor. CPU temp suele faltar sin admin.", bg=BG, fg=MUTED).pack(anchor="w", padx=24)
        box = tk.Text(self.main, bg=CARD, fg=FG, relief="flat", font=("Consolas", 10))
        box.pack(fill="both", expand=True, padx=24, pady=12)
        if LOG.exists():
            lines = [ln for ln in LOG.read_text(encoding="utf-8", errors="replace").splitlines()
                     if any(k in ln for k in ("CPU", "GPU", "WARNING", "theme", "COM"))]
            box.insert("1.0", "\n".join(lines[-22:]))
        box.configure(state="disabled")
        self.foot.set("Modo sensores.")

    def page_ajustes(self, cfg):
        tk.Label(self.main, text="Ajustes", bg=BG, fg=FG, font=("Segoe UI", 18, "bold")).pack(anchor="w", padx=24, pady=(16, 10))
        form = tk.Frame(self.main, bg=BG)
        form.pack(anchor="w", padx=24)
        tk.Label(form, text="Puerto COM", bg=BG, fg=MUTED, font=("Segoe UI", 10)).grid(row=0, column=0, sticky="w", pady=8)
        com = tk.Entry(form, bg=CARD, fg=FG, insertbackground=FG, relief="flat", font=("Segoe UI", 12), width=16)
        com.insert(0, cfg.get("COM_PORT") or "COM3")
        com.grid(row=0, column=1, padx=12, ipady=6)
        tk.Label(form, text="Brillo", bg=BG, fg=MUTED, font=("Segoe UI", 10)).grid(row=1, column=0, sticky="w", pady=8)
        br = tk.Entry(form, bg=CARD, fg=FG, insertbackground=FG, relief="flat", font=("Segoe UI", 12), width=8)
        br.insert(0, cfg.get("BRIGHTNESS") or "25")
        br.grid(row=1, column=1, padx=12, sticky="w", ipady=6)
        def save():
            write_key("COM_PORT", com.get().strip() or "COM3")
            write_key("BRIGHTNESS", br.get().strip() or "25")
            self.foot.set("Guardado. Aplica un tema para recargar la pantalla.")
            messagebox.showinfo("Centro Turing", "Guardado.")
        tk.Button(self.main, text="Guardar", command=save, bg=AMBER, fg="#1a1208",
                  relief="flat", font=("Segoe UI", 11, "bold"), padx=16, pady=8).pack(anchor="w", padx=24, pady=18)
        self.foot.set("COM y brillo. No abre un segundo monitor hasta que apliques un tema.")

if __name__ == "__main__":
    App().mainloop()
