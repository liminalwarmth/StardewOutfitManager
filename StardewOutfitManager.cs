using System;
using System.Linq;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewOutfitManager.Menus;
using StardewOutfitManager.Managers;
using StardewOutfitManager.Utils;
using StardewOutfitManager.Data;
using StardewValley.Objects;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using System.Threading;

namespace StardewOutfitManager
{
    public class StardewOutfitManager : Mod
    {
        // Load Game Assets
        internal static AssetManager assetManager;

        // Outfit name manager for random name suggestions
        internal static OutfitNameManager outfitNameManager;

        // Set up managers for each local player (per-screen handles local co-op)
        internal static PlayerManager playerManager;

        // Mod configuration
        internal static ModConfig Config;

        // Static monitor reference for use by managers
        internal static IMonitor ModMonitor;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Load configuration
            Config = helper.ReadConfig<ModConfig>();

            // Store monitor reference for managers
            ModMonitor = Monitor;

            // Set up global manager functions
            assetManager = new AssetManager(helper);
            outfitNameManager = new OutfitNameManager(helper);
            playerManager = new PlayerManager(helper);

            // Enable Harmony patches
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            // Checked events
            helper.Events.Display.RenderingActiveMenu += OnMenuRender;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        // Register with Generic Mod Config Menu if available
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () =>
                {
                    Helper.WriteConfig(Config);
                    // Invalidate cached assets so config changes take effect without restart
                    Helper.GameContent.InvalidateCache("Data/Furniture");
                    Helper.GameContent.InvalidateCache("Data/Shops");
                }
            );

