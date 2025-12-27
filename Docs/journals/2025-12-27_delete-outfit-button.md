# Branch: liminalwarmth/delete-outfit-button

Delete outfit button, rename functionality, and UI polish for FavoritesMenu.

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
