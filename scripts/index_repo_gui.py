"""
GUI to download GitHub repos into indexed/source|prime|reference and index to Serena.
Run via run-index-repo.bat
"""

import os
import queue
import re
import subprocess
import sys
import threading
import tkinter as tk
from pathlib import Path
from tkinter import messagebox, simpledialog, ttk

TIERS = ("source", "prime", "reference")

# Dark theme colors (VS Code / Cursor style)
_DARK_BG = "#1e1e1e"
_DARK_FG = "#d4d4d4"
_DARK_FG_DIM = "#858585"
_DARK_ENTRY_BG = "#3c3c3c"
_DARK_BTN_BG = "#0e639c"
_DARK_BTN_FG = "#ffffff"
_DARK_SELECT = "#264f78"


def ask_tier(parent) -> str | None:
    """Show a dialog with tier dropdown. Returns selected tier or None if cancelled."""
    dialog = tk.Toplevel(parent)
    dialog.title("Select Tier")
    dialog.transient(parent)
    dialog.grab_set()
    dialog.geometry("280x100")
    dialog.configure(bg=_DARK_BG)
    f = ttk.Frame(dialog, padding=10)
    f.pack(fill=tk.BOTH, expand=True)
    ttk.Label(f, text="Tier:").pack(anchor=tk.W)
    var = tk.StringVar(value="prime")
    cb = ttk.Combobox(f, textvariable=var, values=TIERS, state="readonly", width=20)
    cb.pack(anchor=tk.W, pady=(0, 10))
    result = [None]

    def ok():
        result[0] = var.get()
        dialog.destroy()

    def cancel():
        dialog.destroy()

    btn_f = ttk.Frame(f)
    btn_f.pack(anchor=tk.W)
    ttk.Button(btn_f, text="OK", command=ok).pack(side=tk.LEFT, padx=(0, 5))
    ttk.Button(btn_f, text="Cancel", command=cancel).pack(side=tk.LEFT)
    dialog.wait_window()
    return result[0]
from typing import Callable, Optional, Tuple


# Paths - indexed under C:\oni-serena for Serena find_symbol relative_path (must be real subdir)
def get_paths():
    script_dir = Path(__file__).resolve().parent
    workspace = script_dir.parent  # ONIMiserableMods (on network)
    mods_root = workspace.parent   # \\...\ONI Mods
    # indexed lives under oni-serena so relative_path works (Serena rejects symlinks outside project root)
    serena_project = Path("C:/oni-serena")
    indexed_root = serena_project / "indexed"
    if not indexed_root.exists():
        indexed_root.mkdir(parents=True, exist_ok=True)
        for t in ("source", "prime", "reference"):
            (indexed_root / t).mkdir(exist_ok=True)
    serena_project = serena_project if serena_project.exists() else mods_root
    return workspace, indexed_root, serena_project


def parse_github_url(url: str) -> Optional[Tuple[str, str]]:
    """Extract owner/repo from GitHub URL. Returns (owner, repo) or None."""
    url = url.strip().rstrip("/")
    patterns = [
        r"github\.com[:/]([^/]+)/([^/]+?)(?:\.git)?$",
        r"https?://github\.com/([^/]+)/([^/]+?)(?:\.git)?/?$",
    ]
    for p in patterns:
        m = re.search(p, url, re.I)
        if m:
            repo = m.group(2)
            if repo.endswith(".git"):
                repo = repo[:-4]
            return m.group(1), repo
    return None


def repo_folder_name(owner: str, repo: str) -> str:
    return f"{owner}-{repo}" if owner != repo else repo


def ensure_clone(url: str, dest: Path, log_callback) -> bool:
    """Clone repo or pull if exists."""
    if dest.exists():
        log_callback(f"Folder exists: {dest}")
        log_callback("Running git pull...")
        r = subprocess.run(
            ["git", "pull"],
            cwd=dest,
            capture_output=True,
            text=True,
            timeout=120,
        )
        if r.returncode != 0:
            log_callback(f"git pull failed: {r.stderr or r.stdout}")
            return False
        log_callback("git pull OK")
        return True

    dest.parent.mkdir(parents=True, exist_ok=True)
    log_callback(f"Cloning {url} -> {dest}")
    r = subprocess.run(
        ["git", "clone", "--depth", "1", url, str(dest)],
        capture_output=True,
        text=True,
        timeout=300,
    )
    if r.returncode != 0:
        log_callback(f"git clone failed: {r.stderr or r.stdout}")
        return False
    log_callback("Clone OK")
    return True


