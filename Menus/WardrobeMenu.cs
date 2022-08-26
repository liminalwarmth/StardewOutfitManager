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

namespace StardewOutfitManager.Menus
{
    // This class defines the new Wardrobe customization menu
    internal class WardrobeMenu : IClickableMenu
    {
        private Rectangle _portraitBox;
        private Farmer _displayFarmer;
        private string hoverText = "";

        // Item Display Name Labels
        private ClickableComponent descriptionLabel;
        private ClickableComponent hatLabel;
        private ClickableComponent shirtLabel;
        private ClickableComponent pantsLabel;
        private ClickableComponent shoesLabel;

        // Basic UI Button Groups
        public List<ClickableComponent> labels = new List<ClickableComponent>();
        public List<ClickableComponent> itemLabels = new List<ClickableComponent>();
        public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

        // Additional Buttons
        public ClickableTextureComponent okButton;

        // Menu, Inventory Lists, List Indexes
        public ShopMenu dresserMenu = StardewOutfitManager.tabSwitcher.originalDresserMenu;
        public StorageFurniture dresserObject = StardewOutfitManager.tabSwitcher.dresserObject;
        public List<ISalable> hatStock = new List<ISalable>();
        public List<ISalable> shirtStock = new List<ISalable>();
        public List<ISalable> pantsStock = new List<ISalable>();
        public List<ISalable> shoesStock = new List<ISalable>();
        public List<ISalable> ringsStock = new List<ISalable>();
        public int hatIndex = -1;
        public int shirtIndex = -1;
        public int pantsIndex = -1;
        public int shoesIndex = -1;
        public int priorItemIndex = -1;

        // Equipment Display
        public const int region_hat = 101;
        public const int region_ring1 = 102;
        public const int region_ring2 = 103;
        public const int region_boots = 104;
        public const int region_shirt = 108;
        public const int region_pants = 109;
        public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

        // Character Customization Variables

        // Main Wardrobe Menu Functionality
        public WardrobeMenu() : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            /// WARDROBE DATA
            // Add the current farmer equipment, if any, to the wardrobe stock lists
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
            // Add the inventory from the base game dresser menu, if any, to wardrobe stock lists
            foreach (ISalable key in dresserMenu.itemPriceAndStock.Keys)
            {
                if (!(key is Item item))
                {
                    continue;
                }
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
            topLeft = Utility.getTopLeftPositionForCenteringOnScreen(128, 192);
            _portraitBox = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + 64, 256, 384);
            _displayFarmer = Game1.player;
            _displayFarmer.faceDirection(2);
            _displayFarmer.FarmerSprite.StopAnimation();