            // Wardrobe Options section
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Wardrobe Options"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.IncludeRingsInOutfits,
                setValue: value => Config.IncludeRingsInOutfits = value,
                name: () => "Include Rings in Outfits",
                tooltip: () => "When enabled, rings are saved and equipped as part of outfits. When disabled, rings are excluded from outfit management and ring slots are hidden from the UI."
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                getValue: () => Config.DresserInventorySharing.ToString(),
                setValue: value => Config.DresserInventorySharing = Enum.Parse<DresserSharingMode>(value),
                name: () => "Dresser Inventory Sharing",
                tooltip: () => "Controls how dressers share their inventory. Individual: Each dresser is separate. Touching: Adjacent dressers share inventory. Same Building: All dressers in a house/cabin share inventory.",
                allowedValues: new[] { "Individual", "Touching", "SameBuilding" },
                formatAllowedValue: value => value switch
                {
                    "Individual" => "Individual (Separate)",
                    "Touching" => "Touching (Adjacent)",
                    "SameBuilding" => "Same Building (House/Cabin)",
                    _ => value
                }
            );

            // New Dresser Types section
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "New Dresser Types (Mirror & Small)"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.StartingDresser,
                setValue: value => Config.StartingDresser = value,
                name: () => "Starting Dresser",
                tooltip: () => "When enabled, new farmers will receive a free Small Oak Dresser in their starting inventory. This gives players immediate access to the outfit management system without needing to purchase or find a dresser first. In multiplayer, each farmer receives their own dresser once."
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.RobinSellsDressers,
                setValue: value => Config.RobinSellsDressers = value,
                name: () => "Robin Sells",
                tooltip: () => "When enabled, Robin's Carpenter Shop will stock 2 random dressers each day (from all Mirror and Small Dresser types). The selection changes daily and is the same for all players."
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.TravelingMerchantSellsDressers,
                setValue: value => Config.TravelingMerchantSellsDressers = value,
                name: () => "Travel Merchant Sells",
                tooltip: () => "When enabled, Mirror Dressers can randomly appear in the Traveling Merchant's inventory on Fridays and Sundays. The cart picks one random furniture item per visit, so dressers won't appear every time. Prices range from 250g to 2500g regardless of base value."
            );

            // Hair & Accessory Options section
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Hair & Accessory Options"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.IncludeFacialHair,
                setValue: value => {
                    Config.IncludeFacialHair = value;
                    Utils.AccessoryMethods.InvalidateCache();
                },
                name: () => "Include Facial Hair",
                tooltip: () => "When enabled, facial hair (beards, mustaches, etc.) appear in the accessory picker. When disabled, only non-facial-hair accessories like glasses and earrings are available."
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.IncludeModdedHairstyles,
                setValue: value => Config.IncludeModdedHairstyles = value,
                name: () => "Include Modded Hairstyles",
                tooltip: () => "When enabled, custom hairstyles from mods appear in the hair picker. When disabled, only vanilla hairstyles (0-73) are available."
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.IncludeModdedAccessories,
                setValue: value => {
                    Config.IncludeModdedAccessories = value;
                    Utils.AccessoryMethods.InvalidateCache();
                },
                name: () => "Include Modded Accessories",
                tooltip: () => "When enabled, custom accessories from mods appear in the accessory picker. When disabled, only vanilla 1.6 accessories are available."
            );
        }

        // Mod data key for tracking starting dresser (stored in player.modData for multiplayer sync)
        private const string StartingDresserReceivedKey = "LiminalWarmth.StardewOutfitManager/ReceivedStartingDresser";
        private const string StartingDresserItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallOakDresser";

        // Give starting dresser to new farmers if enabled (per-farmer in multiplayer)
        // Uses player.modData which syncs properly for farmhands (unlike Helper.Data.WriteSaveData)
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.StartingDresser)
                return;

            // Check if this player has already received the starting dresser
            // Using player.modData which persists in save and syncs in multiplayer
            if (!Game1.player.modData.ContainsKey(StartingDresserReceivedKey))
            {
                // Create and give the Small Oak Dresser
                var dresser = ItemRegistry.Create(StartingDresserItemId);
                Game1.player.addItemToInventory(dresser);

                // Mark this player as having received the dresser
                Game1.player.modData[StartingDresserReceivedKey] = "true";

                Monitor.Log($"Gave starting Small Oak Dresser to farmer {Game1.player.Name}.", LogLevel.Info);
            }
        }

        // Handle content API requests for custom furniture
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            assetManager.HandleAssetRequested(e);
        }

        // Invalidate caches when relevant assets are reloaded (e.g., when mods add/remove accessories)
        private void OnAssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            foreach (var name in e.NamesWithoutLocale)
            {
                if (name.IsEquivalentTo("Characters/Farmer/accessories"))
                {
                    AccessoryMethods.InvalidateCache();
                }
            }
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuRender(object sender, RenderingActiveMenuEventArgs e)
        {
            // Our opening event where we store the dresser object only triggers when it's both a ShopMenu AND a Dresser AND we aren't yet managing that menu
            if (Game1.activeClickableMenu is ShopMenu originalMenu && originalMenu.ShopId == "Dresser" && playerManager.menuManager.Value == null)
            {
                // Get a reference to the dresser
                StorageFurniture originalDresser = (StorageFurniture)originalMenu.source;
                // Close the OG dresser menu before it opens
                Game1.activeClickableMenu.exitThisMenuNoSound();

                // Get all linked dressers based on config (individual, touching, or same building)
                var linkedDressers = DresserLinkingMethods.GetLinkedDressers(originalDresser);

                // Attempt to lock all linked dressers - fails if any is already locked
                if (DresserLinkingMethods.TryLockAllDressers(linkedDressers))
                {
                    // Load the favorites data (or create a new favorites data object if no file exists) if we haven't yet
                    if (playerManager.favoritesData.Value == null) { playerManager.loadFavoritesDataFromFile(); }

                    // Create a new menu manager instance for the active player
                    playerManager.menuManager.Value = new MenuManager();

                    // Store both the primary dresser and all linked dressers
                    playerManager.menuManager.Value.primaryDresser = originalDresser;
                    playerManager.menuManager.Value.linkedDressers = linkedDressers;

                    // Store the dresser's display name for the tab hover text
                    playerManager.menuManager.Value.dresserDisplayName = originalDresser.DisplayName ?? "Dresser";

                    // Initialize category selection to current in-game season
                    playerManager.menuManager.Value.selectedCategory = MenuManager.GetCurrentSeasonCategory();

                    // Open to the last used tab (0 = Wardrobe, 1 = Favorites, 2 = Dresser)
                    int lastTab = playerManager.lastUsedTab.Value;
                    switch (lastTab)
                    {
                        case 1:
                            Game1.activeClickableMenu = new FavoritesMenu();
                            break;
                        case 2:
                            // Get combined items from all linked dressers
                            List<Item> list = playerManager.menuManager.Value.GetCombinedDresserItems();
                            list.Sort(originalDresser.SortItems);
                            Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
                            foreach (Item item in list)
                            {
                                contents[item] = new int[2] { 0, 1 };
                            }
                            Game1.activeClickableMenu = new NewDresserMenu(contents) { source = originalDresser };
                            break;
                        default:
                            Game1.activeClickableMenu = new WardrobeMenu();
                            break;
                    }

                    // Start managing the menu we just opened
                    playerManager.menuManager.Value.activeManagedMenu = Game1.activeClickableMenu;
                    playerManager.menuManager.Value.positionActiveTab(lastTab);
                    Game1.playSound("bigSelect");
                }
                else
                {
                    // Failed to lock - another player is using a connected dresser
                    Game1.playSound("cancel");
                    Game1.addHUDMessage(new HUDMessage("Another player is using a connected dresser.", HUDMessage.error_type));
                }
            }
        }

        // Handle player input outside of the ICLickable Menu framework
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            MenuManager menuManager = playerManager.menuManager.Value;
            if (menuManager != null)
            {
                Vector2 cursorPos = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
                // Process player input
                foreach (SButton btn in e.Pressed)
                {
                    // Double check a prior button didn't already close the menu
                    if (menuManager.activeManagedMenu != null)
                    {
                        // Check if there's a child menu (like NamingMenu) active - if so, don't intercept input
                        bool hasChildMenu = menuManager.activeManagedMenu.GetChildMenu() != null;

                        // TODO: Suppressing this might not be necessary once I rewrite the new DresserMenu class since I can route close actions directly to cleanup
                        // Suppress the menu/cancel buttons and keys so that I can control menu exit and handle cleanup
                        // But only if there's no child menu active (child menus like NamingMenu handle their own input)
                        if (!hasChildMenu && (Game1.options.cancelButton.Any((InputButton p) => p.ToSButton() == btn) ||
                        Game1.options.menuButton.Any((InputButton p) => p.ToSButton() == btn) ||
                        btn == Buttons.Y.ToSButton() || btn == Buttons.Start.ToSButton() ||
                        btn == Buttons.Back.ToSButton() || btn == Buttons.B.ToSButton()))
                        {
                            // Suppress the key and perform clean exit of all menus
                            Helper.Input.Suppress(btn);
                            playerManager.cleanMenuExit();
                        }
                        // Else pass the buttons on to the menuManager to process its own button press events (alongside the active menu)
                        else if (!hasChildMenu) { menuManager.handleTopBarInput(btn, (int)cursorPos.X, (int)cursorPos.Y); }
                    }
                }
            }
        }

        // Save favorites data file at end of day (before save dialog)
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            playerManager.saveFavoritesDataToFile();
        }
    }
}