def dotnet_restore(folder: Path, log_callback) -> bool:
    """Run dotnet restore if .sln or .csproj found."""
    slns = list(folder.rglob("*.sln"))
    csprojs = list(folder.rglob("*.csproj"))
    if not slns and not csprojs:
        log_callback("No .sln/.csproj found, skipping dotnet restore")
        return True
    target = slns[0] if slns else csprojs[0]
    log_callback(f"Running dotnet restore {target.relative_to(folder)}...")
    r = subprocess.run(
        ["dotnet", "restore", str(target)],
        cwd=folder,
        capture_output=True,
        text=True,
        timeout=180,
    )
    if r.returncode != 0:
        log_callback(f"dotnet restore warning: {r.stderr or r.stdout}")
    else:
        log_callback("dotnet restore OK")
    return True


def ensure_junctions(serena_project: Path, log_callback) -> bool:
    """No-op: Serena uses parent folder (ONI Mods) as project; junctions not needed on network shares."""
    log_callback("Using parent folder (no junctions needed)")
    return True


_SOLUTIONDIR_BLOCK = """
\t<!-- SolutionDir is undefined when loading standalone .csproj (e.g. C# LSP). Default to repo root. -->
\t<PropertyGroup>
\t\t<SolutionDir Condition="'$(SolutionDir)' == ''">$(MSBuildThisFileDirectory)</SolutionDir>
\t</PropertyGroup>
"""


def _patch_props_solutiondir(props_path: Path, log_callback) -> bool:
    try:
        text = props_path.read_text(encoding="utf-8", errors="replace")
    except OSError as e:
        log_callback(f"  Could not read {props_path.name}: {e}")
        return False
    if "SolutionDir Condition" in text and "MSBuildThisFileDirectory" in text:
        return False
    if "<Import " not in text:
        return False
    import_pattern = re.compile(r"<Import[\s\S]*?(?:\s*/>|</Import>)")
    matches = list(import_pattern.finditer(text))
    if not matches:
        return False
    insert_pos = matches[-1].end()
    new_text = text[:insert_pos] + _SOLUTIONDIR_BLOCK + text[insert_pos:]
    try:
        props_path.write_text(new_text, encoding="utf-8")
    except OSError as e:
        log_callback(f"  Could not write {props_path.name}: {e}")
        return False
    log_callback(f"  Patched {props_path.name}: SolutionDir fallback")
    return True


def _patch_targets_core_skip(targets_path: Path, log_callback) -> bool:
    try:
        text = targets_path.read_text(encoding="utf-8", errors="replace")
    except OSError as e:
        log_callback(f"  Could not read {targets_path.name}: {e}")
        return False
    if "MSBuildRuntimeType" in text:
        return False
    old_part = "Condition=\"'$(GameLibsFolder)' != '../Lib'"
    new_part = "Condition=\"'$(MSBuildRuntimeType)' != 'Core' and '$(GameLibsFolder)' != '../Lib'"
    if old_part not in text:
        return False
    new_text = text.replace(old_part, new_part)
    if new_text == text:
        return False
    try:
        targets_path.write_text(new_text, encoding="utf-8")
    except OSError as e:
        log_callback(f"  Could not write {targets_path.name}: {e}")
        return False
    log_callback(f"  Patched {targets_path.name}: skip Publicise under .NET Core")
    return True


def _patch_csproj_empty_targetframework(csproj_path: Path, log_callback) -> bool:
    """Add TargetFramework when SDK-style project has none (LSP gets empty when import fails)."""
    try:
        text = csproj_path.read_text(encoding="utf-8", errors="replace")
    except OSError as e:
        log_callback(f"  Could not read {csproj_path.name}: {e}")
        return False
    if "Sdk=\"Microsoft.NET.Sdk\"" not in text and "Sdk='Microsoft.NET.Sdk'" not in text:
        return False
    if "TargetFramework" in text or "TargetFrameworks" in text:
        return False
    # Insert after first <PropertyGroup> opening tag
    match = re.search(r"(<PropertyGroup>)", text)
    if not match:
        return False
    insert_pos = match.end(1)
    new_text = text[:insert_pos] + "\n    <TargetFramework>net471</TargetFramework>" + text[insert_pos:]
    try:
        csproj_path.write_text(new_text, encoding="utf-8")
    except OSError as e:
        log_callback(f"  Could not write {csproj_path.name}: {e}")
        return False
    log_callback(f"  Patched {csproj_path.name}: added TargetFramework net471 (was empty)")
    return True


