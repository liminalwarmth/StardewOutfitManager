# Stardew Outfit Manager - Development Journal

Entries are listed newest-first. Review this before starting any task.

---

## 2025-12-25 19:00 - Feature Requests from idea.txt

**Task:** Implement remaining items from idea.txt (#8, #9, #10)

### Changes Made

**1. Save Favorites at End of Day** (`StardewOutfitManager.cs`)
- Changed from `GameLoop.Saved` event to `GameLoop.DayEnding` event
- Favorites now save before the save dialog appears, ensuring data is persisted even if the game crashes during save

**2. Label Cycling Wrap-Around** (`WardrobeMenu.cs`)
- Gamepad navigation now wraps: pressing up on Hat (top) goes to Accessory (bottom), and vice versa
- Updated `customSnapBehavior()` to handle direction 0 (up) and 2 (down) for wrap-around
- Added `CUSTOM_SNAP_BEHAVIOR` to Hat's `upNeighborID` and Accessory's `downNeighborID`
- Play "shiny4" sound on wrap for feedback

**3. Season Default Category** - Already Implemented
- Verified: `FavoritesMenu.cs:397` correctly sets default category to current season when menu opens
- Spring/Summer/Fall/Winter detected via `Game1.IsSpring` etc., falls back to "All Outfits"

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-25 18:00 - Bug Fixes & Improvements

**Task:** Fix bugs and issues identified during codebase review

### Bug Fixes

**1. Outfit Slot Click Bounds Check** (`FavoritesMenu.cs:868`)
- Added bounds check before accessing `outfitSlotsFiltered` array to prevent index out of bounds crash

**2. Sorting Logic Inverted** (`FavoritesMenu.cs:650-653`)
- Fixed: `isFavorite == false` items were going to `favorited` list (backwards)
- Now correctly sorts: favorited → regular → unavailable+favorited → unavailable

**3. Custom Hat Sprite Lookup** (`ModTools.cs:183`)
- Changed from parsing `Hat.ItemId` string to using `Hat.ParentSheetIndex`
- Now works correctly with modded hats that have non-numeric ItemIds

**4. Hover Display Using Wrong Data** (`FavoritesMenu.cs:142-184`)
- Hover infobox was checking `Game1.player` equipment instead of the outfit slot's data
- Now uses `outfitAvailabileItems` dictionary from the slot being hovered

### Code Cleanup

**5. Removed Unused AssetManager Fields**
- Removed: `wardrobeBackgroundTexture`, `bgTextureSpring`, `bgTextureSummer`, `bgTextureFall`, `bgTextureWinter`
- Removed: Empty `AssembleHairIndex()` method stub
- Removed: Unused `using` statements

### Feature Implementation

**6. Window Resize Handling** (All menus)
- **FavoritesMenu**: Full reposition of portrait, rotation buttons, OK button, outfit box, navigation arrows, scrollbar, category buttons, outfit buttons
- **WardrobeMenu**: Full reposition of portrait, equipment icons, selection buttons, labels, category buttons, save favorite button
- **NewDresserMenu**: Added top tab repositioning to existing resize handler
- **MenuManager**: Added `repositionTopTabButtons()` method for consistent tab positioning

### Multiplayer Compatibility
- All fixes maintain split-screen local co-op support through `PerScreen<>` state management
- No changes to dresser mutex locking behavior

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-25 17:00 - SDV 1.6 ShopMenu Comparison

**Task:** Compare cleaned NewDresserMenu against SDV 1.6 ShopMenu for potential improvements

### Conclusion: No Changes Needed

SDV 1.6's ShopMenu improvements (`ItemStockInformation` struct, `ShopTabClickableTextureComponent` with Filter lambdas, safety timer) are designed for commerce shops with prices/trades. Our dresser is a free inventory view - these patterns don't apply.

The dresser tab is tangential to the mod's core value (Wardrobe/Favorites). Current implementation works correctly post-cleanup.

---

## 2025-12-25 16:00 - NewDresserMenu.cs Cleanup

**Task:** Remove dead code from cloned ShopMenu (SDV 1.5.6)

### Summary
Aggressive refactor of `NewDresserMenu.cs` to remove all cruft that was irrelevant to the Dresser use case. The file was originally copied from SDV 1.5.6's ShopMenu and contained code for NPCs, other store contexts, currency types, buyback systems, etc.

**Lines removed:** ~673 (1870 → 1197, 36% reduction)

### Changes Made

**Removed Unused Fields:**
- `descriptionText`, `canPurchaseCheck`, `readOnly` (fixed build warnings)
- `portraitPerson`, `potraitPersonDialogue` (NPC portraits never used)
- `sellPercentage` (always 1.0)
- `buyBackItems`, `buyBackItemsToResellTomorrow` (dresser bypasses buyback)

**Removed Dead Methods:**
- `setUpShopOwner()` - 230+ lines of NPC dialogue for Robin, Clint, Willy, Pierre, etc.
- `CanBuyback()`, `BuyBuybackItem()`, `AddBuybackItem()` - buyback system not used

**Removed Dead Code Blocks:**
- Portrait drawing in `draw()` and `updatePosition()`
- Default sell behavior in `receiveLeftClick()` and `receiveRightClick()` - dresser uses `onSell` callback
- Non-Dresser `storeContext` checks (Catalogue, Furniture Catalogue, QiGemShop, etc.)
- `actionWhenPurchased()` calls - always skipped for storage shops

**Simplified:**
- `applyTab()` - removed Catalogue/Furniture Catalogue logic, kept only Dresser tab filtering
- `performHoverAction()` - removed non-storage-shop sell price display

**Files Modified:** `Menus/NewDresserMenu.cs`

**Build Status:** Compiles with no warnings in NewDresserMenu.cs

---

## 2025-12-25 - PR Review Fixes

**Task:** Address PR review feedback for cleaner code

### Dynamic Outfit Naming - CHANGED
**Files:** `Menus/WardrobeMenu.cs:505-521`, `Menus/FavoritesMenu.cs:270-288`
- **Problem:** Outfit names stored at save time (e.g., "Spring Outfit 3") left gaps when outfits were deleted
- **Fix:** Save outfits with empty Name, compute display names dynamically based on roster position
- Added `GetDisplayName()` helper to OutfitSlot class
- Custom names still respected; empty string triggers auto-naming from category position

### Backwards-Compatible List Overloads - REMOVED
**File:** `Utils/FavoritesMethods.cs`
- Deleted 3 List-based overloads that built Dictionary internally:
  - `isAvailable(List<Item>)`
  - `GetOutfitItemAvailability(List<Item>)`
  - `GetItemByReferenceID(string, List<Item>)`
- All active code already uses efficient Dictionary-based lookups

### Dead Code Cleanup
**File:** `Utils/OutfitMethods.cs:99-122`
- Deleted commented-out `WearFavoriteOutfit` method (replaced by `equipFavoriteOutfit`)

### Build Status
- **Build succeeded** with same 8 pre-existing warnings

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
