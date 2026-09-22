# -*- coding: utf-8 -*-
"""Widgets premium reutilizables."""
from __future__ import annotations
from pathlib import Path
import customtkinter as ctk
from PIL import Image, ImageDraw
from . import theme as T


class StatusPill(ctk.CTkFrame):
    def __init__(self, master, label: str, value: str, ok: bool = True, **kwargs):
        super().__init__(master=master, fg_color=T.CARD, corner_radius=14, border_width=1, border_color=T.BORDER, **kwargs)
        ctk.CTkLabel(self, text=label, text_color=T.MUTED, font=(T.FONT, 11)).pack(anchor="w", padx=14, pady=(10, 0))
        row = ctk.CTkFrame(self, fg_color="transparent")
        row.pack(anchor="w", padx=14, pady=(2, 12), fill="x")
        ctk.CTkLabel(row, text="●", text_color=T.SUCCESS if ok else T.DANGER, font=(T.FONT, 12)).pack(side="left", padx=(0, 6))
        ctk.CTkLabel(row, text=value, text_color=T.TEXT, font=(T.FONT, 15, "bold")).pack(side="left")


class ScreenPreview(ctk.CTkFrame):
    """Simula la Turing 3.5 landscape con bisel."""

    def __init__(self, master, width=360, height=240, **kwargs):
        w, h = int(width), int(height)
        super().__init__(
            master=master,
            fg_color=T.SURFACE_2,
            corner_radius=18,
            border_width=1,
            border_color=T.BORDER,
            **kwargs,
        )
        self._pw, self._ph = w, h
        self._photo = None
        self.canvas = ctk.CTkLabel(master=self, text="preview")
        self.canvas.pack(padx=16, pady=16)
        self.set_placeholder("Sin preview")

    def set_placeholder(self, text: str = "Sin preview"):
        img = Image.new("RGB", (self._pw, self._ph), (18, 24, 34))
        dr = ImageDraw.Draw(img)
        dr.rectangle((0, 0, self._pw - 1, 8), fill=(61, 204, 199))
        try:
            dr.text((16, max(12, self._ph // 2 - 8)), text[:40], fill=(139, 155, 171))
        except Exception:
            pass
        self._show(img)

    def show_image(self, path: Path | None):
        if not path or not Path(path).is_file():
            self.set_placeholder("Tema sin imagen")
            return
        try:
            img = Image.open(path).convert("RGB")
            img = img.resize((self._pw, self._ph), Image.Resampling.LANCZOS)
            dr = ImageDraw.Draw(img)
            dr.rectangle((0, 0, self._pw - 1, 4), fill=(61, 204, 199))
            self._show(img)
        except Exception:
            self.set_placeholder("Error al cargar")

    def _show(self, img: Image.Image):
        self._photo = ctk.CTkImage(light_image=img, dark_image=img, size=(self._pw, self._ph))
        self.canvas.configure(image=self._photo, text="")


class PrimaryButton(ctk.CTkButton):
    def __init__(self, master, text, command=None, color=T.PRIMARY, **kwargs):
        hover = T.PRIMARY_DIM if color == T.PRIMARY else color
        super().__init__(
            master=master,
            text=text,
            command=command,
            fg_color=color,
            hover_color=hover,
            text_color="#071014",
            corner_radius=12,
            font=(T.FONT, 13, "bold"),
            height=42,
            **kwargs,
        )


class GhostButton(ctk.CTkButton):
    def __init__(self, master, text, command=None, **kwargs):
        super().__init__(
            master=master,
            text=text,
            command=command,
            fg_color=T.SURFACE_2,
            hover_color=T.BORDER,
            text_color=T.TEXT,
            corner_radius=12,
            font=(T.FONT, 12),
            height=38,
            **kwargs,
        )


