# Journal: rings-config-option branch

## 2025-12-27: Implemented IncludeRingsInOutfits config option

### Summary
Added a new config option `IncludeRingsInOutfits` (default: `true`) that controls whether rings are included in outfit management. When disabled, rings are excluded from saving/loading, availability checks, equipping, and the ring slots are hidden from the UI entirely.

### Changes Made

**ModConfig.cs**
- Added `IncludeRingsInOutfits` boolean property with default `true`

**StardewOutfitManager.cs**
- Added GMCM registration for the new option under "Wardrobe Options" section

**Utils/FavoritesMethods.cs**
- `SaveNewOutfit()`: Conditionally add ring slots only when enabled
- `outfitExistsInFavorites()`: Skip ring comparison when disabled; switched to `GetValueOrDefault()` for null-safe access (backwards compatibility)
- `isAvailable()`: Skip ring slots when checking availability
- `GetOutfitItemAvailability()`: Skip ring slots in availability dictionary
- `dressDisplayFarmerWithAvailableOutfitPieces()`: Only modify display farmer's rings when enabled
- `equipFavoriteOutfit()`: Skip ring slots in both unequip and equip loops

**Utils/OutfitMethods.cs**
- `ItemExchange()`: Added config guard to ring slot handling for defensive programming

**Menus/FavoritesMenu.cs**
- Constructor: Dynamic grid layout (3 cols with rings, 2 cols without), conditional ring slot creation, updated neighbor IDs
- `DrawOutfitHoverInfobox()`: Dynamic grid (2x3 or 2x2), conditional slot keys and empty icons
- `gameWindowSizeChanged()`: Dynamic grid repositioning

**Menus/WardrobeMenu.cs**
- Constructor: Dynamic grid layout, conditional ring slot creation
- `gameWindowSizeChanged()`: Dynamic grid repositioning

### Backwards Compatibility
- Existing outfits with ring data: Ring data preserved in JSON, simply ignored when disabled
- Outfits saved without rings: When re-enabled, empty ring slots shown (null-safe access via `GetValueOrDefault()`)
- Mixed state handled naturally by null-safe patterns

### Build Status
- `dotnet build`: 0 errors, 0 warnings
