# Journal: Enhanced Hair & Accessory Support

**Branch:** `liminalwarmth/medan-v1`
**Date:** 2025-12-27

## Summary

Added dynamic detection for modded hair/accessories, config options for beards and modded content, and validation for saved outfits.

## Work Completed

### Config Options Added (`ModConfig.cs`)
- `IncludeBeardsAsAccessories` (default: false) - Include beards/facial hair in accessory picker
- `IncludeModdedHairstyles` (default: true) - Include modded hairstyles
- `IncludeModdedAccessories` (default: true) - Include modded accessories

### New Utility: `Utils/AccessoryMethods.cs`
- Dynamic accessory detection via texture dimensions
- Beard filtering based on config (indices 0-5 and 20-22)
- Duckbill exclusion (index 19)
- Cache with invalidation on config change or asset reload
- Constants for SDV 1.6 vanilla range (30 accessories, indices 0-29)

### Updated `Utils/OutfitMethods.cs`
- `HairSwap`: Now filters modded hairstyles based on config
- `AccessorySwap`: Uses dynamic list from AccessoryMethods
- `GetHairOrAccessoryName`: Fixed key mapping to match indices directly

### Updated `Utils/FavoritesMethods.cs`
- `IsHairIndexValid()`: Validates hair index exists in current hairstyle list
- `IsAccessoryIndexValid()`: Validates accessory index exists in current range
- `isAvailable()`: Now checks hair/accessory validity before marking outfit available
- `dressDisplayFarmerWithAvailableOutfitPieces()`: Only applies valid hair/accessory
- `equipFavoriteOutfit()`: Only applies valid hair/accessory

### Updated `StardewOutfitManager.cs`
- GMCM registration for new config options
- Cache invalidation when config changes
- Asset invalidation handler for `Characters/Farmer/accessories`

### Updated `Assets/Accessories/BaseGame/AccNames.json`
- Keys now directly match indices (key "0" = index 0)
- Simplified labels: "Accessory 1" through "Accessory 30"
- Covers vanilla range (indices 0-29)

### Updated `Assets/Hair/BaseGame/HairNames.json`
- Keys now directly match indices (key "0" = index 0)
- Covers all 74 vanilla hairstyles (indices 0-73)

## Issues Fixed

### Off-by-one label/visual mismatch
User reported accessory labels were offset from visuals (e.g., "stubble" label showing full beard visual).

**Root Cause:** Two issues combined:
1. `GetHairOrAccessoryName` was adding `value++` before JSON lookup
2. AccNames.json used 1-indexed keys (key "1" = index 0)

**Fix:**
- Removed `value++` from `GetHairOrAccessoryName` in OutfitMethods.cs
- Updated AccNames.json to use direct index keys (key "0" = index 0)
- Updated HairNames.json similarly

### Empty accessory slots included
User reported "Accessory 31" and "Accessory 32" showed no visual.

**Root Cause:** VANILLA_MAX_INDEX was incorrectly set, but indices 30 and 31 are empty texture slots.

**Fix:** Changed VANILLA_MAX_INDEX to 29 in AccessoryMethods.cs.

### Accessory names simplified
User requested simple numbered labels instead of descriptive names.

**Fix:** AccNames.json now uses "Accessory 1" through "Accessory 30" (corresponding to indices 0-29).

## Technical Notes

### SDV 1.6 Accessory Layout
- 30 valid accessories (indices 0-29), plus -1 for None
- Indices 30-31 are empty texture slots
- Beards (via `isAccessoryFacialHair`): 0-5 and 19-22
- Duckbill: 19 (always excluded from picker)
- Non-beard: 6-18 and 23-29

### Texture-based Detection
- Vanilla texture: 4 rows x 8 columns = 32 slots (indices 0-31)
- Indices 30-31 are empty in vanilla
- Modded textures add extra rows
- Detection: if rows > 4, calculate extra accessories

### JSON Key Mapping
- Keys directly match indices
- Key "-1" = None
- Key "0" = index 0 ("Accessory 1")
- Key "29" = index 29 ("Accessory 30")

## Status

Feature complete. All bugs fixed. Ready for PR.
