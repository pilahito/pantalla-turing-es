# -*- coding: utf-8 -*-
"""Dashboard premium — Fase 1."""
from __future__ import annotations
import customtkinter as ctk
from . import theme as T
from . import core
from . import lang as I
from .widgets import StatusPill, ScreenPreview, PrimaryButton


class PanelView(ctk.CTkFrame):
    def __init__(self, master, app, **kwargs):
        super().__init__(master, fg_color="transparent", **kwargs)
        self.app = app
        self._build()

    def _build(self):
        for w in self.winfo_children():
            w.destroy()
        cfg = core.read_cfg()
        on = core.monitor_running()

        head = ctk.CTkFrame(self, fg_color="transparent")
        head.pack(fill="x", padx=20)
        ctk.CTkLabel(head, text=I.t("panel_title"), text_color=T.TEXT, font=(T.FONT, 22, "bold")).pack(side="left")
        ctk.CTkLabel(head, text=I.t("panel_subtitle"), text_color=T.MUTED, font=(T.FONT, 12)).pack(side="left", padx=12)
        ctk.CTkLabel(self, text=I.t("motto"), text_color=T.MUTED, font=(T.FONT, 10)).pack(anchor="w", padx=20, pady=(2, 0))

        body = ctk.CTkFrame(self, fg_color="transparent")
        body.pack(fill="both", expand=True, padx=20, pady=8)

        left = ctk.CTkFrame(body, fg_color="transparent")
        left.pack(side="left", fill="y", padx=(0, 16))
        self.preview = ScreenPreview(left, width=360, height=240)
        self.preview.pack()
        prev = core.theme_preview_path(cfg.get("THEME") or "")
        self.preview.show_image(prev)
        ctk.CTkLabel(
            left,
            text=f"{I.t('theme_label')}: {cfg.get('THEME') or '—'} · {I.t('preview_hint')}",
            text_color=T.MUTED,
            font=(T.FONT, 11),
        ).pack(pady=(6, 0))

        right = ctk.CTkFrame(body, fg_color="transparent")
        right.pack(side="left", fill="both", expand=True)

        pills = ctk.CTkFrame(right, fg_color="transparent")
        pills.pack(fill="x")
        StatusPill(pills, I.t("screen"), I.t("on") if on else I.t("off"), ok=on).pack(
            side="left", padx=(0, 8), fill="x", expand=True
        )
        StatusPill(pills, I.t("com"), cfg.get("COM_PORT") or "AUTO", ok=True).pack(
            side="left", padx=8, fill="x", expand=True
        )
        StatusPill(pills, I.t("brightness"), f"{cfg.get('BRIGHTNESS') or '25'}%", ok=True).pack(
            side="left", padx=(8, 0), fill="x", expand=True
        )

        ctk.CTkLabel(right, text=I.t("brightness"), text_color=T.MUTED, font=(T.FONT, 12)).pack(anchor="w", pady=(18, 4))
        self.bright = ctk.CTkSlider(
            right, from_=1, to=100, number_of_steps=99,
            progress_color=T.PRIMARY, button_color=T.PRIMARY, button_hover_color=T.PRIMARY_DIM,
            command=self._on_bright_slide,
        )
        try:
            self.bright.set(float(cfg.get("BRIGHTNESS") or 25))
        except Exception:
            self.bright.set(25)
        self.bright.pack(fill="x", pady=(0, 4))
        self.bright_lbl = ctk.CTkLabel(right, text=f"{int(self.bright.get())}%", text_color=T.TEXT, font=(T.FONT, 12, "bold"))
        self.bright_lbl.pack(anchor="e")

        themes = [t["id"] for t in core.list_themes()]
        ctk.CTkLabel(right, text=I.t("theme_quick"), text_color=T.MUTED, font=(T.FONT, 12)).pack(anchor="w", pady=(12, 4))
        self.theme_var = ctk.StringVar(value=cfg.get("THEME") or (themes[0] if themes else ""))
        self.theme_menu = ctk.CTkOptionMenu(
            right, values=themes or ["—"], variable=self.theme_var,
            fg_color=T.CARD, button_color=T.SURFACE_2, button_hover_color=T.BORDER,
            dropdown_fg_color=T.SURFACE, command=self._on_theme_pick,
        )
        self.theme_menu.pack(fill="x")

        actions = ctk.CTkFrame(right, fg_color="transparent")
        actions.pack(fill="x", pady=20)
        PrimaryButton(actions, I.t("btn_on"), command=self._on, color=T.SUCCESS).pack(side="left", padx=(0, 8))
        PrimaryButton(actions, I.t("btn_off"), command=self._off, color=T.DANGER).pack(side="left", padx=8)
        PrimaryButton(actions, I.t("btn_restart"), command=self._restart, color=T.AMBER).pack(side="left", padx=8)

        tip = ctk.CTkLabel(
            right, text=I.t("tip_panel"), text_color=T.MUTED, font=(T.FONT, 11), wraplength=420, justify="left",
        )
        tip.pack(anchor="w", pady=(8, 0))

    def _on_bright_slide(self, v):
        self.bright_lbl.configure(text=f"{int(float(v))}%")

    def _on_theme_pick(self, name):
        self.preview.show_image(core.theme_preview_path(name))

    def _apply_bright_theme(self):
        core.write_key("BRIGHTNESS", str(int(self.bright.get())))
        th = self.theme_var.get()
        if th and th != "—":
            core.write_key("THEME", th)

    def _on(self):
        try:
            self._apply_bright_theme()
            core.start_monitor()
            self.app.toast(I.t("toast_on"), win=True)
            self.app.show_page("panel")
        except Exception as e:
            self.app.error("Error", str(e))

    def _off(self):
        core.kill_monitor()
        self.app.toast(I.t("toast_off"), win=True)
        self.app.show_page("panel")

    def _restart(self):
        try:
            self._apply_bright_theme()
            core.reload_screen()
            self.app.toast(I.t("toast_restart"), win=True)
            self.app.show_page("panel")
        except Exception as e:
            self.app.error("Error", str(e))
