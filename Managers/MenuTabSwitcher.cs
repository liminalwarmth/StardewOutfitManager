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
using StardewModdingAPI;

namespace StardewOutfitManager.Managers
{
    // This class defines the methods which are used to universally insert tob bar tab menu switcher functionality to all custom menus in the mod
    internal class MenuTabSwitcher
    {
        // Holds dresser object for interaction with all menus
        public StorageFurniture dresserObject;

        // Tab buttons
        public ClickableTextureComponent wardrobeButton;
        public ClickableTextureComponent favoritesButton;
        public ClickableTextureComponent dresserButton;
        public List<ClickableComponent> topbarButtons = new List<ClickableComponent>();

        // Active tab visualization
        internal int tabYPosition;
        internal int currentTab;
        
        public void handleTopBarInput(SButton button, int cursorX, int cursorY)
        {   
            switch (button)
            {
                // Controller Functions
                case SButton.RightTrigger:
                    SwitchMenuTab(Math.Clamp(currentTab + 1, 0, 2));
                    break;

                case SButton.LeftTrigger:
                    SwitchMenuTab(Math.Clamp(currentTab - 1, 0, 2));
                    break;

                // Mouse and Keyboard Functions
                case SButton.MouseLeft:
                    handleTopBarLeftClick(cursorX, cursorY);
                    break;

                default:
                    break;
            }
        }

        // Close the menu and perform cleanup functions
        public void cleanExit()
        {

        }

        public void includeTopTabButtons(IClickableMenu baseMenu)
        {
            // Set Default Y Position
            tabYPosition = baseMenu.yPositionOnScreen - IClickableMenu.spaceToClearTopBorder + 32;
            // Top Bar Buttons
            topbarButtons.Add(wardrobeButton = new ClickableTextureComponent("Wardrobe", new Rectangle(baseMenu.xPositionOnScreen + baseMenu.width - IClickableMenu.spaceToClearSideBorder - 192, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.wardrobeTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 2000,
                rightNeighborID = 2001
            });
            topbarButtons.Add(favoritesButton = new ClickableTextureComponent("Favorites", new Rectangle(wardrobeButton.bounds.X + 64, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.favoritesTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 2001,
                leftNeighborID = 2000,
                rightNeighborID = 2002
            });
            topbarButtons.Add(dresserButton = new ClickableTextureComponent("Dresser", new Rectangle(wardrobeButton.bounds.X + 128, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.dresserTabTexture, new Rectangle(0, 0, 16, 16), 4f)
            {
                myID = 2002,
                leftNeighborID = 2001
            });
        }
        
        // Switch Menu Tab
        public void SwitchMenuTab(int tabNumber)
        {
            // Only do something if the player has clicked a tab that they're not already on
            if (tabNumber != currentTab) {
                Game1.activeClickableMenu.exitThisMenuNoSound();
                Game1.playSound("shwip");
                topbarButtons.Clear();
                switch (tabNumber)
                {
                    // Switch to Wardrobe Menu
                    case 0:
                        {
                            Game1.activeClickableMenu = new WardrobeMenu();
                            break;
                        }
                    // Switch to Favorites Menu
                    case 1:
                        {
                            Game1.activeClickableMenu = new FavoritesMenu();
                            break;
                        }
                    // Switch to New Dresser Menu
                    case 2:
                        {
                            List<Item> list = dresserObject.heldItems.ToList();
                            list.Sort(dresserObject.SortItems);
                            Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
                            foreach (Item item in list)
                            {
                                contents[item] = new int[2] { 0, 1 };
                            }
                            Game1.activeClickableMenu = new NewDresserMenu(contents)
                            {
                                source = dresserObject,
                                behaviorBeforeCleanup = delegate
                                {
                                    dresserObject.mutex.ReleaseLock();
                                    dresserObject.OnMenuClose();
                                    StardewOutfitManager.tabSwitcher.onMenuCloseCleanupBehavior();
                                }
                            };
                            break;
                        }
                }
                positionActiveTab(tabNumber);
            }
        }

        public void handleTopBarLeftClick(int x, int y, bool Playsound = true)
        {
            if (wardrobeButton.containsPoint(x, y))
            {
                SwitchMenuTab(0);
            }
            if (favoritesButton.containsPoint(x, y))
            {
                SwitchMenuTab(1);
            }
            if (dresserButton.containsPoint(x, y))
            {
                SwitchMenuTab(2);
            }
        }

        public void handleTopBarOnHover(int x, int y, ref string hoverTextField)
        {
            if (wardrobeButton.containsPoint(x, y))
            {
                hoverTextField = "Wardrobe";
            }
            else if (favoritesButton.containsPoint(x, y))
            {
                hoverTextField = "Favorites";
            }
            else if (dresserButton.containsPoint(x, y))
            {
                hoverTextField = "Dresser";
            }
        }

        public void drawTopBar(SpriteBatch b)
        {
            foreach (ClickableTextureComponent topbarButton in topbarButtons)
            {
                topbarButton.draw(b);
            }

        }

        public void positionActiveTab(int activeTab)
        {
            for (int i = 0; i < this.topbarButtons.Count; i++)
            {
                if (i == activeTab)
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
            currentTab = activeTab;
        }

        // Universal Menu Controls

        // Set up this cleanup behavior in the menus so that the top tabs are always cleared
        public void onMenuCloseCleanupBehavior()
        {
            topbarButtons.Clear();
        }
    }
}
