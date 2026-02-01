"""
Funky GUI for Promote & Release workflows.
Promote: cherry-pick mod from development to master
Release: build, zip, commit, push, tag
Run via promote-release.bat
"""

import queue
import re
import subprocess
import sys
import threading
import tkinter as tk
from pathlib import Path
from tkinter import messagebox, ttk
from typing import Callable, List, Optional


# Retro-funky palette
COLORS = {
    "bg_dark": "#0d1117",
    "bg_panel": "#161b22",
    "accent_cyan": "#58a6ff",
    "accent_green": "#3fb950",
    "accent_orange": "#d29922",
    "accent_pink": "#db61a2",
    "accent_purple": "#bc8cff",
    "text": "#c9d1d9",
    "text_dim": "#8b949e",
}


def get_solution_root() -> Path:
    return Path(__file__).resolve().parent.parent


def get_mod_projects(root: Path) -> List[str]:
    exclude = {".", "lib", "release", "packages", "infrastructure", "BlankProject", "scripts", "jarvis"}
    projects = []
    for d in sorted(root.iterdir()):
        if not d.is_dir() or d.name.startswith(".") or d.name in exclude:
            continue
        csproj = list(d.glob("*.csproj"))
        mod_info = d / "mod_info.yaml"
        if csproj and mod_info.exists():
            projects.append(d.name)
    return projects


def get_mod_version(root: Path, project: str) -> str:
    mod_info = root / project / "mod_info.yaml"
    if not mod_info.exists():
        return "unknown"
    try:
        text = mod_info.read_text(encoding="utf-8", errors="replace")
        m = re.search(r"version:\s*(\d+\.\d+\.\d+(?:\.\d+)?)", text)
        return m.group(1) if m else "unknown"
    except Exception:
        return "unknown"


def run_powershell_script(
    script_name: str,
    root: Path,
    log_callback: Callable[[str], None],
    project: Optional[str] = None,
    force: bool = True,
) -> bool:
    script_path = root / "scripts" / script_name
    if not script_path.exists():
        log_callback(f"ERROR: {script_name} not found at {root}")
        return False
    log_callback(f"Running {script_name}...")
    cmd = ["powershell", "-ExecutionPolicy", "Bypass", "-NoProfile", "-File", str(script_path)]
    if project:
        cmd.extend(["-Project", project])
    if force:
        cmd.append("-Force")
    try:
        result = subprocess.run(
            cmd,
            cwd=root,
            capture_output=True,
            text=True,
            timeout=600,
        )
        for line in (result.stdout or "").splitlines():
            if line.strip():
                log_callback(line)
        for line in (result.stderr or "").splitlines():
            if line.strip():
                log_callback(f"[stderr] {line}")
        return result.returncode == 0
    except subprocess.TimeoutExpired:
        log_callback("ERROR: Script timed out")
        return False
    except Exception as e:
        log_callback(f"ERROR: {e}")
        return False


class PromoteReleaseApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Promote & Release — ONI Miserable Mods")
        self.root.minsize(420, 360)
        self.root.geometry("520x480")
        self.root.configure(bg=COLORS["bg_dark"])

        self.root.option_add("*Font", "Consolas 10")
        self.root.option_add("*Background", COLORS["bg_dark"])
        self.root.option_add("*Foreground", COLORS["text"])

        self.solution_root = get_solution_root()
        self.projects = get_mod_projects(self.solution_root)
        self._log_queue = queue.Queue()
        self._work_done: Optional[bool] = None

        self._build_ui()

    def _build_ui(self):
        main = tk.Frame(self.root, bg=COLORS["bg_dark"], padx=16, pady=16)
        main.pack(fill=tk.BOTH, expand=True)

        # Header with funky title
        header = tk.Frame(main, bg=COLORS["bg_dark"])
        header.pack(fill=tk.X, pady=(0, 12))
        title = tk.Label(
            header,
            text="◇ PROMOTE & RELEASE ◇",
            font=("Consolas", 14, "bold"),
            fg=COLORS["accent_cyan"],
            bg=COLORS["bg_dark"],
        )
        title.pack()
        sub = tk.Label(
            header,
            text="Cherry-pick to master · Build · Zip · Tag",
            font=("Consolas", 9),
            fg=COLORS["text_dim"],
            bg=COLORS["bg_dark"],
        )
        sub.pack(pady=(2, 0))

        # Project selector
        sel_frame = tk.Frame(main, bg=COLORS["bg_dark"])
        sel_frame.pack(fill=tk.X, pady=8)
        tk.Label(sel_frame, text="Mod project:", fg=COLORS["text"], bg=COLORS["bg_dark"]).pack(side=tk.LEFT, padx=(0, 8))
        self.project_var = tk.StringVar(value=self.projects[0] if self.projects else "")
        self.project_combo = ttk.Combobox(
            sel_frame,
            textvariable=self.project_var,
            values=self.projects,
            state="readonly",
            width=32,
        )
        self.project_combo.pack(side=tk.LEFT)
        if self.projects:
            self.project_combo.current(0)
        self._refresh_version_label(sel_frame)

        # Action buttons
        btn_frame = tk.Frame(main, bg=COLORS["bg_dark"])
        btn_frame.pack(fill=tk.X, pady=12)
        self.promote_btn = tk.Button(
            btn_frame,
            text="  ◆  PROMOTE to Master  ",
            font=("Consolas", 10, "bold"),
            fg=COLORS["bg_dark"],
            bg=COLORS["accent_green"],
            activebackground=COLORS["accent_green"],
            activeforeground=COLORS["bg_dark"],
            relief=tk.FLAT,
            padx=12,
            pady=6,
            cursor="hand2",
            command=self._on_promote,
        )
        self.promote_btn.pack(side=tk.LEFT, padx=(0, 8))
        self.release_btn = tk.Button(
            btn_frame,
            text="  ◆  RELEASE (build & tag)  ",
            font=("Consolas", 10, "bold"),
            fg=COLORS["bg_dark"],
            bg=COLORS["accent_orange"],
            activebackground=COLORS["accent_orange"],
            activeforeground=COLORS["bg_dark"],
            relief=tk.FLAT,
            padx=12,
            pady=6,
            cursor="hand2",
            command=self._on_release,
        )
        self.release_btn.pack(side=tk.LEFT)

        # Info
        info = tk.Label(
            main,
            text="Promote: dev → master (cherry-pick) · Release: build, zip, commit, push, tag",
            font=("Consolas", 8),
            fg=COLORS["text_dim"],
            bg=COLORS["bg_dark"],
        )
        info.pack(pady=(0, 8))

        tk.Frame(main, height=1, bg=COLORS["accent_purple"]).pack(fill=tk.X, pady=4)
        tk.Label(main, text="Output:", fg=COLORS["text_dim"], bg=COLORS["bg_dark"]).pack(anchor=tk.W)
        log_container = tk.Frame(main, bg=COLORS["bg_panel"], relief=tk.FLAT)
        log_container.pack(fill=tk.BOTH, expand=True, pady=4)
        self.log_text = tk.Text(
            log_container,
            wrap=tk.WORD,
            height=12,
            font=("Consolas", 9),
            bg=COLORS["bg_panel"],
            fg=COLORS["text"],
            insertbackground=COLORS["accent_cyan"],
            relief=tk.FLAT,
            padx=8,
            pady=8,
        )
        sb = tk.Scrollbar(log_container, command=self.log_text.yview, bg=COLORS["bg_panel"])
        self.log_text.configure(yscrollcommand=sb.set)
        self.log_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        sb.pack(side=tk.RIGHT, fill=tk.Y)

        self.project_combo.bind("<<ComboboxSelected>>", lambda _: self._refresh_version_label(sel_frame))

        self._log(f"Solution root: {self.solution_root}")
        self._log(f"Projects: {len(self.projects)} mods")
        if not self.projects:
            self._log("WARNING: No mod projects found (need .csproj + mod_info.yaml)")

    def _refresh_version_label(self, parent: tk.Frame = None):
        proj = self.project_var.get()
        ver = get_mod_version(self.solution_root, proj) if proj else "—"
        if hasattr(self, "_ver_label"):
            self._ver_label.config(text=f"v{ver}")
        elif parent:
            self._ver_label = tk.Label(parent, text=f"v{ver}", fg=COLORS["accent_pink"], bg=COLORS["bg_dark"])
            self._ver_label.pack(side=tk.LEFT, padx=(8, 0))

    def _log(self, msg: str):
        self.log_text.configure(state=tk.NORMAL)
        self.log_text.insert(tk.END, msg + "\n")
        self.log_text.see(tk.END)
        self.log_text.configure(state=tk.DISABLED)
        self.root.update_idletasks()

    def _queue_log(self, msg: str):
        self._log_queue.put(msg)

    def _drain_log(self):
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
                    self._log(msg)
        except queue.Empty:
            pass
        if self._work_done is not None:
            done = self._work_done
            self._work_done = None
            self.promote_btn.configure(state=tk.NORMAL)
            self.release_btn.configure(state=tk.NORMAL)
            if done:
                messagebox.showinfo("Done", "Operation completed successfully.")
            else:
                messagebox.showerror("Error", "Operation failed. Check the output log.")
            return
        self.root.after(80, self._drain_log)

    def _run_promote(self):
        proj = self.project_var.get()
        if not proj:
            self._queue_log("No project selected.")
            self._queue_log("__DONE_FALSE__")
            return
        ok = run_powershell_script(
            "promote-mod.ps1", self.solution_root, self._queue_log, project=proj, force=True
        )
        self._queue_log("__DONE_TRUE__" if ok else "__DONE_FALSE__")

    def _on_promote(self):
        proj = self.project_var.get()
        if not proj:
            messagebox.showerror("Error", "Select a mod project first.")
            return
        branch = self._get_branch()
        if branch != "development":
            messagebox.showerror("Branch Check", f"Promote must run from 'development'. Current: {branch}")
            return
        if not messagebox.askyesno("Promote", f"Promote {proj} from development to master?"):
            return
        self.promote_btn.configure(state=tk.DISABLED)
        self.release_btn.configure(state=tk.DISABLED)
        self._log("-" * 50)
        self._log(f"PROMOTE: {proj}")
        threading.Thread(target=self._run_promote, daemon=True).start()
        self._drain_log()

    def _run_release(self):
        proj = self.project_var.get()
        ok = run_powershell_script(
            "release-mod.ps1", self.solution_root, self._queue_log, project=proj, force=True
        )
        self._queue_log("__DONE_TRUE__" if ok else "__DONE_FALSE__")

    def _on_release(self):
        proj = self.project_var.get()
        if not proj:
            messagebox.showerror("Error", "Select a mod project first.")
            return
        branch = self._get_branch()
        if branch != "master":
            messagebox.showerror("Branch Check", f"Release must run from 'master'. Current: {branch}")
            return
        ver = get_mod_version(self.solution_root, proj)
        if not messagebox.askyesno("Release", f"Release {proj} v{ver}? (build, zip, commit, push, tag)"):
            return
        self.promote_btn.configure(state=tk.DISABLED)
        self.release_btn.configure(state=tk.DISABLED)
        self._log("-" * 50)
        self._log(f"RELEASE: {proj} v{ver}")
        threading.Thread(target=self._run_release, daemon=True).start()
        self._drain_log()

    def _get_branch(self) -> str:
        try:
            r = subprocess.run(
                ["git", "rev-parse", "--abbrev-ref", "HEAD"],
                cwd=self.solution_root,
                capture_output=True,
                text=True,
                timeout=5,
            )
            return (r.stdout or "").strip() or "unknown"
        except Exception:
            return "unknown"

    def run(self):
        self.root.mainloop()


def main():
    app = PromoteReleaseApp()
    app.run()


if __name__ == "__main__":
    main()
