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
using StardewOutfitManager.Utils;
using StardewOutfitManager.Managers;
using StardewOutfitManager.Data;
using System.Xml.Linq;
using static StardewOutfitManager.Menus.FavoritesMenu;

namespace StardewOutfitManager.Menus
{
    // This class defines the new Wardrobe customization menu
    internal class WardrobeMenu : IClickableMenu
    {
        // Reference Dresser Object
        internal StorageFurniture dresserObject = StardewOutfitManager.playerManager.menuManager.Value.dresserObject;
        // Reference Top Tab Menu Manager
        internal MenuManager menuManager = StardewOutfitManager.playerManager.menuManager.Value;
        // Reference Favorites Data
        internal FavoritesData favoritesData = StardewOutfitManager.playerManager.favoritesData.Value;

        // Display Farmer and PortraitBox
        private Rectangle _portraitBox;
        private Farmer _displayFarmer;
        private string hoverText = "";
        private Item hoveredItem = null;

        // Item Display Name Labels
        private ClickableComponent descriptionLabel;
        private ClickableComponent hatLabel;
        private ClickableComponent shirtLabel;
        private ClickableComponent pantsLabel;
        private ClickableComponent shoesLabel;
        private ClickableComponent hairLabel;
        private ClickableComponent accLabel;

        // Basic UI Button Groups
        public List<ClickableComponent> labels = new();
        public List<ClickableComponent> itemLabels = new();
        public List<ClickableComponent> leftSelectionButtons = new();
        public List<ClickableComponent> rightSelectionButtons = new();

        // Favorites and Category Buttons
        public List<ClickableComponent> categoryButtons = new();
        public ClickableTextureComponent saveFavoriteButton;
        public Rectangle saveFavoriteBox;
        public ClickableComponent categorySelected;
        public Rectangle categoryShading;

        // Snap Regions
        internal const int LABELS = 10000;
        internal const int PORTRAIT = 20000;
        internal const int CATEGORIES = 30000;

        // Additional Buttons
        public ClickableTextureComponent okButton;

        // Menu, Inventory Lists, List Indexes
        public List<Item> hatStock = new();
        public List<Item> shirtStock = new();
        public List<Item> pantsStock = new();
        public List<Item> shoesStock = new();
        public List<Item> ringsStock = new();
        public int hatIndex = -1;
        public int shirtIndex = -1;
        public int pantsIndex = -1;
        public int shoesIndex = -1;

        // Equipment Display
        public const int region_hat = 101;
        public const int region_ring1 = 102;
        public const int region_ring2 = 103;
        public const int region_boots = 104;
        public const int region_shirt = 108;
        public const int region_pants = 109;
        public List<ClickableComponent> equipmentIcons = new();

        // Main Wardrobe Menu Functionality
        public WardrobeMenu() : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            // WARDROBE DATA
            // Add the current farmer equipment, if any, to the wardrobe stock lists and set any current worn equipment as index 0 (-1 is nothing in the slot)
            if (Game1.player.hat.Value != null)
            {
                hatStock.Add(Game1.player.hat.Value);
                hatIndex = 0;
            }
            if (Game1.player.shirtItem.Value != null)
            {
                shirtStock.Add(Game1.player.shirtItem.Value);
                shirtIndex = 0;
            }
            if (Game1.player.pantsItem.Value != null)
            {
                pantsStock.Add(Game1.player.pantsItem.Value);
                pantsIndex = 0;
            }
            if (Game1.player.boots.Value != null)
            {
                shoesStock.Add(Game1.player.boots.Value);
                shoesIndex = 0;
            }
            // Generate inventory lists directly from the base game dresser object, if any, to wardrobe stock lists
            List<Item> list = dresserObject.heldItems.ToList();
            list.Sort(dresserObject.SortItems);
            foreach (Item item in list)
            {
                if (item.Category == -95)
                {
                    hatStock.Add(item);
                }
                else if (item is Clothing && (item as Clothing).clothesType.Value == Clothing.ClothesType.SHIRT)
                {
                    shirtStock.Add(item as Clothing);
                }
                else if (item is Clothing && (item as Clothing).clothesType.Value == Clothing.ClothesType.PANTS)
                {
                    pantsStock.Add(item as Clothing);
                }
                else if (item.Category == -97)
                {
                    shoesStock.Add(item);
                }
                else if (item.Category == -96)
                {
                    ringsStock.Add(item);
                }
            }

