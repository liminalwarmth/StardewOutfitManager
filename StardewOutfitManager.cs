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

namespace StardewOutfitManager
{
    public class StardewOutfitManager : Mod
    {
        // Managers
        internal static AssetManager assetManager;
        internal static MenuTabSwitcher tabSwitcher;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Set up manager functions
            assetManager = new AssetManager(helper);
            tabSwitcher = new MenuTabSwitcher();

            // Enable Harmony patches
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            // Menu change event
            helper.Events.Display.RenderingActiveMenu += this.OnMenuRender;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Display.RenderedActiveMenu += this.MenuFinishedRendering;
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuRender(object sender, RenderingActiveMenuEventArgs e)
        {
            // Our opening event where we store the dresser object only triggers when it's both a ShopMenu AND a Dresser
            if (Game1.activeClickableMenu is ShopMenu originalMenu && originalMenu.storeContext == "Dresser")
            {
                // Update the held original dresser reference to the source dresser object of the Dresser ShopMenu being opened
                tabSwitcher.dresserObject = (StorageFurniture)originalMenu.source;
                // Close the OG dresser menu before it opens
                Game1.activeClickableMenu.exitThisMenuNoSound();
                // Open a new instance of the primary menu
                Game1.activeClickableMenu = new WardrobeMenu();
                tabSwitcher.positionActiveTab(0);
                Game1.playSound("bigSelect");
            } 
        }

        // Draw our topbar navigation tabs on top of the menus we're opening
        private void MenuFinishedRendering(object sender, RenderedActiveMenuEventArgs e)
        {

        }

        // Handle player input outside of the ICLickable Menu framework
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is WardrobeMenu || Game1.activeClickableMenu is NewDresserMenu || Game1.activeClickableMenu is FavoritesMenu)
            {
                if (e.Pressed.Contains(Buttons.RightTrigger.ToSButton())) {
                    tabSwitcher.handleTopBarInput(Buttons.RightTrigger.ToSButton());
                }
                if (e.Pressed.Contains(Buttons.LeftTrigger.ToSButton())) {
                    tabSwitcher.handleTopBarInput(Buttons.LeftTrigger.ToSButton());
                }
                if (e.Pressed.Contains(Buttons.RightShoulder.ToSButton()))
                {
                    tabSwitcher.handleTopBarInput(Buttons.RightShoulder.ToSButton());
                }
                if (e.Pressed.Contains(Buttons.LeftShoulder.ToSButton()))
                {
                    tabSwitcher.handleTopBarInput(Buttons.LeftShoulder.ToSButton());
                }
                // Hijack all cancel and menu buttons to perform clean exit
                foreach (SButton btn in e.Pressed)
                {
                    if (Game1.options.cancelButton.Any((InputButton p) => p.ToSButton() == btn) ||
                        Game1.options.menuButton.Any((InputButton p) => p.ToSButton() == btn) ||
                        btn == Buttons.Y.ToSButton() || btn == Buttons.Start.ToSButton() || 
                        btn == Buttons.Back.ToSButton() || btn == Buttons.B.ToSButton())
                    {
                        Helper.Input.Suppress(btn);
                        tabSwitcher.cle
                    }
                }
            }
        }
    }
}
