﻿using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewOutfitManager.Menus;
using StardewOutfitManager.Managers;
using StardewValley.Objects;

namespace StardewOutfitManager
{
    public class StardewOutfitManager : Mod
    {
        // Managers
        internal static AssetManager assetManager;
        internal static MenuTabSwitcher tabSwitcher;

        // Original Dresser Menu Reference
        //internal static ShopMenu priorShopMenu;
        //internal object dresserObject;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Set up manager functions
            assetManager = new AssetManager(helper);
            tabSwitcher = new MenuTabSwitcher();

            // Menu change event
            helper.Events.Display.RenderingActiveMenu += this.OnMenuChanged;
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuChanged(object sender, RenderingActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu originalMenu)
            {
                if (originalMenu.storeContext == "Dresser" && originalMenu is not NewDresserMenu)
                {
                    priorShopMenu = (ShopMenu)Game1.activeClickableMenu;
                    dresserObject = priorShopMenu.source;
                    Game1.activeClickableMenu = new WardrobeMenu();
                    priorShopMenu.exitThisMenuNoSound();
                }
            }
        }
    }
}
