# Tiny Clipboard Normalizer (Windows + macOS)

Um app minimalista que fica na bandeja/menu bar e **normaliza automaticamente texto no clipboard**, removendo/substituindo caracteres invisíveis/equivalentes (ex.: NBSP/ZWSP/soft hyphen, aspas curvas, dashes, ellipsis) **sem alterar quebras de linha (CR/LF)**.  
O app mantém contadores de caracteres afetados (sessão e total).

---

## Estrutura do repositório

- `win/TinyClipboardTray/`  
  Implementação Windows (C# / WinForms tray app).

- `mac/TinyClipboardNormalizer/`  
  Implementação macOS (Swift / AppKit menu bar app).

- `.github/workflows/`  
  Pipeline de build (GitHub Actions) para gerar o `.app` e zipar.

---

## Downloads / Onde encontrar os binários

### macOS (`.app` dentro de `.zip`)

O macOS build é gerado pelo **GitHub Actions** e publicado como **artifact** do workflow.

1. Vá em **GitHub → aba Actions**
2. Abra o workflow **build-macos**
3. Abra o último run com sucesso
4. Em **Artifacts**, baixe:
   - `TinyClipboardNormalizer-macos12-x86_64.zip`

Esse `.zip` contém `TinyClipboardNormalizer.app`.

> Observação: o build é `x86_64` (Intel) com deployment target macOS 12. Em Macs Apple Silicon (M1/M2/M3), roda via Rosetta 2.

### Windows (`.exe`)

O executável do Windows é produzido a partir do projeto em `win/TinyClipboardTray/`.

- Código-fonte do app: `win/TinyClipboardTray/`
- O `.exe` pode ser encontrado após build local em:
  - `win/TinyClipboardTray/bin/Release/net8.0-windows/win-x64/publish/TinyClipboardTray.exe` (publish)
  - ou `win/TinyClipboardTray/bin/Release/net8.0-windows/win-x64/TinyClipboardTray.exe` (dependendo do tipo de build)

> Recomendação: use o executável da pasta `publish/` para distribuição.

---

## Como usar (comportamento comum)

Quando habilitado, o app:

- observa o clipboard **apenas para texto**
- aplica normalização caractere-a-caractere (inclui remoções e substituições)
- evita loop ao reescrever o clipboard
- mantém contadores:
  - **Session affected chars**: desde que o app abriu
  - **All time affected chars**: persistente entre execuções

---

## macOS: instalação e primeira execução (sem assinatura/notarização)

1. Baixe `TinyClipboardNormalizer-macos12-x86_64.zip` (ver seção Downloads).
2. Descompacte (duplo clique) para obter `TinyClipboardNormalizer.app`.
3. Mova para `/Applications` (recomendado) ou `~/Applications`.

### Se o macOS bloquear (Gatekeeper / Quarantine)

**Método A (Finder):**

- botão direito no app → **Open** → confirmar **Open** novamente.

**Método B (Terminal):**
Se estiver em `/Applications`:

```bash
xattr -dr com.apple.quarantine "/Applications/TinyClipboardNormalizer.app"
```

Se estiver em `~/Applications`:

```bash
xattr -dr com.apple.quarantine "$HOME/Applications/TinyClipboardNormalizer.app"
```

### Uso no macOS

- O app **não aparece no Dock** (menu bar only).
- Um ícone aparece na **menu bar**.
- Menu contém:
  - Session affected chars (somente leitura)
  - All time affected chars (somente leitura)
  - Enabled (liga/desliga)
  - Quit

---

## Windows: uso

- O app aparece como ícone na **system tray**.
- Menu contém:
  - Session affected chars (somente leitura)
  - All time affected chars (somente leitura)
  - Enabled (liga/desliga)
  - Exit

---

## Regras de normalização (resumo)

O normalizador (em ambas as plataformas) segue as regras principais:

- Remove: soft hyphen (U+00AD)
- Remove: format controls e bidi controls (ex.: ZWSP/ZWJ/ZWNJ, LRM/RLM, RLI/LRI/FSI/PDI, BOM etc.)
- Substitui espaços especiais (NBSP e família) por espaço ASCII
- Normaliza aspas simples/duplas para `'` e `"`
- Normaliza dashes/minus para `-`
- Substitui ellipsis `…` por `...`
- **Preserva CR/LF** (não altera quebras de linha)

---

## Desenvolvimento

- Windows app: edite em `win/TinyClipboardTray/`
- macOS app: edite em `mac/TinyClipboardNormalizer/`
- Build macOS é feito no GitHub Actions (runner macOS), sem necessidade de Xcode local.

---

## Limitações / Próximos passos (opcional)

- O app macOS não é assinado/notarizado (pode exigir “Open”/remoção de quarantine).
- Futuro: build arm64/universal, Launch at Login, assinatura/notarização.
