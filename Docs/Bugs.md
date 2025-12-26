# Stardew Outfit Manager - Bug Tracker

Track known issues and bugs here. Mark items as fixed when resolved.

---

## Open Issues

### 1. Custom/Modded Hats Display Incorrectly
**File:** `Utils/ModTools.cs:181-184`
**Severity:** Low
**Description:** The hat sprite lookup parses `Hat.ItemId` as an integer to get the sprite index. Custom/modded hats with non-numeric ItemIds will fall back to sprite index 0 (first hat in the sprite sheet).

**Current behavior:**
```csharp
int hatSpriteIndex = 0;
if (who.hat.Value != null && int.TryParse(who.hat.Value.ItemId, out int parsedIndex))
{
    hatSpriteIndex = parsedIndex;
}
```

**Possible fix:** Look up the hat's actual sprite index from game data rather than parsing the ItemId directly. May need to use `ItemRegistry` or similar SDV 1.6 APIs.

---

### 2. Pre-existing Bugs (from migration plan)

These existed before the SMAPI 1.6 migration:

#### 2.1 FavoritesMenu Display Bug
**File:** `Menus/FavoritesMenu.cs:82`
**Description:** "Activate the outfit slot and display the model outfit" - exact issue unclear from comment

#### 2.2 Sleeve Color Sticky
**File:** `Menus/FavoritesMenu.cs:95`
**Description:** Recolor reflection method not working correctly in 2x draw custom script. Sleeve colors persist incorrectly when switching outfits.

#### 2.3 Rings Handling Issue
**File:** `Utils/OutfitMethods.cs`
**Description:** Unspecified problem with rings - needs investigation

#### 2.4 Custom Content Pack Support
**Files:** `Utils/FavoritesMethods.cs:39, 93`
**Description:** Hair and accessory indexes incomplete for custom content packs

#### 2.5 Performance - Duplicate Lookups
**File:** `Utils/FavoritesMethods.cs:89`
**Description:** Duplicate lookups in favorites matching could be optimized with a dictionary cache

---

### 3. NewDresserMenu Cloned Code
**File:** `Menus/NewDresserMenu.cs`
**Severity:** Technical Debt
**Description:** ~1800 lines of cloned ShopMenu code from SDV 1.5.6. Contains many unused fields that generate build warnings. Consider:
- Refactoring to extend ShopMenu properly if SDV 1.6 allows
- Using callbacks/delegates instead of cloning
- At minimum, removing obviously unused fields

---

## Fixed Issues

(None yet)
