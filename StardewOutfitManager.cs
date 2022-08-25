using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewOutfitManager.Managers;
using StardewOutfitManager.Menus;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

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

            // Menu change event
            helper.Events.Display.RenderingActiveMenu += this.OnMenuChanged;
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuChanged(object sender, RenderingActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu originalMenu)
            {
                if (originalMenu.storeContext == "Dresser")
                {
                    tabSwitcher.originalDresserMenu = (ShopMenu)Game1.activeClickableMenu;
                    tabSwitcher.dresserObject = (StorageFurniture)tabSwitcher.originalDresserMenu.source;
                    Game1.activeClickableMenu = new WardrobeMenu();
                    Game1.playSound("dwoop");
                    tabSwitcher.positionActiveTab(0);
                    tabSwitcher.originalDresserMenu.exitThisMenuNoSound();
                }
            }
        }
    }
}
