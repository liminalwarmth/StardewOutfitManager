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
using StardewValley.Objects;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace StardewOutfitManager
{
    public class StardewOutfitManager : Mod
    {
        // Managers
        internal static AssetManager assetManager;
        internal static MenuManager menuManager;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Set up manager functions
            assetManager = new AssetManager(helper);
            menuManager = new MenuManager();

            // Enable Harmony patches
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            // Checked events
            helper.Events.Display.RenderingActiveMenu += this.OnMenuRender;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            //helper.Events.Display.RenderedActiveMenu += this.MenuFinishedRendering; - may not need this at all
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuRender(object sender, RenderingActiveMenuEventArgs e)
        {
            // Our opening event where we store the dresser object only triggers when it's both a ShopMenu AND a Dresser AND we aren't yet managing that menu
            if (Game1.activeClickableMenu is ShopMenu originalMenu && originalMenu.storeContext == "Dresser" && menuManager.activeManagedMenu == null)
            {
                // Update the held original dresser reference to the source dresser object of the Dresser ShopMenu being opened
                menuManager.dresserObject = (StorageFurniture)originalMenu.source;
                // Close the OG dresser menu before it opens
                Game1.activeClickableMenu.exitThisMenuNoSound();
                // Open a new instance of the primary menu
                Game1.activeClickableMenu = new WardrobeMenu();
                // Start managing the menu we just opened
                menuManager.activeManagedMenu = Game1.activeClickableMenu;
                menuManager.positionActiveTab(0);
                Game1.playSound("bigSelect");
            }
        }

        // Handle player input outside of the ICLickable Menu framework
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (menuManager.activeManagedMenu != null)
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
        
        // Draw anything else that we want over the top of other items
        private void MenuFinishedRendering(object sender, RenderedActiveMenuEventArgs e)
        {
            if (menuManager.activeManagedMenu != null)
            {
                //
            }
        }
    }
}
