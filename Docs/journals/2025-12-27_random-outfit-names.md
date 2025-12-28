# Branch: liminalwarmth/random-outfit-names

Add random outfit name suggester with season-specific name pools. When saving or renaming outfits, the naming dialog now opens with a random name suggestion from the appropriate season's pool. The dice button generates season-specific names instead of pet names.

---

## 2025-12-27 12:45 - Random Outfit Name Suggester Implementation

**Task:** Implement season-specific random name suggestions for outfit naming

### Changes Made

**1. Created Outfit Names JSON Files** (`Assets/OutfitNames/`)
- Added 5 JSON files with ~100+ names each: SpringOutfitNames.json, SummerOutfitNames.json, FallOutfitNames.json, WinterOutfitNames.json, SpecialOutfitNames.json
- JSON structure: `{"Season": "Spring", "Names": ["Daffodil", "Tulip", ...]}`
- Names are thematic to each season (flowers, weather, activities, moods, etc.)

**2. Created OutfitNameManager Class** (`Managers/OutfitNameManager.cs`)
- Loads all 5 JSON files at mod initialization
- Provides `GetRandomName(category)` method to get random name from season pool
- Falls back to "{Category} Outfit" if loading fails

**3. Enhanced OutfitNamingMenu** (`Menus/FavoritesMenu.cs`)
- Added `_outfitCategory` field to track which season pool to use for random names
- Added `_onCancelCallback` action for Escape key handling
- Override `receiveLeftClick()` to intercept dice button clicks and use our season-specific names
- Override `receiveKeyPress()` to handle Escape key for cancel functionality
- Changed `minLength = 1` to require non-empty names (was 0)

**4. Updated WardrobeMenu Save Flow** (`Menus/WardrobeMenu.cs`)
- Save button now opens naming dialog instead of immediately saving
- Pre-populates text box with random name from selected category
- Added `_pendingOutfitCategory` field to track category for callback
- Added `OnNewOutfitNamed()` and `OnNewOutfitCancelled()` callbacks
- Added child menu guards to `receiveKeyPress()`, `receiveGamePadButton()`, `receiveLeftClick()`, and `performHoverAction()`

**5. Updated FavoritesMenu Rename Flow** (`Menus/FavoritesMenu.cs`)
- Updated constructor call to pass category and cancel callback
- For outfits with empty names (legacy), suggests a random name
- Added `OnRenameCancelled()` callback
- Simplified `OnOutfitRenamed()` since empty names no longer allowed

**6. Simplified GetDisplayName()** (`Menus/FavoritesMenu.cs`)
- Removed complex position-based auto-naming logic
- Now just returns `modelOutfit.Name ?? "{Category} Outfit"` for backwards compatibility

**7. Registered OutfitNameManager** (`StardewOutfitManager.cs`)
- Added `internal static OutfitNameManager outfitNameManager` field
- Initialize in `Entry()` after AssetManager

### Files Modified
- `Assets/OutfitNames/SpringOutfitNames.json` - NEW: Spring name pool
- `Assets/OutfitNames/SummerOutfitNames.json` - NEW: Summer name pool
- `Assets/OutfitNames/FallOutfitNames.json` - NEW: Fall name pool
- `Assets/OutfitNames/WinterOutfitNames.json` - NEW: Winter name pool
- `Assets/OutfitNames/SpecialOutfitNames.json` - NEW: Special name pool
- `Managers/OutfitNameManager.cs` - NEW: Name loading and random selection
- `Menus/FavoritesMenu.cs` - Enhanced OutfitNamingMenu, updated callbacks, simplified GetDisplayName
- `Menus/WardrobeMenu.cs` - Added naming dialog to save flow, added child menu guards
- `StardewOutfitManager.cs` - Registered OutfitNameManager

### Build Status
- **Build succeeded with 0 warnings**

### Behavior Changes
- **Before**: Saving an outfit immediately saved with no name, auto-generated as "Spring Outfit 1"
- **After**: Saving opens naming dialog with random suggestion like "Buttercup" or "Meadow"
- **Before**: Dice button generated pet-style names ("Fluffy", "Spot")
- **After**: Dice button generates season-specific names matching the outfit's category
- **Before**: Empty names allowed, would clear custom name
- **After**: Names required (minLength=1), Escape cancels without saving

---

## 2025-12-27 13:00 - Check for Duplicate Outfits Before Naming Dialog

**Task:** Move duplicate outfit check to happen before the naming dialog opens

### Problem
The duplicate check was happening in `SaveNewOutfit()` *after* the player named the outfit. This wasted the player's time naming an outfit that couldn't be saved.

