# Creating Git Tags in Visual Studio

Visual Studio has built-in Git support that makes creating tags easy through the UI.

## Method 1: Using Team Explorer (Visual Studio 2019 and earlier)

### Creating a Tag

1. **Open Team Explorer**
   - Go to **View** → **Team Explorer** (or press `Ctrl+\, Ctrl+M`)

2. **Navigate to Branches**
   - In Team Explorer, click on **Branches**
   - Right-click on the branch you want to tag (usually `main` or `master`)
   - Select **New Tag...**

3. **Enter Tag Information**
   - **Tag name**: Enter your tag (e.g., `BonbonTreeBoost-v1.0.0`)
   - **Tag message**: Optional description (e.g., "BonbonTreeBoost version 1.0.0")
   - Click **Create Tag**

4. **Push the Tag**
   - After creating the tag, you'll see it in the branches list
   - Right-click on the tag
   - Select **Push Tag...**
   - Choose your remote (usually `origin`)
   - Click **Push**

## Method 2: Using Git Changes Window (Visual Studio 2022)

### Creating a Tag

1. **Open Git Changes**
   - Go to **View** → **Git Changes** (or press `Ctrl+0, G`)
   - Or click the **Git** icon in the status bar at the bottom

2. **Open Command Palette**
   - Click the **...** (three dots) menu in the Git Changes window
   - Select **View Command History** or use **View** → **Other Windows** → **Git Command Line**

3. **Create Tag via Command**
   - In the Git Command Line window, type:
     ```
     git tag BonbonTreeBoost-v1.0.0
     ```
   - Press Enter

4. **Push the Tag**
   - In the same Git Command Line window, type:
     ```
     git push origin BonbonTreeBoost-v1.0.0
     ```
   - Press Enter

## Method 3: Using Git Repository Window (Visual Studio 2022)

### Creating a Tag

1. **Open Git Repository**
   - Go to **Git** → **Manage Branches** (or press `Ctrl+0, B`)
   - Or use **View** → **Git Repository**

2. **View Tags**
   - In the Git Repository window, expand **Tags**
   - You'll see existing tags (if any)

3. **Create New Tag**
   - Right-click in the Tags section
   - Select **New Tag...**
   - Enter tag name: `BonbonTreeBoost-v1.0.0`
   - Enter optional message
   - Click **Create**

4. **Push the Tag**
   - Right-click on the newly created tag
   - Select **Push Tag...**
   - Choose remote: `origin`
   - Click **Push**

## Method 4: Using Visual Studio's Integrated Terminal

### Creating a Tag

