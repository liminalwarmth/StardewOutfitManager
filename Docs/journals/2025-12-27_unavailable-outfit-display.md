# Unavailable Outfit Display Enhancement

**Branch:** `liminalwarmth/unavailable-outfit-display`
**Date:** 2025-12-27

## Summary

Enhanced the display of unavailable outfits to show which specific items are missing. Previously, unavailable outfits just showed a cancel sign on the outfit card with a shadow overlay. Now, the item preview grids show cancel signs on individual missing item slots, and the farmer previews (in outfit cards and rename menu) show the full intended outfit even when items are missing.

## Changes Made

### Data Model Update
- **FavoritesModel.cs**: Added `ItemIds` dictionary to store `QualifiedItemId` for each equipment slot
  - Keys: "Hat", "Shirt", "Pants", "Shoes", "LeftRing", "RightRing"
  - Values: QualifiedItemId like "(H)6", "(S)1001", "(P)0", "(B)505", "(R)516"
  - Enables recreating items for display when they're missing from dresser

### Saving Outfit Data
- **FavoritesMethods.cs - SaveNewOutfit()**: Now stores `QualifiedItemId` for each item alongside the tag
  - Forward-compatible: new outfits get full item recreation support
  - Legacy outfits continue to work but can't recreate missing items for display

### New Helper Method
- **FavoritesMethods.cs - GetOutfitIntendedItems()**: Creates temporary items from stored QualifiedItemIds
  - Used for rendering the "intended" outfit on farmer previews
  - Applies stored dye colors to recreated clothing items
  - Returns null for slots without ItemIds (legacy data)

### OutfitSlot Updates
- Added `outfitIntendedItems` dictionary to store recreated items
- New `DressModelFarmerWithIntendedOutfit()` method dresses the preview farmer with intended items
  - Falls back to available items for legacy outfits without ItemIds
  - Small previews (outfit cards, rename menu) now show full outfit even when items missing

### Visual Updates

#### Hover Infobox Item Grid
- Available items: Normal display
- Missing items with ItemId: Shows item sprite with cancel sign overlay
- Missing items without ItemId (legacy): Shows empty icon with cancel sign
- Empty by design: Shows empty icon (no cancel sign)

#### Equipment Icons Grid (Below Portrait)
- Same behavior as hover infobox
- Cancel signs appear on slots with missing items when an outfit is selected

### Tooltip Updates
- Hovering over missing item slots shows the item tooltip with "(Missing)" suffix appended to the display name
- Legacy outfits without ItemIds show "Missing {slot}" text
- Gamepad navigation also updated with same behavior

### Code Quality Fixes (Code Review Follow-up)
- Fixed logic bug in `isMissingItem` calculation (operator precedence issue)
- Added `hoveredItemIsMissing` field to track when tooltip should show "(Missing)" suffix
- Extracted cancel overlay drawing to `DrawCancelOverlay()` helper method to reduce duplication

## Visual Behavior Summary

| Location | Main Portrait | Outfit Cards | Rename Menu | Item Grids |
|----------|---------------|--------------|-------------|------------|
| Shows | Available only | Intended outfit | Intended outfit | Missing items with cancel |
| Farmer | Partial | Full (if ItemIds) | Full (if ItemIds) | N/A |

## Legacy Compatibility

Outfits saved before this change:
- Continue to work normally
- Can still be equipped (available items only)
- Missing slots show empty icons with cancel signs (can't show actual items)
- Farmer previews show available items only

## Files Modified

- `Models/FavoritesModel.cs`
- `Utils/FavoritesMethods.cs`
- `Menus/FavoritesMenu.cs`
