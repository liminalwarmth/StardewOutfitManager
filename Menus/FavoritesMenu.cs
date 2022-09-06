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
using StardewOutfitManager.Managers;
using StardewOutfitManager.Utils;
using StardewOutfitManager.Data;
using System.Reflection.Metadata.Ecma335;
using static StardewValley.Menus.LoadGameMenu;

namespace StardewOutfitManager.Menus
{

    // Look into public Farmer CreateFakeEventFarmer()

    // This class defines the Favorites outfit selection menu
    internal class FavoritesMenu : IClickableMenu
    {
        // Outfit button slot definition
        public abstract class OutfitSlot : IDisposable
        {
            protected FavoritesMenu menu;

            public OutfitSlot(FavoritesMenu menu)
            {
                this.menu = menu;
            }

            public abstract void Activate();

            public abstract void Draw(SpriteBatch b, int i);

            public virtual void Dispose()
            {
            }
        }

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
        private Texture2D outFitDisplayBG = StardewOutfitManager.assetManager.bgTextureFall;

        // Basic UI Button Groups
        public List<ClickableComponent> labels = new();
        public List<ClickableComponent> leftSelectionButtons = new();
        public List<ClickableComponent> rightSelectionButtons = new();

        // Outfit buttons
        public List<ClickableComponent> outfitButtons = new();
        private int currentOutfitIndex = 0;

        // Scroll bar and controls
        public ClickableTextureComponent upArrow;
        public ClickableTextureComponent downArrow;
        public ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private bool scrolling;

        // Available items for outfits
        public List<Item> playerOwnedItems = new();

        // Snap Regions
        internal const int LABELS = 10000;
        internal const int PORTRAIT = 20000;

        // Additional Buttons
        public ClickableTextureComponent okButton;

        // Main Wardrobe Menu Functionality
        public FavoritesMenu() : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            /// FAVORITES MENU
            // Set up menu structure
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            // Set up portrait and farmer
            _portraitBox = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + 64, 256, 384);
            _displayFarmer = Game1.player;
            _displayFarmer.faceDirection(2);
            _displayFarmer.FarmerSprite.StopAnimation();

