# Stardew Outfit Manager - Development Journal

Entries are listed newest-first. Review this before starting any task.

---

## 2025-12-26 21:45 - Add Config Options for Shop Integration

**Task:** Add config toggles for Robin's shop and Traveling Merchant dresser sales.

### Changes Made

**1. Added new config options** (`ModConfig.cs`)
- `RobinSellsDressers` (default: true) - Controls Robin's shop integration
- `TravelingMerchantSellsDressers` (default: true) - Controls Traveling Cart random sale

**2. Updated GMCM registration** (`StardewOutfitManager.cs`)
- Added two new bool options with descriptive tooltips
- All three options now have detailed explanatory hover text

**3. Added conditional logic** (`Managers/AssetManager.cs`)
- Data/Furniture patching now uses config to set `off_limits_for_random_sale` dynamically
- Data/Shops patching is now conditional on `RobinSellsDressers` config

### Files Modified
- `ModConfig.cs`
- `StardewOutfitManager.cs`
- `Managers/AssetManager.cs`

### Build Status
- Compiles successfully with no errors or warnings

---

## 2025-12-26 21:30 - Config System, GMCM, Robin's Shop, and Traveling Cart

**Task:** Add config system with GMCM support, Robin's shop integration for custom dressers, and Traveling Merchant integration for Mirror Dressers.

### Changes Made

**1. Created config system**
- `ModConfig.cs` - Config class with `StartingDresser` boolean (default: true)
- `IGenericModConfigMenuApi.cs` - API interface for GMCM integration
- `Models/ModSaveData.cs` - Per-save data to track if starting dresser was given

**2. Updated entry point** (`StardewOutfitManager.cs`)
- Added `internal static ModConfig Config` field
- Load config in Entry() via `helper.ReadConfig<ModConfig>()`
- Added `GameLaunched` event handler for GMCM registration
- Added `SaveLoaded` event handler to give Small Oak Dresser on new saves

**3. Added Robin's Carpenter Shop integration** (`Managers/AssetManager.cs`)
- Patches `Data/Shops` to add 14 custom dressers to Robin's furniture rotation
- Day-based rotation matching vanilla pattern:
  - Monday: Oak, Birch, Gold Mirror Dressers
  - Tuesday: Walnut, Mahogany Mirror Dressers
  - Wednesday: Modern, White Mirror Dressers
  - Thursday: Oak, Birch, Gold Small Dressers
  - Friday: Walnut, Mahogany Small Dressers
  - Saturday: Modern, White Small Dressers

**4. Added Traveling Merchant integration** (`Managers/AssetManager.cs`)
- Changed Mirror Dressers' `off_limits_for_random_sale` from `true` to `false`
- Mirror Dressers now appear in Traveling Cart's random furniture pool (Fri/Sun)
- Small Dressers remain exclusive to Robin's shop

**5. Updated CLAUDE.md**
- Added note about config file for conditional logic

### Files Created
- `ModConfig.cs`
- `IGenericModConfigMenuApi.cs`
- `Models/ModSaveData.cs`

### Files Modified
- `StardewOutfitManager.cs`
- `Managers/AssetManager.cs`
- `CLAUDE.md`

### Build Status
- Compiles successfully with no errors or warnings

---

## 2025-12-26 17:15 - Generate Small Dresser Color Variants

**Task:** Generate all 8 color variants of Small Dresser using palette swapping from Mirror Dresser colors.

### Changes Made

**1. Generated Small Dresser color variants via Python palette swap**
- Extracted wood color palettes from Mirror Dresser variants
- Created palette mapping from Oak's 4 wood tones to each variant's tones
- Generated 7 new PNGs: birch, walnut, mahogany, black, white, gold, modern

**2. Updated AssetManager with all Small Dresser variants** (`Managers/AssetManager.cs`)
- Added 7 new Data/Furniture entries for Small Dressers
- Updated GetFurnitureTexturePath() with paths for all 8 Small Dresser textures
- Pricing: Base 2500g, Gold 3500g, Modern 3000g (matching Mirror Dresser pattern)

### Files Modified
- `Managers/AssetManager.cs` - Added 7 more Small Dresser entries and texture paths
- `Assets/Objects/Furniture/Small Dresser/` - Added birch.png, walnut.png, mahogany.png, black.png, white.png, gold.png, modern.png

### Build Status
- Compiles successfully with no errors or warnings

### Total Custom Furniture
Now 16 items total: 8 Mirror Dressers + 8 Small Dressers

---

## 2025-12-26 16:45 - Add Custom Dresser Furniture Items

