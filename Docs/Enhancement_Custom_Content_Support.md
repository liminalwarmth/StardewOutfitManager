# Custom Content Support Enhancement Plan

## Overview
Add support for custom hair styles and accessories from content packs (mods like "Coii's Hair Pack", etc.).

---

## Problem Statement

The mod currently saves hair and accessory as integer indexes:
```csharp
// FavoritesMethods.cs:39-43
outfit.Hair = player.hair.Value;
outfit.HairIndex = "";  // Unused placeholder
outfit.Accessory = player.accessory.Value;
outfit.AccessoryIndex = "";  // Unused placeholder
```

This works for vanilla content where hair/accessory IDs are stable integers. However:
1. Custom content packs add new hair/accessory options
2. These may use string-based ItemIds in SDV 1.6
3. Loading order may affect numeric indexes
4. Saved outfits may reference wrong styles if mods change

---

## Research Required

### Phase 0: Research SDV 1.6 Custom Content APIs

Before implementation, research these questions:

1. **How does SDV 1.6 identify custom hair/accessories?**
   - Are they still integer indexes or string ItemIds?
   - How do content packs register new styles?
   - Check `Data/HairData` and `Data/AccessoryData` if they exist

2. **How do other mods handle this?**
   - Fashion Sense approach
   - Get Glam approach
   - Any SMAPI content pack standards

3. **What's the stable identifier?**
   - UniqueID from content pack manifest?
   - Asset path?
   - Custom modData key?

### Research Resources
- [Content Patcher documentation](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md)
- [SDV 1.6 content changes](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6#Custom_items)
- Fashion Sense source code

---

## Tentative Implementation (pending research)

### Data Model Changes
```csharp
// FavoriteOutfit.cs - add fields for content pack reference
public string HairContentPackId { get; set; }  // e.g., "Coii.HairPack"
public string HairAssetKey { get; set; }       // e.g., "assets/hair_style_42"
public string AccessoryContentPackId { get; set; }
public string AccessoryAssetKey { get; set; }
```

### Saving Logic
```csharp
// When saving outfit, detect if hair/accessory is from content pack
if (IsCustomContent(player.hair.Value))
{
    outfit.HairIndex = GetContentPackReference(player.hair.Value);
}
else
{
    outfit.Hair = player.hair.Value;  // Vanilla - use integer
}
```

### Loading Logic
```csharp
// When loading outfit
if (!string.IsNullOrEmpty(outfit.HairIndex))
{
    // Resolve content pack reference to current index
    int resolvedIndex = ResolveContentPackIndex(outfit.HairIndex);
    if (resolvedIndex >= 0)
    {
        farmer.changeHairStyle(resolvedIndex);
    }
    else
    {
        // Content pack not installed - show warning or use fallback
    }
}
```

---

## Validation Requirements

### Checking Hair/Accessory Validity
```csharp
// FavoritesMethods.cs:110 - existing TODO
// TODO: Need to add checks for hair and Accessory validity
// when/if we're using custom hair and accessory indexes
```

When loading an outfit:
1. Check if the hair style index exists
2. Check if the accessory index exists
3. If either missing, show indicator (similar to missing items)
4. Still allow equipping available pieces

---

## Files to Modify

| File | Changes |
|------|---------|
| `Data/FavoriteOutfit.cs` | Add content pack reference fields |
| `Utils/FavoritesMethods.cs` | Update save/load to handle custom content |
| `Menus/FavoritesMenu.cs` | Show warnings for missing custom content |

---

## Testing Plan

1. Install a hair content pack (e.g., Coii's Hair Pack)
2. Create outfit with custom hair style
3. Save game, reload - verify hair persists
4. Disable hair content pack, reload - verify graceful degradation
5. Re-enable pack, verify outfit works again

---

## Risks

- **Breaking existing saves**: Need migration for existing outfit data
- **Mod compatibility**: Different content packs may use different patterns
- **Performance**: Resolving content pack references on every draw

---

## Status: BLOCKED - Needs Research

This enhancement cannot proceed until Phase 0 research is complete. The implementation details depend heavily on how SDV 1.6 and content packs handle custom hair/accessory identification.
