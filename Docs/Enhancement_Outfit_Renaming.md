# Outfit Renaming Enhancement Plan

## Overview
Allow users to rename saved outfits from the default auto-generated names ("Spring Outfit 1") to custom names.

---

## Current State

- **Data Structure**: `FavoriteOutfit.Name` property already exists and supports custom names
- **Auto-naming**: `GetDisplayName()` in `OutfitSlot` generates position-based names when `Name` is empty
- **Persistence**: JSON serialization already handles the `Name` property
- **Save Flow**: `WardrobeMenu` save button passes empty string for name

---

## Design Considerations

### Button Placement Options

**Option A: Rename Button Next to Delete**
- Add dedicated Rename and Delete buttons visible when an outfit is selected
- Buttons could appear below the outfit card display or in a toolbar area
- Requires UI space allocation

**Option B: Context Menu on Right-Click**
- Right-click (or gamepad button) on outfit slot opens context menu
- Options: Rename, Delete, Move to Category, Toggle Favorite
- More discoverable for multiple actions

**Option C: Inline Edit on Double-Click**
- Double-clicking outfit name enters edit mode
- Simple but may conflict with selection behavior

**Option D: Edit in Hover Infobox**
- Add small edit icon in the hover infobox
- Clicking opens text input overlay

### Text Input Approach

Stardew Valley uses `TextBox` class with `Game1.keyboardDispatcher`:
```csharp
TextBox nameTextBox = new TextBox(...);
nameTextBox.OnEnterPressed += OnRenameConfirmed;
Game1.keyboardDispatcher.Subscriber = nameTextBox;
```

### Gamepad Considerations
- Need virtual keyboard support for console/gamepad users
- Consider using `Game1.showTextEntry()` if available in SDV 1.6

---

## Implementation Requirements

1. **UI Component**: Button or interaction to trigger rename
2. **Text Input Handler**: TextBox creation and keyboard dispatcher binding
3. **Confirmation Logic**: Update `FavoriteOutfit.Name` and refresh display
4. **Validation**: Handle empty names (revert to auto-generated), special characters
5. **State Management**: Track which outfit is being renamed

---

## Files to Modify

- `Menus/FavoritesMenu.cs` - Add rename UI and text input handling
- Possibly `Menus/WardrobeMenu.cs` - Option to name outfit at save time

---

## Testing Checklist

- [ ] Can rename any saved outfit
- [ ] Name persists after save/load game
- [ ] Empty names fall back to auto-generated
- [ ] Special characters handled properly
- [ ] ESC/cancel reverts to previous name
- [ ] Works with mouse
- [ ] Works with gamepad (virtual keyboard)
- [ ] Rename updates display immediately

---

## Dependencies

- None external
- Requires design decision on UI approach before implementation
