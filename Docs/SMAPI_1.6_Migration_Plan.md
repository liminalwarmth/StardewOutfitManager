# Stardew Outfit Manager - SMAPI 4.0 / SDV 1.6 Migration Plan

## Overview
This mod was last updated for SMAPI 3.x / Stardew Valley 1.5.x. This document outlines all changes needed for compatibility with SMAPI 4.0+ and Stardew Valley 1.6.15.

---

## Research Summary

### Key Documentation Sources
- [Modding:Migrate to Stardew Valley 1.6](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
- [Modding:Migrate to SMAPI 4.0](https://wiki.stardewvalley.net/Modding:Migrate_to_SMAPI_4.0)
- [Modding:Migrate to Stardew Valley 1.6.16](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6.16) - Future version prep
- [Fashion Sense GitHub](https://github.com/Floogen/FashionSense) - Reference implementation for FarmerRenderer patches

### Decompiled Game Source (for API research)
- **SDV 1.6 Decompiled**: [Dannode36/StardewValleyDecompiled](https://github.com/Dannode36/StardewValleyDecompiled)
  - ShopMenu.cs: `Stardew Valley/StardewValley.Menus/ShopMenu.cs`
  - FarmerRenderer.cs: `Stardew Valley/StardewValley/FarmerRenderer.cs`
- **SDV 1.5.6 Decompiled**: [WeDias/StardewValley](https://github.com/WeDias/StardewValley) - For comparing changes

### Reference Mod Analysis: Fashion Sense
Fashion Sense is a popular mod that does extensive FarmerRenderer patching. Key patterns observed:
1. Uses **Harmony reverse patches** to call private methods like `executeRecolorActions`
2. Uses **SMAPI reflection helper** (`_helper.Reflection.GetField<T>()`) for accessing private fields like `baseTexture`
3. Still uses `who.IsMale` in rendering code (may still exist as computed property)

---

## Phase 0: Development Setup

### CLAUDE.md Project Rules
Create a `CLAUDE.md` file in the project root with rules for working on this codebase.

**File to create:** `CLAUDE.md` (project root)

```markdown
# Stardew Outfit Manager - Claude Code Rules

## Before Starting Any Task

1. **Research First**: Before making any code changes related to Stardew Valley or SMAPI APIs:
   - Check the [Stardew Valley 1.6 Migration Guide](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
   - Check the [SMAPI 4.0 Migration Guide](https://wiki.stardewvalley.net/Modding:Migrate_to_SMAPI_4.0)
   - Search for specific API changes if unsure about method signatures or property names
   - Look at reference mods like [Fashion Sense](https://github.com/Floogen/FashionSense) for patterns

2. **Understand the Context**: Read the relevant source files completely before making changes. Don't assume - verify.

## Completion Requirements

Before reporting that any task is complete, you MUST:

1. **Verify All Steps Completed**: Review the task requirements and confirm each step was addressed
2. **Check for Missing Items**: Explicitly list what was changed and verify nothing was skipped
3. **Test Compilation**: Run `dotnet build` to verify the code compiles without errors
4. **Review TODOs**: Check if any new TODOs were introduced or if existing ones were addressed
5. **Update Documentation**: If behavior changed, update relevant docs or comments

## Project-Specific Notes

- This mod targets **Stardew Valley 1.6.x** with **SMAPI 4.x**
- Uses **Harmony** for patching game internals
- Key APIs: FarmerRenderer (custom drawing), StorageFurniture (dresser), IClickableMenu (UI)
- Deploy script: `./deploy.sh` builds and copies to Mods folder for testing

## Testing Workflow

1. Make changes
2. Run `./deploy.sh` to build and deploy
3. Launch Stardew Valley
4. Test at a dresser (farmhouse has one by default)
5. Check SMAPI console for errors
```

---

### Deploy Script
Create a shell script to build and deploy the mod to your Stardew Valley Mods folder for testing.

**File to create:** `deploy.sh` (project root)

```bash
#!/bin/bash
# Build and deploy StardewOutfitManager to Stardew Valley Mods folder

set -e  # Exit on error

MOD_NAME="StardewOutfitManager"
MODS_DIR="$HOME/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS/Mods"
TARGET_DIR="$MODS_DIR/$MOD_NAME"

echo "Building $MOD_NAME..."
dotnet build -c Release

echo "Deploying to $TARGET_DIR..."

# Remove old version if exists
if [ -d "$TARGET_DIR" ]; then
    rm -rf "$TARGET_DIR"
fi

# Create mod directory
mkdir -p "$TARGET_DIR"

# Copy built files
cp -r bin/Release/net6.0/* "$TARGET_DIR/"

# Copy assets
cp -r Assets "$TARGET_DIR/"

# Copy manifest
cp manifest.json "$TARGET_DIR/"

echo "Done! $MOD_NAME deployed to Mods folder."
echo "Launch Stardew Valley to test."
```

**Usage:**
```bash
chmod +x deploy.sh   # First time only
./deploy.sh          # Build and deploy
```

---

## Phase 1: Critical Build Fixes

These changes are required for the mod to compile and load.

### 1.1 Project Configuration
**File:** `StardewOutfitManager.csproj`

```xml
<!-- Change from -->
<TargetFramework>net5.0</TargetFramework>
<PackageReference Include="Pathoschild.Stardew.ModBuildConfig" Version="4.0.1" />

<!-- Change to -->
<TargetFramework>net6.0</TargetFramework>
<PackageReference Include="Pathoschild.Stardew.ModBuildConfig" Version="4.1.1" />
```

### 1.2 Manifest Update
**File:** `manifest.json`

```json
// Change from
"MinimumApiVersion": "3.0.0"

// Change to
"MinimumApiVersion": "4.0.0"
```

### 1.3 ShopMenu Detection Fix
**File:** `StardewOutfitManager.cs` (line 50)

The `storeContext` property was replaced by `ShopId` in SDV 1.6.

```csharp
// OLD (broken):
originalMenu.storeContext == "Dresser"

// NEW:
originalMenu.ShopId == "Dresser"
```

---

## Phase 2: API Updates

### 2.1 Gender Property Changes
**Files affected:** `Utils/ModTools.cs`

The wiki states `Farmer.isMale` was replaced by a `Gender` enum. However, Fashion Sense (updated for 1.6) still uses `IsMale`, suggesting it may exist as a computed property for backwards compatibility.

**Recommended approach:** Try `IsMale` first during testing. If compilation fails, replace with:

```csharp
// If IsMale doesn't compile, use:
who.Gender == Gender.Male
```

**Locations in ModTools.cs:**
- Line 69: `if (!who.IsMale)` - pants rect offset
- Line 89: `who.IsMale && who.FacingDirection != 2` - eye rendering
- Line 158: `who.IsMale == true ? 0 : 4` - height offset
- Lines 202, 236, 243, 250, 251, 294: Various hair style offset calculations

### 2.2 Clothing Type Enum
**File:** `Menus/WardrobeMenu.cs` (lines 118, 123)

The `clothesType` field may have changed to use an enum instead of integers.

```csharp
// Current code:
(item as Clothing).clothesType.Value == 0  // shirt
(item as Clothing).clothesType.Value == 1  // pants

// If this breaks, try:
(item as Clothing).clothesType.Value == Clothing.ClothesType.SHIRT
(item as Clothing).clothesType.Value == Clothing.ClothesType.PANTS
```

---

## Phase 3: FarmerRenderer Reflection (High Risk)

**File:** `Utils/ModTools.cs`

This is the highest-risk area. The mod uses Harmony's `AccessTools` to call private FarmerRenderer methods.

### 3.1 Current Implementation (may break)
```csharp
// Line 28 - Calling private method
AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions")
    .Invoke(who.FarmerRenderer, new object[] { who });

// Line 32 - Accessing private field
var baseTexture = AccessTools.FieldRefAccess<FarmerRenderer, Texture2D>(
    Game1.player.FarmerRenderer, "baseTexture");
```

### 3.2 Recommended Approach (from Fashion Sense)

Fashion Sense uses a more robust pattern:

**Option A: Harmony Reverse Patch** (preferred)
```csharp
// Create a stub method
[HarmonyReversePatch]
[HarmonyPatch(typeof(FarmerRenderer), "executeRecolorActions")]
internal static void ExecuteRecolorActionsReversePatch(FarmerRenderer instance, Farmer who)
{
    throw new NotImplementedException("Harmony will replace this");
}

// In Apply():
harmony.CreateReversePatcher(
    AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions", new[] { typeof(Farmer) }),
    new HarmonyMethod(typeof(YourPatchClass), nameof(ExecuteRecolorActionsReversePatch))
).Patch();

// Usage:
ExecuteRecolorActionsReversePatch(who.FarmerRenderer, who);
```

**Option B: SMAPI Reflection Helper**
```csharp
// For field access, use SMAPI's reflection helper:
var baseTexture = Helper.Reflection.GetField<Texture2D>(
    who.FarmerRenderer, "baseTexture").GetValue();
```

### 3.3 Implementation Decision Point
Try the current implementation first. If it fails:
1. If `executeRecolorActions` method signature changed → use reverse patch
2. If `baseTexture` field was renamed/removed → check SDV source for new field name

---

## Phase 3.5: NewDresserMenu.cs - HIGH RISK

**File:** `Menus/NewDresserMenu.cs`

This file is a "hacky port" of decompiled SDV v1.5.6 ShopMenu code (see comments lines 19-26). SDV 1.6 changed ShopMenu significantly.

### Key SDV 1.6 ShopMenu Changes (from decompiled source)
- `ShopId` is now **required** (throws ArgumentNullException if null)
- New `ShopCachedTheme` inner class for visual themes
- Constructors changed - all now require `shopId` parameter
- Callbacks (`onPurchase`, `onSell`) replace inheritance-based customization
- New `ItemStockInformation` parameter for stock management
- Context-specific setup moved to `setUpStoreForContext()` using `ShopId` switching

### Risk Areas
- Over 1000 lines of cloned game code from 1.5.6
- Constructor signatures changed significantly
- `storeContext` replaced by `ShopId` in game's ShopMenu

### Strategy
1. Build first and let compiler errors reveal what changed
2. The file has its own `storeContext` field (line 63) - no change needed there
3. Compare against [SDV 1.6 ShopMenu.cs](https://github.com/Dannode36/StardewValleyDecompiled/blob/main/Stardew%20Valley/StardewValley.Menus/ShopMenu.cs) if major issues arise
4. Consider if SDV 1.6's callback-based approach could simplify our implementation

---

## Phase 4: Other API Verifications

These should work but need testing:

### 4.1 Methods to Verify
- `Farmer.GetPantsIndex()` - used in ModTools.cs:65
- `Farmer.GetShirtIndex()` - used in ModTools.cs:169
- `Clothing.GetMaxPantsValue()` - used in ModTools.cs:66
- `Clothing.GetMaxShirtValue()` - used in ModTools.cs:170
- `Boots.indexInColorSheet` - used in OutfitMethods.cs:68, FavoritesMethods.cs:245
- `StorageFurniture.heldItems` - used throughout for dresser contents

### 4.2 New Helper Methods Available
SDV 1.6 added these which could simplify rendering code:
- `Farmer.GetDisplayPants()` - returns texture and sprite index
- `Farmer.GetDisplayShirt()` - returns texture and sprite index
- `Farmer.CanDyePants()` / `Farmer.CanDyeShirt()`

Consider using these in future refactoring.

---

## Files to Modify Summary

| File | Priority | Changes |
|------|----------|---------|
| `CLAUDE.md` | Critical | **NEW** - Project rules for Claude Code |
| `deploy.sh` | Critical | **NEW** - Build & deploy script |
| `StardewOutfitManager.csproj` | Critical | net5.0→net6.0, update NuGet |
| `manifest.json` | Critical | MinimumApiVersion 3.0→4.0 |
| `StardewOutfitManager.cs` | Critical | storeContext→ShopId |
| `Menus/NewDresserMenu.cs` | High | Verify ShopMenu internals against SDV 1.6 |
| `Utils/ModTools.cs` | High | IsMale checks, possibly reflection patterns |
| `Menus/WardrobeMenu.cs` | Medium | Verify clothesType enum |
| `Menus/FavoritesMenu.cs` | Medium | Verify clothesType enum, IsMale |
| `Utils/OutfitMethods.cs` | Low | Verify boots color sheet |
| `Utils/FavoritesMethods.cs` | Low | Verify boots color sheet |

### Codebase Verification Notes
We verified the following are NOT used (safe for SDV 1.6):
- Mod does NOT use `ParentSheetIndex` (uses category constants instead)
- Mod does NOT use `indexInTileSheetFemale` (gender-neutral clothing safe)
- Mod already uses SMAPI 4.0-style `helper.ModContent` API
- Mod does NOT use deprecated `IAssetLoader`/`IAssetEditor` interfaces

---

## Testing Checklist

### Build Phase
- [ ] Project compiles with .NET 6.0
- [ ] No SMAPI loading errors

### Basic Functionality
- [ ] Dresser interaction opens WardrobeMenu (not vanilla ShopMenu)
- [ ] Can cycle through hats, shirts, pants, shoes with arrow buttons
- [ ] Can cycle through hair styles and accessories
- [ ] Farmer preview renders correctly at 2x scale
- [ ] Equipment icons display correctly

### Favorites System
- [ ] Can save a new favorite outfit
- [ ] Favorites persist after game save/load
- [ ] Can load/equip a favorite outfit

### Edge Cases
- [ ] Works in local co-op (per-screen data isolation)
- [ ] Menu closes properly (mutex release, cleanup)
- [ ] Gamepad controls work

---

## Pre-existing Bugs (Post-Migration)

These existed before and should be addressed after compatibility is achieved:

1. **FavoritesMenu display bug** - "Activate the outfit slot and display the model outfit" (FavoritesMenu.cs:82)
2. **Sleeve color sticky** - Recolor reflection not working in 2x draw (FavoritesMenu.cs:95)
3. **Rings handling issue** - Unspecified problem (OutfitMethods.cs)
4. **Custom content pack support** - Hair/accessory indexes incomplete (FavoritesMethods.cs:39, 93)
5. **Performance** - Duplicate lookups in favorites matching (FavoritesMethods.cs:89)

---

## Implementation Order

### Phase 0: Development Setup
- [x] Create `CLAUDE.md` with project rules and research requirements
- [ ] Create `deploy.sh` script for build/test workflow
  - [ ] Create the script file
  - [ ] Make it executable with `chmod +x`
  - [ ] Test that it runs (even if build fails initially)

### Phase 1: Critical Build Fixes
- [ ] Update `StardewOutfitManager.csproj`
  - [ ] Change TargetFramework from net5.0 to net6.0
  - [ ] Update Pathoschild.Stardew.ModBuildConfig to 4.1.1
- [ ] Update `manifest.json`
  - [ ] Change MinimumApiVersion from 3.0.0 to 4.0.0
- [ ] Fix `StardewOutfitManager.cs`
  - [ ] Change `storeContext` to `ShopId` on line 50

### Phase 2: Build and Fix Compilation Errors (40 errors found)

#### 2.1 clothesType Enum Changes (2 errors)
**Files:** `WardrobeMenu.cs:118,122`, `FavoritesMenu.cs:502`
```csharp
// OLD: (item as Clothing).clothesType.Value == 0
// NEW: (item as Clothing).clothesType.Value == Clothing.ClothesType.SHIRT
// OLD: (item as Clothing).clothesType.Value == 1
// NEW: (item as Clothing).clothesType.Value == Clothing.ClothesType.PANTS
```

#### 2.2 changeShoeColor int→string (4 errors)
**Files:** `OutfitMethods.cs:64,68`, `FavoritesMethods.cs:240,245`
```csharp
// OLD: farmer.changeShoeColor(12)
// NEW: farmer.changeShoeColor("12")
// OLD: farmer.changeShoeColor(farmer.boots.Value.indexInColorSheet.Value)
// NEW: farmer.changeShoeColor(farmer.boots.Value.indexInColorSheet.Value.ToString())
```

#### 2.3 ModTools.cs API Changes (5+ errors)
- Line 22: `who.facingDirection` → `who.FacingDirection` (property case change) or use `.Value`
- Line 66: `Clothing.GetMaxPantsValue()` removed → need alternative
- Line 102: `animationFrame.secondaryArm` removed → check SDV 1.6 AnimationFrame
- Line 170: `Clothing.GetMaxShirtValue()` removed → need alternative

#### 2.4 FavoritesMenu.cs shirtColor Removed (4 errors)
**Lines:** 536, 568
```csharp
// OLD: farmer.shirtColor = player.shirtColor
// shirtColor property was removed in SDV 1.6
// Shirt color is now handled through the Clothing item itself
```

#### 2.5 NewDresserMenu.cs ShopMenu Clone (25+ errors)
Major API changes from cloning SDV 1.5.6 ShopMenu:
- `(bool)NetBool` → use `.Value` property
- `(int)NetInt` → use `.Value` property
- `actionWhenPurchased()` → now requires `shopId` parameter
- `hasItemInInventory()` → method removed, use alternatives
- `removeItemsFromInventory()` → method removed
- `isRecipe` → `IsRecipe` (capitalization)
- Draw method signature changes (arg 13 int→Color?)
- Various other SDV 1.6 internal changes

**Strategy:** Fix these in groups, building after each to verify

### Phase 3: Deploy and Test Basic Loading
- [ ] Run `./deploy.sh` successfully
- [ ] Launch Stardew Valley
- [ ] Verify mod loads without SMAPI errors
- [ ] Check SMAPI console output

### Phase 4: Test Core Functionality
- [ ] Test dresser interaction opens WardrobeMenu
- [ ] Test cycling through hats
- [ ] Test cycling through shirts
- [ ] Test cycling through pants
- [ ] Test cycling through shoes
- [ ] Test cycling through hair styles
- [ ] Test cycling through accessories
- [ ] Test farmer preview renders correctly

### Phase 5: Test Advanced Features
- [ ] Test saving a favorite outfit
- [ ] Test loading a favorite outfit
- [ ] Test favorites persist after save/load
- [ ] Test menu closes properly (mutex release)
- [ ] Test gamepad controls if applicable

### Phase 6: Fix Issues Found During Testing
- [ ] Document any runtime errors from SMAPI console
- [ ] Fix FarmerRenderer reflection if broken
  - [ ] Consider Harmony reverse patch pattern
  - [ ] Consider SMAPI reflection helper pattern
- [ ] Fix any other issues found

### Phase 7: (Optional) Address Pre-existing Bugs
- [ ] FavoritesMenu display bug
- [ ] Sleeve color sticky issue
- [ ] Rings handling issue
- [ ] Custom content pack support
- [ ] Performance optimization

---

## Future: SDV 1.6.16 Preparation

**Note:** SDV 1.6.16 is an unreleased future version. The migration guide exists to help mod authors prepare. These changes are NOT needed for 1.6.15 compatibility.

When 1.6.16 releases, the following changes will be needed:

### addItemToInventoryBool Removal
The method will be **completely removed** in 1.6.16. Replace with `TryAddToInventory()`:
- `Menus/NewDresserMenu.cs` lines 897, 1141, 1484
- `Managers/MenuManager.cs` line 114

```csharp
// OLD:
Game1.player.addItemToInventoryBool(item)

// NEW:
Game1.player.TryAddToInventory(item).AnyAdded
```

---

## Rollback Plan

If migration proves too complex, the current codebase remains functional for SDV 1.5.x users. Consider:
- Creating a `1.5-compat` branch before changes
- Documenting minimum SDV version in manifest for 1.5 users