def _patch_csproj_net48_compat(csproj_path: Path, log_callback) -> bool:
    """Bump net471 -> net48 when project references UtilLibs or CommonLib (both target net48)."""
    try:
        text = csproj_path.read_text(encoding="utf-8", errors="replace")
    except OSError as e:
        log_callback(f"  Could not read {csproj_path.name}: {e}")
        return False
    refs_net48_lib = "UtilLibs" in text or "CommonLib" in text
    if not refs_net48_lib or "net471" not in text:
        return False
    new_text = text.replace("<TargetFramework>net471</TargetFramework>", "<TargetFramework>net48</TargetFramework>")
    if new_text == text:
        return False
    try:
        csproj_path.write_text(new_text, encoding="utf-8")
    except OSError as e:
        log_callback(f"  Could not write {csproj_path.name}: {e}")
        return False
    log_callback(f"  Patched {csproj_path.name}: net471 -> net48 (UtilLibs/CommonLib compat)")
    return True


def patch_repo_for_lsp(repo_path: Path, log_callback: Callable[[str], None]) -> bool:
    changed = False
    if (repo_path / "Directory.Build.props").exists():
        if _patch_props_solutiondir(repo_path / "Directory.Build.props", log_callback):
            changed = True
    if (repo_path / "Directory.Build.targets").exists():
        if _patch_targets_core_skip(repo_path / "Directory.Build.targets", log_callback):
            changed = True
    for csproj in repo_path.rglob("*.csproj"):
        if _patch_csproj_empty_targetframework(csproj, log_callback):
            changed = True
        elif _patch_csproj_net48_compat(csproj, log_callback):
            changed = True
    return changed


def patch_all_indexed(indexed_root: Path, log_callback: Callable[[str], None]) -> None:
    for tier in ("source", "prime", "reference"):
        tier_path = indexed_root / tier
        if not tier_path.exists():
            continue
        try:
            subdirs = [d for d in tier_path.iterdir() if d.is_dir()]
        except OSError as e:
            log_callback(f"Could not list {tier_path}: {e}")
            continue
        for subdir in subdirs:
            if list(subdir.rglob("*.csproj")) or (subdir / "Directory.Build.props").exists() or (subdir / "Directory.Build.targets").exists():
                log_callback(f"Patching {tier}/{subdir.name}...")
                patch_repo_for_lsp(subdir, log_callback)


def restore_all_indexed(indexed_root: Path, log_callback: Callable[[str], None]) -> None:
    for tier in ("source", "prime", "reference"):
        tier_path = indexed_root / tier
        if not tier_path.exists():
            continue
        try:
            subdirs = [d for d in tier_path.iterdir() if d.is_dir()]
        except OSError as e:
            log_callback(f"Could not list {tier_path}: {e}")
            continue
        for subdir in subdirs:
            dotnet_restore(subdir, log_callback)


# LSP noise: suppress C# LSP project-loading warnings/errors (unresolved deps, load failures)
_LSP_NOISE_PATTERNS = (
    "solidlsp.language_servers",
    "LanguageServerProjectLoader",
    "unresolved dependencies",
    "Error while loading",
)


def _should_suppress_lsp_line(line: str) -> bool:
    """True if line is LSP project-loading noise to suppress from log."""
    return any(p in line for p in _LSP_NOISE_PATTERNS)


def _run_streaming(
    cmd: list,
    cwd: Path,
    log_callback: Callable[[str], None],
    timeout: int = 300,
    env: Optional[dict] = None,
    filter_lsp_noise: bool = False,
) -> int:
    """Run command and stream stdout/stderr to log_callback in real time."""
    proc_env = {**os.environ}
    if env:
        proc_env.update(env)
    proc = subprocess.Popen(
        cmd,
        cwd=cwd,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        bufsize=1,
        env=proc_env,
    )
    for line in iter(proc.stdout.readline, ""):
        line = line.rstrip()
        if line:
            if filter_lsp_noise and _should_suppress_lsp_line(line):
                continue
            log_callback(line)
    proc.wait(timeout=timeout)
    return proc.returncode