### Changes Made

**1. Added `PlayerOutfitAlreadyExists()` Method** (`Utils/FavoritesMethods.cs`)
- New extension method on `FavoritesData` to check if player's current outfit exists
- Uses `getExistingItemTag()` to read existing tags without creating new ones
- Compares against all saved favorites by equipment, hair, accessory, and dye colors

**2. Added Early Duplicate Check** (`Menus/WardrobeMenu.cs`)
- Check `favoritesData.PlayerOutfitAlreadyExists()` before opening naming dialog
- If duplicate found, play cancel sound and return early
- Naming dialog only opens if outfit is unique

### Files Modified
- `Utils/FavoritesMethods.cs` - Added `PlayerOutfitAlreadyExists()` and `getExistingItemTag()` methods
- `Menus/WardrobeMenu.cs` - Added early duplicate check before naming dialog

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-27 13:15 - Outfit Name Display Above Preview Window

**Task:** Display the current outfit name above the preview window in both WardrobeMenu and FavoritesMenu. Gray out save button when outfit already exists.

### Changes Made

**1. Added `FindMatchingOutfitName()` Method** (`Utils/FavoritesMethods.cs`)
- New extension method on `FavoritesData` to find matching outfit by name
- Returns the outfit name if a match is found, or null if no match
- Matches full outfit: equipment, hair, accessory, and dye colors
- Refactored `PlayerOutfitAlreadyExists()` to delegate to this method

