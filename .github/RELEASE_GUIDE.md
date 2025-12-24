# GitHub Release Guide

## Overview

This repository uses **Git tags** to trigger automatic releases. When you push a tag to GitHub, it automatically builds and releases **one specific mod**.

## What is a Git Tag?

A Git tag is like a bookmark that marks a specific point in your code history. Think of it as a "snapshot" of your code at a particular moment.

## How Releases Work

### Normal Code Updates (No Release)

When you push regular code changes:
```bash
git add .
git commit -m "Updated BonbonTreeBoost"
git push origin main
```
**Result:** Code is updated on GitHub, but **NO release is created**.

### Creating a Release

To create a release, you need to create and push a **tag**:

```bash
# Step 1: Make sure your code is committed and pushed
git add .
git commit -m "Ready to release BonbonTreeBoost v1.0.0"
git push origin main

# Step 2: Create a tag with the mod name and version
git tag BonbonTreeBoost-v1.0.0

# Step 3: Push the tag to GitHub
git push origin BonbonTreeBoost-v1.0.0
```

**Result:** GitHub Actions automatically:
1. Detects the tag push
2. Extracts "BonbonTreeBoost" as the mod name
3. Extracts "1.0.0" as the version
4. Builds only the BonbonTreeBoost mod
5. Creates a GitHub release with the mod's zip file

## Tag Format

The tag **must** include:
1. **The mod name** (which mod to release)
2. **The version** (what version number)

### Supported Formats

All of these work:

```
BonbonTreeBoost-v1.0.0          ← Recommended (easiest to read)
vBonbonTreeBoost-1.0.0         ← Also works
BonbonTreeBoost-1.0.0          ← Also works (no 'v' prefix)
```

### Examples for Different Mods

```bash
# Release BonbonTreeBoost version 1.0.0
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0

# Release CopyMaterials version 2.1.3
git tag CopyMaterials-v2.1.3
git push origin CopyMaterials-v2.1.3

# Release EmptyStorage version 1.5.0
git tag EmptyStorage-v1.5.0
git push origin EmptyStorage-v1.5.0

# Release MorningExercise version 1.0.0
git tag MorningExercise-v1.0.0
git push origin MorningExercise-v1.0.0
```

### What WON'T Work

These tags will be **rejected** (no release created):

```
v1.0.0              ← Missing mod name (which mod?)
1.0.0               ← Missing mod name
release-v1.0.0      ← Not a valid mod name
```

## Step-by-Step Example

Let's say you want to release **BonbonTreeBoost version 1.2.0**:

### 1. Make sure your code is ready
```bash
# Check what files changed
git status

# Add and commit your changes
git add .
git commit -m "Added new features to BonbonTreeBoost"
git push origin main
```

### 2. Create the tag
```bash
# Create a tag with format: ModName-vVersion
git tag BonbonTreeBoost-v1.2.0
```

### 3. Push the tag
```bash
# Push just the tag (not your code)
git push origin BonbonTreeBoost-v1.2.0
```

### 4. Wait for GitHub Actions
- Go to the **Actions** tab on GitHub
- You'll see a workflow running called "Create Release"
- It will:
  - Build BonbonTreeBoost
  - Package it into a zip file
  - Create a GitHub release
  - Attach the zip file to the release

### 5. Check the release
- Go to the **Releases** page on GitHub
- You'll see a new release: "BonbonTreeBoost 1.2.0"
- Download the zip file from there

## Releasing Multiple Mods

Each mod gets its **own separate release**. You can release them at different times:

```bash
# Release BonbonTreeBoost today
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0

# Release CopyMaterials next week
git tag CopyMaterials-v2.0.0
git push origin CopyMaterials-v2.0.0

# Release EmptyStorage next month
git tag EmptyStorage-v1.5.0
git push origin EmptyStorage-v1.5.0
```

Each tag creates a **separate release** - they don't interfere with each other.

## Version Numbers

Use semantic versioning:
- **Major.Minor.Patch** (e.g., 1.0.0)
- **Major.Minor.Patch.Build** (e.g., 1.0.0.5) - also works

Examples:
- `1.0.0` - First release
- `1.1.0` - Added new features
- `1.0.1` - Bug fix
- `2.0.0` - Major update

## Viewing Tags

To see all your tags:
```bash
git tag
```

To see tags for a specific mod:
```bash
git tag | grep BonbonTreeBoost
```

## Deleting Tags (if you make a mistake)

If you create a tag by mistake:

