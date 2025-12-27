# Stardew Outfit Manager - Development Journal

Entries are listed newest-first. Review this before starting any task.

---

## 2025-12-27 13:15 - Fix Delete After Name Clear, Revert Hover Text Centering

**Task:** Fix bug where outfits with cleared names couldn't be deleted; revert hover text centering

### Changes Made

**1. Fixed Null Name Delete Bug** (`Utils/FavoritesMethods.cs`)
- `outfitExistsInFavorites()` used `outfit.Name.Equals(favorite.Name)` which fails when Name is null
- Changed to `string.Equals(outfit.Name, favorite.Name)` for null-safe comparison
- Outfits with cleared custom names can now be deleted properly

**2. Reverted Hover Text Centering** (`FavoritesMenu.cs`)
- User preferred left-aligned names in hover popup
- Changed back to `boxX + padding` positioning

### Files Modified
- `Utils/FavoritesMethods.cs` - Null-safe string comparison in outfitExistsInFavorites
- `Menus/FavoritesMenu.cs` - Reverted hover text to left-aligned

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-27 13:00 - Fix Empty Name and Center Hover Text

**Task:** Allow truly empty names via OK button, center outfit names in hover popup

### Changes Made

**1. Created OutfitNamingMenu Subclass** (`FavoritesMenu.cs`)
- NamingMenu's `minLength` is protected with default value 1 (can't accept empty text)
- Created `OutfitNamingMenu : NamingMenu` subclass that sets `minLength = 0`
- Now users can click OK with empty text to clear custom name

**2. Centered Outfit Name in Hover Popup** (`FavoritesMenu.cs`) - *Reverted in next entry*
- Name was left-aligned at `boxX + padding`
- Now centered using `boxX + (boxWidth - nameSize.X) / 2`

### Files Modified
- `Menus/FavoritesMenu.cs` - Added OutfitNamingMenu class, centered hover text

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-27 12:30 - UI Polish Round 2: Button Size, Rename Flow, Empty Names

**Task:** Enlarge buttons, fix rename button state, allow empty names for auto-naming

### Changes Made

**1. Enlarged Button Backgrounds** (`FavoritesMenu.cs`)
- Increased button size from 48x48 to 56x56 (icons stay at 32x32)
- More padding around icons: now 12px instead of 8px
- Updated both button initialization and window resize handler

**2. Fixed Rename Button Staying Depressed** (`FavoritesMenu.cs`)
- Added `renameButton.scale = renameButton.baseScale;` after click sound
- Button now resets to neutral state before NamingMenu opens

**3. Allow Empty String to Reset to Auto-Name** (`FavoritesMenu.cs`)
- Empty/whitespace names now clear the custom name instead of being rejected
- Outfit reverts to auto-generated name (e.g., "Spring Outfit 1")
- Changed `GetDisplayName()` from private to internal for callback access

**4. Created Enhancement Doc** (`Docs/Enhancement_Random_Outfit_Names.md`)
- Documents future feature for seasonal/thematic random names
- Includes name lists for Spring, Summer, Fall, Winter, Special categories
- Outlines implementation approaches

### Files Modified
- `Menus/FavoritesMenu.cs` - Button sizing, click handler, rename callback
- `Docs/Enhancement_Random_Outfit_Names.md` - New documentation file

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-27 11:30 - UI Polish: Buttons and NamingMenu

**Task:** Fix button hover scaling, add shading, fix NamingMenu overlap/centering

### Changes Made

**1. Fixed Button Hover Scaling** (`FavoritesMenu.cs`)
- Changed button `baseScale` from 3f/4f to 1f
- With baseScale=1, hover adds 0.1 to get scale=1.1 → **10% visible growth**
- Previous baseScale values caused only 2.5-3.3% growth (imperceptible)
- Both buttons now properly expand from center with visible animation

**2. Added Beveled Button Background** (`FavoritesMenu.cs`)
- Changed from plain tan texture `Game1.mouseCursors` Rectangle(384, 373, 18, 18)
- Now uses `Game1.menuTexture` Rectangle(0, 256, 60, 60) - standard button with built-in shading
- Same texture used by "Save Outfit" button for visual consistency

**3. Fixed NamingMenu Banner/TextBox Overlap** (`FavoritesMenu.cs`)
- Banner is 72px tall (18px × 4 scale), bottom at height/2 - 56
- Previous 64px shift caused 8px overlap with banner
- Reduced shift to 40px, creating 16px gap between banner bottom and textbox top

