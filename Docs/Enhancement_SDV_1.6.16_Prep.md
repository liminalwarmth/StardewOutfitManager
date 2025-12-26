# SDV 1.6.16 Preparation Plan

## Overview
Stardew Valley 1.6.16 is an unreleased future version. This plan documents API changes that will break compatibility and the fixes needed when it releases.

**Status:** NOT NEEDED YET - Current mod targets SDV 1.6.15

---

## Breaking Change: addItemToInventoryBool Removal

The `addItemToInventoryBool()` method will be **completely removed** in SDV 1.6.16.

### Current Usage

| File | Line | Current Code |
|------|------|--------------|
| `Menus/NewDresserMenu.cs` | 897 | `Game1.player.addItemToInventoryBool(item)` |
| `Menus/NewDresserMenu.cs` | 1141 | `Game1.player.addItemToInventoryBool(item)` |
| `Menus/NewDresserMenu.cs` | 1484 | `Game1.player.addItemToInventoryBool(item)` |
| `Managers/MenuManager.cs` | 114 | `Game1.player.addItemToInventoryBool(item)` |

### Required Fix

Replace all occurrences with the new `TryAddToInventory()` method:

```csharp
// OLD:
Game1.player.addItemToInventoryBool(item)

// NEW:
Game1.player.TryAddToInventory(item).AnyAdded
```

### Search Pattern
```bash
grep -rn "addItemToInventoryBool" --include="*.cs"
```

---

## Implementation Steps

### When SDV 1.6.16 Releases

1. [ ] Update `manifest.json` MinimumGameVersion if needed
2. [ ] Search for all `addItemToInventoryBool` usages
3. [ ] Replace with `TryAddToInventory().AnyAdded`
4. [ ] Build and test
5. [ ] Update version number and release

### Testing Checklist
- [ ] Items transfer correctly from dresser to inventory
- [ ] Items transfer correctly from inventory to dresser
- [ ] Full inventory handling works (items drop on ground)
- [ ] No duplicate items created

---

## Reference
- [SDV 1.6.16 Migration Guide](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6.16)

---

## Status: DEFERRED

Do not implement until SDV 1.6.16 is released. The current code works fine with SDV 1.6.15.
