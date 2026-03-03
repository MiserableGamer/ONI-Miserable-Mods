# Git hooks

These hooks run only when installed. **Run once** (from repo root):

```powershell
.\scripts\setup-solution-hooks.ps1
```

Or manually:

```bash
git config core.hooksPath .githooks
```

Then, after switching to master, run `.\scripts\regenerate-solution.ps1 -OnlyIfMaster` if the solution was wrong, and close/reopen the solution in your IDE.

- **post-checkout**: After switching branches, if you're on `master`, regenerates `ONIMiserableMods.slnx` from the projects on disk so the solution always matches the branch. Close and reopen the solution in your IDE after switching to master if the project list was wrong.
- **post-merge**: After a merge (e.g. when the GUI promotes a project from development to master by merging), regenerates the solution on master so newly promoted projects are included automatically. Close and reopen the solution to see the new project.
