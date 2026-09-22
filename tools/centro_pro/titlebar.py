# -*- coding: utf-8 -*-
"""Barra de título personalizada (sin chrome nativo de Windows)."""
from __future__ import annotations
import customtkinter as ctk
from . import theme as T
from . import lang as I


class TitleBar(ctk.CTkFrame):
    def __init__(self, master, title: str, on_close, on_minimize=None, on_lang=None, **kwargs):
        super().__init__(master, height=52, fg_color=T.SURFACE, corner_radius=0, **kwargs)
        self._on_close = on_close
        self._on_minimize = on_minimize
        self._on_lang = on_lang
        self._drag = {"x": 0, "y": 0}
        self.pack_propagate(False)

        left = ctk.CTkFrame(self, fg_color="transparent")
        left.pack(side="left", fill="y", padx=12)
        ctk.CTkLabel(left, text="◆", text_color=T.PRIMARY, font=(T.FONT, 16, "bold")).pack(side="left", padx=(0, 8))
        titles = ctk.CTkFrame(left, fg_color="transparent")
        titles.pack(side="left")
        self.title_lbl = ctk.CTkLabel(titles, text=title, text_color=T.TEXT, font=(T.FONT, 13, "bold"))
        self.title_lbl.pack(anchor="w")
        self.motto_lbl = ctk.CTkLabel(titles, text=I.t("motto"), text_color=T.MUTED, font=(T.FONT, 10))
        self.motto_lbl.pack(anchor="w")

        right = ctk.CTkFrame(self, fg_color="transparent")
        right.pack(side="right", padx=6)

        lang_box = ctk.CTkFrame(right, fg_color=T.SURFACE_2, corner_radius=8)
        lang_box.pack(side="left", padx=(0, 8))
        self.btn_es = ctk.CTkButton(
            lang_box, text="ES", width=34, height=26, corner_radius=6,
            fg_color=T.PRIMARY if I.lang() == "es" else "transparent",
            text_color="#071014" if I.lang() == "es" else T.TEXT,
            hover_color=T.BORDER, command=lambda: self._lang("es"),
        )
        self.btn_es.pack(side="left", padx=2, pady=2)
        self.btn_en = ctk.CTkButton(
            lang_box, text="EN", width=34, height=26, corner_radius=6,
            fg_color=T.PRIMARY if I.lang() == "en" else "transparent",
            text_color="#071014" if I.lang() == "en" else T.TEXT,
            hover_color=T.BORDER, command=lambda: self._lang("en"),
        )
        self.btn_en.pack(side="left", padx=2, pady=2)

        self.btn_min = ctk.CTkButton(
            right, text="—", width=36, height=28, corner_radius=8,
            fg_color=T.SURFACE_2, hover_color=T.BORDER, text_color=T.TEXT, command=self._minimize,
        )
        self.btn_min.pack(side="left", padx=3)
        self.btn_close = ctk.CTkButton(
            right, text="✕", width=36, height=28, corner_radius=8,
            fg_color=T.SURFACE_2, hover_color=T.DANGER, text_color=T.TEXT, command=self._on_close,
        )
        self.btn_close.pack(side="left", padx=3)

        for w in (self, left, titles):
            w.bind("<ButtonPress-1>", self._start_drag)
            w.bind("<B1-Motion>", self._do_drag)

    def _lang(self, code: str):
        if self._on_lang:
            self._on_lang(code)

    def set_title(self, title: str):
        self.title_lbl.configure(text=title)
        self.motto_lbl.configure(text=I.t("motto"))
        self.btn_es.configure(
            fg_color=T.PRIMARY if I.lang() == "es" else "transparent",
            text_color="#071014" if I.lang() == "es" else T.TEXT,
        )
        self.btn_en.configure(
            fg_color=T.PRIMARY if I.lang() == "en" else "transparent",
            text_color="#071014" if I.lang() == "en" else T.TEXT,
        )

    def _minimize(self):
        if self._on_minimize:
            self._on_minimize()
        else:
            self.winfo_toplevel().iconify()

    def _start_drag(self, e):
        self._drag["x"] = e.x_root
        self._drag["y"] = e.y_root

    def _do_drag(self, e):
        win = self.winfo_toplevel()
        dx = e.x_root - self._drag["x"]
        dy = e.y_root - self._drag["y"]
        self._drag["x"] = e.x_root
        self._drag["y"] = e.y_root
        win.geometry(f"+{win.winfo_x() + dx}+{win.winfo_y() + dy}")
