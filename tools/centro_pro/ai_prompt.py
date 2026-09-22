# -*- coding: utf-8 -*-
"""Generador de prompts para fondos IA 3.5\" (480x320)."""
from __future__ import annotations
import customtkinter as ctk
from . import theme as T
from . import lang as I


class AiPromptView(ctk.CTkFrame):
    def __init__(self, master, app, **kwargs):
        super().__init__(master, fg_color="transparent", **kwargs)
        self.app = app
        self._build()

    def _build(self):
        for w in self.winfo_children():
            w.destroy()

        head = ctk.CTkFrame(self, fg_color="transparent")
        head.pack(fill="x", padx=20, pady=(8, 4))
        ctk.CTkLabel(head, text=I.t("ai_title"), text_color=T.TEXT, font=(T.FONT, 22, "bold")).pack(anchor="w")
        ctk.CTkLabel(head, text=I.t("ai_subtitle"), text_color=T.MUTED, font=(T.FONT, 12)).pack(anchor="w", pady=(4, 0))

        form = ctk.CTkFrame(self, fg_color=T.SURFACE, corner_radius=14)
        form.pack(fill="x", padx=20, pady=12)

        styles = I.tmap("styles")
        colors = I.tmap("colors")
        elements = I.tmap("elements")

        self._style_keys = list(styles.keys()) or ["cyberpunk"]
        self._color_keys = list(colors.keys()) or ["neon"]
        self._elem_keys = list(elements.keys()) or ["metrics"]

        def row(label_key, values_map, keys):
            box = ctk.CTkFrame(form, fg_color="transparent")
            box.pack(fill="x", padx=16, pady=8)
            ctk.CTkLabel(box, text=I.t(label_key), text_color=T.MUTED, font=(T.FONT, 12)).pack(anchor="w")
            labels = [values_map.get(k, k) for k in keys]
            var = ctk.StringVar(value=labels[0] if labels else "")
            menu = ctk.CTkOptionMenu(
                box,
                values=labels or ["—"],
                variable=var,
                fg_color=T.CARD,
                button_color=T.SURFACE_2,
                button_hover_color=T.BORDER,
                dropdown_fg_color=T.SURFACE,
            )
            menu.pack(fill="x", pady=(4, 0))
            return var, labels, keys

        self.style_var, self.style_labels, self.style_keys = row("ai_style", styles, self._style_keys)
        self.color_var, self.color_labels, self.color_keys = row("ai_colors", colors, self._color_keys)
        self.elem_var, self.elem_labels, self.elem_keys = row("ai_elements", elements, self._elem_keys)

        size = ctk.CTkFrame(form, fg_color="transparent")
        size.pack(fill="x", padx=16, pady=8)
        ctk.CTkLabel(size, text=I.t("ai_size"), text_color=T.MUTED, font=(T.FONT, 12)).pack(anchor="w")
        ctk.CTkLabel(size, text=I.t("ai_size_value"), text_color=T.PRIMARY, font=(T.FONT, 13, "bold")).pack(anchor="w", pady=(4, 0))

        actions = ctk.CTkFrame(form, fg_color="transparent")
        actions.pack(fill="x", padx=16, pady=(4, 16))
        ctk.CTkButton(
            actions,
            text=I.t("ai_generate"),
            command=self._generate,
            fg_color=T.PRIMARY,
            text_color="#071014",
            corner_radius=12,
            height=40,
            font=(T.FONT, 13, "bold"),
        ).pack(side="left")
        ctk.CTkButton(
            actions,
            text=I.t("ai_copy"),
            command=self._copy,
            fg_color=T.SURFACE_2,
            text_color=T.TEXT,
            corner_radius=12,
            height=40,
        ).pack(side="left", padx=8)

        ctk.CTkLabel(self, text=I.t("ai_prompt_box"), text_color=T.MUTED, font=(T.FONT, 12)).pack(anchor="w", padx=20)
        self.out = ctk.CTkTextbox(self, height=140, fg_color=T.CARD, text_color=T.TEXT, corner_radius=12, font=(T.FONT, 12))
        self.out.pack(fill="x", padx=20, pady=(6, 8))
        ctk.CTkLabel(self, text=I.t("ai_hint"), text_color=T.MUTED, font=(T.FONT, 11), wraplength=640, justify="left").pack(anchor="w", padx=20)
        self._generate()

    def _pick(self, var, labels, keys):
        label = var.get()
        try:
            i = labels.index(label)
            return keys[i]
        except Exception:
            return keys[0] if keys else ""

    def _generate(self):
        sk = self._pick(self.style_var, self.style_labels, self.style_keys)
        ck = self._pick(self.color_var, self.color_labels, self.color_keys)
        ek = self._pick(self.elem_var, self.elem_labels, self.elem_keys)
        styles = I.tmap("styles")
        colors = I.tmap("colors")
        elements = I.tmap("elements")
        prompt = I.t(
            "prompt_template",
            style=styles.get(sk, sk),
            colors=colors.get(ck, ck),
            elements=elements.get(ek, ek),
        )
        self.out.delete("1.0", "end")
        self.out.insert("1.0", prompt)

    def _copy(self):
        text = self.out.get("1.0", "end").strip()
        self.clipboard_clear()
        self.clipboard_append(text)
        self.app.toast(I.t("ai_copied"), win=True)