            // Player display window movement buttons
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 40, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1.25f));
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X + 256 - 40, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1.25f));

            // Basic UI Functionality Buttons
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 1000,
                upNeighborID = 2000
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
            if (Game1.options.SnappyMenus && Game1.options.gamepadControls)
            {
                snapToDefaultClickableComponent();
            }

            // Build new item index for swapping
            if (Game1.player.hat.Value != null)
            {
                playerOwnedItems.Add(Game1.player.hat.Value);
            }
            if (Game1.player.shirtItem.Value != null)
            {
                playerOwnedItems.Add(Game1.player.shirtItem.Value);
            }
            if (Game1.player.pantsItem.Value != null)
            {
                playerOwnedItems.Add(Game1.player.pantsItem.Value);
            }
            if (Game1.player.boots.Value != null)
            {
                playerOwnedItems.Add(Game1.player.boots.Value);
            }
            if (Game1.player.leftRing.Value != null)
            {
                playerOwnedItems.Add(Game1.player.leftRing.Value);
            }
            if (Game1.player.rightRing.Value != null)
            {
                playerOwnedItems.Add(Game1.player.rightRing.Value);
            }

            // Generate remaining inventory lists directly from the base game dresser object
            List<Item> list = dresserObject.heldItems.ToList();
            list.Sort(dresserObject.SortItems);
            foreach (Item item in list)
            {
                if (item.Category == -95)
                {
                    playerOwnedItems.Add(item);
                }
                else if (item is Clothing && (item as Clothing).clothesType.Value == 0)
                {
                    playerOwnedItems.Add(item as Clothing);
                }
                else if (item is Clothing && (item as Clothing).clothesType.Value == 1)
                {
                    playerOwnedItems.Add(item as Clothing);
                }
                else if (item.Category == -97)
                {
                    playerOwnedItems.Add(item);
                }
                else if (item.Category == -96)
                {
                    playerOwnedItems.Add(item);
                }
            }

            // Generate outfit navigation scroll
            upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
            {
                myID = 97865,
                downNeighborID = 106,
                leftNeighborID = 3546
            };
            downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
            {
                myID = 106,
                upNeighborID = 97865,
                leftNeighborID = 3546
            };
            scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);


            // Generate scrollable outfit buttons
            int outfitsXoffset = IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + 256 + 96;
            int outfitsYoffset = 64;
            for (int i = 0; i < 4; i++)
            {
                outfitButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + outfitsXoffset, yPositionOnScreen + 16 + i * ((height - 256) / 4) + outfitsYoffset, width - 32 - outfitsXoffset, (height - 256) / 4 + 4), i.ToString() ?? "")
                {
                    myID = i + 3546,
                    rightNeighborID = 97865,
                    fullyImmutable = true
                });
            }
        }

        // OUTFIT DISPLAY
        // Create list of favorite outfits as scrollable tabs

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
            // Scrollbar arrows
            //if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
            if (this.downArrow.containsPoint(x, y))
            {
                this.downArrowPressed();
                Game1.playSound("shwip");
            }
            //else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
            else if (this.upArrow.containsPoint(x, y))
            {
                this.upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.scrollBar.containsPoint(x, y))
            {
                this.scrolling = true;
            }
            else if (!this.downArrow.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            // currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));

            // ok button
            if (okButton.containsPoint(x, y))
            {
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
                //StardewOutfitManager.playerManager.cleanMenuExit();
                //test wear favorite outfit
                this.WearFavoriteOutfit(dresserObject, _displayFarmer, favoritesData.Favorites[0], playerOwnedItems);
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

        // Handle menu selection clicks
        private void selectionClick(string name, int change)
        {
            switch (name)
            {
                case "Direction":
                    {
                        _displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
                        _displayFarmer.FarmerSprite.StopAnimation();
                        _displayFarmer.completelyStopAnimatingOrDoingAction();
                        Game1.playSound("stoneStep");
                        break;
                    }
            }
        }

        // Handle scrolling
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (this.scrolling)
            {
                int y2 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upArrow.bounds.Height + 20));
                float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
                //this.currentItemIndex = Math.Min(Math.Max(0, this.forSale.Count - 4), Math.Max(0, (int)((float)this.forSale.Count * percentage)));
                //this.setScrollBarToCurrentIndex();
                //this.updateSaleButtonNeighbors();
                if (y2 != this.scrollBar.bounds.Y)
                {
                    Game1.playSound("shiny4");
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            this.scrolling = false;
        }

        /*
        private void setScrollBarToCurrentIndex()
        {
            if (this.forSale.Count > 0)
            {
                this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.forSale.Count - 4 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + 4;
                if (this.currentItemIndex == this.forSale.Count - 4)
                {
                    this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
            {
                this.downArrowPressed();
                Game1.playSound("shiny4");
            }
        }
        */

        private void downArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            //this.currentItemIndex++;
            //this.setScrollBarToCurrentIndex();
            //this.updateSaleButtonNeighbors();
        }

        private void upArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            //this.currentItemIndex--;
            //this.setScrollBarToCurrentIndex();
            //this.updateSaleButtonNeighbors();
        }

        // DRAW

        //protected virtual void drawSlotBackground(SpriteBatch b, int i, OutfitSlot slot)
        protected virtual void drawSlotBackground(SpriteBatch b, int i)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.outfitButtons[i].bounds.X, this.outfitButtons[i].bounds.Y, this.outfitButtons[i].bounds.Width, this.outfitButtons[i].bounds.Height, Color.Wheat, 4f, drawShadow: false);
        }

        public override void draw(SpriteBatch b)
        {
            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }
            // General UI (title, background)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, "Favorite Outfits", base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Farmer portrait
            b.Draw(outFitDisplayBG, _portraitBox, Color.White);
            FarmerRenderer.isDrawingForUI = true;
            CustomModTools.DrawCustom.drawFarmerScaled(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(_portraitBox.Center.X - 64, _portraitBox.Bottom - 320), Color.White, 2f, _displayFarmer);
            FarmerRenderer.isDrawingForUI = false;

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

            // stolen from load menu
            for (int i = 0; i < outfitButtons.Count; i++)
            {
                if (currentOutfitIndex + i < outfitButtons.Count)
                {
                    drawSlotBackground(b, i);
                    //outfitButtons[currentOutfitIndex + i].Draw(b,i);
                }
            }

            // Draw navigation
            // TODO: gate the scrollbar on if enough items are present for scrolling
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f);
            scrollBar.draw(b);
            
            upArrow.draw(b);
            downArrow.draw(b);

            // Draw TopBar
            menuManager.drawTopBar(b);

            // Draw labels
            foreach (ClickableComponent c in labels)
            {
                if (!c.visible)
                {
                    continue;
                }
                Color color = Game1.textColor;
                Utility.drawTextWithShadow(b, c.name, Game1.smallFont, new Vector2((c.bounds.X + 21) - Game1.smallFont.MeasureString(c.name).X / 2f, c.bounds.Y + 5f), color);
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
    }
}
