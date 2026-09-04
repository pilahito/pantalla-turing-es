"""Pone STATS. y DISPLAY_SIZE 3.5\" en temas OnePiece rotos."""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[1] / "res" / "themes"
for d in sorted(root.glob("OnePiece*")):
    y = d / "theme.yaml"
    if not y.exists():
        continue
    t = y.read_text(encoding="utf-8-sig")
    t = re.sub(r'DISPLAY_SIZE:\s*["\']?3\.5["\']?', 'DISPLAY_SIZE: 3.5"', t)
    if re.search(r"^STATS:\s*$", t, re.M):
        y.write_text(t, encoding="utf-8")
        print("ok size", d.name)
        continue
    # Envolver CPU/GPU/MEMORY de primer nivel en STATS
    lines = t.splitlines(True)
    out = []
    inserted = False
    i = 0
    while i < len(lines):
        line = lines[i]
        if not inserted and re.match(r"^(CPU|GPU|MEMORY|DISK|NET|DATE):", line):
            out.append("STATS:\n")
            inserted = True
        if inserted and re.match(r"^(CPU|GPU|MEMORY|DISK|NET|DATE):", line):
            out.append("  " + line)
            i += 1
            while i < len(lines) and (lines[i].startswith(" ") or lines[i].startswith("\t") or lines[i].strip() == ""):
                if lines[i].strip() == "":
                    out.append(lines[i])
                else:
                    out.append("  " + lines[i] if not lines[i].startswith("  ") else lines[i])
                i += 1
            continue
        out.append(line)
        i += 1
    y.write_text("".join(out), encoding="utf-8")
    print("fixed", d.name)