            // Equipment slot displays
            int eqIconXOffset = _portraitBox.X + _portraitBox.Width/2 - 81 - 16;
            int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64), "Hat")
            {
                myID = 101,
                leftNeighborID = 102,
                downNeighborID = 108,
                upNeighborID = 101,
                rightNeighborID = 105,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64), "Shirt")
            {
                myID = 108,
                upNeighborID = 101,
                downNeighborID = 109,
                rightNeighborID = 105,
                leftNeighborID = 103,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64), "Pants")
            {
                myID = 109,
                upNeighborID = 108,
                rightNeighborID = 105,
                leftNeighborID = 104,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64), "Boots")
            {
                myID = 104,
                upNeighborID = 103,
                rightNeighborID = 109,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64), "Left Ring")
            {
                myID = 102,
                downNeighborID = 103,
                upNeighborID = 102,
                rightNeighborID = 101,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64), "Right Ring")
            {
                myID = 103,
                upNeighborID = 102,
                downNeighborID = 104,
                rightNeighborID = 108,
                fullyImmutable = true
            });

            // Player display window movement buttons
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 40, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1.25f)
            {
                myID = 522,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X + 256 - 40, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1.25f)
            {
                myID = 523,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            // Hat cycle buttons
            int selectorBtnsX = _portraitBox.Right + 128;
            int selectorBtnsY = _portraitBox.Y - 32;
            int yOffset = 0;
            int yBtnSpacing = 96;
            leftSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 514,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Hat"));
            rightSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 515,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            hatLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69, 1, 1), _displayFarmer.hat.Value == null ? "None" : _displayFarmer.hat.Value.DisplayName);
            itemLabels.Add(hatLabel);

            // Shirt cycle buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 516,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Shirt"));
            rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 517,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            shirtLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69, 1, 1), _displayFarmer.shirtItem.Value == null ? "None" : _displayFarmer.shirtItem.Value.DisplayName);
            itemLabels.Add(shirtLabel);

            // Pants cycle buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 518,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Pants"));
            rightSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 519,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            pantsLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69, 1, 1), _displayFarmer.pantsItem.Value == null ? "None" : _displayFarmer.pantsItem.Value.DisplayName);
            itemLabels.Add(pantsLabel);

            // Shoes Buttons
            yOffset += yBtnSpacing;
            leftSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(selectorBtnsX - 64, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 520,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 28, 1, 1), "Shoes"));
            rightSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(selectorBtnsX + 128, selectorBtnsY + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 521,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            shoesLabel = new ClickableComponent(new Rectangle(selectorBtnsX + 128 - 86, selectorBtnsY + yOffset + 69, 1, 1), _displayFarmer.boots.Value == null ? "None" : _displayFarmer.boots.Value.DisplayName);
            itemLabels.Add(shoesLabel);

            // Top Bar Tab Switcher Buttons
            StardewOutfitManager.tabSwitcher.includeTopTabButtons(this);

            // Basic UI Functionality Buttons
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 505,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
        }

        // Handle menu selection clicks
        private void selectionClick(string name, int change)
        {
            switch (name)
            {
                case "Hat":
                    {
                        ClothingSwap(name, ref hatIndex, ref hatStock, change);
                        break;
                    }
                case "Shirt":
                    {
                        ClothingSwap(name, ref shirtIndex, ref shirtStock, change);
                        break;
                    }
                case "Pants":
                    {
                        ClothingSwap(name, ref pantsIndex, ref pantsStock, change);
                        break;
                    }
                case "Shoes":
                    {
                        ClothingSwap(name, ref shoesIndex, ref shoesStock, change);
                        break;
                    }
                case "Direction":
                    {
                        _displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
                        _displayFarmer.FarmerSprite.StopAnimation();
                        _displayFarmer.completelyStopAnimatingOrDoingAction();
                        Game1.playSound("pickUpItem");
                        break;
                    }
            }
        }

        // Receive left click action
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
                exitThisMenu();
                Game1.playSound("coin");
            }
            StardewOutfitManager.tabSwitcher.handleTopBarLeftClick(x, y);
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
            StardewOutfitManager.tabSwitcher.handleTopBarOnHover(x, y);
        }


        // Handle GamePad Trigger Buttons
        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b != Buttons.RightTrigger && b != Buttons.LeftTrigger)
            {
                return;
            }
            if (base.currentlySnappedComponent != null && base.currentlySnappedComponent.myID >= 3546)
            {
                //
            }
            else
            {
                this.snapToDefaultClickableComponent();
            }
            Game1.playSound("shiny4");
        }

        // Draw Wardrobe Menu
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
            StardewOutfitManager.tabSwitcher.drawTopBar(b);
            okButton.draw(b);

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

        // Clothing Swap keeps the dresser inventory, stock lists, and player in sync and equips items shown in the display menu on the player
        private void ClothingSwap(string itemCategory, ref int itemIndex, ref List<ISalable> stockList, int change)
        {
            ISalable removeFromDresser = null;
            ISalable addToDresser = null;

            // Move to next or prior clothing item and update current and prior item indexes for reference, based on arrow direction change
            priorItemIndex = itemIndex;
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
            // Calculate clothing items to be swapped between dresser and player, if any
            if (itemIndex == -1)
            {
                // Case: Moving to or staying on no item equipped, possible prior item
                removeFromDresser = null;
                // Case: Moving from equipped item to no item equipped
                if (priorItemIndex != itemIndex)
                {
                    addToDresser = stockList[priorItemIndex];
                }
            }
            else
            {
                // Case: Moving from no item equipped to an item equipped
                if (priorItemIndex == -1)
                {
                    removeFromDresser = stockList[itemIndex];
                    addToDresser = null;
                }
                // Case: Moving from an equipped item to another equipped item
                else
                {
                    removeFromDresser = stockList[itemIndex];
                    addToDresser = stockList[priorItemIndex];
                }
            }

            // Swap dresser inventory and player equipped items
            if (addToDresser != null)
            {
                if (addToDresser is Item)
                {
                    dresserObject.heldItems.Add(addToDresser as Item);
                    if (dresserMenu != null && dresserMenu is ShopMenu)
                    {
                        Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
                        List<Item> list = dresserObject.heldItems.ToList();
                        list.Sort(dresserObject.SortItems);
                        foreach (Item item in list)
                        {
                            contents[item] = new int[2] { 0, 1 };
                        }
                        (dresserMenu as ShopMenu).setItemPriceAndStock(contents);
                    }
                }
            }
            // If an item is being taken from the dresser, we need to equip it on the player
            if (removeFromDresser != null)
            {
                // Remove from dresser storage
                dresserObject.heldItems.Remove(removeFromDresser as Item);
                dresserMenu.forSale.Remove(removeFromDresser);
                dresserMenu.itemPriceAndStock.Remove(removeFromDresser);
                // Equip on player
                Item equipThing = removeFromDresser as Item;
                if (equipThing.Category == -95)
                {
                    _displayFarmer.hat.Set(equipThing as StardewValley.Objects.Hat);
                    hatLabel.name = _displayFarmer.hat.Value.DisplayName;
                }
                else if (equipThing is Clothing && (equipThing as Clothing).clothesType.Value == 0)
                {
                    _displayFarmer.shirtItem.Set(equipThing as StardewValley.Objects.Clothing);
                    shirtLabel.name = _displayFarmer.shirtItem.Value.DisplayName;
                }
                else if (equipThing is Clothing && (equipThing as Clothing).clothesType.Value == 1)
                {
                    _displayFarmer.pantsItem.Set(equipThing as StardewValley.Objects.Clothing);
                    pantsLabel.name = _displayFarmer.pantsItem.Value.DisplayName;
                }
                else if (equipThing.Category == -97)
                {
                    _displayFarmer.boots.Set(equipThing as StardewValley.Objects.Boots);
                    shoesLabel.name = _displayFarmer.boots.Value.DisplayName;
                    _displayFarmer.changeShoeColor(_displayFarmer.boots.Value.indexInColorSheet.Value);
                }
            }
            // If no item was taken from the dresser, the player isn't wearing anything in that slot
            else
            {
                if (itemCategory == "Hat")
                {
                    _displayFarmer.hat.Set(null);
                    hatLabel.name = "None";
                }
                else if (itemCategory == "Shirt")
                {
                    _displayFarmer.shirtItem.Set(null);
                    shirtLabel.name = "None";
                }
                else if (itemCategory == "Pants")
                {
                    _displayFarmer.pantsItem.Set(null);
                    pantsLabel.name = "None";
                }
                else if (itemCategory == "Shoes")
                {
                    _displayFarmer.boots.Set(null);
                    shoesLabel.name = "None";
                    _displayFarmer.changeShoeColor(12);
                }
            }
            _displayFarmer.UpdateClothing();
            _displayFarmer.completelyStopAnimatingOrDoingAction();
            Game1.playSound("pickUpItem");
        }
    }
}