def run_serena_index(workspace: Path, log_callback: Callable[[str], None]) -> bool:
    """Run Serena project index with streaming output."""
    log_callback("Running Serena project index...")
    env = {
        **os.environ,
        "PYTHONUNBUFFERED": "1",
        "MSBUILDMAXCPUCOUNT": "0",  # Use all cores for C# LSP / MSBuild project loading
    }
    code = _run_streaming(
        ["uvx", "--from", "git+https://github.com/oraios/serena", "serena", "project", "index"],
        workspace,
        log_callback,
        timeout=600,
        env=env,
        filter_lsp_noise=True,
    )
    if code != 0:
        log_callback("Serena index returned non-zero")
        return False
    log_callback("Serena index OK")
    return True


def run_index_workflow(repo_url: str, tier: str, workspace: Path, indexed_root: Path, serena_project: Path, log_callback) -> bool:
    """Full workflow: clone, restore, Serena index."""
    parsed = parse_github_url(repo_url)
    if not parsed:
        log_callback("Invalid GitHub URL")
        return False

    owner, repo = parsed
    folder_name = repo_folder_name(owner, repo)
    tier_path = indexed_root / tier
    dest = tier_path / folder_name

    log_callback(f"Tier: {tier}")
    log_callback(f"Destination: {dest}")
    log_callback("-" * 40)

    if not ensure_clone(repo_url, dest, log_callback):
        return False
    log_callback("Patching for LSP compatibility...")
    patch_repo_for_lsp(dest, log_callback)
    dotnet_restore(dest, log_callback)
    ensure_junctions(serena_project, log_callback)
    return run_serena_index(serena_project, log_callback)


def _setup_dark_theme(root: tk.Tk):
    root.configure(bg=_DARK_BG)
    style = ttk.Style()
    style.theme_use("clam")
    style.configure(".", background=_DARK_BG, foreground=_DARK_FG)
    style.configure("TFrame", background=_DARK_BG)
    style.configure("TLabel", background=_DARK_BG, foreground=_DARK_FG)
    style.configure("TButton", background=_DARK_BTN_BG, foreground=_DARK_BTN_FG)
    style.map("TButton", background=[("active", "#1177bb")])
    style.configure("TCombobox", fieldbackground=_DARK_ENTRY_BG, foreground=_DARK_FG, background=_DARK_ENTRY_BG)
    style.configure("Horizontal.TSeparator", background=_DARK_FG_DIM)
    style.configure("Vertical.TScrollbar", background=_DARK_ENTRY_BG, troughcolor=_DARK_BG)


class IndexRepoApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Index Repository for Serena")
        self.root.minsize(400, 300)
        self.root.geometry("550x450")
        _setup_dark_theme(self.root)

        self.workspace, self.indexed_root, self.serena_project = get_paths()
        self.log_text = None
        self.run_btn = None
        self.index_only_btn = None
        self.restore_btn = None
        self.patch_btn = None
        self.verify_btn = None
        self._log_queue = queue.Queue()
        self._work_done = None

    def build_ui(self):
        main = ttk.Frame(self.root, padding=10)
        main.pack(fill=tk.BOTH, expand=True)

        ttk.Label(main, text="Index for Serena", font=("", 11)).pack(pady=(0, 10))

        btn_frame = ttk.Frame(main)
        btn_frame.pack(pady=5)
        self.run_btn = ttk.Button(btn_frame, text="Download & Index Repo", command=self.on_index_click)
        self.run_btn.pack(side=tk.LEFT, padx=2)
        self.index_only_btn = ttk.Button(btn_frame, text="Index Now", command=self.on_index_only_click)
        self.index_only_btn.pack(side=tk.LEFT, padx=2)
        self.restore_btn = ttk.Button(btn_frame, text="Restore Indexed", command=self.on_restore_click)
        self.restore_btn.pack(side=tk.LEFT, padx=2)
        self.patch_btn = ttk.Button(btn_frame, text="Patch Indexed", command=self.on_patch_click)
        self.patch_btn.pack(side=tk.LEFT, padx=2)
        self.verify_btn = ttk.Button(btn_frame, text="Verify", command=self.on_verify_click)
        self.verify_btn.pack(side=tk.LEFT, padx=2)

        ttk.Label(main, text="Index Now: patch+index. Restore: dotnet restore. Patch: LSP fixes. indexed: C:\\oni-serena\\indexed", font=("", 8), foreground=_DARK_FG_DIM).pack(pady=2)

        ttk.Separator(main, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=10)

        ttk.Label(main, text="Log:").pack(anchor=tk.W)
        log_frame = ttk.Frame(main)
        log_frame.pack(fill=tk.BOTH, expand=True, pady=5)
        self.log_text = tk.Text(
            log_frame,
            wrap=tk.WORD,
            height=12,
            state=tk.DISABLED,
            bg=_DARK_ENTRY_BG,
            fg=_DARK_FG,
            insertbackground=_DARK_FG,
            selectbackground=_DARK_SELECT,
            selectforeground=_DARK_FG,
        )
        sb = ttk.Scrollbar(log_frame, command=self.log_text.yview)
        self.log_text.configure(yscrollcommand=sb.set)
        self.log_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        sb.pack(side=tk.RIGHT, fill=tk.Y)

    def log(self, msg: str):
        self.log_text.configure(state=tk.NORMAL)
        self.log_text.insert(tk.END, msg + "\n")
        self.log_text.see(tk.END)
        self.log_text.configure(state=tk.DISABLED)
        self.root.update_idletasks()

    def _drain_log_queue(self):
        """Process log messages from background thread (main-thread only)."""
        try:
            while True:
                msg = self._log_queue.get_nowait()
                if msg == "__DONE_TRUE__":
                    self._work_done = True
                    break
                elif msg == "__DONE_FALSE__":
                    self._work_done = False
                    break
                else:
                    self.log(msg)
        except queue.Empty:
            pass
        if self._work_done is not None:
            done = self._work_done
            self._work_done = None
            self.run_btn.configure(state=tk.NORMAL)
            self.index_only_btn.configure(state=tk.NORMAL)
            self.restore_btn.configure(state=tk.NORMAL)
            self.patch_btn.configure(state=tk.NORMAL)
            self.verify_btn.configure(state=tk.NORMAL)
            self._ask_another_and_maybe_exit(done)
            return
        self.root.after(50, self._drain_log_queue)

    def _queue_log(self, msg: str):
        """Thread-safe: put message for main thread to display."""
        self._log_queue.put(msg)

    def clear_log(self):
        self.log_text.configure(state=tk.NORMAL)
        self.log_text.delete(1.0, tk.END)
        self.log_text.configure(state=tk.DISABLED)

    def _ask_another_and_maybe_exit(self, ok: bool):
        if ok:
            msg = "Indexing complete. Index another?"
        else:
            msg = "Indexing had errors. Try again?"
        if not messagebox.askyesno("Continue?", msg, default="yes" if ok else "no"):
            self.root.quit()
            self.root.destroy()
            sys.exit(0)

    def _run_index_only(self):
        try:
            self._queue_log("Patching indexed repos for LSP compatibility...")
            patch_all_indexed(self.indexed_root, self._queue_log)
            ensure_junctions(self.serena_project, self._queue_log)
            ok = run_serena_index(self.serena_project, self._queue_log)
        except Exception as e:
            self._queue_log(f"Error: {e}")
            ok = False
        self._queue_log("__DONE_TRUE__" if ok else "__DONE_FALSE__")

    def on_index_only_click(self):
        """Patch + Serena index (for manually added source)."""
        self.run_btn.configure(state=tk.DISABLED)
        self.index_only_btn.configure(state=tk.DISABLED)
        self.restore_btn.configure(state=tk.DISABLED)
        self.patch_btn.configure(state=tk.DISABLED)
        self.verify_btn.configure(state=tk.DISABLED)
        self.clear_log()
        self._queue_log("Index Now: patch + Serena index")
        self._queue_log("-" * 40)
        threading.Thread(target=self._run_index_only, daemon=True).start()
        self._drain_log_queue()

    def _run_full_workflow(self, repo_url: str, tier: str):
        try:
            ok = run_index_workflow(
                repo_url, tier, self.workspace, self.indexed_root, self.serena_project, self._queue_log,
            )
        except Exception as e:
            self._queue_log(f"Error: {e}")
            ok = False
        self._queue_log("__DONE_TRUE__" if ok else "__DONE_FALSE__")

    def on_index_click(self):
        repo_url = simpledialog.askstring("GitHub Repository", "Enter GitHub repo URL (e.g. https://github.com/owner/repo):")
        if not repo_url or not repo_url.strip():
            return

        tier = ask_tier(self.root)
        if not tier:
            return

        self.run_btn.configure(state=tk.DISABLED)
        self.index_only_btn.configure(state=tk.DISABLED)
        self.restore_btn.configure(state=tk.DISABLED)
        self.patch_btn.configure(state=tk.DISABLED)
        self.verify_btn.configure(state=tk.DISABLED)
        self.clear_log()
        self._queue_log(f"URL: {repo_url}")
        self._queue_log(f"Indexed root: {self.indexed_root}")
        threading.Thread(target=self._run_full_workflow, args=(repo_url, tier), daemon=True).start()
        self._drain_log_queue()

    def on_restore_click(self):
        """Run dotnet restore on all indexed projects."""
        self.run_btn.configure(state=tk.DISABLED)
        self.index_only_btn.configure(state=tk.DISABLED)
        self.restore_btn.configure(state=tk.DISABLED)
        self.patch_btn.configure(state=tk.DISABLED)
        self.verify_btn.configure(state=tk.DISABLED)
        self.clear_log()
        self._queue_log("Restore Indexed: dotnet restore on all projects")
        self._queue_log("-" * 40)
        threading.Thread(target=self._run_restore_indexed, daemon=True).start()
        self._drain_log_queue()

    def _run_restore_indexed(self):
        try:
            restore_all_indexed(self.indexed_root, self._queue_log)
            self._queue_log("Restore complete.")
        except Exception as e:
            self._queue_log(f"Error: {e}")
        self._queue_log("__DONE_TRUE__")

    def on_patch_click(self):
        """Apply LSP compatibility patches to all indexed repos."""
        self.run_btn.configure(state=tk.DISABLED)
        self.index_only_btn.configure(state=tk.DISABLED)
        self.restore_btn.configure(state=tk.DISABLED)
        self.patch_btn.configure(state=tk.DISABLED)
        self.verify_btn.configure(state=tk.DISABLED)
        self.clear_log()
        self._queue_log("Patch Indexed: LSP compatibility fixes")
        self._queue_log("-" * 40)
        threading.Thread(target=self._run_patch_indexed, daemon=True).start()
        self._drain_log_queue()

    def _run_patch_indexed(self):
        try:
            patch_all_indexed(self.indexed_root, self._queue_log)
            self._queue_log("Patch complete.")
        except Exception as e:
            self._queue_log(f"Error: {e}")
        self._queue_log("__DONE_TRUE__")

    def on_verify_click(self):
        """Check index status and show how to verify."""
        self.clear_log()
        self.log("Verification:")
        self.log("-" * 40)
        cache_dir = self.serena_project / ".serena" / "cache"
        if cache_dir.exists():
            try:
                files = list(cache_dir.rglob("*"))
                file_count = sum(1 for f in files if f.is_file())
                self.log(f"Index cache: {cache_dir}")
                self.log(f"Cache files: {file_count}")
                self.log("Index appears to exist.")
            except Exception as e:
                self.log(f"Could not read cache: {e}")
        else:
            self.log("No .serena/cache found. Run Index Now or Download & Index first.")
        self.log("")
        self.log("To verify in Cursor: ask the AI to use find_symbol with")
        self.log('  relative_path "indexed/source", "indexed/prime", or "indexed/reference"')
        self.log("")
        self.log("Serena dashboard: http://localhost:24282/dashboard/index.html")
        self.log("(Available when Serena MCP is running in Cursor)")

    def run(self):
        self.build_ui()
        self.log(f"Workspace: {self.workspace}")
        self.log(f"Indexed root: {self.indexed_root}")
        self.log(f"Serena project: {self.serena_project}")
        self.root.mainloop()


def main():
    app = IndexRepoApp()
    app.run()


if __name__ == "__main__":
    main()
