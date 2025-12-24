# Troubleshooting: Workflow Not Running

If the workflow doesn't start when you push a tag, check these common issues:

## 1. Workflow File Not Committed/Pushed

**Most common issue!** The workflow file must be committed and pushed to GitHub.

### Check if workflow file is committed:

```bash
# Check if .github/workflows/release.yml is tracked by git
git ls-files .github/workflows/release.yml
```

If it returns nothing, the file isn't committed.

### Fix: Commit and push the workflow file

```bash
# Add the workflow file
git add .github/workflows/release.yml

# Commit it
git commit -m "Add GitHub Actions release workflow"

# Push to GitHub
git push origin main
```

## 2. Check GitHub Actions is Enabled

1. Go to your GitHub repository
2. Click **Settings** tab
3. Click **Actions** in the left sidebar
4. Under **General** → **Actions permissions**, make sure:
   - ✅ "Allow all actions and reusable workflows" is selected
   - ✅ "Allow GitHub Actions to create and approve pull requests" is enabled (if needed)

## 3. Check Workflow File Location

The workflow file **must** be at:
```
.github/workflows/release.yml
```

**Not:**
- `.github/release.yml` ❌
- `workflows/release.yml` ❌
- `.github/workflow/release.yml` ❌ (note: "workflow" vs "workflows")

## 4. Check Workflow File Syntax

The workflow file must be valid YAML. Common issues:

- **Indentation**: Must use spaces, not tabs
- **File extension**: Must be `.yml` or `.yaml`
- **Encoding**: Must be UTF-8

### Validate YAML online:
- Use https://www.yamllint.com/ to check for syntax errors

## 5. Check Tag Format

The tag must match one of these formats:
- `ModName-v1.0.0` ✅
- `vModName-1.0.0` ✅
- `ModName-1.0.0` ✅

**Supported mod names** (case-sensitive):
- BonbonTreeBoost
- CopyMaterials
- DebugFogOfWar
- EmptyStorage
- LongerArms
- MorningExercise
- ThresholdFixed

## 6. Verify Tag Was Pushed

```bash
# Check remote tags
git ls-remote --tags origin

# Should show your tag, e.g.:
# abc123... refs/tags/BonbonTreeBoost-v1.0.0
```

If the tag doesn't appear, it wasn't pushed correctly.

## 7. Check Actions Tab

1. Go to GitHub repository
2. Click **Actions** tab
3. Look for:
   - **"Create Release"** workflow in the list
   - Any failed or skipped workflow runs
   - Check if workflows are enabled (you might see a banner)

## 8. Check Repository Settings

1. Go to **Settings** → **Actions**
2. Check **Workflow permissions**:
   - Should be "Read and write permissions"
   - Or at least "Read repository contents and packages permissions"

## 9. Manual Workflow Trigger (Testing)

To test if workflows work at all, you can manually trigger a workflow run:

1. Go to **Actions** tab
2. Click **"Create Release"** workflow
3. Click **"Run workflow"** button (if available)
4. This will help determine if the issue is with the trigger or the workflow itself

## 10. Check for Workflow Errors

If the workflow appears but doesn't run:

1. Go to **Actions** tab
2. Click on the workflow
3. Check for:
   - Red X (failed)
   - Yellow circle (in progress)
   - Gray circle (skipped/cancelled)
4. Click on it to see error messages

## Quick Diagnostic Steps

Run these commands to diagnose:

```bash
# 1. Check if workflow file exists locally
ls -la .github/workflows/release.yml

# 2. Check if it's tracked by git
git ls-files .github/workflows/release.yml

# 3. Check git status
git status

# 4. Check if file is in .gitignore
git check-ignore -v .github/workflows/release.yml
```

## Common Solutions

### Solution 1: Commit and Push Workflow File

```bash
git add .github/workflows/release.yml
git commit -m "Add release workflow"
git push origin main
```

Then push your tag again:
```bash
git push origin BonbonTreeBoost-v1.0.0
```

### Solution 2: Re-push Tag After Workflow is Committed

If you created the tag before committing the workflow:

```bash
# Delete the tag locally and remotely
git tag -d BonbonTreeBoost-v1.0.0
git push origin --delete BonbonTreeBoost-v1.0.0

# Make sure workflow is committed and pushed
git add .github/workflows/release.yml
git commit -m "Add release workflow"
git push origin main

# Recreate and push the tag
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0
```

### Solution 3: Check .gitignore

Make sure `.github/workflows/` is NOT in `.gitignore`:

```bash
# Check if ignored
git check-ignore -v .github/workflows/release.yml

# If it shows the file, remove that line from .gitignore
```

## Still Not Working?

If none of these work:

1. **Check GitHub status**: https://www.githubstatus.com/
2. **Check repository permissions**: Make sure you have admin access
3. **Try a simple test workflow**: Create a minimal workflow to test if Actions work at all
4. **Check GitHub logs**: Go to Settings → Actions → Runners to see if there are any errors

## Test Workflow (Minimal)

Create a simple test to verify Actions work:

```yaml
# .github/workflows/test.yml
name: Test
on:
  push:
    tags:
      - '*'
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - run: echo "Workflow triggered!"
```

If this doesn't run, the issue is with GitHub Actions setup, not the release workflow.