**4. Re-centered NamingMenu (TextBox Only)** (`FavoritesMenu.cs`)
- Previously centered the entire assembly (textbox + buttons)
- Now centers just the textbox relative to screen
- Buttons positioned to the right of centered textbox with same gaps

**5. Created Enhancement Doc for Custom Gamepad Keyboard** (`Docs/Enhancement_Custom_Gamepad_Keyboard.md`)
- Documents future enhancement to create custom OutfitTextEntryMenu
- Would show "Name This Outfit" title and outfit preview for gamepad users
- Deferred until controller playtesting to assess actual need

### Files Modified
- `Menus/FavoritesMenu.cs` - Button init, drawing, NamingMenu positioning
- `Docs/Enhancement_Custom_Gamepad_Keyboard.md` - New documentation file

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-27 10:15 - NamingMenu Vertical Spacing Reduction

**Task:** Reduce vertical gap between title banner and textbox in NamingMenu by half

### Changes Made

**1. Reduced Vertical Spacing** (`FavoritesMenu.cs`)
- Default NamingMenu has 128px gap between title banner and textbox
- Added vertical shift of 64px to move textbox and buttons closer to banner
- Now the gap is ~64px (half the original)

### Gamepad Virtual Keyboard Compatibility

Researched how Stardew handles gamepad text entry:
- **NamingMenu**: Static layout, positions textbox at viewport center
- **TextEntryMenu**: Separate overlay created by `Game1.showTextEntry()` for gamepad
- **TextEntryMenu handles its own positioning** - moves textbox 96px above virtual keyboard independently
- Our vertical shift does NOT affect gamepad keyboard display since TextEntryMenu positions independently

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 15:30 - Rename/Delete Button Improvements

**Task:** Use Maru's Wrench for rename icon, 32x32 icons, hover scaling for both buttons

### Changes Made

**1. Maru's Wrench Icon for Rename Button** (`FavoritesMenu.cs`)
- Added `weaponsTexture` field to load `TileSheets/weapons` texture
- Changed rename button icon from edit/pencil to Maru's Wrench (weapon ID 36)
- Sprite coordinates: Rectangle(64, 64, 16, 16) - calculated from ID 36 in 8-wide grid

**2. Icon Size Restored to 32x32** (`FavoritesMenu.cs`)
- Both rename and delete icons now base at 32x32 (was 28x28)
- Scaling is applied on hover, so they grow from this base size

**3. Hover Scaling for Both Buttons** (`FavoritesMenu.cs`)
- Both buttons now properly scale on hover (border AND icon)
- Uses `scale / baseScale` ratio to calculate scaled dimensions
- Box background and icon interior both enlarge together
- Scaling smoothly animates like the OK button

