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

        // Snap Regions
        internal const int LABELS = 10000;
        internal const int PORTRAIT = 20000;

        // Additional Buttons
        public ClickableTextureComponent okButton;
        public ClickableTextureComponent saveFavoriteButton;

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
                else if (item is Clothing && (item as Clothing).clothesType.Value == 0)
                {
                    shirtStock.Add(item as Clothing);
                }
                else if (item is Clothing && (item as Clothing).clothesType.Value == 1)
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
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            // Set up portrait and farmer
            _portraitBox = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + 64, 256, 384);
            _displayFarmer = Game1.player;
            _displayFarmer.faceDirection(2);
            _displayFarmer.FarmerSprite.StopAnimation();

            // Equipment slot displays
            int eqIconXOffset = _portraitBox.X + _portraitBox.Width / 2 - 81 - 16;
            int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64), "Hat"));
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64), "Shirt"));
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64), "Pants"));
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64), "Boots"));
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64), "Left Ring"));
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64), "Right Ring"));

            // Player display window movement buttons
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 40, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1.25f));
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X + 256 - 40, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1.25f));

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
                upNeighborID = 2000, // first top tab
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
                leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
                region = LABELS
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Accessory", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16 + arrowOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
            accLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69 + labelSpacing, 1, 1), this.GetHairOrAccessoryName("Accessory", _displayFarmer.accessory.Value));
            itemLabels.Add(accLabel);

            // Confirm Button
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 9999,
                upNeighborID = 1005,
                leftNeighborID = 9998
            };

            // Save as Favorite Button
            saveFavoriteButton = new ClickableTextureComponent("SaveFavorite", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56 - 74, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
            {
                myID = 9998,
                upNeighborID = 1005,
                leftNeighborID = 1005,
                rightNeighborID = 9999
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

        // CONTROLS

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
                    // Direction will be 1 (right) or 3 (left) with one exception for these
                    int change = direction == 1 ? 1 : -1;
                    // Left and Right Change the Current Item in Slot
                    if (direction != 2 && direction != 0)
                    {
                        selectionClick(getCurrentlySnappedComponent().name, change);
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
                selectionClick("Direction", 1);
            }
            else if (b == Buttons.LeftShoulder)
            {
                selectionClick("Direction", -1);
            }
            if (b == Buttons.A && getCurrentlySnappedComponent().region == LABELS)
            {
                setCurrentlySnappedComponentTo(9999);
                snapCursorToCurrentSnappedComponent();
                Game1.playSound("smallSelect");
            }
        }

        // Left click action
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (leftSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent c2 in leftSelectionButtons)
                {
                    if (c2.containsPoint(x, y))
                    {
                        selectionClick(c2.name, -1);
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
                        selectionClick(c.name, 1);
                        if (c.scale != 0f)
                        {
                            c.scale -= 0.25f;
                            c.scale = Math.Max(0.75f, c.scale);
                        }
                    }
                }
            }
            if (okButton.containsPoint(x, y))
            {
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
                StardewOutfitManager.playerManager.cleanMenuExit();
            }
            if (saveFavoriteButton.containsPoint(x, y))
            {
                saveFavoriteButton.scale -= 0.25f;
                saveFavoriteButton.scale = Math.Max(0.75f, okButton.scale);
                favoritesData.SaveNewOutfit(_displayFarmer, "TEMP_CAT", "TEMP_NAME");
                Game1.playSound("dwop");
            }
            menuManager.handleTopBarLeftClick(x, y);
        }

        // Handle On-Hover and Resetting Button States
        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";
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
            if (okButton.containsPoint(x, y))
            {
                okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
            }
            else
            {
                okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
            }
            if (saveFavoriteButton.containsPoint(x, y))
            {
                saveFavoriteButton.scale = Math.Min(saveFavoriteButton.scale + 0.02f, saveFavoriteButton.baseScale + 0.1f);
            }
            else
            {
                saveFavoriteButton.scale = Math.Max(saveFavoriteButton.scale - 0.02f, saveFavoriteButton.baseScale);
            }
            menuManager.handleTopBarOnHover(x, y, ref hoverText);
        }

        // Game Window Resize - TODO
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            xPositionOnScreen = Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;

            // TODO: Reposition buttons
            // TODO: Reposition tabs?
        }

        // DRAW
        public override void draw(SpriteBatch b)
        {
            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }

            // General UI (title, background)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, "Wardrobe", base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Farmer portrait
            b.Draw(StardewOutfitManager.assetManager.wardrobeBackgroundTexture, _portraitBox, Color.White);
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

            // Draw buttons
            foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
            {
                leftSelectionButton.draw(b);
            }
            foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
            {
                rightSelectionButton.draw(b);
            }
            okButton.draw(b);
            saveFavoriteButton.draw(b);

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

            // Draw hover text
            if (!hoverText.Equals(""))
            {
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }

        // Handle menu selection clicks
        private void selectionClick(string name, int change)
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
                        _displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
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
    }
}