**2. Updated WardrobeMenu Layout** (`Menus/WardrobeMenu.cs`)
- Added `_matchedOutfitName` field to track if current outfit matches a saved favorite
- Added `UpdateMatchedOutfitName()` method called on category/equipment changes
- Shifted portrait Y position from +64 to +88 (24px down to make room for name label)
- Added name label drawing above portrait, showing matched name or "New Outfit"
- Gray out save button when displaying a matched outfit (can't save duplicates)
- Block save button click when outfit already exists

**3. Updated FavoritesMenu Layout** (`Menus/FavoritesMenu.cs`)
- Shifted portrait Y position from +64 to +88 (24px down for consistency)
- Added name label drawing above portrait showing selected outfit's name
- Updated `gameWindowSizeChanged()` to match new portrait position

### Files Modified
- `Utils/FavoritesMethods.cs` - Added `FindMatchingOutfitName()`, refactored `PlayerOutfitAlreadyExists()`
- `Menus/WardrobeMenu.cs` - Added name display, save button graying, layout shift
- `Menus/FavoritesMenu.cs` - Added name display, layout shift

### Build Status
- **Build succeeded with 0 warnings**

### Behavior Changes
- **WardrobeMenu**: Shows "New Outfit" above preview, or the name of a matching saved outfit
- **WardrobeMenu**: Save button is grayed out when current outfit matches a saved favorite
- **FavoritesMenu**: Shows selected outfit's name above preview window
- Both menus have consistent layout with name label in the same position

---

## 2025-12-27 13:30 - Code Review Against Modding Reference Standards

**Task:** Review code against new `Docs/SDV_Modding_Reference.md` standards and make improvements

### Changes Made

**1. Fixed modData Key to Use Proper UniqueID Prefix** (`Utils/FavoritesMethods.cs`)
- Added `FavoriteItemTagKey` constant to `FavoritesDataMethods` class
- Changed key from `"StardewOutfitManagerFavoriteItem"` to `"LiminalWarmth.StardewOutfitManager/FavoriteItem"`
- Updated all 6 usages to reference the constant instead of hardcoded string
- Follows SMAPI modding convention for modData keys

**2. Simplified Path Handling** (`Managers/OutfitNameManager.cs`)
- Removed unnecessary `GetInternalAssetName()` call for path construction
- Use simple `Path.Combine("Assets", "OutfitNames", ...)` for cross-platform paths
- `helper.ModContent.Load<T>()` handles relative paths correctly

### Files Modified
- `Utils/FavoritesMethods.cs` - Added constant with proper UniqueID prefix, updated 6 usages
- `Managers/OutfitNameManager.cs` - Simplified path handling

### Build Status
- **Build succeeded with 0 warnings**

### Breaking Change
- Existing outfit data using the old key format will no longer work. Delete old favorites data files to start fresh.

---

## 2025-12-27 13:45 - Graceful Handling of Malformed Save Data

**Task:** Fix menu crash caused by malformed/old favorites data after modData key change

### Problem
Menu failed to open after the modData key was changed from `StardewOutfitManagerFavoriteItem` to `LiminalWarmth.StardewOutfitManager/FavoriteItem`. Old save data caused `KeyNotFoundException` when accessing `favorite.Items["Hat"]` etc.

### Changes Made

**1. Safe Dictionary Access** (`Utils/FavoritesMethods.cs`)
- Added `getItemTag()` helper method for null-safe dictionary access using `TryGetValue`
- Updated `outfitExistsInFavorites()` to use `getItemTag()` instead of direct indexer
- Updated `FindMatchingOutfitName()` to use `getItemTag()` and add null checks
- Both methods now skip malformed favorites instead of crashing

**2. Robust Data Loading** (`Managers/PlayerManager.cs`)
- Wrapped `loadFavoritesDataFromFile()` in try-catch to handle any deserialization errors
- Added validation to check if loaded data has valid `Favorites` list
- Added cleanup step that removes malformed outfits (missing `Items` dictionary)
- Logs warnings when data is malformed or cleaned up
- Falls back to fresh `FavoritesData()` on any error

### Files Modified
- `Utils/FavoritesMethods.cs` - Added `getItemTag()`, fixed all dictionary access to be null-safe
- `Managers/PlayerManager.cs` - Added error handling, validation, and cleanup on load

### Build Status
- **Build succeeded with 0 warnings**

### Result
- Old/malformed favorites data is now handled gracefully
- Menu will open even with corrupt data (data gets cleaned or reset)
- SMAPI console shows warnings when data issues are detected

---

## 2025-12-27 14:00 - Fix Constructor Ordering Bug in WardrobeMenu

**Task:** Fix NullReferenceException when opening WardrobeMenu

### Problem
After adding the `UpdateMatchedOutfitName()` call, the menu crashed with:
```
NullReferenceException: Object reference not set to an instance of an object.
   at StardewOutfitManager.Menus.WardrobeMenu.UpdateMatchedOutfitName() in WardrobeMenu.cs:line 362
```

Root cause: `UpdateMatchedOutfitName()` was being called on line 156, but `categorySelected` (which it accesses via `categorySelected.name`) wasn't initialized until line 311.

### Changes Made

**1. Moved `UpdateMatchedOutfitName()` Call** (`Menus/WardrobeMenu.cs`)
- Removed premature call from line 155-156 (before `categorySelected` was set)
- Added call after line 308 where `categorySelected` is assigned
- Added comment explaining the dependency

### Files Modified
- `Menus/WardrobeMenu.cs` - Moved `UpdateMatchedOutfitName()` to after `categorySelected` initialization

### Build Status
- **Build succeeded with 0 warnings**

### Lesson Learned
When adding new initialization code to a constructor, verify that all dependencies (fields accessed by the new code) are already initialized at that point in the constructor.

---

## 2025-12-27 15:30 - UI Polish Pass: Outfit Display and Save Button

**Task:** Seven UI tweaks for visual polish and UX improvements

### Changes Made

**1. Outfit Name Display (WardrobeMenu + FavoritesMenu)**
- Changed from `Game1.smallFont` to `SpriteText` (decorative font)
- Centered vertically in gap between menu border and portrait box
- Calculate: `gapTop + (_portraitBox.Y - gapTop) / 2 - nameHeight / 2`

**2. Save Button Hover Animation (WardrobeMenu)**
- Added condition `&& _matchedOutfitName == null` to hover scale logic
- Button no longer animates when disabled (outfit already saved)

**3. Save Button Disabled Appearance (WardrobeMenu)**
- Removed gray tint approach (was causing inverted text)
- Now draws button normally with `Color.White`
- Overlays `Color.Black * 0.4f` rectangle when disabled

**4. Seasonal Background for Saved Outfits (WardrobeMenu)**
- Added static background rectangles: `bgDefault`, `bgSpring`, `bgSummer`, `bgFall`, `bgWinter`, `bgSpecial`
- Added `_portraitBackground` field
- Added `GetBackgroundForCategory()` helper method
- `UpdateMatchedOutfitName()` now sets `_portraitBackground` based on saved state

**5. Update Display After Save (WardrobeMenu)**
- Added `UpdateMatchedOutfitName()` call in `OnNewOutfitNamed` callback after successful save
- Display immediately shows saved outfit name and seasonal background

**6. "Current Outfit" Label (FavoritesMenu)**
- Changed from empty string to "Current Outfit" when `outfitSlotSelected` is null
- Also switched to SpriteText for consistency with WardrobeMenu

### Files Modified
- `Menus/WardrobeMenu.cs` - Items 1-5 (name display, button behavior, background, save callback)
- `Menus/FavoritesMenu.cs` - Item 6 ("Current Outfit" label)

### Build Status
- **Build succeeded with 0 warnings**