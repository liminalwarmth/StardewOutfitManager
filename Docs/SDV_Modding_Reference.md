# Stardew Valley 1.6 Modding Reference

Quick reference for SMAPI 4.x / SDV 1.6+ development. Focuses on patterns that prevent common errors.

---

## Project Setup

```xml
<ItemGroup>
  <PackageReference Include="Pathoschild.Stardew.ModBuildConfig" Version="4.*" />
</ItemGroup>

<PropertyGroup>
  <TargetFramework>net6.0</TargetFramework>
  <EnableHarmony>true</EnableHarmony>
  <!-- <GamePath>C:\...\Stardew Valley</GamePath> -->
</PropertyGroup>
```

**manifest.json:** Set `"MinimumApiVersion": "4.0.0"` for SMAPI 4.x compatibility.

```csharp
public class MyMod : Mod
{
    internal static IMonitor ModMonitor;
    internal static IModHelper ModHelper;
    internal static ModConfig Config;
    internal static string ModId;

    public override void Entry(IModHelper helper)
    {
        ModMonitor = Monitor;
        ModHelper = helper;
        ModId = ModManifest.UniqueID;
        Config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Content.AssetReady += OnAssetReady;

        // Harmony: use code API, not PatchAll() with annotations
        var harmony = new Harmony(ModManifest.UniqueID);
        MyPatches.Apply(harmony);
    }
}
```

**Cross-platform:** Use `Path.Combine` and `Helper.DirectoryPath`; don't infer mod folder from assembly location. Use `Constants.DataPath` for the game's data folder.

---

## State Gating (Prevent NullRefs)

```csharp
if (!Context.IsWorldReady) return;      // World not loaded yet
if (!Context.IsPlayerFree) return;      // Player in menu/cutscene
if (!Context.CanPlayerMove) return;     // Player can't act
if (!Context.IsMainPlayer) return;      // Host-only logic (MP)
// Split-screen: use Context.ScreenId + PerScreen<T>
```

---

## Critical 1.6 Breaking Changes

### Item Identification
```csharp
// OLD (broken): numeric index matches multiple types
if (item.ParentSheetIndex == 128) { }

// NEW: QualifiedItemId is globally unique
if (item.QualifiedItemId == "(O)128") { }

// Type prefixes (common vanilla - see Modding:Items for authoritative list):
// (O)Object  (BC)BigCraftable  (F)Furniture  (W)Weapon  (B)Boots
// (H)Hat  (P)Pants  (S)Shirt  (T)Tool  (TR)Trinket  (M)Mannequin
// (WP)Wallpaper  (FL)Flooring

// Prefer TypeDefinitionId for type checks:
if (item.TypeDefinitionId == ItemRegistry.type_object) { }

// ID sanity helpers:
if (ItemRegistry.HasItemId(item, "128")) { }      // Works with qualified OR unqualified
string qid = ItemRegistry.QualifyItemId("128");   // Returns "(O)128"
if (!ItemRegistry.Exists("(O)128")) return;       // Validate before creating

// GOTCHA: QualifyItemId resolves by searching types in order until found
// If IDs exist in multiple types, use explicit prefix OR specify type:
string bcId = ItemRegistry.QualifyItemId("128", ItemRegistry.type_bigCraftable);  // "(BC)128"
```

### Item Creation
```csharp
// OLD: new Object(128, 1);
// NEW:
Item item = ItemRegistry.Create("(O)128");
Item stack = ItemRegistry.Create("(O)128", amount: 5);

// GOTCHA: Invalid IDs return Error Item by default
Item safe = ItemRegistry.Create("(O)999999");                    // Returns Error Item
Item nullable = ItemRegistry.Create("(O)999999", allowNull: true); // Returns null
Hat hat = ItemRegistry.Create<Hat>("(H)6");                      // Typed creation

// Manual constructors take ItemId (unqualified), NOT QualifiedItemId:
Item obj = new StardewValley.Object("128", 1);  // Correct
// Item obj = new StardewValley.Object("(O)128", 1);  // WRONG
// Same applies to Hat("6"), Clothing("1"), etc.

// Debug: use `list_items` console command to look up IDs in-game
```