**Task:** Add 9 custom dresser furniture items (Mirror Dressers and Small Dresser) to the game using SMAPI's content API.

### Changes Made

**1. Added IModHelper reference to AssetManager** (`Managers/AssetManager.cs`)
- Added `private readonly IModHelper helper` field
- Stored helper reference in constructor for use in content API methods

**2. Subscribed to AssetRequested event** (`StardewOutfitManager.cs`)
- Added `helper.Events.Content.AssetRequested += OnAssetRequested` in Entry()
- Created handler method that delegates to AssetManager.HandleAssetRequested()

**3. Implemented HandleAssetRequested method** (`Managers/AssetManager.cs`)
- Patches `Data/Furniture` to add 9 custom dresser entries:
  - 8 Mirror Dresser variants (Birch, Black, Gold, Mahogany, Modern, Oak, Walnut, White) - 1x2 tiles
  - 1 Small Oak Dresser - 1x1 tile
- Loads custom textures when game requests `LiminalWarmth.StardewOutfitManager/Furniture/[Name]`
- Uses existing PNG assets from `Assets/Objects/Furniture/` folder

### Technical Notes
- Custom dressers automatically work with existing outfit management UI because they use `type="dresser"` which creates `StorageFurniture` that opens `ShopMenu` with `ShopId == "Dresser"`
- Furniture IDs prefixed with mod's UniqueID for mod compatibility
- Uses SMAPI's content API directly (no Content Patcher dependency)

### Files Modified
- `Managers/AssetManager.cs` - Added helper field, HandleAssetRequested method, GetFurnitureTexturePath method
- `StardewOutfitManager.cs` - Added AssetRequested event subscription and handler

### Build Status
- Compiles successfully with no errors or warnings

### Testing
- Use CJB Item Spawner to spawn furniture (e.g., "Oak Mirror Dresser")
- Place dresser and interact to verify outfit management menu opens

---

## 2025-12-26 - Tab Persistence & Outfit Card Facing Direction

**Task:** Remember last tab across dresser sessions and fix outfit card facing direction

### Changes Made

**1. Remember Last Tab Across Dresser Sessions** (`PlayerManager.cs`, `StardewOutfitManager.cs`)
- Added `lastUsedTab` PerScreen field to PlayerManager (persists across dresser opens within a play session)
- Save current tab to `lastUsedTab` when menu is closed via `cleanMenuExit()`
- Open dresser to last used tab instead of always opening to Wardrobe
- Works across different dressers - if you close on Favorites tab, any dresser you open will start on Favorites

**2. Outfit Card Farmers Always Face Forward** (`FavoritesMenu.cs`)
- OutfitSlot constructor now sets `modelFarmer.faceDirection(2)` after creating the farmer
- This overrides the shared `farmerFacingDirection` setting from MenuManager
- Main display farmer (left side) still respects user's rotation setting
- Outfit preview cards in the grid always show farmer facing forward for consistency

### Files Modified
- `Managers/PlayerManager.cs`
- `StardewOutfitManager.cs`
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Shared Season Selection & Dresser Display Name

**Task:** Share season selection across tabs and display actual dresser name

### Changes Made

**1. Shared Season Selection Across Tabs** (`MenuManager.cs`, `WardrobeMenu.cs`, `FavoritesMenu.cs`)
- Added `selectedCategory` field to MenuManager to share season selection across all tabs
- Added `GetCurrentSeasonCategory()` helper method to get current in-game season as category string
- Initialized to current in-game season when dresser is first opened
- WardrobeMenu and FavoritesMenu now read from and write to shared state
- Special handling: If "All Outfits" (FavoritesMenu-only) is selected when switching to WardrobeMenu, falls back to current in-game season

**2. Dresser Display Name** (`MenuManager.cs`, `StardewOutfitManager.cs`, `NewDresserMenu.cs`)
- Added `dresserDisplayName` field to MenuManager (e.g., "Junimo Dresser")
- Captured from `originalDresser.DisplayName` when dresser is opened
- Tab hover text now shows actual dresser name instead of "Dresser"
- NewDresserMenu scroll title now shows actual dresser name

