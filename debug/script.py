from pathlib import Path

# SCRIPT_DIR é root/debug
SCRIPT_DIR = Path(__file__).resolve().parent
# ROOT é root/
ROOT = SCRIPT_DIR.parent
# OUT é root/debug/debug_log/logs.log
OUT = ROOT / "debug" / "debug_log" / "logs.log"

OUT.parent.mkdir(parents=True, exist_ok=True)

KEY_FILES = [
    "ClipboardNormalizer.cs",
    "StatsStore.cs",
    "Program.cs",
    "Form1.cs",
    "Form1.Designer.cs",
    "TinyClipboardTray.csproj",
]

SEARCH_TERMS = [
    "NBSP", "ZWSP", "soft hyphen", "bidi", "ellipsis", "en dash", "em dash",
    "UserDefaults", "Registry", "clipboard", "pasteboard", "changeCount",
    "enabled", "toggle", "session", "all time", "total",
]

IGNORE_DIRS = {"bin", "obj", ".git"}

def read_text(p: Path) -> str:
    return p.read_text(encoding="utf-8", errors="replace")

def main():
    lines_out = []

    lines_out.append(f"ROOT={ROOT}")
    gi = ROOT / ".gitignore"
    lines_out.append(f".gitignore exists={gi.exists()}")
    if gi.exists():
        lines_out.append("\n--- .gitignore (first 200 lines) ---")
        gtxt = read_text(gi).splitlines()
        for i, line in enumerate(gtxt[:200], start=1):
            lines_out.append(f"{i:04d}: {line}")

    # localizar projeto Windows (pasta TinyClipboardTray)
    win_root = ROOT / "TinyClipboardTray"
    lines_out.append(f"\nwin_root={win_root} exists={win_root.exists()}")

    # dump dos arquivos chave (conteúdo completo)
    for name in KEY_FILES:
        matches = list(ROOT.rglob(name))
        if not matches:
            lines_out.append(f"\n=== FILE NOT FOUND: {name} ===")
            continue

        for p in matches:
            # ignorar bin/obj
            if any(part in IGNORE_DIRS for part in p.parts):
                continue
            lines_out.append("\n" + "=" * 120)
            lines_out.append(f"FILE: {p}")
            lines_out.append("=" * 120)
            try:
                text = read_text(p)
            except Exception as e:
                lines_out.append(f"READ ERROR: {e}")
                continue
            # com line numbers
            for i, line in enumerate(text.splitlines(), start=1):
                lines_out.append(f"{i:04d}: {line}")

    # varredura por termos relevantes em .cs (fora bin/obj)
    lines_out.append("\n" + "#" * 120)
    lines_out.append("SCAN: search terms in *.cs (excluding bin/obj/.git)")
    lines_out.append("#" * 120)

    cs_files = []
    for p in ROOT.rglob("*.cs"):
        if any(part in IGNORE_DIRS for part in p.parts):
            continue
        cs_files.append(p)

    for p in sorted(cs_files):
        try:
            text = read_text(p)
        except Exception as e:
            lines_out.append(f"\n--- READ ERROR: {p} :: {e} ---")
            continue

        hits = []
        lines = text.splitlines()
        for idx, line in enumerate(lines, start=1):
            low = line.lower()
            if any(term.lower() in low for term in SEARCH_TERMS):
                hits.append(idx)

        if not hits:
            continue

        lines_out.append("\n" + "-" * 120)
        lines_out.append(f"HITS FILE: {p}")
        lines_out.append("-" * 120)

        # imprimir trechos curtos com contexto
        printed = 0
        for ln in hits:
            if printed >= 40:
                lines_out.append("... (limite por arquivo atingido)")
                break
            start = max(1, ln - 3)
            end = min(len(lines), ln + 3)
            lines_out.append(f"\n--- hit @ line {ln} ---")
            for j in range(start, end + 1):
                prefix = ">>" if j == ln else "  "
                lines_out.append(f"{prefix} {j:04d}: {lines[j-1]}")
            printed += 1

    OUT.write_text("\n".join(lines_out), encoding="utf-8")
    print(f"OK: wrote {OUT}")

if __name__ == "__main__":
    main()