### Inventory Operations
```csharp
// OLD: Game1.player.getItemCount(128);
// NEW:
int count = Game1.player.Items.CountId("(O)128");
bool has = Game1.player.Items.ContainsId("(O)128", minCount: 5);
Game1.player.Items.ReduceId("(O)128", count: 10);
```

### HashSet Fields (No Longer Lists)
```csharp
// OLD: for loop with RemoveAt
// NEW: set operations
Game1.player.mailReceived.RemoveWhere(id => id.StartsWith($"{ModId}_"));
Game1.player.mailReceived.Add($"{ModId}_Flag");
// Affected: mailReceived, eventsSeen, achievements, professions, etc.
```

### Item Categories (Use Constants)
```csharp
// Prefer constants over magic numbers
if (item.Category == Object.hatCategory) { }       // -95
if (item.Category == Object.ringCategory) { }     // -96
if (item.Category == Object.bootsCategory) { }    // -97
if (item.Category == Object.clothingCategory) { } // -100
// 1.6+: Object.trinketCategory, Object.booksCategory, Object.skillBooksCategory

// For clothing subtypes:
if (item is Clothing c && c.clothesType.Value == Clothing.ClothesType.SHIRT) { }
```

### Flavored Items Gotcha
```csharp
// For wine/jelly/etc: base item shares ID; flavor is in preservedParentSheetIndex OR heldObject.
// Despite the name, preservedParentSheetIndex is the *flavor item ID* (not ParentSheetIndex).
if (item is Object obj && obj.preservedParentSheetIndex.Value == "454") { /* Ancient Fruit */ }
```

---

## SMAPI Events

| Event | Use Case |
|-------|----------|
| `GameLoop.GameLaunched` | Mod integrations (GMCM), runs once |
| `GameLoop.SaveLoaded` | Per-save init, player available |
| `GameLoop.ReturnedToTitle` | Cleanup/reset per-save state |
| `GameLoop.DayStarted` | Daily logic, world loaded |
| `GameLoop.DayEnding` | Save data before day ends |
| `GameLoop.Saving` | Raised for everyone; gate save writes with `Context.IsMainPlayer` |
| `GameLoop.Saved` | After save completes |
| `GameLoop.UpdateTicked` | Periodic logic (~60/sec) - use `e.IsMultipleOf(n)` |
| `GameLoop.OneSecondUpdateTicked` | Less frequent periodic logic |
| `Display.MenuChanged` | Intercept/replace menus |
| `Display.Rendered` | Draw over everything |
| `Input.ButtonPressed` | Single button press detection |
| `Input.ButtonsChanged` | Multiple simultaneous inputs |
| `Content.AssetRequested` | Load/edit game assets |
| `Content.AssetReady` | React when asset reloads |
| `Content.AssetsInvalidated` | Clear caches when any mod invalidates |
| `Content.LocaleChanged` | Clear cached translations on language change |

**Notes:**
- Events use a snapshot; if you need current state, re-check `Game1.*` (esp. `activeClickableMenu`)
- Use `e.IsMultipleOf(30)` in `UpdateTicked`, or use `OneSecondUpdateTicked` for less frequent checks

---

## Input API

```csharp
// Suppress input (alternative to Harmony for blocking actions)
Helper.Input.Suppress(SButton.MouseLeft);

// Semantic button checks
if (button.IsUseToolButton()) { }
if (button.IsActionButton()) { }
```

**GOTCHA:** Don't evaluate keybind combos in `ButtonPressed`; use `ButtonsChanged` instead. `ButtonPressed` fires once per physical button, so combos can trigger multiple times.

---

## Content API

### Load Your Own Files (Not Patchable)
```csharp
// Use ModContent for files you don't need other mods to patch
Texture2D tex = Helper.ModContent.Load<Texture2D>("assets/sprite.png");
var data = Helper.ModContent.Load<Dictionary<string, string>>("assets/data.json");
```

