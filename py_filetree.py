import os
from pathlib import Path

# Configurações
SKIP_DIRS = {"node_modules", "bin", "obj", ".git", ".wrangler", "dist", "debug_log", "__pycache__"}
ROOT = Path(__file__).parent.absolute()
OUT = ROOT / "debug_log" / "current_filetree.log"

def generate_tree(dir_path, prefix=""):
    entries = sorted(list(dir_path.iterdir()), key=lambda x: (x.is_file(), x.name))
    entries = [e for e in entries if e.name not in SKIP_DIRS]
    
    lines = []
    for i, entry in enumerate(entries):
        is_last = (i == len(entries) - 1)
        connector = "└── " if is_last else "├── "
        
        lines.append(f"{prefix}{connector}{entry.name}{'/' if entry.is_dir() else ''}")
        
        if entry.is_dir():
            new_prefix = prefix + ("    " if is_last else "│   ")
            lines.extend(generate_tree(entry, new_prefix))
    return lines

def main():
    tree_lines = [f"{ROOT} (Raiz de Migração)"]
    tree_lines.extend(generate_tree(ROOT))
    
    output_text = "\n".join(tree_lines)
    
    # Garante que a pasta de log existe
    OUT.parent.mkdir(exist_ok=True)
    OUT.write_text(output_text, encoding="utf-8")
    
    print(f"Filetree gerado com sucesso em: {OUT}")
    # Também imprime no console para visualização imediata
    print("\n" + output_text)

if __name__ == "__main__":
    main()