```bash
# Delete local tag
git tag -d BonbonTreeBoost-v1.0.0

# Delete remote tag
git push origin --delete BonbonTreeBoost-v1.0.0
```

## Tagging Commits with Multiple Projects

**Yes!** You can tag a commit that changed multiple projects/mod. The workflow will only build and release the mod specified in the tag name.

### Example Scenario

Let's say you made one commit that updated both `BonbonTreeBoost` and `CopyMaterials`:

```bash
# Your commit changed files in both projects:
# - BonbonTreeBoost/BonbonTreeBoost.cs
# - CopyMaterials/CopyMaterialsMod.cs
git commit -m "Updated both mods"
git push origin main
```

**You can still create separate releases:**

```bash
# Release only BonbonTreeBoost from this commit
git tag BonbonTreeBoost-v1.0.0 HEAD
git push origin BonbonTreeBoost-v1.0.0
# → Creates release with BonbonTreeBoost (as it was in this commit)

# Later, release CopyMaterials from the same commit
git tag CopyMaterials-v2.0.0 HEAD
git push origin CopyMaterials-v2.0.0
# → Creates release with CopyMaterials (as it was in this commit)
```

### How It Works

- **Tags point to commits** - Not to specific files or projects
- **Workflow reads the tag name** - Extracts which mod to build (e.g., "BonbonTreeBoost")
- **Only that mod gets built** - Even if the commit changed multiple mods
- **Release contains only that mod** - Other mods in the commit are ignored

### Real-World Example

You push a commit that updates 3 mods:
- BonbonTreeBoost (ready to release)
- CopyMaterials (needs more testing)
- EmptyStorage (work in progress)

You can:
1. Tag and release BonbonTreeBoost now: `git tag BonbonTreeBoost-v1.0.0`
2. Tag and release CopyMaterials later: `git tag CopyMaterials-v2.0.0` (same commit or different)
3. Don't release EmptyStorage until it's ready

Each tag creates a separate release for only that specific mod.

## Tagging Old Commits

**Yes, you can tag commits that were already pushed!** This is useful if you want to release code from a previous commit.

### Tag the Latest Commit
```bash
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0
```

### Tag a Specific Commit from the Past

1. **Find the commit hash:**
   ```bash
   git log --oneline
   ```
   This shows: `a1b2c3d Commit message`

2. **Tag that specific commit:**
   ```bash
   git tag BonbonTreeBoost-v1.0.0 a1b2c3d
   git push origin BonbonTreeBoost-v1.0.0
   ```

**Example:** If you pushed code last week and want to release it now:
```bash
# See commits
git log --oneline
# a1b2c3d Latest commit
# d4e5f6g Last week's commit  ← Tag this one

# Tag last week's commit
git tag BonbonTreeBoost-v1.0.0 d4e5f6g
git push origin BonbonTreeBoost-v1.0.0
```

The release will be created based on that specific commit, not the latest code.

## Summary

- **Regular commits** = Code updates, no release
- **Tag with mod name + version** = Automatic release for that mod
- **Each mod** = Separate releases, independent of each other
- **Tag format** = `ModName-v1.0.0` (mod name, dash, v, version)
- **Can tag old commits** = You can create tags on any commit, not just the latest

## How to Run the Workflow

The workflow runs **automatically** when you push a tag. See [HOW_TO_RUN_WORKFLOW.md](HOW_TO_RUN_WORKFLOW.md) for detailed instructions.

**Quick steps:**
1. Commit and push your code
2. Create tag: `git tag ModName-v1.0.0`
3. Push tag: `git push origin ModName-v1.0.0`
4. Workflow runs automatically on GitHub
5. Check **Actions** tab for progress
6. Check **Releases** tab for the result

## Quick Reference

### Using Command Line

```bash
# 1. Commit your changes
git add .
git commit -m "Your message"
git push origin main

# 2. Create and push tag (replace ModName and version)
git tag ModName-v1.0.0
git push origin ModName-v1.0.0

# Done! Release will be created automatically.
# Check GitHub Actions tab to see it running.
```

### Using Visual Studio

See [VISUAL_STUDIO_TAGS.md](VISUAL_STUDIO_TAGS.md) for detailed instructions on creating tags through Visual Studio's UI.

**Quick method in Visual Studio:**
1. Press `Ctrl+` ` to open terminal
2. Type: `git tag ModName-v1.0.0`
3. Press Enter
4. Type: `git push origin ModName-v1.0.0`
5. Press Enter
6. Go to GitHub → Actions tab to see workflow running