### Expose Patchable Assets via AssetRequested
```csharp
// Only use this when you WANT other mods to be able to patch your assets
private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
{
    if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{ModId}/PatchableData"))
        e.LoadFromModFile<Dictionary<string, string>>("assets/data.json", AssetLoadPriority.Low);
}

// IMPORTANT: When consuming patchable assets, load via GameContent (not ModContent)
// so other mods' patches apply:
var patchedData = Helper.GameContent.Load<Dictionary<string, string>>($"Mods/{ModId}/PatchableData");
```

### Pass Mod Assets to Game Code
```csharp
// If game code needs an asset key (e.g. tilesheet ImageSource), use:
string assetKey = Helper.ModContent.GetInternalAssetName("assets/tilesheet.png").Name;
```

### Edit Game Data
```csharp
if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
{
    e.Edit(asset => {
        var data = asset.AsDictionary<string, ObjectData>().Data;
        data[$"{ModId}_Item"] = new ObjectData { /* ... */ };
    });
}
```

### Edit Image
```csharp
e.Edit(asset => {
    var editor = asset.AsImage();
    IRawTextureData patch = Helper.ModContent.Load<IRawTextureData>("assets/patch.png");
    editor.PatchImage(patch, targetArea: new Rectangle(0, 0, 64, 64));
});
```

### Cache Invalidation & AssetReady
```csharp
Helper.GameContent.InvalidateCache("Data/Objects");
// GOTCHA: For non-English, ensure your invalidation hits localized variants too

// Subscribe in Entry(), refresh caches when assets reload
private void OnAssetReady(object sender, AssetReadyEventArgs e)
{
    if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{ModId}/PatchableData"))
        _cachedData = null;  // Force reload on next access
}
// Note: AssetReady only fires when something requests the asset; invalidation alone doesn't trigger it
```

---

## Data Persistence

| Method | Scope | Use For |
|--------|-------|---------|
| `player.modData[key]` | Per-player, syncs in MP | Player flags (prefix with UniqueID!) |
| `Helper.Data.ReadSaveData<T>()` | Per-save, mod-scoped | Global save progression (host only!) |
| `Helper.Data.ReadGlobalData<T>()` | Per-computer, mod-scoped | Machine-wide settings/cache |
| `Helper.Data.ReadJsonFile<T>()` | Mod folder file | Config-like data, content packs |

**Notes:**
- `modData` keys need UniqueID prefix; `ReadSaveData`/`ReadGlobalData` keys are already mod-scoped
- `ReadSaveData`/`WriteSaveData` only work for host; farmhands see `Saving` event but can't use save API
- Item stack split/merge copies modData in non-obvious ways

```csharp
// modData example (always use UniqueID prefix)
Game1.player.modData[$"{ModId}/Flag"] = "true";

// SaveData example (no prefix needed - already scoped to your mod)
var data = Helper.Data.ReadSaveData<MyData>("progression");
```

---

## Dates (Prevent Calendar Math Bugs)

```csharp
var today = SDate.Now();
if (today >= new SDate(15, "summer")) { }
var tomorrow = today.AddDays(1);
```

---

## UI/Menu Development

### IClickableMenu Checklist
- Inherit `IClickableMenu`, call base constructor with dimensions
- Center: `Utility.getTopLeftPositionForCenteringOnScreen(width, height)`
- Override: `draw()`, `receiveLeftClick()`, `performHoverAction()`
- Recalc positions in `gameWindowSizeChanged()`
- Call `populateClickableComponentList()` + `snapToDefaultClickableComponent()` for gamepad
- Use `Game1.uiViewport` for UI coords, not `Game1.viewport`
- Never load textures in `draw()` - cache them

### Components & Controller Nav
```csharp
// Basic region
var btn = new ClickableComponent(new Rectangle(x, y, w, h), "name");

// With texture
var texBtn = new ClickableTextureComponent("name", new Rectangle(x, y, 64, 64),
    null, "hover text", Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f);

// Controller nav (REQUIRED for gamepad support)
btn.myID = 1001;
btn.leftNeighborID = 1000;
btn.rightNeighborID = 1002;
btn.upNeighborID = -99999;  // no neighbor
```

