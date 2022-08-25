using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;

namespace StardewOutfitManager.Menus
{
    // Initialize a custom original dresser menu
    internal class NewDresserMenu : ShopMenu
    {
        public List<ClickableComponent> sidebarButtons = new List<ClickableComponent>();

        public NewDresserMenu(Dictionary<ISalable, int[]> itemPriceAndStock, StorageFurniture source, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null) 
            : base(itemPriceAndStock, 0, null, on_purchase, on_sell, context)
        {
            // Get rid of close button
            upperRightCloseButton = null;
            // Add top tab buttons
            StardewOutfitManager.tabSwitcher.includeTopTabButtons(this);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            StardewOutfitManager.tabSwitcher.handleTopBarLeftClick(x, y);
        }
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            StardewOutfitManager.tabSwitcher.handleTopBarOnHover(x, y);
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            //StardewOutfitManager.tabSwitcher.drawTopBar(b);
        }
    }
}
