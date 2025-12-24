# How to Run the Release Workflow

The GitHub Actions workflow runs **automatically** when you push a tag. Here's how to trigger it:

## Quick Steps

### 1. Make sure your code is committed and pushed

```bash
git add .
git commit -m "Your changes"
git push origin main
```

### 2. Create and push a tag

The tag format must be: `ModName-vVersion`

```bash
# Example: Release BonbonTreeBoost version 1.0.0
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0
```

### 3. Workflow runs automatically

- Go to GitHub → **Actions** tab
- You'll see "Create Release" workflow running
- It will:
  1. Build the mod
  2. Package it into a zip file
  3. Create a GitHub release
  4. Attach the zip file

### 4. Check the release

- Go to GitHub → **Releases** tab
- Your release will appear when the workflow completes
- Download the zip file from there

## Step-by-Step Example

Let's say you want to release **EmptyStorage version 1.5.0**:

### In Visual Studio Terminal

1. **Open terminal**: Press `Ctrl+` ` (backtick)

2. **Check your changes are committed**:
   ```bash
   git status
   ```
   If there are uncommitted changes, commit them:
   ```bash
   git add .
   git commit -m "Ready to release EmptyStorage v1.5.0"
   git push origin main
   ```

3. **Create the tag**:
   ```bash
   git tag EmptyStorage-v1.5.0
   ```

4. **Push the tag**:
   ```bash
   git push origin EmptyStorage-v1.5.0
   ```

5. **Done!** The workflow will start automatically.

### Check Workflow Progress

1. Go to your GitHub repository
2. Click the **Actions** tab
3. You'll see "Create Release" workflow running
4. Click on it to see the build progress
5. Wait for it to complete (usually 2-5 minutes)

### View the Release

1. Go to **Releases** tab on GitHub
2. You'll see: "EmptyStorage 1.5.0"
3. Download the zip file

## Troubleshooting

### Workflow not starting?

- **Check tag format**: Must be `ModName-vVersion` (e.g., `BonbonTreeBoost-v1.0.0`)
- **Check tag was pushed**: Use `git ls-remote --tags origin` to see remote tags
- **Check Actions tab**: Go to GitHub → Actions to see if workflow is queued/running

### Workflow failed?

- Click on the failed workflow run
- Check the error messages in the logs
- Common issues:
  - Mod name not recognized (check supported mods list)
  - Build errors (check the "Build mod" step)
  - Missing files (check the "Package mod" step)

### Tag already exists?

If you need to recreate a release:

```bash
# Delete local tag
git tag -d BonbonTreeBoost-v1.0.0

# Delete remote tag
git push origin --delete BonbonTreeBoost-v1.0.0

# Create new tag
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0
```

## Supported Mod Names

Make sure your tag uses one of these exact mod names:
- `BonbonTreeBoost`
- `CopyMaterials`
- `DebugFogOfWar`
- `EmptyStorage`
- `LongerArms`
- `MorningExercise`
- `ThresholdFixed`

## Tag Format Examples

✅ **Correct:**
- `BonbonTreeBoost-v1.0.0`
- `CopyMaterials-v2.1.3`
- `EmptyStorage-v1.5.0`

❌ **Wrong:**
- `v1.0.0` (missing mod name)
- `bonbontreeboost-v1.0.0` (wrong case - must match exactly)
- `BonbonTreeBoost-1.0.0` (missing 'v' - this might work but not recommended)

## Summary

**To run the workflow:**
1. Commit and push your code
2. Create tag: `git tag ModName-vVersion`
3. Push tag: `git push origin ModName-vVersion`
4. Workflow runs automatically
5. Check Actions tab for progress
6. Check Releases tab for the result

That's it! No manual triggering needed - just push a tag and the workflow runs automatically.

