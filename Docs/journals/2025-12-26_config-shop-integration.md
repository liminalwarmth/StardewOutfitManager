# Branch: config-shop-integration

Config system, GMCM integration, Robin's Carpenter Shop dresser sales, and Traveling Merchant integration.

---

## 2025-12-26 21:45 - Add Config Options for Shop Integration

**Task:** Add config toggles for Robin's shop and Traveling Merchant dresser sales.

### Changes Made

**1. Added new config options** (`ModConfig.cs`)
- `RobinSellsDressers` (default: true) - Controls Robin's shop integration
- `TravelingMerchantSellsDressers` (default: true) - Controls Traveling Cart random sale

**2. Updated GMCM registration** (`StardewOutfitManager.cs`)
- Added two new bool options with descriptive tooltips
- All three options now have detailed explanatory hover text

**3. Added conditional logic** (`Managers/AssetManager.cs`)
- Data/Furniture patching now uses config to set `off_limits_for_random_sale` dynamically
- Data/Shops patching is now conditional on `RobinSellsDressers` config

### Files Modified
- `ModConfig.cs`
- `StardewOutfitManager.cs`
- `Managers/AssetManager.cs`

### Build Status
- Compiles successfully with no errors or warnings

---

## 2025-12-26 21:30 - Config System, GMCM, Robin's Shop, and Traveling Cart

**Task:** Add config system with GMCM support, Robin's shop integration for custom dressers, and Traveling Merchant integration for Mirror Dressers.

### Changes Made

**1. Created config system**
- `ModConfig.cs` - Config class with `StartingDresser` boolean (default: true)
- `IGenericModConfigMenuApi.cs` - API interface for GMCM integration
- `Models/ModSaveData.cs` - Per-save data to track if starting dresser was given

**2. Updated entry point** (`StardewOutfitManager.cs`)
- Added `internal static ModConfig Config` field
- Load config in Entry() via `helper.ReadConfig<ModConfig>()`
- Added `GameLaunched` event handler for GMCM registration
- Added `SaveLoaded` event handler to give Small Oak Dresser on new saves

**3. Added Robin's Carpenter Shop integration** (`Managers/AssetManager.cs`)
- Patches `Data/Shops` to add 14 custom dressers to Robin's furniture rotation
- Day-based rotation matching vanilla pattern:
  - Monday: Oak, Birch, Gold Mirror Dressers
  - Tuesday: Walnut, Mahogany Mirror Dressers
  - Wednesday: Modern, White Mirror Dressers
  - Thursday: Oak, Birch, Gold Small Dressers
  - Friday: Walnut, Mahogany Small Dressers
  - Saturday: Modern, White Small Dressers

**4. Added Traveling Merchant integration** (`Managers/AssetManager.cs`)
- Changed Mirror Dressers' `off_limits_for_random_sale` from `true` to `false`
- Mirror Dressers now appear in Traveling Cart's random furniture pool (Fri/Sun)
- Small Dressers remain exclusive to Robin's shop

**5. Updated CLAUDE.md**
- Added note about config file for conditional logic

### Files Created
- `ModConfig.cs`
- `IGenericModConfigMenuApi.cs`
- `Models/ModSaveData.cs`

### Files Modified
- `StardewOutfitManager.cs`
- `Managers/AssetManager.cs`
- `CLAUDE.md`

### Build Status
- Compiles successfully with no errors or warnings

---

## 2025-12-26 17:15 - Generate Small Dresser Color Variants

**Task:** Generate all 8 color variants of Small Dresser using palette swapping from Mirror Dresser colors.

### Changes Made

**1. Generated Small Dresser color variants via Python palette swap**
- Extracted wood color palettes from Mirror Dresser variants
- Created palette mapping from Oak's 4 wood tones to each variant's tones
- Generated 7 new PNGs: birch, walnut, mahogany, black, white, gold, modern

**2. Updated AssetManager with all Small Dresser variants** (`Managers/AssetManager.cs`)
- Added 7 new Data/Furniture entries for Small Dressers
- Updated GetFurnitureTexturePath() with paths for all 8 Small Dresser textures
- Pricing: Base 2500g, Gold 3500g, Modern 3000g (matching Mirror Dresser pattern)

### Files Modified
- `Managers/AssetManager.cs` - Added 7 more Small Dresser entries and texture paths
- `Assets/Objects/Furniture/Small Dresser/` - Added birch.png, walnut.png, mahogany.png, black.png, white.png, gold.png, modern.png

### Build Status
- Compiles successfully with no errors or warnings

### Total Custom Furniture
Now 16 items total: 8 Mirror Dressers + 8 Small Dressers

---

## 2025-12-26 16:45 - Add Custom Dresser Furniture Items

**Task:** Add 9 custom dresser furniture items (Mirror Dressers and Small Dresser) to the game using SMAPI's content API.

### Changes Made

**1. Added IModHelper reference to AssetManager** (`Managers/AssetManager.cs`)
- Added `private readonly IModHelper helper` field
- Stored helper reference in constructor for use in content API methods

**2. Subscribed to AssetRequested event** (`StardewOutfitManager.cs`)
- Added `helper.Events.Content.AssetRequested += OnAssetRequested` in Entry()
- Created handler method that delegates to AssetManager.HandleAssetRequested()

**3. Implemented HandleAssetRequested method** (`Managers/AssetManager.cs`)
- Patches `Data/Furniture` to add 9 custom dresser entries:
  - 8 Mirror Dresser variants (Birch, Black, Gold, Mahogany, Modern, Oak, Walnut, White) - 1x2 tiles
  - 1 Small Oak Dresser - 1x1 tile
- Loads custom textures when game requests `LiminalWarmth.StardewOutfitManager/Furniture/[Name]`
- Uses existing PNG assets from `Assets/Objects/Furniture/` folder

### Technical Notes
- Custom dressers automatically work with existing outfit management UI because they use `type="dresser"` which creates `StorageFurniture` that opens `ShopMenu` with `ShopId == "Dresser"`
- Furniture IDs prefixed with mod's UniqueID for mod compatibility
- Uses SMAPI's content API directly (no Content Patcher dependency)

### Files Modified
- `Managers/AssetManager.cs` - Added helper field, HandleAssetRequested method, GetFurnitureTexturePath method
- `StardewOutfitManager.cs` - Added AssetRequested event subscription and handler

### Build Status
- Compiles successfully with no errors or warnings

### Testing
- Use CJB Item Spawner to spawn furniture (e.g., "Oak Mirror Dresser")
- Place dresser and interact to verify outfit management menu opens