### Files Modified
- `Managers/MenuManager.cs`
- `StardewOutfitManager.cs`
- `Menus/WardrobeMenu.cs`
- `Menus/FavoritesMenu.cs`
- `Menus/NewDresserMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Support Dyed Clothing Variations in Outfits

**Task:** Allow same clothing item with different dye colors to be saved as separate outfits

### Problem
When a player re-dyes a clothing item and tries to save a new outfit, the duplicate detection would block it because it only compared item tags (GUIDs), not the actual dye color. This meant you couldn't save two outfits that differed only by the color of the same pants.

### Solution
Added clothing color tracking to outfit saving and comparison:

**1. Data Model Update** (`Models/FavoritesModel.cs`)
- Added `ItemColors` dictionary to `FavoriteOutfit` class
- Stores dye colors for Shirt and Pants as "R,G,B,A" strings

**2. Save Logic Update** (`Utils/FavoritesMethods.cs`)
- Added `getClothingColorString()` helper to extract color from dyeable Clothing items
- `SaveNewOutfit()` now captures clothing colors when saving

**3. Comparison Logic Update** (`Utils/FavoritesMethods.cs`)
- Added `getItemColor()` helper for backwards-compatible color retrieval
- `outfitExistsInFavorites()` now compares Shirt and Pants colors in addition to item tags
- Handles legacy outfits without color data (null == null comparison)

### Behavior
- Same item with different dye = different outfit (can save both)
- Same item with same dye = duplicate (blocked as before)
- Legacy outfits without color data continue to work

### Files Modified
- `Models/FavoritesModel.cs`
- `Utils/FavoritesMethods.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Hide Scroll Arrows in NewDresserMenu

**Task:** Hide scroll arrows when no scrolling is needed in dresser tab

### Fix Made

**Hide Scroll Arrows When All Items Fit** (`NewDresserMenu.cs`)
- Moved `upArrow.draw(b)` and `downArrow.draw(b)` inside the `if (forSale.Count > 4)` block
- Now arrows, scrollbar runner, and scrollbar only appear when there are more than 4 items
- Previously arrows were always visible even when filtering to a small category (e.g., 3 hats)

### Files Modified
- `Menus/NewDresserMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - UI Polish: Facing Direction & Standard Tooltips

**Task:** Two usability tweaks from testing

### Changes Made

**1. Maintain Farmer Facing Direction Across Tabs/Outfits** (`MenuManager.cs`, `WardrobeMenu.cs`, `FavoritesMenu.cs`, `Utils/FavoritesMethods.cs`)
- Added `farmerFacingDirection` field to MenuManager for shared state
- All menus now read from `menuManager.farmerFacingDirection` instead of hardcoded `faceDirection(2)`
- Rotation button clicks save the new direction to MenuManager
- Removed hardcoded `faceDirection(2)` from `dressDisplayFarmerWithAvailableOutfitPieces()` - facing direction is now managed by the calling menu
- Direction persists when switching tabs, selecting outfits, or resetting outfit selection
- Only resets when completely exiting the wardrobe system

**2. Standard Item Tooltips for Equipment Icons** (`WardrobeMenu.cs`, `FavoritesMenu.cs`)
- Added `hoveredItem` field to track actual Item objects on hover
- Created `GetEquipmentSlotItem()` method in FavoritesMenu (replaces text-building approach)
- Updated draw methods to use `IClickableMenu.drawToolTip()` with the item
- Equipment slot hover now displays the exact same tooltip as inventory/shop items
- Empty slots still show "Empty X Slot" text using standard hover text

### Files Modified
- `Managers/MenuManager.cs`
- `Menus/WardrobeMenu.cs`
- `Menus/FavoritesMenu.cs`
- `Utils/FavoritesMethods.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - More Bug Fixes from Testing

**Task:** Address 3 issues from continued testing

### Fixes Made

**1. WardrobeMenu Disappearing on Resize** (`WardrobeMenu.cs`, `FavoritesMenu.cs`)
- Fixed by calling `base.gameWindowSizeChanged()` FIRST, then setting our custom centered position
- Manually reposition close button after setting position
- Applied same fix to FavoritesMenu for consistency

**2. Rotation Button Position Alignment** (`WardrobeMenu.cs`)
- Updated direction button positions to match FavoritesMenu
- Changed from `-40` / `+256-40` to `-42` / `+256-38` for slightly more spacing
- Updated both constructor and resize handler

**3. Hat Display in 2x Farmer Preview** (`Utils/ModTools.cs`)
- Updated to use SDV 1.6's ItemRegistry API for hat texture lookup
- Changed from `who.hat.Value.ParentSheetIndex` to `ItemRegistry.GetDataOrErrorItem(who.hat.Value.QualifiedItemId)`
- Now uses `hatItemData.SpriteIndex` and `hatItemData.GetTexture()` for proper hat rendering
- Added `StardewValley.ItemTypeDefinitions` using directive

