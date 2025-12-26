# Stardew Outfit Manager - Development Journal

Entries are listed newest-first. Review this before starting any task.

---

## 2025-12-25 - Migration Complete & Enhancement Plans

**Task:** Archive completed migration plan and create future enhancement documents

### Migration Plan Archived
- Checked off remaining Phase 5 items (user confirmed testing complete)
- Marked custom content support as deferred to separate plan
- Moved `SMAPI_1.6_Migration_Plan.md` to `Docs/archive/`

### Enhancement Plans Created
Three separate enhancement documents for future work:

1. **`Enhancement_UI_Improvements.md`** - Multi-phase plan covering:
   - Phase 1: Game window resize handling
   - Phase 2: Outfit hover infobox
   - Phase 3: Outfit renaming

2. **`Enhancement_Custom_Content_Support.md`** - Standalone plan for:
   - Custom hair/accessory support from content packs
   - Requires research before implementation

3. **`Enhancement_SDV_1.6.16_Prep.md`** - Version-triggered plan for:
   - `addItemToInventoryBool` → `TryAddToInventory` migration
   - Deferred until SDV 1.6.16 releases

### Build Warnings Review
- 8 warnings remain (all pre-existing unused fields)
- AssetManager.cs: 5 unused texture placeholders
- NewDresserMenu.cs: 3 unused fields from ShopMenu clone
- Not bugs, just dead code from incomplete features

---

## 2025-12-25 - Scrolling, Save Button & Code Cleanup

**Task:** Fix scrollbar drag, add outfit name generation, clean up stale comments

### Scrollbar Drag - FIXED
**File:** `Menus/FavoritesMenu.cs:944-977, 1019-1029`
- **Problem:** Scrollbar could be clicked but dragging did nothing (entire `leftClickHeld()` was commented out)
- **Fix:** Implemented working scrollbar drag logic:
  - Clamps scrollbar position to runner bounds during drag
  - Calculates percentage and updates `currentOutfitIndex` in real-time
  - Simplified `setScrollBarToCurrentIndex()` formula with proper bounds clamping

### Save Favorite Button - FIXED
**File:** `Menus/WardrobeMenu.cs:505-524`
- **Problem:** Button saved outfits with empty names
- **Fix:** Auto-generates unique names like "Spring Outfit 1", "Summer Outfit 2"
- Added feedback sound for duplicate outfit detection

### Stale Comment Cleanup
- Removed `*DISPLAY BUG*` and `*BUG* sleeve color` comments (issues fixed previously)
- Removed outdated TODO comments for implemented features
- Cleaned up profanity and commented-out debugging code from scrolling attempts

### Build Status
- **Build succeeded** with same warnings as before

---

## 2025-12-25 - Pre-existing Bug Fixes

**Task:** Fix pre-existing bugs identified during migration (sleeve color, rings, performance)

### Bug 1: Sleeve Color Sticky - FIXED
**File:** `Utils/ModTools.cs:32`
- **Problem:** Display farmer used wrong FarmerRenderer texture cache
- **Root Cause:** `baseTexture` was accessed from `Game1.player.FarmerRenderer` instead of `who.FarmerRenderer`
- **Fix:** Changed to use `who.FarmerRenderer` for the baseTexture field access

### Bug 2: Rings Not Displayed on Model Farmer - FIXED
**File:** `Utils/FavoritesMethods.cs:248-259`
- **Problem:** `dressDisplayFarmerWithAvailableOutfitPieces()` method omitted LeftRing and RightRing handling
- **Fix:** Added ring equipment logic matching the pattern used for other equipment slots

### Bug 3: Performance - Duplicate Lookups - FIXED
**Files:** `Utils/FavoritesMethods.cs`, `Menus/FavoritesMenu.cs`
- **Problem:** O(n) linear searches through item list repeated 12 times per outfit (6 items × 2 methods)
- **Fix:** Added `BuildItemTagLookup()` method to pre-build a Dictionary<string, Item> from item tags
- Added overloaded methods accepting the lookup dictionary for O(1) item retrieval
- Updated FavoritesMenu constructor to build lookup once and pass to all OutfitSlot constructors

### Gamepad Code Review
- Reviewed all gamepad handling code for SDV 1.6 compatibility
- **Status:** No changes needed - all IClickableMenu gamepad APIs unchanged

### Build Status
- **Build succeeded** with same warnings as before (unused fields from cloned ShopMenu code)

---

## 2025-12-25 - SMAPI 1.6 Migration Complete

**Task:** Migrate mod from SDV 1.5.x/SMAPI 3.x to SDV 1.6.15/SMAPI 4.0

### Project Configuration
- Updated `StardewOutfitManager.csproj`: `net5.0` → `net6.0`, ModBuildConfig `4.0.1` → `4.1.1`
- Updated `manifest.json`: `MinimumApiVersion` `3.0.0` → `4.0.0`

### StardewOutfitManager.cs
- Fixed ShopMenu detection: `storeContext` → `ShopId`

### WardrobeMenu.cs & FavoritesMenu.cs
- Fixed clothesType enum: `== 0/1` → `== Clothing.ClothesType.SHIRT/PANTS`

### FavoritesMenu.cs
- Removed `shirtColor` assignments (property removed in SDV 1.6 - color now comes from Clothing item)

### OutfitMethods.cs & FavoritesMethods.cs
- Fixed `changeShoeColor()`: `int` → `string` parameter

### ModTools.cs
- Fixed `who.facingDirection` → `who.FacingDirection`
- Removed `Clothing.GetMaxPantsValue()` / `GetMaxShirtValue()` calls (methods removed, index validation now internal)
- Removed `animationFrame.secondaryArm` usage (property removed)
- Fixed `Hat.which` → `Hat.ItemId` (now string-based, parsed for sprite index)

### NewDresserMenu.cs (HIGH RISK - cloned from SDV 1.5.6 ShopMenu)
- Fixed `isRecipe` → `IsRecipe` (capitalization)
- Fixed `isMale` → `IsMale` (capitalization)
- Fixed `Stats.getStat()` → `Stats.Get()`
- Fixed `edibility` → `Edibility` (capitalization)
- Fixed `hasItemInInventory()` → `Items.ContainsId()` (method removed)
- Fixed `removeItemsFromInventory()` → `Items.ReduceId()` (method removed)
- Fixed `actionWhenPurchased()` → `actionWhenPurchased(shopId)` (now requires parameter)
- Fixed `SpriteText.drawString` color param: `int` → `Color?`
- Fixed `getHoveredItemExtraItemIndex()` return type: `int` → `string`
- Fixed `drawHoverText` parameter types

### Documentation
- Added reference resources to `CLAUDE.md` (decompiled source links, migration guides)
- Updated `Docs/SMAPI_1.6_Migration_Plan.md` with detailed error breakdown

### Build Status
- **Build succeeded** with only warnings (unused fields from cloned ShopMenu code)
- Mod deployed to Mods folder
