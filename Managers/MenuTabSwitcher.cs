﻿using System;
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
        public ShopMenu originalDresserMenu;
        public StorageFurniture dresserObject;
        internal int tabYPosition;
        internal int currentTab = 0;

        public void includeTopTabButtons(IClickableMenu baseMenu)
        {
            // Set Default Y Position
            tabYPosition = baseMenu.yPositionOnScreen - IClickableMenu.spaceToClearTopBorder + 32;
            // Right Sidebar Buttons
            topbarButtons.Add(wardrobeButton = new ClickableTextureComponent("Wardrobe", new Rectangle(baseMenu.xPositionOnScreen + baseMenu.width - IClickableMenu.spaceToClearSideBorder - 192, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.wardrobeTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 611,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            topbarButtons.Add(favoritesButton = new ClickableTextureComponent("Favorites", new Rectangle(wardrobeButton.bounds.X + 64, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.favoritesTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 612,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            topbarButtons.Add(dresserButton = new ClickableTextureComponent("Dresser", new Rectangle(wardrobeButton.bounds.X + 128, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.dresserTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 613,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
        }

        // Trigger new dresser menu
        public void ShowNewDresserMenu()
        {
            List<Item> list = dresserObject.heldItems.ToList();
            list.Sort(dresserObject.SortItems);
            Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
            foreach (Item item in list)
            {
                contents[item] = new int[2] { 0, 1 };
            }
            Game1.activeClickableMenu = new NewDresserMenu(contents, dresserObject, dresserObject.onDresserItemWithdrawn, dresserObject.onDresserItemDeposited, "Dresser")
            {
                behaviorBeforeCleanup = delegate
                {
                    dresserObject.mutex.ReleaseLock();
                    dresserObject.OnMenuClose();
                }
            };
        }

        public void handleTopBarLeftClick(int x, int y, bool Playsound = true)
        {
            if (wardrobeButton.containsPoint(x, y))
            {
                if (Game1.activeClickableMenu != null && currentTab != 0)
                {
                    //wardrobeButton.scale -= 0.25f;
                    //wardrobeButton.scale = Math.Max(0.75f, wardrobeButton.scale);
                    Game1.playSound("shwip");
                    IClickableMenu priorMenu = Game1.activeClickableMenu;
                    topbarButtons.Clear();
                    //ShowWardrobeMenu();
                    positionActiveTab(0);
                    //priorMenu.exitThisMenuNoSound();
                }
            }
            if (favoritesButton.containsPoint(x, y))
            {
                if (Game1.activeClickableMenu != null && currentTab != 1)
                {
                    //favoritesButton.scale -= 0.25f;
                    //favoritesButton.scale = Math.Max(0.75f, favoritesButton.scale);
                    Game1.playSound("shwip");
                    IClickableMenu priorMenu = Game1.activeClickableMenu;
                    topbarButtons.Clear();
                    //ShowFavoritesMenu();
                    positionActiveTab(1);
                    //priorMenu.exitThisMenuNoSound();
                }
            }
            if (dresserButton.containsPoint(x, y))
            {
                if (Game1.activeClickableMenu != null && currentTab != 2)
                {
                    //dresserButton.scale -= 0.25f;
                    //dresserButton.scale = Math.Max(0.75f, dresserButton.scale);
                    Game1.playSound("shwip");
                    IClickableMenu priorMenu = Game1.activeClickableMenu;
                    topbarButtons.Clear();
                    ShowNewDresserMenu();
                    positionActiveTab(2);
                    priorMenu.exitThisMenuNoSound();
                }
            }
        }

        public void handleTopBarOnHover(int x, int y)
        {
            // Add hover text
        }

        public void drawTopBar(SpriteBatch b)
        {
            foreach (ClickableTextureComponent topbarButton in topbarButtons)
            {
                topbarButton.draw(b);
            }

        }

        public void positionActiveTab(int currentTab)
        {
            for (int i = 0; i < this.topbarButtons.Count; i++)
            {
                if (i == currentTab)
                {
                    topbarButtons[i].bounds.Y = tabYPosition + 8;
                    topbarButtons[i].scale = 4.1f;
                }
                else
                {
                    topbarButtons[i].bounds.Y = tabYPosition;
                    topbarButtons[i].scale = 4f;
                }
            }
        }

        // Add on exit function to clear tab list
    }
}