### Technical Details
- Weapon ID 36 (Maru's Wrench) located at row 4, col 4 in the 8-column weapons spritesheet
- Hover scaling: `float renameScaleFactor = renameButton.scale / renameButton.baseScale;`
- Scaled dimensions applied to both `drawTextureBox` and `b.Draw` for icon

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Delete Button Restyle to Match Wrench

**Task:** Make delete button visually match the wrench button style

### Changes Made

**1. Resized Delete Button to Match Wrench** (`FavoritesMenu.cs`)
- Changed delete button from 48x48 to 56x56 (same as wrench button)
- Both buttons now have the same outer dimensions

**2. Lighter Background Texture** (`FavoritesMenu.cs`)
- Changed from thick wooden border (`Game1.menuTexture` Rectangle(0, 256)) to light tan background
- Now uses `Game1.mouseCursors` Rectangle(384, 373, 18, 18) - same tan texture as main menu window
- No extra padding around the box - draws at exact button bounds

**3. Better Icon Padding** (`FavoritesMenu.cs`)
- Icon now drawn at 32x32 with 12px padding on each side (previously 36x36 with 6px padding)
- More breathing room inside the button to match how the wrench icon sits in its button

### Visual Changes
- Both buttons now 56x56 with same outer edge alignment
- Delete button has lighter tan/beige background like the wrench
- Delete icon has more interior padding, less cramped appearance

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - NamingMenu UI Polish Round 2

**Task:** Fix hover text persistence, wrench icon button styling, and NamingMenu centering

### Changes Made

**1. Clear Hover Text on Rename Click** (`FavoritesMenu.cs`)
- Added `hoverText = ""` immediately after clicking the rename button
- Prevents "Rename Outfit" text from being dragged around while NamingMenu is open

**2. Wrench Icon Without Button Box** (`FavoritesMenu.cs`)
- Removed the `drawTextureBox` background from the wrench button (it's self-contained)
- Made rename button bounds 56x56 (same size as the old box background)
- Draw the wrench icon at full button size: `b.Draw(Game1.mouseCursors2, renameButton.bounds, ...)`
- Adjusted delete button position to account for new rename button size

**3. NamingMenu Proper Centering** (`FavoritesMenu.cs`)
- Calculate total width of the entire assembly: textbox + gap + OK button + gap + random button
- Center the assembly as a unit under the banner (not just the textbox)
- Increased gap between textbox and OK button from 16px to 24px to prevent overlap
- Layout: `[TextBox (384)] -- gap (24) -- [OK (64)] -- gap (8) -- [Random (64)]`

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - NamingMenu Polish & Wrench Icon

**Task:** Fix five issues with the rename functionality

### Changes Made

**1. Fix Hover Text Persisting During NamingMenu** (`FavoritesMenu.cs`)
- Added guard at start of `performHoverAction` to return early when `GetChildMenu() != null`
- Clears hoverText and outfitSlotHovered before returning to prevent stale values

**2. Fix NamingMenu Button Layout** (`FavoritesMenu.cs`)
- After widening the textbox, reposition the OK and random buttons:
  - `doneNamingButton.bounds.X = textBox.X + textBox.Width + 16`
  - `randomButton.bounds.X = doneNamingButton.bounds.X + doneNamingButton.bounds.Width + 8`
- Buttons now appear to the right of the wider textbox instead of overlapping it

**3. Fix Name Truncation When Reopening NamingMenu** (`FavoritesMenu.cs`)
- Set `namingMenu.textBox.limitWidth = false` to disable truncation
- Re-set `namingMenu.textBox.Text` after disabling limitWidth to ensure full name appears
- The truncation was caused by NamingMenu constructor setting text before we could modify width/limitWidth

**4. Change Edit Icon to Wrench** (`FavoritesMenu.cs`)
- Changed rename button from appearance icon (96, 208, 16, 16) to wrench icon (154, 154, 20, 20)
- Updated both the button definition and the draw code to use the new icon coordinates
- Adjusted scale from 3f to 2.4f to match the 20x20 source size

**5. Verified Custom Names Persist Across Days**
- Confirmed `FavoriteOutfit.Name` property is part of the serialized model
- `saveFavoritesDataToFile()` is called in `OnDayEnding` event handler
- Data saves to JSON file keyed by player name, save folder, and multiplayer ID

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Longer Outfit Names & Dynamic Hover Box

**Task:** Allow longer outfit names and scale hover box to fit

### Changes Made

**1. Increased NamingMenu Character Limit** (`FavoritesMenu.cs`)
- After creating NamingMenu, configure its textBox properties:
  - `textBox.textLimit = 32` (allows up to 32 characters instead of ~12-14)
  - `textBox.Width = 384` (increased from 256 to fit more characters)
  - Re-centered the textbox with new width

**2. Dynamic Hover Infobox Width** (`FavoritesMenu.cs`)
- Changed `DrawOutfitHoverInfobox` to calculate box width based on the larger of:
  - Grid width (3 × 64 + padding = 224px) - for the item slots
  - Name width (measured via `Game1.smallFont.MeasureString`) - for long names
- Item grid is now centered horizontally if the box is wider than the grid
- Removed name text scaling - text displays at full size since box expands to fit

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Fix NamingMenu E Key Dismissal (Root Cause)

**Task:** Fix E key closing entire menu while typing in NamingMenu

### Root Cause
The `OnButtonsChanged` event handler in `StardewOutfitManager.cs` was intercepting ALL menu/cancel button presses and calling `cleanMenuExit()` without checking if a child menu (like NamingMenu) was active. This bypassed the game's normal input dispatch which correctly sends input to child menus.

### Changes Made

**1. Child Menu Check in Input Handler** (`StardewOutfitManager.cs`)
- Added check for `menuManager.activeManagedMenu.GetChildMenu() != null` before intercepting menu buttons
- When a child menu is active, the input handler now lets the game's normal input dispatch handle the key (which sends it to NamingMenu)
- Also skip passing buttons to `handleTopBarInput` when child menu is active

### Files Modified
- `StardewOutfitManager.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Rename Button Polish & Input Handling

**Task:** Fix button spacing, hotkey dismissal issues, and verify multiplayer/gamepad support

### Changes Made

**1. Button Spacing** (`FavoritesMenu.cs`)
- Increased gap between rename and delete buttons from 8px to 16px
- Updated both constructor and window resize handler

**2. Child Menu Input Blocking** (`FavoritesMenu.cs`)
- Added `GetChildMenu()` checks to all input handlers to prevent parent menu from processing input while NamingMenu is active
- Affected methods:
  - `receiveKeyPress()` - prevents E key from closing menu
  - `receiveGamePadButton()` - prevents gamepad input interference
  - `receiveLeftClick()` - prevents accidental clicks
  - `receiveScrollWheelAction()` - prevents scroll interference

**3. Multiplayer/Gamepad Verification**
- Confirmed PerScreen state management for per-player data
- Confirmed unique favorites data per player (name + save folder + multiplayer ID)
- Confirmed mutex locking for dresser access
- Buttons have proper myID values (9997, 9998) and neighbor IDs for gamepad navigation

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Rename Button & Delete Button Improvements

**Task:** Add rename functionality and fix various UI issues for delete/rename buttons

### Changes Made

**1. Rename Button UI** (`FavoritesMenu.cs`)
- Added `renameButton` ClickableTextureComponent positioned above the delete button
- Uses the appearance/customize icon from `Game1.mouseCursors2` at (96, 208, 16, 16)
- Button only visible when an outfit is selected
- Shows "Rename Outfit" hover text

**2. Rename Button Click Handler** (`FavoritesMenu.cs`)
- Click opens Stardew's standard `NamingMenu` as a child menu overlay
- Uses `SetChildMenu()` to overlay the NamingMenu on top of FavoritesMenu (menu stays visible behind)
- On rename completion:
  - Updates `modelOutfit.Name` in the data model
  - Updates cached `outfitName` in the slot for immediate hover display
  - Plays "coin" sound effect

**3. NamingMenu Callback** (`FavoritesMenu.cs`)
- `OnOutfitRenamed(string newName)` handles the callback
- Clears child menu via `SetChildMenu(null)` to return to FavoritesMenu
- Empty/whitespace names are ignored (original name kept)

**4. Hover Display Fix** (`FavoritesMenu.cs`)
- The hover infobox now shows renamed outfit names immediately
- Both the data model (`modelOutfit.Name`) and cached display name (`outfitName`) are updated on rename

### Icon Coordinates Used
- Rename button: `Game1.mouseCursors2` Rectangle(96, 208, 16, 16) - appearance/customize icon
- Delete button: `Game1.mouseCursors` Rectangle(322, 498, 12, 12) - cancel/X icon

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

---

## 2025-12-26 - Delete Outfit Button

**Task:** Add ability to delete favorite outfits from the Favorites tab

### Changes Made

**1. Delete Button UI** (`FavoritesMenu.cs`)
- Added `deleteButton` ClickableTextureComponent field
- Button positioned to the right of the portrait preview box
- Uses the cancel/X icon from `Game1.mouseCursors` at (322, 498)
- Button only visible when an outfit is selected
- Shows "Delete Outfit" hover text

**2. Delete Button Click Handler** (`FavoritesMenu.cs`)
- Click triggers `OutfitSlot.Delete()` method (already existed)
- Plays "trashcan" sound effect on deletion
- Delete method internally:
  - Calls `ResetSelectedOutfit()` to show player's current outfit on default background
  - Removes outfit from `FavoritesData` via `DeleteOutfit()` extension method
  - Removes outfit slot from UI list
  - Re-filters, sorts, and repositions remaining outfits

**3. Hover and Scale Animation** (`FavoritesMenu.cs`)
- Button scales up on hover (matches okButton behavior)
- Only responds to hover when an outfit is selected

**4. Window Resize Support** (`FavoritesMenu.cs`)
- Delete button repositions correctly on window resize

### Behavior
- Select any outfit in the grid to see the delete button appear
- Click delete button to permanently remove the outfit
- After deletion, preview returns to player's current equipment on default background
- Outfit data saved at end of day (existing save behavior, no changes needed)

### Files Modified
- `Menus/FavoritesMenu.cs`

### Build Status
- **Build succeeded with 0 warnings**

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
