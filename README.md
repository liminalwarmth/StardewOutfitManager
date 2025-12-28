# Stardew Outfit Manager

A visual wardrobe mod for Stardew Valley inspired by Get Dressed. Design outfits from all of your clothes, hats, and shoes. Save favorites by season. Quickly swap favorite outfits at your dresser.

## Why This Mod?

A different approach from Fashion Sense. I wanted to keep using Stardew's built-in clothing system—discovering hats in the mines, earning festival gear, buying from the mouse—but with better tools to organize what I'd collected into outfits. This mod works with native game systems rather than replacing them, so your existing wardrobe stays intact and it's more performant.

## What It Does

Replaces the vanilla dresser interface with a three-tab system:

**Wardrobe** — Cycle through hats, shirts, pants, shoes, and rings with arrow buttons while watching your farmer update in a live preview. Change hairstyles and accessories too. Put together a look and save it as a favorite.

**Favorites** — Your saved outfits organized by season (Spring, Summer, Fall, Winter, Special). Each outfit displays as a card showing your farmer wearing it. Missing a piece? Visual indicators show what's unavailable. Click to preview, one button to equip. Outfits get randomly generated seasonal names, or rename them yourself.

**Dresser** — Your dresser inventory with category filters for quickly finding specific items.

## Features

- **New dresser furniture** — 14 new dresser variants including mirror dressers and small dressers for more decorating options. Robin sells them, the Traveling Merchant occasionally stocks mirrors, and new players start with a small dresser.
- **Dresser sharing** — Configure whether dressers have individual inventories, share when touching, or share across an entire building.
- **Modded content support** — Works with modded hairstyles and accessories (configurable). Facial hair toggle available.
- **Rings in outfits** — Optionally include rings when saving and equipping outfits.
- **Multiplayer support** — Each player has their own saved outfits. Mutex locking prevents conflicts.
- **Gamepad support** — Full controller navigation.

## Configuration

Edit `config.json` or use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) to adjust dresser sharing, modded content inclusion, ring handling, and shop integration.

## Installation

1. Install [SMAPI](https://smapi.io/) (4.0+)
2. Drop the mod folder into `Stardew Valley/Mods`
3. Play the game

## Requirements

- Stardew Valley 1.6+
- SMAPI 4.0+

## Building from Source

```bash
git clone https://github.com/yourusername/StardewOutfitManager.git
cd StardewOutfitManager
dotnet build
```

The build automatically deploys to your Mods folder.

## Credits

**Author**: Liminal Warmth

**Special thanks to:**
- Jinxiewinxie and Advize for the Get Dressed Mod which inspired this one
- Elizabeth at the Stardew Valley Discord for base game hair name list
- Minakie for the [Get Dressed dresser retextures](https://www.nexusmods.com/stardewvalley/mods/648)
- Zilchaz for permission to use their beautiful Seasonal Display Backgrounds on the favorites tab
- Atravita, Pathoschild, and the SDV modding Discord community for answering questions and sharing code examples during development
