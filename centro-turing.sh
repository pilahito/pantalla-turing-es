#!/usr/bin/env bash
# Centro nuevo en Linux. La pantalla sigue siendo main.py, un solo proceso.
ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"
if [[ -x "$ROOT/venv/bin/python" ]]; then
  PY="$ROOT/venv/bin/python"
else
  PY="python3"
fi
exec "$PY" "$ROOT/tools/centro_turing.py"
