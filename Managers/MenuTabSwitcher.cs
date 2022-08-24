using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley;
using StardewOutfitManager.Menus;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewOutfitManager.Managers
{
    // This class defines the methods which are used to universally insert tob bar tab menu switcher functionality to all custom menus in the mod
    internal class MenuTabSwitcher
    {
        public ClickableTextureComponent wardrobeButton;
        public ClickableTextureComponent favoritesButton;
        public ClickableTextureComponent dresserButton;
        public List<ClickableComponent> topbarButtons = new List<ClickableComponent>();
        internal StorageFurniture dresser = (StorageFurniture)StardewOutfitManager.dresserObject;


        public void includeTopTabButtons(IClickableMenu baseMenu)
        {
            // Right Sidebar Buttons
            topbarButtons.Add(wardrobeButton = new ClickableTextureComponent("Wardrobe", new Rectangle(baseMenu.xPositionOnScreen + baseMenu.width - IClickableMenu.spaceToClearSideBorder - 192, baseMenu.yPositionOnScreen - IClickableMenu.spaceToClearTopBorder + 48, 64, 64), null, null, StardewOutfitManager.assetManager.wardrobeTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 611,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            topbarButtons.Add(favoritesButton = new ClickableTextureComponent("Favorites", new Rectangle(wardrobeButton.bounds.X + 64, wardrobeButton.bounds.Y, 64, 64), null, null, StardewOutfitManager.assetManager.favoritesTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 612,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            topbarButtons.Add(dresserButton = new ClickableTextureComponent("Dresser", new Rectangle(wardrobeButton.bounds.X + 128, wardrobeButton.bounds.Y, 64, 64), null, null, StardewOutfitManager.assetManager.dresserTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 613,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
        }

        // Trigger new dresser submenu
        public void ShowNewDresserMenu()
        {
            List<Item> list = dresser.heldItems.ToList();
            list.Sort(dresser.SortItems);
            Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
            foreach (Item item in list)
            {
                contents[item] = new int[2] { 0, 1 };
            }
            Game1.activeClickableMenu = new NewDresserMenu(contents, dresser, dresser.onDresserItemWithdrawn, dresser.onDresserItemDeposited, "Dresser")
            {
                behaviorBeforeCleanup = delegate
                {
                    dresser.mutex.ReleaseLock();
                    dresser.OnMenuClose();
                }
            };
        }

        public void handleTopBarLeftClick(int x, int y, bool Playsound = true)
        {
            if (wardrobeButton.containsPoint(x, y))
            {
                if (Game1.activeClickableMenu is WardrobeMenu)
                {
                    wardrobeButton.scale -= 0.25f;
                    wardrobeButton.scale = Math.Max(0.75f, wardrobeButton.scale);
                    Game1.playSound("shwip");
                }
            }
            if (favoritesButton.containsPoint(x, y))
            {
                if (Game1.activeClickableMenu is WardrobeMenu)
                {
                    favoritesButton.scale -= 0.25f;
                    favoritesButton.scale = Math.Max(0.75f, favoritesButton.scale);
                    Game1.playSound("shwip");
                }
            }
            if (dresserButton.containsPoint(x, y))
            {
                if (Game1.activeClickableMenu is WardrobeMenu)
                {
                    dresserButton.scale -= 0.25f;
                    dresserButton.scale = Math.Max(0.75f, dresserButton.scale);
                    Game1.playSound("shwip");
                    //Game1.activeClickableMenu.exitThisMenuNoSound();
                    ShowNewDresserMenu();
                }
            }
        }

        public void handleTopBarOnHover(int x, int y)
        {
            foreach (ClickableTextureComponent c5 in topbarButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    c5.scale = Math.Min(c5.scale + 0.02f, 4.1f);
                }
                else
                {
                    c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
                }
            }
        }

        public void drawTopBar(SpriteBatch b)
        {
            foreach (ClickableTextureComponent topbarButton in topbarButtons)
            {
                topbarButton.draw(b);
            }

        }
    }
}