### Files Modified
- `Menus/WardrobeMenu.cs`
- `Menus/FavoritesMenu.cs`
- `Utils/ModTools.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Additional UI Polish Fixes

**Task:** Address 3 more issues from manual testing

### Fixes Made

**1. Hide Scroll Arrows When No Scrolling Available** (`FavoritesMenu.cs`)
- Wrapped scroll arrow and scrollbar drawing in `if (scrollBar.visible)` check
- Arrows now only appear when there are more outfits than fit on screen

**2. Show Current Equipment Before Outfit Selected** (`FavoritesMenu.cs`)
- Equipment icons below portrait now show player's current equipment when no outfit is selected
- Once an outfit is selected, shows that outfit's items instead
- Updated `GetEquipmentSlotHoverText()` to also handle both cases for hover text

**3. Remove Extra Background from Dresser Tab** (`NewDresserMenu.cs`)
- Removed redundant full-size background box at line 1078
- The inventory and item list already have their own background boxes

### Files Modified
- `Menus/FavoritesMenu.cs`
- `Menus/NewDresserMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Bug Fixes from Manual Testing

**Task:** Address 5 issues found during manual testing of UI improvements

### Fixes Made

**1. Window Re-centering on Resize** (`WardrobeMenu.cs`, `FavoritesMenu.cs`)
- Used `Utility.getTopLeftPositionForCenteringOnScreen()` to properly center menu
- Called `base.gameWindowSizeChanged()` AFTER setting position so close button repositions correctly

**2. Hat Display in 2x Farmer Preview** (`Utils/ModTools.cs`)
- Hat position offsets now multiplied by scale factor
- Fixed `hatXOffset` and `hatYOffset` calculations to scale properly at 2x

**3. Item Icon Centering in Hover Preview** (`FavoritesMenu.cs`)
- Changed hover infobox grid from 48x48 to 64x64 item slots
- Now matches WardrobeMenu pattern where items draw at scale 1.0

**4. Hover Text for WardrobeMenu Equipment Icons** (`WardrobeMenu.cs`)
- Added equipment icon hover detection in `performHoverAction()`
- Shows item tooltip when mouse hovers over equipment slots

**5. Full Item Description in Hover Text** (`FavoritesMenu.cs`, `WardrobeMenu.cs`)
- Changed from `DisplayName` only to `DisplayName + getDescription()`
- Both menus now show full item info on hover

### Files Modified
- `Menus/FavoritesMenu.cs`
- `Menus/WardrobeMenu.cs`
- `Utils/ModTools.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-25 21:00 - Outfit Hover Infobox

**Task:** Add hover infobox to outfit cards showing outfit name and item preview grid

### Changes Made

**1. Outfit Hover Tracking** (`FavoritesMenu.cs`)
- Added `outfitSlotHovered` field to track which outfit is being hovered
- Updated `performHoverAction()` to detect hover over outfit buttons

**2. Hover Infobox Display** (`FavoritesMenu.cs`)
- Added `DrawOutfitHoverInfobox()` method
- Shows outfit name at top
- Displays 2x3 grid of item icons (Hat, Shirt, Left Ring / Boots, Pants, Right Ring)
- Missing items shown as faded empty slot icons
- Infobox positioned to right of hovered slot (or left if it would clip off screen)

**3. Gamepad Support** (`FavoritesMenu.cs`)
- Infobox also appears when gamepad navigates to an outfit slot

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-25 20:30 - Equipment Icons in FavoritesMenu

**Task:** Add equipment icons below portrait in FavoritesMenu to show selected outfit items (matching WardrobeMenu pattern)

### Changes Made

**1. Equipment Icons Display** (`FavoritesMenu.cs`)
- Added `equipmentIcons` list with 6 ClickableComponent slots (Hat, Shirt, Pants, Boots, Left Ring, Right Ring)
- Icons positioned below portrait in 2x3 grid layout
- Shows actual items from selected outfit or empty slot icons when missing
- Full gamepad navigation support with `myID`, `region`, and neighbor IDs

**2. Hover Text for Items** (`FavoritesMenu.cs`)
- Added `GetEquipmentSlotHoverText()` helper method
- Mouse hover over equipment icon shows item name tooltip
- Gamepad snap to equipment icon also shows tooltip
- Missing items show "Empty X Slot" text

**3. Window Resize Support** (`FavoritesMenu.cs`)
- Equipment icons reposition correctly on window resize

**4. Cleanup** (`FavoritesMenu.cs`)
- Removed obsolete `isHovered`, `hoverBox`, `lastWorn`, `itemAvailabilityIcons` fields from OutfitSlot class
- Removed incomplete hover infobox draw code from OutfitSlot.Draw()

### Build Status
- **Build succeeded with 0 warnings**

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
