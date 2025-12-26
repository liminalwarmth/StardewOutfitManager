# Stardew Outfit Manager - Development Journal

Entries are listed newest-first. Review this before starting any task.

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