### Common Game1.mouseCursors Regions
```csharp
new Rectangle(352, 495, 12, 11)  // Left arrow
new Rectangle(365, 495, 12, 11)  // Right arrow
new Rectangle(128, 256, 64, 64)  // OK button
new Rectangle(322, 498, 12, 12)  // X icon
new Rectangle(310, 392, 16, 16)  // Star icon
```

### Drawing
```csharp
IClickableMenu.drawTextureBox(b, x, y, width, height, Color.White);
b.DrawString(Game1.smallFont, "Text", new Vector2(x, y), Color.Black);
SpriteText.drawString(b, "Styled", x, y);
drawMouse(b);  // Always last
```

---

## Harmony Patching

**Use as last resort.** Prefer SMAPI events + `Helper.Input.Suppress()`. Avoid `[HarmonyPatch]` annotations + `PatchAll()` - SMAPI can't rewrite them for cross-platform compatibility.

```csharp
public static class MyPatches
{
    private static IMonitor Monitor;

    public static void Apply(Harmony harmony)
    {
        Monitor = MyMod.ModMonitor;
        harmony.Patch(
            original: AccessTools.Method(typeof(Target), nameof(Target.Method)),
            postfix: new HarmonyMethod(typeof(MyPatches), nameof(Method_Postfix))
        );
    }

    // Patch methods MUST be static
    // Postfix: runs after original
    public static void Method_Postfix(Target __instance, ref bool __result)
    {
        try { __result = true; }
        catch (Exception ex) { Monitor.Log($"Error: {ex}", LogLevel.Error); }
    }

    // Prefix: return false to skip original
    public static bool Method_Prefix(Target __instance)
    {
        try { return true; }
        catch { return true; }  // Always run original on error
    }
}
```

**Inlining gotcha:** If a patch doesn't apply on some platforms (esp. Mac), suspect JIT inlining; patch a caller or a larger method instead.

---

## Multiplayer

### Per-Screen State (Split-Screen)
```csharp
// MUST be readonly to prevent clearing all screens
private readonly PerScreen<MenuManager> menuManager = new();
private readonly PerScreen<int> lastTab = new(() => 0);

menuManager.Value = new MenuManager();
var specific = lastTab.GetValueForScreen(0);
```

### Persistent Per-Player Data
```csharp
// Syncs in multiplayer, persists in save (always use UniqueID prefix)
if (!Game1.player.modData.ContainsKey($"{ModId}/Flag"))
    Game1.player.modData[$"{ModId}/Flag"] = "true";
```

### Mutex for Shared Furniture
```csharp
if (!dresser.mutex.IsLocked())
{
    dresser.mutex.RequestLock(delegate {
        Game1.activeClickableMenu = new MyMenu(dresser);
    });
}
// On exit: dresser.mutex.ReleaseLock();
```

### Location Iteration
```csharp
// DON'T: Game1.locations (farmhands miss some)
// DO:
Utility.ForAllLocations(loc => { /* ... */ });
```

---

## Configuration (GMCM)

```csharp
public class ModConfig { public bool Feature { get; set; } = true; }

private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
{
    var api = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
    if (api is null) return;

    api.Register(mod: ModManifest,
        reset: () => Config = new ModConfig(),
        save: () => Helper.WriteConfig(Config));

    api.AddBoolOption(mod: ModManifest, name: () => "Feature",
        getValue: () => Config.Feature, setValue: v => Config.Feature = v);
}
```

---

## References

- [SDV 1.6 Migration](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
- [Modding:Items](https://stardewvalleywiki.com/Modding:Items) - Authoritative item ID / ItemRegistry docs
- [SMAPI Events](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events)
- [Content API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content)
- [Data API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Data)
- [Input API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Input)
- [Utilities (Context, SDate)](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Utilities)
- [Harmony API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony)
- [Get Started](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started) - ModBuildConfig, manifest
- [Decompiled Source](https://github.com/Dannode36/StardewValleyDecompiled)
