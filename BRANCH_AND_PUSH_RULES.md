# Branch and push rules

## Branches

| Branch        | Purpose                    | Push to remote? |
|--------------|----------------------------|------------------|
| **development** | Leading branch, all projects + support | **No** (local only) |
| **master**      | Released mods only (promoted projects) | **Yes** |

## Development (local only)

- Contains **all projects** and necessary support files.
- **DEVELOPMENT_BRANCH.txt** – present only on development; branch flag for agents.
- **project-versions.yaml** – present only on development; source of truth for project status (e.g. PROMOTED).
- Never push this branch.

## Master (pushed)

- Contains **only promoted projects** (status PROMOTED in project-versions.yaml on dev) and necessary support files.
- **Releases folder** – `release/` holds built projects and zips for Workshop and GitHub releases. Only zips are tracked (`release/*.zip`); temp subdirs are ignored.
- **Never on master (and never pushed):**
  - BlankProject
  - BlankProjectSteamTest
  - SteamWorkshopUploader
  - `.cursor` and related
  - Scripts and file-management files (e.g. `all-mods-version.txt`, `scripts/`, `*.ps1`, `*.bat`)

## Scripts and tooling

- **D:\mcp\oni-serena\scripts** holds a Python GUI and supporting scripts (3-2-1 backups).
- This tooling controls **all** movement: Development → Master → GitHub releases.
- The repo does not track `scripts/` or `*.ps1`/`*.bat` (see `.gitignore`). Scripts reference the solution/repo path when needed.

## Workshop

- **Auto-upload** from the scripts is planned but not working yet.
- **For now:** uploads are done manually via the Klei Uploader.
- SteamWorkshopUploader (and related) are not in the repo and are not pushed.
