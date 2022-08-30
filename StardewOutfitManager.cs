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

namespace StardewOutfitManager
{
    public class StardewOutfitManager : Mod
    {
        // Load Game Assets
        internal static AssetManager assetManager;

        // Set up managers for each local player (per-screen handles local co-op)
        private PerScreen<MenuManager> playerMenuManager;
        private PerScreen<FavoritesData> playerFavoritesData;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Set up global manager functions
            assetManager = new AssetManager(helper);
            playerMenuManager = new PerScreen<MenuManager>();

            // Enable Harmony patches
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            // Checked events
            helper.Events.Display.RenderingActiveMenu += this.OnMenuRender;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        // Clean exit when the menus close for the active player
        public void OnMenuClosed()
        {
            playerMenuManager.Value = null;
        }

        // Create a new local player context each time a save is loaded
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //playerFavoritesData.Value = new FavoritesData("test");
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuRender(object sender, RenderingActiveMenuEventArgs e)
        {
            // Our opening event where we store the dresser object only triggers when it's both a ShopMenu AND a Dresser AND we aren't yet managing that menu
            if (Game1.activeClickableMenu is ShopMenu originalMenu && originalMenu.storeContext == "Dresser" && playerMenuManager.Value == null)
            {
                // Create a new menu manager instance for the active player
                playerMenuManager.Value = new MenuManager();
                // Update the held original dresser reference to the source dresser object of the Dresser ShopMenu being opened
                playerMenuManager.Value.dresserObject = (StorageFurniture)originalMenu.source;
                // Close the OG dresser menu before it opens
                Game1.activeClickableMenu.exitThisMenuNoSound();
                // Open a new instance of the primary menu
                Game1.activeClickableMenu = new WardrobeMenu();
                // Start managing the menu we just opened
                playerMenuManager.Value.activeManagedMenu = Game1.activeClickableMenu;
                playerMenuManager.Value.positionActiveTab(0);
                Game1.playSound("bigSelect");
            }
        }

        // Handle player input outside of the ICLickable Menu framework
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            MenuManager menuManager = playerMenuManager.Value;
            if (menuManager != null)
            {
                Vector2 cursorPos = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
                // Process player input
                foreach (SButton btn in e.Pressed)
                {
                    // Double check a prior button didn't already close the menu
                    if (menuManager.activeManagedMenu != null)
                    {
                        // Suppress the menu/cancel buttons and keys so that I can control menu exit and handle cleanup
                        if (Game1.options.cancelButton.Any((InputButton p) => p.ToSButton() == btn) ||
                        Game1.options.menuButton.Any((InputButton p) => p.ToSButton() == btn) ||
                        btn == Buttons.Y.ToSButton() || btn == Buttons.Start.ToSButton() ||
                        btn == Buttons.Back.ToSButton() || btn == Buttons.B.ToSButton())
                        {
                            // Supress the key and perform clean exit of all menus
                            Helper.Input.Suppress(btn);
                            menuManager.cleanExit();
                        }
                        // Else pass the buttons on to the menuManager to process its own button press events (alongside the active menu)
                        else { menuManager.handleTopBarInput(btn, (int)cursorPos.X, (int)cursorPos.Y); }
                    }
                }
            }
        }
    }
}
