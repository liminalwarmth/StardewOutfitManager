# Enhancement: Randomized Seasonal Outfit Names

## Overview

Currently, the random button on the outfit naming dialog generates generic random names. This enhancement would generate thematic outfit names based on the outfit's season category.

## Current Behavior

The NamingMenu's random button uses the game's default random name generation (designed for farm/animal names), which doesn't fit the context of outfit naming.

## Proposed Enhancement

Generate seasonal/thematic outfit names that vary by the outfit's category:

### Spring Names
- "Spring Bloom", "Cherry Blossom", "April Showers", "Garden Party"
- "Flower Festival", "Dewdrop Morning", "Pastel Dream", "Rainy Day"
- "Fresh Start", "Salmonberry Stroll", "Egg Hunt", "Sprout"

### Summer Names
- "Summer Breeze", "Beach Day", "Luau Look", "Sunny Side"
- "Ocean Waves", "Starfruit Sunset", "Festival Night", "Firefly"
- "Tropical Getaway", "Melon Fresh", "Jellyfish Dance", "Heatwave"

### Fall Names
- "Autumn Harvest", "Pumpkin Spice", "Cozy Flannel", "Falling Leaves"
- "Scarecrow Chic", "Mushroom Forager", "Fair Day", "Amber Glow"
- "Cranberry Crunch", "Spirit's Eve", "Harvest Moon", "Rustic"

### Winter Names
- "Winter Wonderland", "Snowflake", "Frost Bite", "Cozy Cabin"
- "Ice Festival", "Night Market", "Fireplace Glow", "Hibernation"
- "Feast of Winter Star", "Frozen Lake", "Hot Cocoa", "Blizzard"

### Special Names
- "Festival Ready", "Stardrop", "Galaxy Glam", "Prismatic"
- "Wizard's Apprentice", "Junimo Joy", "Valley Legend", "Mayor's Gala"
- "Wedding Day", "Adventurer's Gear", "Starlight", "Lucky Purple"

## Implementation Approach

### Option 1: Patch NamingMenu's Random Button

Use Harmony to patch the random button's click handler when it's our outfit naming context:

```csharp
[HarmonyPatch(typeof(NamingMenu))]
public static class NamingMenuPatch
{
    // Patch the randomButton click to use our seasonal names
    // Would need to identify if this is our outfit naming menu
}
```

### Option 2: Create Custom NamingMenu

Create `OutfitNamingMenu` that extends NamingMenu and overrides the random button behavior:

```csharp
public class OutfitNamingMenu : NamingMenu
{
    private string _outfitCategory;

    public OutfitNamingMenu(doneNamingBehavior b, string title, string defaultName, string category)
        : base(b, title, defaultName)
    {
        _outfitCategory = category;
        // Replace randomButton's click handler
    }

    private string GetRandomSeasonalName()
    {
        var names = GetNamesForCategory(_outfitCategory);
        return names[Game1.random.Next(names.Count)];
    }
}
```

### Option 3: Replace Random Button After Creation

After creating NamingMenu, replace the random button's texture component with our own that calls custom logic:

```csharp
NamingMenu namingMenu = new NamingMenu(...);
// Store reference to outfit category
// Replace or patch the randomButton click behavior
```

## Data Structure

```csharp
public static class SeasonalOutfitNames
{
    public static readonly Dictionary<string, List<string>> NamesByCategory = new()
    {
        ["Spring"] = new List<string> { "Spring Bloom", "Cherry Blossom", ... },
        ["Summer"] = new List<string> { "Summer Breeze", "Beach Day", ... },
        ["Fall"] = new List<string> { "Autumn Harvest", "Pumpkin Spice", ... },
        ["Winter"] = new List<string> { "Winter Wonderland", "Snowflake", ... },
        ["Special"] = new List<string> { "Festival Ready", "Stardrop", ... }
    };

    public static string GetRandomName(string category)
    {
        if (NamesByCategory.TryGetValue(category, out var names))
        {
            return names[Game1.random.Next(names.Count)];
        }
        return $"{category} Outfit {Game1.random.Next(1, 100)}";
    }
}
```

## Considerations

1. **Uniqueness**: Should check if name is already used by another outfit in same category
2. **Localization**: Name lists would need translation support for non-English players
3. **Extensibility**: Could allow players to add custom name lists via config file
4. **Context**: Need to pass outfit category from FavoritesMenu to NamingMenu

## Recommended Approach

**Option 3** (Replace Random Button After Creation) is simplest:
1. After creating NamingMenu, store the current outfit's category
2. Create a custom click handler that generates seasonal names
3. Attach to the existing randomButton

This avoids Harmony patches and doesn't require a full menu subclass.

## Status

**Priority**: Low (nice-to-have feature for flavor)

**Dependencies**: None - can be implemented independently
