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

        // Set up managers for each local player (per-screen handles local co-op)
        internal static PlayerManager playerManager;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Set up global manager functions
            assetManager = new AssetManager(helper);
            playerManager = new PlayerManager(helper);

            // Enable Harmony patches
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            // Checked events
            helper.Events.Display.RenderingActiveMenu += OnMenuRender;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
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
                // Check if this particular dresser is locked for use by another player before opening the interaction menu
                if (!originalDresser.mutex.IsLocked())
                {
                    // Lock the dresser so other players can't use it
                    originalDresser.mutex.RequestLock(delegate { });
                    // Load the favorites data (or create a new favorites data object if no file exists) if we haven't yet
                    if (playerManager.favoritesData.Value == null) { playerManager.loadFavoritesDataFromFile(); }
                    // Create a new menu manager instance for the active player
                    playerManager.menuManager.Value = new MenuManager();
                    // Update the held original dresser reference to the source dresser object of the Dresser ShopMenu being closed
                    playerManager.menuManager.Value.dresserObject = originalDresser;
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
                            List<Item> list = originalDresser.heldItems.ToList();
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
