# UI Improvements Enhancement Plan

## Overview
This plan covers user interface improvements to enhance usability and polish.

---

## Phase 1: Game Window Resize Handling

### Problem
Both WardrobeMenu and FavoritesMenu do not properly handle game window resizing. UI elements become misaligned or clip outside the viewport.

### Files Affected
- `Menus/WardrobeMenu.cs` (lines 602-614)
- `Menus/FavoritesMenu.cs` (lines 920-932)

### Current State
Both files have placeholder TODOs:
```csharp
// *TODO* Game Window Resize
public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
{
    base.gameWindowSizeChanged(oldBounds, newBounds);
    // TODO: Reposition buttons
    // TODO: Reposition tabs?
}
```

### Implementation Approach
1. Store relative positions or margins for all UI elements
2. In `gameWindowSizeChanged()`:
   - Recalculate `xPositionOnScreen` and `yPositionOnScreen` based on new viewport
   - Call a `RepositionElements()` method to update all component bounds
3. Consider extracting layout constants to make repositioning easier

### Testing
- [ ] Resize window while in WardrobeMenu
- [ ] Resize window while in FavoritesMenu
- [ ] Verify all buttons, tabs, and scroll elements remain functional
- [ ] Test with different aspect ratios

---

## Phase 2: Outfit Hover Infobox

### Problem
When hovering over outfit slots in FavoritesMenu, no detailed information is shown about the outfit contents.

### Files Affected
- `Menus/FavoritesMenu.cs` (line 133, draw method)

### Current State
Partial implementation exists with a TODO:
```csharp
// *TODO* Draw infobox if hovered on (or snapped to)
```

### Implementation Approach
1. Track which OutfitSlot is currently hovered (already have `currentlySnapped`)
2. In draw method, after drawing outfit slots:
   - Check if mouse is over any slot OR gamepad has snapped to a slot
   - Draw an infobox showing:
     - Outfit name
     - List of items (hat, shirt, pants, shoes, rings)
     - Missing items highlighted in red/grayed out
     - Last worn date (if tracked)
3. Position infobox to avoid clipping off screen edges

### Design Considerations
- Match Stardew Valley's tooltip/hover box styling
- Consider performance - don't rebuild info string every frame
- Handle keyboard/gamepad navigation (show info for snapped slot)

### Testing
- [ ] Hover over outfit slot shows item list
- [ ] Missing items clearly indicated
- [ ] Infobox doesn't clip off screen edges
- [ ] Works with gamepad navigation

---

## Phase 3: Outfit Renaming

### Problem
Outfits are saved with auto-generated names ("Spring Outfit 1") but users cannot rename them.

### Files Affected
- `Menus/FavoritesMenu.cs` (new rename UI)
- `Data/FavoriteOutfit.cs` (Name property already exists)

### Current State
- Save button in WardrobeMenu auto-generates names
- Outfit names display in FavoritesMenu
- No way to edit names after creation

### Implementation Approach

**Option A: Inline Rename (simpler)**
1. Add a "Rename" button to outfit slot hover/selection
2. Clicking opens a text input overlay
3. Use `Game1.keyboardDispatcher` with a text box component
4. Save changes on enter/confirm

**Option B: Context Menu (more features)**
1. Right-click or button press on outfit slot shows context menu
2. Options: Rename, Delete, Move to Category
3. Selecting Rename opens text input

### Stardew Valley Text Input Pattern
```csharp
// Reference: Game1.keyboardDispatcher, TextBox class
TextBox nameTextBox = new TextBox(...);
nameTextBox.OnEnterPressed += OnRenameConfirmed;
Game1.keyboardDispatcher.Subscriber = nameTextBox;
```

### Testing
- [ ] Can rename any saved outfit
- [ ] Name persists after save/load
- [ ] Empty names rejected or auto-filled
- [ ] Special characters handled properly
- [ ] Works with gamepad (virtual keyboard?)

---

## Implementation Order

1. **Window Resize** - Foundation for other UI work
2. **Hover Infobox** - Improves discoverability
3. **Outfit Renaming** - Quality of life feature

---

## Dependencies

- None external
- Phase 2 and 3 can be done in parallel after Phase 1
