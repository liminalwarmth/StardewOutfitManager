# Combined Dresser Inventory Feature

## Date: 2025-12-27
## Branch: liminalwarmth/combined-dresser-inventory

## Summary

Added a new config option allowing dressers to share their inventories based on proximity or location. This enables decorative furniture arrangements (like placing a mirror dresser next to a standard dresser) without accidentally splitting clothing across separate inventories.

## Config Option

**DresserInventorySharing** - Controls how dresser inventories are shared:
- **Individual**: Each dresser has its own separate inventory (vanilla behavior)
- **Touching** (default): Dressers that are touching (8-way adjacent, including diagonals) share the same inventory
- **SameBuilding**: All dressers within the same house or cabin share the same inventory

## Implementation Details

### Files Created
- `Utils/DresserLinkingMethods.cs` - Utility class for:
  - Finding linked dressers based on config mode
  - 8-way flood-fill adjacency detection for "Touching" mode
  - Same-building detection for FarmHouse/Cabin
  - Multi-dresser mutex locking and release
  - Combined inventory aggregation
  - Cross-dresser item removal

- `Patches/FurniturePatches.cs` - Harmony patch to prevent pickup of locked dressers

### Files Modified
- `ModConfig.cs` - Added `DresserSharingMode` enum and `DresserInventorySharing` property
- `IGenericModConfigMenuApi.cs` - Added `AddTextOption` method for dropdown config
- `StardewOutfitManager.cs` - GMCM registration, multi-dresser locking in OnMenuRender
- `Managers/MenuManager.cs` - Changed from `dresserObject` to `primaryDresser` + `linkedDressers`, added `GetCombinedDresserItems()`
- `Managers/PlayerManager.cs` - Updated `cleanMenuExit()` to release all linked dresser locks
- `Menus/NewDresserMenu.cs` - Use combined inventory, deposit to primary, withdraw from correct dresser
- `Menus/WardrobeMenu.cs` - Use combined inventory for item lists
- `Menus/FavoritesMenu.cs` - Use combined inventory for item lookup
- `Utils/OutfitMethods.cs` - Updated `ItemExchange` to use linked dressers

## Multiplayer Mutex Behavior

The key challenge was proper mutex locking in multiplayer:

1. **When opening a dresser**: All linked dressers (based on config) are locked simultaneously
2. **While menu is open**: No other player can open any dresser in the linked cluster
3. **On menu close**: All linked dressers are released
4. **Furniture pickup**: Harmony patch on `Furniture.canBeRemoved()` prevents moving/removing any locked dresser

This prevents race conditions where two players could access the same shared inventory via different physical dressers.

## Item Handling

- **Deposit**: Items always go to the primary dresser (the one the player clicked on)
- **Withdraw**: Items are removed from whichever linked dresser actually contains them
- **Display**: All tabs show combined inventory from all linked dressers
- **Favorites**: Item tag lookup works across all linked dressers

## Adjacency Algorithm (Touching Mode)

Uses 8-way flood-fill BFS:
1. Get bounding box of the starting dresser
2. Find all tiles occupied by the dresser
3. Check 8-way adjacent tiles (including diagonals)
4. Find other dressers at adjacent tiles
5. Recursively check their neighbors until no more connected dressers found

## Build Status

Build succeeded with 0 warnings, 0 errors.

## Testing Needed

- [ ] Individual mode works as before
- [ ] Touching mode: two adjacent dressers share inventory
- [ ] Touching mode: diagonal adjacency works
- [ ] Touching mode: cluster of 3+ dressers works
- [ ] Same Building mode: all dressers in farmhouse share
- [ ] Multiplayer: Player 2 cannot open linked dresser while Player 1 has it open
- [ ] Multiplayer: Player 2 CAN open unlinked dresser in same location
- [ ] Multiplayer: Player 2 cannot pick up/move ANY locked dresser in shared cluster
- [ ] Favorites work with combined inventory
- [ ] Wardrobe works with combined inventory
- [ ] Item deposit goes to primary dresser
- [ ] Item withdraw removes from correct dresser
- [ ] Config menu shows dropdown with 3 options
