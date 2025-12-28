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
        private Rectangle _portraitBackground = new Rectangle(0, 0, 128, 192);
        private Farmer _displayFarmer;
        private string hoverText = "";
        private Item hoveredItem = null;

        // Portrait Box Backgrounds (seasonal)
        private static readonly Rectangle bgDefault = new Rectangle(0, 0, 128, 192);
        private static readonly Rectangle bgSpring = new Rectangle(128, 0, 128, 192);
        private static readonly Rectangle bgSummer = new Rectangle(256, 0, 128, 192);
        private static readonly Rectangle bgFall = new Rectangle(384, 0, 128, 192);
        private static readonly Rectangle bgWinter = new Rectangle(512, 0, 128, 192);
        private static readonly Rectangle bgSpecial = new Rectangle(512, 192, 128, 192);

        // Pending outfit save state (used when naming dialog is open)
        private string _pendingOutfitCategory;

        // Matched outfit name (null if current outfit is new/unsaved)
        private string _matchedOutfitName;

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

        // Rename and Delete buttons (shown when displaying a saved outfit)
        public ClickableTextureComponent renameButton;
        public ClickableTextureComponent deleteButton;
        private Texture2D weaponsTexture;  // For wrench icon

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

            // Set up portrait and farmer (offset +24px to make room for outfit name label above)
            _portraitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder, yPositionOnScreen + 88, 256, 384);
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

            // Check if current outfit matches a saved favorite (must be after categorySelected is set)
            UpdateMatchedOutfitName();

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

            // Rename and Delete buttons (shown when displaying a saved outfit)
            // Position to the left of the portrait box
            weaponsTexture = Game1.content.Load<Texture2D>("TileSheets/weapons");
            renameButton = new ClickableTextureComponent("Rename", new Rectangle(_portraitBox.X - 64, _portraitBox.Y, 56, 56), null, "Rename Outfit", weaponsTexture, new Rectangle(64, 64, 16, 16), 1f)
            {
                myID = 9996,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = 9995,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                rightNeighborID = ClickableComponent.SNAP_AUTOMATIC
            };
            deleteButton = new ClickableTextureComponent("Delete", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + 56 + 8, 56, 56), null, "Delete Outfit", Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 1f)
            {
                myID = 9995,
                upNeighborID = 9996,
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
                // Update matched outfit name for new category
                UpdateMatchedOutfitName();
                Game1.playSound("smallSelect");
            }
        }

        // Update the matched outfit name based on current player equipment and selected category
        private void UpdateMatchedOutfitName()
        {
            string category = categorySelected.name;
            _matchedOutfitName = favoritesData.FindMatchingOutfitName(_displayFarmer, category);

            // Update background based on whether outfit is saved (show seasonal bg for saved outfits)
            _portraitBackground = (_matchedOutfitName != null) ? GetBackgroundForCategory(category) : bgDefault;
        }

        // Get the display name for the current outfit (matched name or "New Outfit")
        private string GetCurrentOutfitDisplayName()
        {
            // Saved outfit names get quotes, "New Outfit" does not
            return _matchedOutfitName != null ? $"\"{_matchedOutfitName}\"" : "New Outfit";
        }

        // Get the seasonal background rectangle for a category
        private static Rectangle GetBackgroundForCategory(string category)
        {
            return category switch
            {
                "Spring" => bgSpring,
                "Summer" => bgSummer,
                "Fall" => bgFall,
                "Winter" => bgWinter,
                "Special" => bgSpecial,
                _ => bgDefault
            };
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
                        UpdateMatchedOutfitName();
                        break;
                    }
                case "Shirt":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref shirtIndex, ref shirtStock, change), shirtLabel);
                        UpdateMatchedOutfitName();
                        break;
                    }
                case "Pants":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref pantsIndex, ref pantsStock, change), pantsLabel);
                        UpdateMatchedOutfitName();
                        break;
                    }
                case "Shoes":
                    {
                        this.ItemExchange(dresserObject, _displayFarmer, name, ClothingSwap(ref shoesIndex, ref shoesStock, change), shoesLabel);
                        UpdateMatchedOutfitName();
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
                        UpdateMatchedOutfitName();
                        break;
                    }
                case "Accessory":
                    {
                        this.AccessorySwap(name, change, _displayFarmer, accLabel);
                        UpdateMatchedOutfitName();
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
            // Don't process key presses when a child menu (like NamingMenu) is active
            if (GetChildMenu() != null)
                return;
            base.receiveKeyPress(key);
        }

        // Handle Game Pad Controls
        public override void receiveGamePadButton(Buttons b)
        {
            // Don't process gamepad buttons when a child menu (like NamingMenu) is active
            if (GetChildMenu() != null)
                return;
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
            // Don't process clicks when a child menu (like NamingMenu) is active
            if (GetChildMenu() != null)
                return;

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
            
            // Save current outfit as favorite - opens naming dialog with random name suggestion
            // Only active if the outfit doesn't already exist (button is grayed out otherwise)
            if (saveFavoriteButton.containsPoint(x, y) && _matchedOutfitName == null)
            {
                saveFavoriteButton.scale -= 0.25f;
                saveFavoriteButton.scale = Math.Max(0.75f, saveFavoriteButton.scale);

                string category = categorySelected.name;

                // Store the pending category for the callback
                _pendingOutfitCategory = category;

                // Get a random name as default suggestion
                string defaultName = StardewOutfitManager.outfitNameManager.GetRandomName(_pendingOutfitCategory);

                // Open naming dialog
                Game1.playSound("smallSelect");
                hoverText = "";  // Clear hover text

                OutfitNamingMenu namingMenu = new OutfitNamingMenu(
                    OnNewOutfitNamed,
                    "Name This Outfit",
                    defaultName,
                    _pendingOutfitCategory,
                    OnNewOutfitCancelled
                );

                // Configure textbox (same as FavoritesMenu)
                namingMenu.textBox.textLimit = 15;  // Max 15 characters for outfit names
                namingMenu.textBox.limitWidth = false;
                namingMenu.textBox.Width = 384;
                namingMenu.textBox.Text = defaultName;

                // Center just the textbox (not the full assembly with buttons)
                int textBoxVisualWidth = namingMenu.textBox.Width + 16;
                int gapAfterTextBox = 16;
                int doneButtonWidth = namingMenu.doneNamingButton.bounds.Width;
                int gapAfterDone = 8;

                int textBoxCenterX = Game1.uiViewport.Width / 2 - textBoxVisualWidth / 2;
                namingMenu.textBox.X = textBoxCenterX;
                namingMenu.doneNamingButton.bounds.X = textBoxCenterX + textBoxVisualWidth + gapAfterTextBox;
                namingMenu.randomButton.bounds.X = namingMenu.doneNamingButton.bounds.X + doneButtonWidth + gapAfterDone;

                // Move textbox and buttons up to reduce gap with title banner
                int verticalShift = 40;
                namingMenu.textBox.Y -= verticalShift;
                namingMenu.doneNamingButton.bounds.Y -= verticalShift;
                namingMenu.randomButton.bounds.Y -= verticalShift;

                SetChildMenu(namingMenu);
            }

            // Rename button - only active when a saved outfit is displayed
            if (renameButton.containsPoint(x, y) && _matchedOutfitName != null)
            {
                renameButton.scale -= 0.25f;
                renameButton.scale = Math.Max(0.75f, renameButton.scale);
                Game1.playSound("smallSelect");
                hoverText = "";

                // Open naming menu with current outfit name
                OutfitNamingMenu namingMenu = new OutfitNamingMenu(
                    OnOutfitRenamed,
                    "Rename Outfit",
                    _matchedOutfitName,
                    categorySelected.name,
                    OnRenameCancelled
                );
                // Configure textbox
                namingMenu.textBox.textLimit = 15;
                namingMenu.textBox.limitWidth = false;
                namingMenu.textBox.Width = 384;
                namingMenu.textBox.Text = _matchedOutfitName;

                // Center the textbox
                int textBoxVisualWidth = namingMenu.textBox.Width + 16;
                int gapAfterTextBox = 16;
                int doneButtonWidth = namingMenu.doneNamingButton.bounds.Width;
                int gapAfterDone = 8;
                int textBoxCenterX = Game1.uiViewport.Width / 2 - textBoxVisualWidth / 2;
                namingMenu.textBox.X = textBoxCenterX;
                namingMenu.doneNamingButton.bounds.X = textBoxCenterX + textBoxVisualWidth + gapAfterTextBox;
                namingMenu.randomButton.bounds.X = namingMenu.doneNamingButton.bounds.X + doneButtonWidth + gapAfterDone;

                // Move up to reduce gap with title banner
                int verticalShift = 40;
                namingMenu.textBox.Y -= verticalShift;
                namingMenu.doneNamingButton.bounds.Y -= verticalShift;
                namingMenu.randomButton.bounds.Y -= verticalShift;

                SetChildMenu(namingMenu);
            }
            else
            {
                renameButton.scale = renameButton.baseScale;
            }

            // Delete button - only active when a saved outfit is displayed
            if (deleteButton.containsPoint(x, y) && _matchedOutfitName != null)
            {
                deleteButton.scale -= 0.25f;
                deleteButton.scale = Math.Max(0.75f, deleteButton.scale);
                Game1.playSound("trashcan");

                // Delete the outfit from favorites
                DeleteCurrentOutfit();
            }
            else
            {
                deleteButton.scale = deleteButton.baseScale;
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
            // Don't update hover text when a child menu (like NamingMenu) is active
            if (GetChildMenu() != null)
            {
                hoverText = "";
                return;
            }

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

            // Save favorite outfit button (only animate if enabled - i.e., outfit not already saved)
            if (saveFavoriteButton.containsPoint(x, y) && _matchedOutfitName == null)
            {
                saveFavoriteButton.scale = Math.Min(saveFavoriteButton.scale + 0.02f, saveFavoriteButton.baseScale + 0.05f);
            }
            else
            {
                saveFavoriteButton.scale = Math.Max(saveFavoriteButton.scale - 0.02f, saveFavoriteButton.baseScale);
            }

            // Rename button hover (only active when saved outfit is displayed)
            if (_matchedOutfitName != null && renameButton.containsPoint(x, y))
            {
                renameButton.scale = Math.Min(renameButton.scale + 0.02f, renameButton.baseScale + 0.1f);
                hoverText = "Rename Outfit";
            }
            else
            {
                renameButton.scale = Math.Max(renameButton.scale - 0.02f, renameButton.baseScale);
            }

            // Delete button hover (only active when saved outfit is displayed)
            if (_matchedOutfitName != null && deleteButton.containsPoint(x, y))
            {
                deleteButton.scale = Math.Min(deleteButton.scale + 0.02f, deleteButton.baseScale + 0.1f);
                hoverText = "Delete Outfit";
            }
            else
            {
                deleteButton.scale = Math.Max(deleteButton.scale - 0.02f, deleteButton.baseScale);
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

        // Callback for NamingMenu when player finishes naming a new outfit
        private void OnNewOutfitNamed(string name)
        {
            SetChildMenu(null);

            string trimmedName = name?.Trim();
            if (!string.IsNullOrEmpty(trimmedName) && _pendingOutfitCategory != null)
            {
                if (favoritesData.SaveNewOutfit(_displayFarmer, _pendingOutfitCategory, trimmedName))
                {
                    Game1.playSound("dwop");
                    // Update display to show the newly saved outfit (name + seasonal background)
                    UpdateMatchedOutfitName();
                }
                else
                {
                    Game1.playSound("cancel"); // Outfit already exists
                }
            }
            _pendingOutfitCategory = null;
        }

        // Callback for when player cancels the naming dialog (Escape key)
        private void OnNewOutfitCancelled()
        {
            SetChildMenu(null);
            _pendingOutfitCategory = null;
        }

        // Callback for when player finishes renaming an existing outfit
        private void OnOutfitRenamed(string newName)
        {
            SetChildMenu(null);

            string trimmedName = newName?.Trim();
            if (!string.IsNullOrEmpty(trimmedName) && _matchedOutfitName != null)
            {
                // Find the matching outfit in favorites and update its name
                var matchingOutfit = favoritesData.Favorites.FirstOrDefault(o =>
                    o.Name == _matchedOutfitName && o.Category == categorySelected.name);

                if (matchingOutfit != null)
                {
                    matchingOutfit.Name = trimmedName;
                    _matchedOutfitName = trimmedName;
                    Game1.playSound("coin");
                }
            }
        }

        // Callback for when player cancels the rename dialog
        private void OnRenameCancelled()
        {
            SetChildMenu(null);
        }

        // Delete the current outfit from favorites (player continues wearing the outfit)
        private void DeleteCurrentOutfit()
        {
            if (_matchedOutfitName != null)
            {
                // Find and remove the matching outfit from favorites
                var matchingOutfit = favoritesData.Favorites.FirstOrDefault(o =>
                    o.Name == _matchedOutfitName && o.Category == categorySelected.name);

                if (matchingOutfit != null)
                {
                    favoritesData.Favorites.Remove(matchingOutfit);
                    // Update display to show as unsaved (name becomes "New Outfit", background becomes default)
                    UpdateMatchedOutfitName();
                }
            }
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

            // Reposition portrait box (offset +24px to make room for outfit name label above)
            _portraitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder, yPositionOnScreen + 88, 256, 384);

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

            // Reposition rename and delete buttons
            renameButton.bounds = new Rectangle(_portraitBox.X - 64, _portraitBox.Y, 56, 56);
            deleteButton.bounds = new Rectangle(_portraitBox.X - 64, _portraitBox.Y + 56 + 8, 56, 56);

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

            // Draw outfit name label above portrait (positioned in gap between inner menu edge and portrait box)
            string outfitName = GetCurrentOutfitDisplayName();
            // The menu border texture is about 24px thick visually
            int innerMenuTop = yPositionOnScreen + 24;
            // Position text 62.5% down the gap (shifted 25% lower from center)
            Vector2 nameSize = Game1.dialogueFont.MeasureString(outfitName);
            float nameX = _portraitBox.Center.X - nameSize.X / 2;
            float nameY = innerMenuTop + (_portraitBox.Y - innerMenuTop) * 0.625f - nameSize.Y / 2f;
            Utility.drawTextWithShadow(b, outfitName, Game1.dialogueFont, new Vector2(nameX, nameY), Game1.textColor);

            // Draw farmer portrait (with seasonal background for saved outfits)
            b.Draw(StardewOutfitManager.assetManager.customSprites, _portraitBox, _portraitBackground, Color.White);
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

            // Draw save favorite button (with dark overlay when disabled)
            float scale = saveFavoriteButton.scale;
            bool saveEnabled = (_matchedOutfitName == null);
            // Calculate scaled button bounds
            int scaledX = (int)(saveFavoriteBox.X + saveFavoriteBox.Width / 2 - (saveFavoriteBox.Width / 2 * scale));
            int scaledY = (int)(saveFavoriteBox.Y + saveFavoriteBox.Height / 2 - (saveFavoriteBox.Height / 2 * scale));
            int scaledWidth = (int)(saveFavoriteBox.Width * scale);
            int scaledHeight = (int)(saveFavoriteBox.Height * scale);
            // Draw button with normal colors
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), scaledX, scaledY, scaledWidth, scaledHeight, Color.White, 1f, false);
            Utility.drawTextWithShadow(b, "Save Outfit", Game1.smallFont, new Vector2(saveFavoriteBox.X + saveFavoriteBox.Width / 2 - Game1.smallFont.MeasureString("Save Outfit").X / 2f * scale, saveFavoriteBox.Y + saveFavoriteBox.Height / 2 - 14 * scale), Game1.textColor, 1f * scale);
            // If disabled, draw semi-transparent dark overlay using same rounded texture (matches button shape)
            if (!saveEnabled)
            {
                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), scaledX, scaledY, scaledWidth, scaledHeight, Color.Black * 0.4f, 1f, false);
            }

            // Draw rename and delete buttons only when a saved outfit is displayed
            if (_matchedOutfitName != null)
            {
                // Rename button
                float renameScaleFactor = renameButton.scale / renameButton.baseScale;
                int renameScaledWidth = (int)(renameButton.bounds.Width * renameScaleFactor);
                int renameScaledHeight = (int)(renameButton.bounds.Height * renameScaleFactor);
                int renameOffsetX = (renameButton.bounds.Width - renameScaledWidth) / 2;
                int renameOffsetY = (renameButton.bounds.Height - renameScaledHeight) / 2;
                int renameIconSize = (int)(32 * renameScaleFactor);
                int renameIconPadding = (renameScaledWidth - renameIconSize) / 2;

                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                    renameButton.bounds.X + renameOffsetX, renameButton.bounds.Y + renameOffsetY,
                    renameScaledWidth, renameScaledHeight, Color.White, 1f, false);
                b.Draw(weaponsTexture, new Rectangle(
                    renameButton.bounds.X + renameOffsetX + renameIconPadding,
                    renameButton.bounds.Y + renameOffsetY + renameIconPadding,
                    renameIconSize, renameIconSize),
                    new Rectangle(64, 64, 16, 16), Color.White);

                // Delete button
                float deleteScaleFactor = deleteButton.scale / deleteButton.baseScale;
                int deleteScaledWidth = (int)(deleteButton.bounds.Width * deleteScaleFactor);
                int deleteScaledHeight = (int)(deleteButton.bounds.Height * deleteScaleFactor);
                int deleteOffsetX = (deleteButton.bounds.Width - deleteScaledWidth) / 2;
                int deleteOffsetY = (deleteButton.bounds.Height - deleteScaledHeight) / 2;
                int deleteIconSize = (int)(32 * deleteScaleFactor);
                int deleteIconPadding = (deleteScaledWidth - deleteIconSize) / 2;

                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                    deleteButton.bounds.X + deleteOffsetX, deleteButton.bounds.Y + deleteOffsetY,
                    deleteScaledWidth, deleteScaledHeight, Color.White, 1f, false);
                b.Draw(Game1.mouseCursors, new Rectangle(
                    deleteButton.bounds.X + deleteOffsetX + deleteIconPadding,
                    deleteButton.bounds.Y + deleteOffsetY + deleteIconPadding,
                    deleteIconSize, deleteIconSize),
                    new Rectangle(322, 498, 12, 12), Color.White);
            }

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
