"""
make_package.py
===============
Creates a full distribution ZIP for Logility Freight Tool.

Output: dist\Logility_Freight_v<YYYY-MM-DD>.zip

Structure inside the ZIP:
  Logility_Freight\
    Logility_Freight.exe          <- from bin\Debug (latest build)
    FedEx.PABST.SS.*.dll          <- managed screen-scraping DLLs
    FedEx.PABST.SS.SSLib.dll
    ScreenLayouts.xml             <- screen layout config (runtime required)
    ScreenLayouts.dtd
    deploy\
      fxf3270.rsf                 <- CICS session profile
      tn3270_dll.dll              <- native TN3270 DLL
    docs\
      FXF3A_Tool_Connection_Guide.docx
      FXF3A_Tool_Technical_Workflow.docx
      MAINFRAME_CONNECTION_GUIDE.md
      FUNCTIONAL_SPEC.md
      TECHNICAL_WORKFLOW.md
      SCREEN_DATA_MODELS.md
      BUILD_GUIDE.md
      Run-Tn3270Subst.bat
      Run-LogilityRuntimeSetup.bat
      Create-Tn3270Subst.ps1
      Setup-LogilityFreightRuntime.ps1

Usage:
  python make_package.py
"""

import zipfile
import shutil
import sys
from datetime import date
from pathlib import Path

ROOT = Path(__file__).parent.resolve()
BIN_DEBUG   = ROOT / "bin" / "Debug"
DEPLOY_SRC  = ROOT / "deploy"
SCREENS_SRC = ROOT / "Screenlayouts"
DOCS_SRC    = ROOT / "docs"
DIST_DIR    = ROOT / "dist"

APP_INNER   = "Logility_Freight"

# ── Application binaries (from bin\Debug — latest build) ────────────────────
APP_FILES = [
    (BIN_DEBUG / "Logility_Freight.exe",   f"{APP_INNER}/Logility_Freight.exe"),
]
# All managed FedEx DLLs alongside the exe
for dll in sorted(BIN_DEBUG.glob("FedEx.PABST.SS.*.dll")):
    APP_FILES.append((dll, f"{APP_INNER}/{dll.name}"))

# ── Runtime config / native DLL ──────────────────────────────────────────────
DEPLOY_FILES = [
    (DEPLOY_SRC / "fxf3270.rsf",      f"{APP_INNER}/deploy/fxf3270.rsf"),
    (BIN_DEBUG  / "tn3270_dll.dll",   f"{APP_INNER}/deploy/tn3270_dll.dll"),
]

# ── Screen layout XML (from bin\Debug — newest copy) ───────────────────────
SCREEN_FILES = [
    (BIN_DEBUG / "ScreenLayouts.xml",    f"{APP_INNER}/ScreenLayouts.xml"),
    (SCREENS_SRC / "ScreenLayouts.dtd",  f"{APP_INNER}/ScreenLayouts.dtd"),
]

# ── Documentation ────────────────────────────────────────────────────────────
DOCS_FILES = [
    "FXF3A_Tool_Connection_Guide.docx",
    "FXF3A_Tool_Technical_Workflow.docx",
    "MAINFRAME_CONNECTION_GUIDE.md",
    "FUNCTIONAL_SPEC.md",
    "TECHNICAL_WORKFLOW.md",
    "SCREEN_DATA_MODELS.md",
    "BUILD_GUIDE.md",
    "Run-Tn3270Subst.bat",
    "Run-LogilityRuntimeSetup.bat",
    "Create-Tn3270Subst.ps1",
    "Setup-LogilityFreightRuntime.ps1",
]

def validate_sources(entries):
    missing = [str(src) for src, _ in entries if not src.exists()]
    if missing:
        print("ERROR: Missing source files:")
        for m in missing:
            print(f"  {m}")
        sys.exit(1)

def build_zip(zip_path: Path):
    all_entries = APP_FILES + DEPLOY_FILES + SCREEN_FILES
    validate_sources(all_entries)

    missing_docs = [f for f in DOCS_FILES if not (DOCS_SRC / f).exists()]
    if missing_docs:
        print("WARNING: Missing docs (will be skipped):")
        for d in missing_docs:
            print(f"  docs/{d}")

    DIST_DIR.mkdir(exist_ok=True)

    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zf:
        for src, arcname in all_entries:
            zf.write(src, arcname)
            print(f"  + {arcname}")

        for fname in DOCS_FILES:
            src = DOCS_SRC / fname
            if src.exists():
                arcname = f"{APP_INNER}/docs/{fname}"
                zf.write(src, arcname)
                print(f"  + {arcname}")

    size_kb = zip_path.stat().st_size // 1024
    print(f"\nPackage created: {zip_path}  ({size_kb} KB)")

if __name__ == "__main__":
    tag = date.today().strftime("%Y-%m-%d")
    zip_name = f"Logility_Freight_v{tag}.zip"
    zip_path = DIST_DIR / zip_name
    print(f"Building {zip_name}...\n")
    build_zip(zip_path)
