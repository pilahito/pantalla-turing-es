# -*- coding: utf-8 -*-
"""Ventana principal Centro Turing — gratis, i18n, bandeja."""
from __future__ import annotations
import customtkinter as ctk
from . import theme as T
from . import core
from . import notify
from . import lang as I
from .titlebar import TitleBar
from .panel import PanelView
from .ai_prompt import AiPromptView
from .tray import SystemTray


class CentroProApp(ctk.CTk):
    def __init__(self):
        super().__init__()
        I.init("es")
        ctk.set_appearance_mode(T.APPEARANCE)
        ctk.set_default_color_theme("dark-blue")
        self.title(I.t("app_title"))
        self.geometry("1100x720")
        self.minsize(980, 640)
        self.configure(fg_color=T.BG)
        self.overrideredirect(True)
        self._page = "panel"
        self._tray = SystemTray(self)
        self._nav_meta = (
            ("panel", "nav_panel"),
            ("themes", "nav_themes"),
            ("editor", "nav_editor"),
            ("weather", "nav_weather"),
            ("ai", "nav_ai"),
            ("settings", "nav_settings"),
            ("about", "nav_about"),
        )

        self.titlebar = TitleBar(
            self,
            f"{I.t('app_title')}  v{core.VERSION}",
            on_close=self.hide_to_tray,
            on_minimize=self.hide_to_tray,
            on_lang=self.set_language,
        )
        self.titlebar.pack(fill="x")

        self.shell = ctk.CTkFrame(self, fg_color=T.BG)
        self.shell.pack(fill="both", expand=True)

        self.nav = ctk.CTkFrame(self.shell, width=200, fg_color=T.SURFACE, corner_radius=0)
        self.nav.pack(side="left", fill="y")
        self.nav.pack_propagate(False)

        self.nav_section = ctk.CTkLabel(
            self.nav, text=I.t("nav_section"), text_color=T.MUTED, font=(T.FONT, 10, "bold")
        )
        self.nav_section.pack(anchor="w", padx=18, pady=(18, 8))
        self._nav_btns = {}
        for key, label_key in self._nav_meta:
            b = ctk.CTkButton(
                self.nav,
                text=f"  {I.t(label_key)}",
                anchor="w",
                height=40,
                corner_radius=10,
                fg_color="transparent",
                hover_color=T.SURFACE_2,
                text_color=T.TEXT,
                font=(T.FONT, 13),
                command=lambda k=key: self._nav(k),
            )
            b.pack(fill="x", padx=10, pady=3)
            self._nav_btns[key] = b

        self.foot = ctk.CTkLabel(
            self.nav, text=I.t("free_badge"), text_color=T.MUTED, font=(T.FONT, 10)
        )
        self.foot.pack(side="bottom", pady=16)

        self.main = ctk.CTkFrame(self.shell, fg_color=T.BG, corner_radius=0)
        self.main.pack(side="left", fill="both", expand=True)

        self.toast_lbl = ctk.CTkLabel(
            self, text="", fg_color=T.CARD, corner_radius=12, text_color=T.TEXT, font=(T.FONT, 12)
        )

        self.protocol("WM_DELETE_WINDOW", self.hide_to_tray)
        I.on_change(lambda _lang: self.after(0, self.refresh_language))
        self._tray.start()
        self.show_page("panel")
        self.after(600, lambda: self.toast(I.t("toast_ready"), win=True))

    def set_language(self, code: str):
        I.set_lang(code)

    def refresh_language(self):
        self.title(I.t("app_title"))
        self.titlebar.set_title(f"{I.t('app_title')}  v{core.VERSION}")
        self.nav_section.configure(text=I.t("nav_section"))
        for key, label_key in self._nav_meta:
            self._nav_btns[key].configure(text=f"  {I.t(label_key)}")
        self.foot.configure(text=I.t("free_badge"))
        self.show_page(self._page)

    def hide_to_tray(self):
        self.withdraw()
        notify.toast(I.t("app_title"), I.t("toast_tray"))

    def restore_from_tray(self):
        self.deiconify()
        self.lift()
        self.focus_force()

    def tray_start_monitor(self):
        try:
            core.start_monitor()
            self.toast(I.t("toast_on"), win=True)
            if self._page == "panel":
                self.show_page("panel")
        except Exception as e:
            self.error("Error", str(e))

    def tray_stop_monitor(self):
        core.kill_monitor()
        self.toast(I.t("toast_off"), win=True)
        if self._page == "panel":
            self.show_page("panel")

    def quit_app(self):
        try:
            self._tray.stop()
        except Exception:
            pass
        self.destroy()

    def _nav(self, key: str):
        if key in ("themes", "weather", "settings", "editor"):
            tips = {
                "themes": I.t("coming_themes"),
                "weather": I.t("coming_weather"),
                "settings": I.t("coming_settings"),
                "editor": I.t("coming_editor"),
            }
            self.toast(tips.get(key, "..."), win=False)
            return
        self.show_page(key)

    def show_page(self, name: str):
        self._page = name
        for w in self.main.winfo_children():
            w.destroy()
        for k, b in self._nav_btns.items():
            b.configure(
                fg_color=T.SURFACE_2 if k == name else "transparent",
                text_color=T.PRIMARY if k == name else T.TEXT,
            )
        if name == "panel":
            PanelView(self.main, self).pack(fill="both", expand=True)
        elif name == "ai":
            AiPromptView(self.main, self).pack(fill="both", expand=True)
        elif name == "about":
            self._about_page()
        else:
            ctk.CTkLabel(
                self.main,
                text=I.t("under_construction", name=name),
                text_color=T.MUTED,
                font=(T.FONT, 16),
            ).pack(pady=40)

    def _about_page(self):
        wrap = ctk.CTkFrame(self.main, fg_color="transparent")
        wrap.pack(fill="both", expand=True, padx=28, pady=24)
        ctk.CTkLabel(wrap, text=I.t("about_title"), text_color=T.TEXT, font=(T.FONT, 22, "bold")).pack(anchor="w")
        ctk.CTkLabel(wrap, text=I.t("motto"), text_color=T.PRIMARY, font=(T.FONT, 13)).pack(anchor="w", pady=(8, 16))
        body = I.t("version", v=core.VERSION) + "\n" + I.t("about_body")
        body = I.t("version", v=core.VERSION) + chr(10) + I.t("about_body")
        ctk.CTkLabel(wrap, text=body, text_color=T.MUTED, font=(T.FONT, 12), justify="left").pack(anchor="w")

    def toast(self, msg: str, win: bool = False, title: str | None = None):
        self.toast_lbl.configure(text=f"  {msg}  ")
        self.toast_lbl.place(relx=0.5, rely=0.92, anchor="center")
        self.after(2200, lambda: self.toast_lbl.place_forget())
        if win:
            notify.toast(title or I.t("app_title"), msg)

    def error(self, title: str, detail: str):
        win = ctk.CTkToplevel(self)
        win.title(title)
        win.geometry("420x180")
        win.configure(fg_color=T.SURFACE)
        ctk.CTkLabel(win, text=title, text_color=T.DANGER, font=(T.FONT, 16, "bold")).pack(pady=(20, 8))
        ctk.CTkLabel(win, text=detail[:240], text_color=T.TEXT, wraplength=360).pack(padx=20)
        ctk.CTkButton(win, text="OK", command=win.destroy, fg_color=T.PRIMARY, text_color="#071014").pack(pady=16)


def run():
    app = CentroProApp()
    app.mainloop()
