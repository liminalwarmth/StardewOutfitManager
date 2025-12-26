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
using StardewOutfitManager.Managers;

namespace StardewOutfitManager.Managers
{
    // This class defines the top bar tabs which are inserted into all menus and the cross-menu equipping and list management functionality
    public class MenuManager
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

        // Shared farmer facing direction across all tabs (persists until menu is closed)
        public int farmerFacingDirection = 2;

        // Shared season/category selection across all tabs (persists until menu is closed)
        // Valid values: "Spring", "Summer", "Fall", "Winter", "Special", "All Outfits" (FavoritesMenu only)
        public string selectedCategory = null;

        // Display name of the dresser being accessed (e.g., "Junimo Dresser")
        public string dresserDisplayName = "Dresser";

        // Get the current in-game season as a category string
        public static string GetCurrentSeasonCategory()
        {
            return Game1.IsSpring ? "Spring" : Game1.IsSummer ? "Summer" : Game1.IsFall ? "Fall" : Game1.IsWinter ? "Winter" : "Special";
        }

        // Hang onto the menu we are currently working with until we perform a CleanExit
        internal IClickableMenu activeManagedMenu;

        public void handleTopBarInput(SButton button, int cursorX, int cursorY)
        {
            if (activeManagedMenu != null)
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

                    default:
                        break;
                }
            }
        }

        public void includeTopTabButtons(IClickableMenu baseMenu)
        {
            // Set Default Y Position
            tabYPosition = baseMenu.yPositionOnScreen - IClickableMenu.spaceToClearTopBorder + 32;
            // Top Bar Buttons
            topbarButtons.Add(wardrobeButton = new ClickableTextureComponent("Wardrobe", new Rectangle(baseMenu.xPositionOnScreen + baseMenu.width - IClickableMenu.spaceToClearSideBorder - 192, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.customSprites, new Rectangle(14, 210, 16, 16), 4f)
            {
                myID = 2000,
                rightNeighborID = 2001,
                leftNeighborID = -99999,
                downNeighborID = -99999
            });
            topbarButtons.Add(favoritesButton = new ClickableTextureComponent("Favorites", new Rectangle(wardrobeButton.bounds.X + 64, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.customSprites, new Rectangle(14, 226, 16, 16), 4f)
            {
                myID = 2001,
                leftNeighborID = 2000,
                rightNeighborID = 2002,
                downNeighborID = -99999
            });
            topbarButtons.Add(dresserButton = new ClickableTextureComponent("Dresser", new Rectangle(wardrobeButton.bounds.X + 128, tabYPosition, 64, 64), null, null, StardewOutfitManager.assetManager.customSprites, new Rectangle(14, 242, 16, 16), 4f)
            {
                myID = 2002,
                leftNeighborID = 2001,
                downNeighborID = -99999
            });
            if (baseMenu.allClickableComponents == null)
            {
                baseMenu.populateClickableComponentList();
            }
            baseMenu.allClickableComponents.Add(wardrobeButton);
            baseMenu.allClickableComponents.Add(favoritesButton);
            baseMenu.allClickableComponents.Add(dresserButton);
        }
        
        // Switch Menu Tab
        public void SwitchMenuTab(int tabNumber)
        {
            if (activeManagedMenu != null)
            {
                // Only do something if the player has clicked a tab that they're not already on
                if (tabNumber != currentTab)
                {
                    // Make sure to first put a held item back in the dresser if we change tabs from the OG dresser menu while something is on the cursor
                    if (Game1.activeClickableMenu is NewDresserMenu menu)
                    {
                        if (menu.heldItem != null)
                        {
                            // Dump the held object back into the dresser and play the deposit noise
                            if (menu.heldItem is ISalable)
                            {
                                dresserObject.AddItem(menu.heldItem as Item);
                                menu.heldItem = null;
                                Game1.playSound("dwop");
                            }
                            // Handle Emergency Edge Case where there is not salable item on the cursor
                            else
                            {
                                Game1.player.addItemToInventoryBool(menu.heldItem as Item);
                            }
                        }
                    }
                    Game1.activeClickableMenu.exitThisMenuNoSound();
                    Game1.playSound("smallSelect");
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
                                // Get the dresser items
                                List<Item> list = dresserObject.heldItems.ToList();
                                list.Sort(dresserObject.SortItems);
                                Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
                                foreach (Item item in list)
                                {
                                    contents[item] = new int[2] { 0, 1 };
                                }
                                // Open the shopMenu the same way the dresser does
                                Game1.activeClickableMenu = new NewDresserMenu(contents)
                                {
                                    source = dresserObject
                                };
                                break;
                            }
                    }
                    // Store the newly opened menu as what we're managing
                    activeManagedMenu = Game1.activeClickableMenu;
                    // Position tabs
                    positionActiveTab(tabNumber);
                }
            }
        }

        public void handleTopBarLeftClick(int x, int y, bool Playsound = true)
        {
            if (activeManagedMenu != null)
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
        }

        public void handleTopBarOnHover(int x, int y, ref string hoverTextField)
        {
            if (activeManagedMenu != null)
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
                    hoverTextField = dresserDisplayName;
                }
            }
        }

        public void drawTopBar(SpriteBatch b)
        {
            if (activeManagedMenu != null)
            {
                foreach (ClickableTextureComponent topbarButton in topbarButtons)
                {
                    topbarButton.draw(b);
                }
            }
        }

        public void positionActiveTab(int activeTab)
        {
            if (activeManagedMenu != null)
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
        }

        // Reposition top tab buttons on window resize
        public void repositionTopTabButtons(IClickableMenu baseMenu)
        {
            if (topbarButtons.Count >= 3)
            {
                tabYPosition = baseMenu.yPositionOnScreen - IClickableMenu.spaceToClearTopBorder + 32;
                int baseX = baseMenu.xPositionOnScreen + baseMenu.width - IClickableMenu.spaceToClearSideBorder - 192;

                wardrobeButton.bounds = new Rectangle(baseX, tabYPosition, 64, 64);
                favoritesButton.bounds = new Rectangle(baseX + 64, tabYPosition, 64, 64);
                dresserButton.bounds = new Rectangle(baseX + 128, tabYPosition, 64, 64);

                // Re-apply active tab offset
                positionActiveTab(currentTab);
            }
        }

        // Universal Menu Controls

        // Set up this cleanup behavior in the menus so that the top tabs are always cleared
        public void onMenuCloseCleanupBehavior()
        {
            if (activeManagedMenu != null)
            {
                topbarButtons.Clear();
            }
        }
    }
}