1. **Open Terminal**
   - Go to **View** → **Terminal** (or press `Ctrl+` ` backtick)
   - Or use **Tools** → **Command Line** → **Developer Command Prompt**

2. **Create and Push Tag**
   ```bash
   git tag BonbonTreeBoost-v1.0.0
   git push origin BonbonTreeBoost-v1.0.0
   ```

## Quick Steps Summary (Recommended for VS 2022)

**Easiest method in Visual Studio 2022:**

1. Press `Ctrl+` ` to open terminal
2. Type: `git tag BonbonTreeBoost-v1.0.0`
3. Press Enter
4. Type: `git push origin BonbonTreeBoost-v1.0.0`
5. Press Enter
6. Done! GitHub Actions will create the release automatically

## Viewing Tags in Visual Studio

### In Git Repository Window
- Go to **Git** → **Manage Branches**
- Expand **Tags** section
- See all your tags listed

### In Team Explorer
- Go to **View** → **Team Explorer**
- Click **Branches**
- Tags appear in the branches list (usually at the bottom)

## Deleting Tags (if you make a mistake)

### Via Terminal (Easiest)
1. Open terminal (`Ctrl+` `)
2. Delete local tag:
   ```
   git tag -d BonbonTreeBoost-v1.0.0
   ```
3. Delete remote tag:
   ```
   git push origin --delete BonbonTreeBoost-v1.0.0
   ```

## Visual Guide for VS 2022

### Step-by-Step with Screenshots Location

1. **Open Git Changes**
   - Bottom status bar → Click **Git** icon
   - Or: **View** → **Git Changes**

2. **Open Terminal**
   - **View** → **Terminal**
   - Or press: `Ctrl+` ` (backtick key, usually above Tab)

3. **Create Tag**
   ```
   git tag BonbonTreeBoost-v1.0.0
   ```

4. **Push Tag**
   ```
   git push origin BonbonTreeBoost-v1.0.0
   ```

5. **Verify**
   - Go to GitHub website
   - Check **Actions** tab - workflow should be running
   - Check **Releases** tab - release will appear when workflow completes

## Tagging Commits with Multiple Projects

**Yes!** You can tag a commit that changed multiple projects. The workflow will only build and release the mod specified in the tag name.

### Example

If one commit updated both `BonbonTreeBoost` and `CopyMaterials`:

```bash
# Tag the current commit for BonbonTreeBoost
git tag BonbonTreeBoost-v1.0.0 HEAD
git push origin BonbonTreeBoost-v1.0.0
# → Releases only BonbonTreeBoost

# Tag the same commit for CopyMaterials (later)
git tag CopyMaterials-v2.0.0 HEAD
git push origin CopyMaterials-v2.0.0
# → Releases only CopyMaterials
```

**Key point:** The tag name determines which mod gets released, not which files were changed in the commit.

## Tagging an Already-Pushed Commit

**Yes!** You can create a tag on any commit, even if it was pushed days or weeks ago.

### Method 1: Tag Current Commit (Easiest)

If you want to tag the latest commit on your current branch:

```bash
git tag BonbonTreeBoost-v1.0.0
git push origin BonbonTreeBoost-v1.0.0
```

### Method 2: Tag a Specific Commit by Hash

If you want to tag a specific commit from the past:

1. **Find the commit hash**
   - In Visual Studio: **View** → **Git Repository** → **Commits**
   - Or use terminal: `git log --oneline`
   - Copy the commit hash (first 7 characters, e.g., `a1b2c3d`)

2. **Create tag on that commit**
   ```bash
   git tag BonbonTreeBoost-v1.0.0 a1b2c3d
   git push origin BonbonTreeBoost-v1.0.0
   ```

### Method 3: Tag a Specific Commit via Visual Studio

1. **View commit history**
   - **View** → **Git Repository**
   - Click **Commits** tab
   - Find the commit you want to tag

2. **Create tag on that commit**
   - Right-click on the commit
   - Select **Create Tag...**
   - Enter tag name: `BonbonTreeBoost-v1.0.0`
   - Click **Create**

3. **Push the tag**
   - Right-click on the tag (in Tags section)
   - Select **Push Tag...**
   - Choose `origin` and click **Push**

### Example: Tagging Last Week's Commit

```bash
# 1. See recent commits
git log --oneline

# Output might look like:
# a1b2c3d (HEAD -> main) Latest commit
# d4e5f6g Commit from yesterday
# h7i8j9k Commit from last week  ← You want to tag this one

# 2. Tag that specific commit
git tag BonbonTreeBoost-v1.0.0 h7i8j9k

# 3. Push the tag
git push origin BonbonTreeBoost-v1.0.0
```

### Important Notes

- **Tags point to specific commits** - They don't move when you make new commits
- **You can tag any commit** - Past, present, or future (if you know the hash)
- **Multiple tags on same commit** - You can have multiple tags pointing to the same commit
- **Tagging doesn't change code** - It just creates a label/reference

## Tips

- **Tag names are case-sensitive**: `BonbonTreeBoost-v1.0.0` is different from `bonbontreeboost-v1.0.0`
- **Use the exact mod name**: Must match one of the supported mods
- **Check before pushing**: Use `git tag` to see all tags before creating a new one
- **Tag format**: `ModName-vVersion` (e.g., `BonbonTreeBoost-v1.0.0`)
- **You can tag old commits**: Tags can be created on any commit, not just the latest one

## Troubleshooting

### "Tag already exists" error
- The tag name is already used
- Either delete the old tag or use a different version number

### "Tag not found" after creating
- Make sure you pushed the tag: `git push origin TagName`
- Tags are local until you push them

### Workflow not running
- Check that the tag format is correct (ModName-vVersion)
- Check GitHub Actions tab for any error messages
- Verify the tag was pushed to the remote repository

