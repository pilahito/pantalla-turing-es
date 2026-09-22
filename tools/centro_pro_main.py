# -*- coding: utf-8 -*-
"""Entrada Centro Turing Pro (Fase 1)."""
import sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))
from centro_pro.app import run

if __name__ == "__main__":
    run()
