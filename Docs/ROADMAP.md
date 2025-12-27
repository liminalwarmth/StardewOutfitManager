# Stardew Outfit Manager - Future Features Roadmap

## Core Enhancements

### Controller Support Improvements
- Gamepad button remapping via Harmony postfix
- Custom config file for button mappings
- Reference code for `Game1.mapGamePadButtonToKey`:
  ```csharp
  public static Keys mapGamePadButtonToKey(Buttons b)
  {
      return b switch
      {
          Buttons.A => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.actionButton),
          Buttons.X => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.useToolButton),
          Buttons.B => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton),
          // ... etc
          _ => Keys.None,
      };
  }
  ```

### Dresser Tab Enhancements
- More dresser options and filtering
- Sorting/categorization improvements

### Starting Dresser Delivery
- Consider switching from direct inventory add to mail-based delivery
- Would provide a welcome letter with the dresser attached
- More thematic/discoverable for new players
- Uses `Data/Mail` and `Data/TriggerActions` instead of `OnSaveLoaded` code

---

## Content Additions

### Accessories
- More accessory options (check Get Glam mod for inspiration)

### Shoes/Footwear
- Shoe redesigns for different feet styles
- Reference mods:
  - [Male Shoe Redesign](https://www.nexusmods.com/stardewvalley/mods/6108)
  - [Female Shoes](https://www.nexusmods.com/stardewvalley/mods/331)

### Default Textures
- "Nothing Equipped" texture redesign for farmer underwear

### Rings
- Noncombat ring additions:
  - Ring of Friendship (interaction bonuses)
  - Speed boost ring
  - Energy boost to consumed food ring

---

## New Systems

### Tailoring Skill
- Custom tailoring skill progression
- New recipes unlocked via skill

### Dye System Rework
- Colored dye as craftable/purchasable items
- Better dye stacking
- Extract recipe: item + component = 2-3 dyes

### Swimsuit System
- Swimsuit items to trigger swimsuit appearance
- Beach/pool context detection

---

## Mod Integration

### Fashion Sense Auto-Itemizer
- Convert Fashion Sense Content Patcher packs into actual items automatically
- Bridge FS appearance options to dresser inventory

### Clothes Mouse Integration
- [Clothes Mouse mod](https://www.nexusmods.com/stardewvalley/mods/4798)
- Consider adding a "Hat Mouse" tab instead of full integration

---

## Discord References
- https://discord.com/channels/137344473976799233/156109690059751424/1015366511084441600
- https://discord.com/channels/137344473976799233/156109690059751424/1015352756204228638