            /// WARDROBE MENU
            // Set up menu structure
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            // Set up portrait and farmer
            _portraitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder, yPositionOnScreen + 64, 256, 384);
            _displayFarmer = Game1.player;
            _displayFarmer.faceDirection(menuManager.farmerFacingDirection);
            _displayFarmer.FarmerSprite.StopAnimation();

            // Equipment slot displays
            int eqIconXOffset = _portraitBox.X + _portraitBox.Width / 2 - 81 - 16;
            int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64), "Hat"));
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64), "Shirt"));
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64), "Pants"));
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64), "Boots"));
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64), "Left Ring"));
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64), "Right Ring"));

            // Player display window movement buttons
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 42, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1.25f));
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X + 256 - 38, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1.25f));

            //int selectorBtnsX = _portraitBox.Right + 128;   // Xpos of button block
            int selectorBtnsX = xPositionOnScreen + width/2 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder - 120;        // Xpos of button block
            int selectorBtnsY = _portraitBox.Y - 32;        // Ypos of button block
            int yOffset = 0;                                // Additional Y offset
            int yBtnSpacing = 96;                           // Vertical space between each button set
            int arrowOffset = 8;                            // Selection arrows offset relative to other buttons
            int labelSpacing = 4;                           // The Y spacing offset between each pair of text labels

            // Hat cycle buttons
            leftSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Hat")
            {
                myID = 1000,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                upNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR, // wrap to Accessory (1005)
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            hatLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), _displayFarmer.hat.Value == null ? "None" : _displayFarmer.hat.Value.DisplayName);
            itemLabels.Add(hatLabel);

            // Shirt cycle buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Shirt")
            {
                myID = 1001,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS

            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            shirtLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), _displayFarmer.shirtItem.Value == null ? "None" : _displayFarmer.shirtItem.Value.DisplayName);
            itemLabels.Add(shirtLabel);

            // Pants cycle buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Pants")
            {
                myID = 1002,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            pantsLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), _displayFarmer.pantsItem.Value == null ? "None" : _displayFarmer.pantsItem.Value.DisplayName);
            itemLabels.Add(pantsLabel);

            // Shoes cycle Buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Shoes")
            {
                myID = 1003,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            shoesLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), _displayFarmer.boots.Value == null ? "None" : _displayFarmer.boots.Value.DisplayName);
            itemLabels.Add(shoesLabel);

            // Hair Cycle Buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Hair")
            {
                myID = 1004,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            hairLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), this.GetHairOrAccessoryName("Hair", _displayFarmer.hair.Value));
            itemLabels.Add(hairLabel);

            // Accessories Buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Accessory", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Accessory")
            {
                myID = 1005,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR, // wrap to Hat (1000)
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Accessory", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            accLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), this.GetHairOrAccessoryName("Accessory", _displayFarmer.accessory.Value));
            itemLabels.Add(accLabel);

            // Confirm Button
            okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 56, yPositionOnScreen + height - borderWidth - spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 9999,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC
            };

            // Favorite Category Buttons
            int catXpos = xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 280;
            int catYpos = _portraitBox.Y;
            categoryButtons.Add(new ClickableTextureComponent("Spring", new Rectangle(catXpos, catYpos, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(14, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Summer", new Rectangle(catXpos + 90, catYpos, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(36, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Fall", new Rectangle(catXpos + 180, catYpos, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(58, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Winter", new Rectangle(catXpos + 45, catYpos + 80, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(80, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Special", new Rectangle(catXpos + 135, catYpos + 80, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(102, 192, 22, 18), 4f));
            for (int i = 0; i < categoryButtons.Count; i++)
            {
                categoryButtons[i].myID = 6000 + i;
                categoryButtons[i].upNeighborID = ClickableComponent.SNAP_AUTOMATIC;
                categoryButtons[i].downNeighborID = ClickableComponent.SNAP_AUTOMATIC;
                categoryButtons[i].leftNeighborID = ClickableComponent.SNAP_AUTOMATIC;
                categoryButtons[i].rightNeighborID = ClickableComponent.SNAP_AUTOMATIC;
                categoryButtons[i].region = CATEGORIES;
            }

            // Set category from shared menu manager state (persists across tab switches)
            // "All Outfits" is a FavoritesMenu-only category that doesn't exist in WardrobeMenu,
            // so we fall back to the current in-game season when switching from FavoritesMenu
            string category = menuManager.selectedCategory;
            if (category == "All Outfits")
            {
                category = MenuManager.GetCurrentSeasonCategory();
                menuManager.selectedCategory = category;
            }
            categorySelected = categoryButtons.FirstOrDefault(c => c.name == category) ?? categoryButtons[0];


            // Save as Favorite Button
            saveFavoriteBox = new Rectangle(catXpos + 45, catYpos + 160, 178, 72);
            saveFavoriteButton = new ClickableTextureComponent("SaveFavorite", saveFavoriteBox, null, null, Game1.menuTexture, new Rectangle(0, 256, 60, 60), 1f)
            {
                myID = 9998,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                rightNeighborID = ClickableComponent.SNAP_AUTOMATIC
            };

            // Top Bar Tab Switcher Buttons
            populateClickableComponentList();
            menuManager.includeTopTabButtons(this);

            // Cleanup Behavior
            behaviorBeforeCleanup = delegate
            {
                menuManager.onMenuCloseCleanupBehavior();
            };

            // Default snap
            if (Game1.options.SnappyMenus && Game1.options.gamepadControls) {
                snapToDefaultClickableComponent();
            }
        }

        // ** METHODS **

        // Change the category filter
        private void ChangeCategory(ClickableComponent newCategory)
        {
            // If we didn't click the category already active
            if (categorySelected.name != newCategory.name)
            {
                // Update the category setting
                categorySelected = newCategory;
                // Save to shared menu manager state
                menuManager.selectedCategory = newCategory.name;
                Game1.playSound("smallSelect");
            }
        }

        // Returns the item in the index which is referenced by the directional change
        private Item ClothingSwap(ref int itemIndex, ref List<Item> stockList, int change)
        {
            // Move to next or prior clothing item and update current and prior item indexes for reference, based on arrow direction change
            int listSize = stockList.Count;
            if (listSize > 0)
            {
                itemIndex += change;
                if (itemIndex >= listSize) itemIndex = -1;
                else if (itemIndex < -1) itemIndex = listSize - 1;
            }
            else
            {
                itemIndex = -1;
            }
            // Return null (no item if -1), otherwise return the item
            return (itemIndex == -1) ? null : stockList[itemIndex];
        }

        // Handle arrow directional selection clicks
        private void SelectionClick(string name, int change)
        {
            switch (name)
            {
                case "Hat":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref hatIndex, ref hatStock, change), hatLabel);
                        break;
                    }
                case "Shirt":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref shirtIndex, ref shirtStock, change), shirtLabel);
                        break;
                    }
                case "Pants":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref pantsIndex, ref pantsStock, change), pantsLabel);
                        break;
                    }
                case "Shoes":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref shoesIndex, ref shoesStock, change), shoesLabel);
                        break;
                    }
                case "Direction":
                    {
                        int newDirection = (_displayFarmer.FacingDirection - change + 4) % 4;
                        _displayFarmer.faceDirection(newDirection);
                        menuManager.farmerFacingDirection = newDirection;
                        _displayFarmer.FarmerSprite.StopAnimation();
                        _displayFarmer.completelyStopAnimatingOrDoingAction();
                        Game1.playSound("stoneStep");
                        break;
                    }
                case "Hair":
                    {
                        this.HairSwap(name, change, _displayFarmer, hairLabel);
                        break;
                    }
                case "Accessory":
                    {
                        this.AccessorySwap(name, change, _displayFarmer, accLabel);
                        break;
                    }
            }
        }

        // ** CONTROLS **

        // Default Snap
        public override void snapToDefaultClickableComponent()
        {
            setCurrentlySnappedComponentTo(1000);
            snapCursorToCurrentSnappedComponent();
        }

        // Custom Snap Behavior
        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            switch (getCurrentlySnappedComponent().region)
            {
                // Label List
                case LABELS:
                    // Direction: 0 = up, 1 = right, 2 = down, 3 = left
                    if (direction == 1 || direction == 3)
                    {
                        // Left and Right change the current item in slot
                        int change = direction == 1 ? 1 : -1;
                        SelectionClick(getCurrentlySnappedComponent().name, change);
                    }
                    else if (direction == 0 && oldID == 1000)
                    {
                        // Up from Hat (1000) wraps to Accessory (1005)
                        setCurrentlySnappedComponentTo(1005);
                        snapCursorToCurrentSnappedComponent();
                        Game1.playSound("shiny4");
                    }
                    else if (direction == 2 && oldID == 1005)
                    {
                        // Down from Accessory (1005) wraps to Hat (1000)
                        setCurrentlySnappedComponentTo(1000);
                        snapCursorToCurrentSnappedComponent();
                        Game1.playSound("shiny4");
                    }
                    break;

                default:
                    break;
            }
        }

        // Key Press
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
        }

        // Handle Game Pad Controls
        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b == Buttons.RightShoulder) 
            {
                SelectionClick("Direction", 1);
            }
            else if (b == Buttons.LeftShoulder)
            {
                SelectionClick("Direction", -1);
            }
            if (b == Buttons.A && getCurrentlySnappedComponent().region == LABELS)
            {
                setCurrentlySnappedComponentTo(9999);
                snapCursorToCurrentSnappedComponent();
                Game1.playSound("smallSelect");
            }
            else if (b == Buttons.A && getCurrentlySnappedComponent() == saveFavoriteButton)
            {
                setCurrentlySnappedComponentTo(9999);
                snapCursorToCurrentSnappedComponent();
            }
        }

        // Left click action
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // Directional arrow navigation
            if (leftSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent c2 in leftSelectionButtons)
                {
                    if (c2.containsPoint(x, y))
                    {
                        SelectionClick(c2.name, -1);
                        if (c2.scale != 0f)
                        {
                            c2.scale -= 0.25f;
                            c2.scale = Math.Max(0.75f, c2.scale);
                        }
                    }
                }
            }
            if (rightSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent c in rightSelectionButtons)
                {
                    if (c.containsPoint(x, y))
                    {
                        SelectionClick(c.name, 1);
                        if (c.scale != 0f)
                        {
                            c.scale -= 0.25f;
                            c.scale = Math.Max(0.75f, c.scale);
                        }
                    }
                }
            }

            // OK / confirm button
            if (okButton.containsPoint(x, y))
            {
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
                StardewOutfitManager.playerManager.cleanMenuExit();
            }
            
            // Save current outfit as favorite (display name computed dynamically from roster position)
            if (saveFavoriteButton.containsPoint(x, y))
            {
                saveFavoriteButton.scale -= 0.25f;
                saveFavoriteButton.scale = Math.Max(0.75f, saveFavoriteButton.scale);

                string category = categorySelected.name;

                if (favoritesData.SaveNewOutfit(_displayFarmer, category, ""))
                {
                    Game1.playSound("dwop");
                }
                else
                {
                    Game1.playSound("cancel"); // Outfit already exists
                }
            }

            // Favorite category buttons
            foreach (ClickableComponent c in categoryButtons)
            {
                if (c.containsPoint(x, y)) { ChangeCategory(c); }
            }

            // Top bar handling
            menuManager.handleTopBarLeftClick(x, y);
        }

        // Handle On-Hover and Resetting Button States
        public override void performHoverAction(int x, int y)
        {
            hoverText = "";
            foreach (ClickableTextureComponent c6 in leftSelectionButtons)
            {
                if (c6.containsPoint(x, y))
                {
                    c6.scale = Math.Min(c6.scale + 0.02f, c6.baseScale + 0.1f);
                }
                else
                {
                    c6.scale = Math.Max(c6.scale - 0.02f, c6.baseScale);
                }
            }
            foreach (ClickableTextureComponent c5 in rightSelectionButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    c5.scale = Math.Min(c5.scale + 0.02f, c5.baseScale + 0.1f);
                }
                else
                {
                    c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
                }
            }
            
            // OK / confirm button
            if (okButton.containsPoint(x, y))
            {
                okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
            }
            else
            {
                okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
            }

            // Save favorite outfit button
            if (saveFavoriteButton.containsPoint(x, y))
            {
                saveFavoriteButton.scale = Math.Min(saveFavoriteButton.scale + 0.02f, saveFavoriteButton.baseScale + 0.05f);
            }
            else
            {
                saveFavoriteButton.scale = Math.Max(saveFavoriteButton.scale - 0.02f, saveFavoriteButton.baseScale);
            }

            // Favorites category buttons
            categoryShading = new Rectangle(0, 0, 0, 0);
            foreach (ClickableTextureComponent c in categoryButtons)
            {
                if (c.containsPoint(x, y))
                {
                    // Assign shading to any hovered categories that aren't selected
                    if (c != categorySelected)
                    {
                        categoryShading = new Rectangle(c.bounds.X + 12, c.bounds.Y + 12, 64, 48);
                    }
                    hoverText = c.name;
                }
            }

            // Equipment icon hover - track hovered item for standard tooltip display
            hoveredItem = null;
            foreach (ClickableComponent c in equipmentIcons)
            {
                if (c.containsPoint(x, y))
                {
                    hoveredItem = c.name switch
                    {
                        "Hat" => Game1.player.hat.Value,
                        "Shirt" => Game1.player.shirtItem.Value,
                        "Pants" => Game1.player.pantsItem.Value,
                        "Boots" => Game1.player.boots.Value,
                        "Left Ring" => Game1.player.leftRing.Value,
                        "Right Ring" => Game1.player.rightRing.Value,
                        _ => null
                    };
                    if (hoveredItem == null)
                    {
                        hoverText = $"Empty {c.name} Slot";
                    }
                    break;
                }
            }

            // Top bar handling
            menuManager.handleTopBarOnHover(x, y, ref hoverText);
        }

        // Game Window Resize - reposition all UI elements
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            // Call base first to handle standard menu repositioning
            base.gameWindowSizeChanged(oldBounds, newBounds);

            // Then recalculate our menu position to center it properly
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            xPositionOnScreen = (int)topLeft.X;
            yPositionOnScreen = (int)topLeft.Y;

            // Reposition close button after we've set our position
            if (upperRightCloseButton != null)
            {
                upperRightCloseButton.bounds = new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48);
            }

            // Reposition portrait box
            _portraitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder, yPositionOnScreen + 64, 256, 384);

            // Reposition equipment icons
            int eqIconXOffset = _portraitBox.X + _portraitBox.Width / 2 - 81 - 16;
            int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
            equipmentIcons[0].bounds = new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64);           // Hat
            equipmentIcons[1].bounds = new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64);      // Shirt
            equipmentIcons[2].bounds = new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64); // Pants
            equipmentIcons[3].bounds = new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64);      // Boots
            equipmentIcons[4].bounds = new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64);     // Left Ring
            equipmentIcons[5].bounds = new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64);// Right Ring

            // Reposition selection buttons
            int selectorBtnsX = xPositionOnScreen + width / 2 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder - 120;
            int selectorBtnsY = _portraitBox.Y - 32;
            int yBtnSpacing = 96;
            int arrowOffset = 8;
            int labelSpacing = 4;

            // Direction buttons (matching FavoritesMenu positioning)
            leftSelectionButtons[0].bounds = new Rectangle(_portraitBox.X - 42, _portraitBox.Bottom - 24, 60, 60);
            rightSelectionButtons[0].bounds = new Rectangle(_portraitBox.X + 256 - 38, _portraitBox.Bottom - 24, 60, 60);

            // Hat, Shirt, Pants, Shoes, Hair, Accessory buttons and labels (indices 1-6)
            for (int i = 0; i < 6; i++)
            {
                int yOffset = i * yBtnSpacing;
                leftSelectionButtons[i + 1].bounds = new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48);
                rightSelectionButtons[i + 1].bounds = new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48);
                labels[i].bounds = new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1);
                itemLabels[i].bounds = new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1);
            }

            // Reposition OK button
            okButton.bounds = new Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 56, yPositionOnScreen + height - borderWidth - spaceToClearTopBorder + 28, 64, 64);

            // Reposition category buttons
            int catXpos = xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 280;
            int catYpos = _portraitBox.Y;
            categoryButtons[0].bounds = new Rectangle(catXpos, catYpos, 88, 72);
            categoryButtons[1].bounds = new Rectangle(catXpos + 90, catYpos, 88, 72);
            categoryButtons[2].bounds = new Rectangle(catXpos + 180, catYpos, 88, 72);
            categoryButtons[3].bounds = new Rectangle(catXpos + 45, catYpos + 80, 88, 72);
            categoryButtons[4].bounds = new Rectangle(catXpos + 135, catYpos + 80, 88, 72);

            // Reposition save favorite button
            saveFavoriteBox = new Rectangle(catXpos + 45, catYpos + 160, 178, 72);
            saveFavoriteButton.bounds = saveFavoriteBox;

            // Reposition top tabs
            menuManager.repositionTopTabButtons(this);
        }

        // ** DRAW **
        // *TODO* make labels look nicer for categories
        public override void draw(SpriteBatch b)
        {
            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }

            // Draw title scroll and background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, "Wardrobe", xPositionOnScreen + width / 2, yPositionOnScreen - 64);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);

            // Draw farmer portrait
            b.Draw(StardewOutfitManager.assetManager.customSprites, _portraitBox, new Rectangle(0, 0, 128, 192), Color.White);
            FarmerRenderer.isDrawingForUI = true;
            CustomModTools.DrawCustom.drawFarmerScaled(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(_portraitBox.Center.X - 64, _portraitBox.Bottom - 320), Color.White, 2f, _displayFarmer);
            FarmerRenderer.isDrawingForUI = false;

            // Draw equipment icons
            foreach (ClickableComponent c in this.equipmentIcons)
            {
                switch (c.name)
                {
                    case "Hat":
                        if (Game1.player.hat.Value != null)
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                            Game1.player.hat.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, 1f, 0.866f, StackDrawType.Hide);
                        }
                        else
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42), Color.White);
                        }
                        break;
                    case "Right Ring":
                        if (Game1.player.rightRing.Value != null)
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                            Game1.player.rightRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
                        }
                        else
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
                        }
                        break;
                    case "Left Ring":
                        if (Game1.player.leftRing.Value != null)
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                            Game1.player.leftRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
                        }
                        else
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
                        }
                        break;
                    case "Boots":
                        if (Game1.player.boots.Value != null)
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                            Game1.player.boots.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
                        }
                        else
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40), Color.White);
                        }
                        break;
                    case "Shirt":
                        if (Game1.player.shirtItem.Value != null)
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                            Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
                        }
                        else
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69), Color.White);
                        }
                        break;
                    case "Pants":
                        if (Game1.player.pantsItem.Value != null)
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                            Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
                        }
                        else
                        {
                            b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68), Color.White);
                        }
                        break;
                }
            }

            // Draw arrows and OK buttons
            foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
            {
                leftSelectionButton.draw(b);
            }
            foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
            {
                rightSelectionButton.draw(b);
            }
            okButton.draw(b);

            // Draw save favorite button
            float scale = saveFavoriteButton.scale;
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)(saveFavoriteBox.X + saveFavoriteBox.Width/2 - (saveFavoriteBox.Width/2 * scale)), (int)(saveFavoriteBox.Y + saveFavoriteBox.Height / 2 - (saveFavoriteBox.Height / 2 * scale)), (int)(saveFavoriteBox.Width - saveFavoriteBox.Width + (saveFavoriteBox.Width * scale)), (int)(saveFavoriteBox.Height - saveFavoriteBox.Height + (saveFavoriteBox.Height * scale)), Color.White, 1f, false);
            Utility.drawTextWithShadow(b, "Save Outfit", Game1.smallFont, new Vector2(saveFavoriteBox.X + saveFavoriteBox.Width / 2 - Game1.smallFont.MeasureString("Save Outfit").X / 2f * scale, saveFavoriteBox.Y + saveFavoriteBox.Height / 2 - 14 * scale), Game1.textColor, 1f * scale);

            // Draw favorite category buttons
            foreach (ClickableTextureComponent categoryButton in categoryButtons)
            {
                categoryButton.draw(b);
            }
            // Shade active and hovered categories
            b.Draw(Game1.staminaRect, new Rectangle(categorySelected.bounds.X + 12, categorySelected.bounds.Y + 12, 64, 48), Color.Black * .4f);
            if (categoryShading != new Rectangle(categorySelected.bounds.X + 12, categorySelected.bounds.Y + 12, 64, 48))
            {
                b.Draw(Game1.staminaRect, categoryShading, Color.Black * .1f);
            }

            // Draw TopBar
            menuManager.drawTopBar(b);

            // Draw labels
            foreach (ClickableComponent c in labels)
            {
                if (!c.visible)
                {
                    continue;
                }

                float offset = 0f;
                Color color = Game1.textColor;
                if (c == descriptionLabel)
                {
                    offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
                }
                else
                {
                    color = Game1.textColor;
                }
                Utility.drawTextWithShadow(b, c.name, Game1.smallFont, new Vector2((c.bounds.X + 21) - Game1.smallFont.MeasureString(c.name).X / 2f, c.bounds.Y + 5f), color);
            }
            foreach (ClickableComponent c in itemLabels)
            {
                if (c.name.Length > 0)
                {
                    Utility.drawTextWithShadow(b, c.name, Game1.smallFont, new Vector2((c.bounds.X + 21) - Game1.smallFont.MeasureString(c.name).X / 2f, c.bounds.Y + 5f), Game1.textColor);
                }
            }

            // Draw hover text or item tooltip
            if (hoveredItem != null)
            {
                IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
            }
            else if (!hoverText.Equals(""))
            {
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}
