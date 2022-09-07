﻿using System;
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
    // This class defines the Favorites outfit selection menu
    internal class FavoritesMenu : IClickableMenu
    {
        // Outfit button slot definition (jacked from load menu, reference it)
        public class OutfitSlot
        {
            // Base variables
            internal FavoritesMenu menu;
            internal bool isAvailable;
            internal bool isFavorite;
            internal bool isSelected;
            public Dictionary<string, Item> outfitAvailabileItems;
            public Farmer modelFarmer;
            public FavoriteOutfit modelOutfit;
            public Texture2D bgSprite;
            public Rectangle bgBox;

            // Hover attributes
            internal bool isHovered = false;
            internal Rectangle hoverBox = new Rectangle(0, 256, 60, 60);
            internal string outfitName;
            internal string lastWorn;
            internal List<ClickableComponent> itemAvailabilityIcons = new();

            public OutfitSlot(FavoritesMenu menu, Farmer player, FavoriteOutfit outfit, List<Item> playerOwnedItems)
            {
                // Set up background and fake model farmer
                this.menu = menu;
                modelOutfit = outfit;
                bgBox = new Rectangle(menu.xPositionOnScreen, menu.yPositionOnScreen, 128, 192);
                bgSprite = GetCategoryBackground(modelOutfit);
                modelFarmer = CreateFakeEventFarmer(player);

                // Establish equipment availability and dress the display farmer in what's available
                outfitAvailabileItems = modelOutfit.GetOutfitItemAvailability(playerOwnedItems);
                modelOutfit.dressDisplayFarmerWithAvailableOutfitPieces(modelFarmer, outfitAvailabileItems);
                // Set outfit slot to unavailable if any necessary items are missing
                isAvailable = outfitAvailabileItems.ContainsValue(null) ? false : true;

                // Set the on-hover attributes and outfit item availability displays
                outfitName = modelOutfit.Name;
                lastWorn = modelOutfit.LastWorn.ToString();
                int eqIconXOffset = hoverBox.X;
                int eqIconYOffset = hoverBox.Y;
                itemAvailabilityIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64), "Hat"));
                itemAvailabilityIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64), "Shirt"));
                itemAvailabilityIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64), "Pants"));
                itemAvailabilityIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64), "Boots"));
                itemAvailabilityIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64), "Left Ring"));
                itemAvailabilityIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64), "Right Ring"));
            }

            // Activate the outfit slot and display the model outfit
            public void Activate()
            {
                // Change this outfit box to selected and all others to unselected
                isSelected = true;
                // Set the display background to match the outfit background
                //menu.outFitDisplayBG = 
                // Dress the main display farmer in the favorites menu in this outfit

                // Set the queued outfit for equipping the player
            }

            // Draw the outfit box
            public void Draw(SpriteBatch b, int i)
            {
                // Draw selection indicator if this outfit is currently being displayed
                if (isSelected)
                {
                    b.Draw(Game1.staminaRect, new Rectangle(bgBox.X - 4, bgBox.Y - 4, 128 + 8, 192 + 8), Color.White);
                }

                // Draw background
                b.Draw(bgSprite, new Vector2(bgBox.X, bgBox.Y), Color.White);

                // Draw farmer within background box bounds
                FarmerRenderer.isDrawingForUI = true;
                modelFarmer.FarmerRenderer.draw(b, modelFarmer.FarmerSprite.CurrentAnimationFrame, modelFarmer.FarmerSprite.CurrentFrame, modelFarmer.FarmerSprite.SourceRect, new Vector2(bgBox.Center.X - 32, bgBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, modelFarmer);
                FarmerRenderer.isDrawingForUI = false;
                
                // Draw shadow and warning cancel icon if unavailable
                if (!isAvailable)
                {
                    // Shadow box
                    b.Draw(Game1.staminaRect, new Rectangle(bgBox.X, bgBox.Y, 128, 192), Color.Black * .5f);
                    // Cancel icon
                    b.Draw(Game1.mouseCursors, new Rectangle(bgBox.X + bgBox.Width - 38, bgBox.Y + 14, 24, 24), new Rectangle(322, 498, 12, 12), Color.White);
                }

                // Draw heart if favorited
                if (isFavorite)
                {
                    b.Draw(Game1.menuTexture, new Rectangle(bgBox.X + 14, bgBox.Y + 14, 28, 28), new Rectangle(63, 772, 28, 28), Color.White);
                }

                // Draw infobox if hovered on (or snapped to)
                if (isHovered)
                {
                    // Draw box
                    
                    // Draw outfit name

                    // Draw last worn text

                    // Draw item availability
                    foreach (ClickableComponent c in itemAvailabilityIcons)
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
                }
            }

            public void Dispose()
            {
            }

            // Create outfit model
            public Farmer CreateFakeEventFarmer(Farmer player)
            {
                Farmer farmer = new Farmer(new FarmerSprite(player.FarmerSprite.textureName.Value), new Vector2(192f, 192f), 1, "", new List<Item>(), player.IsMale);
                farmer.Name = player.Name;
                farmer.displayName = player.displayName;
                farmer.isFakeEventActor = true;
                farmer.changeGender(player.IsMale);
                farmer.changeHairStyle(player.hair.Value);
                farmer.UniqueMultiplayerID = player.UniqueMultiplayerID;
                farmer.shirtItem.Set(player.shirtItem.Value);
                farmer.pantsItem.Set(player.pantsItem.Value);
                farmer.shirt.Set(player.shirt.Value);
                farmer.pants.Set(player.pants.Value);
                farmer.changeShoeColor(player.shoes.Value);
                farmer.boots.Set(player.boots.Value);
                farmer.leftRing.Set(player.leftRing.Value);
                farmer.rightRing.Set(player.rightRing.Value);
                farmer.hat.Set(player.hat.Value);
                farmer.shirtColor = player.shirtColor;
                farmer.pantsColor.Set(player.pantsColor.Value);
                farmer.changeHairColor(player.hairstyleColor.Value);
                farmer.changeSkinColor(player.skin.Value);
                farmer.accessory.Set(player.accessory.Value);
                farmer.changeEyeColor(player.newEyeColor.Value);
                farmer.UpdateClothing();
                farmer.faceDirection(2);
                farmer.FarmerSprite.StopAnimation();
                return farmer;
            }

            // Get outfit category background
            public Texture2D GetCategoryBackground(FavoriteOutfit outfit)
            {
                Texture2D background = StardewOutfitManager.assetManager.wardrobeBackgroundTexture;
                // Check category and return appropriate background
                switch (outfit.Category)
                {
                    case "Spring":
                        background = StardewOutfitManager.assetManager.bgTextureSpring;
                        break;
                }
                return background;
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
        private Rectangle outFitDisplayBG = new Rectangle(0, 0, 128, 192);

        // Portrait Box Backgrounds
        internal Rectangle bgDefault = new Rectangle(0, 0, 128, 192);
        internal Rectangle bgSpring = new Rectangle(128, 0, 128, 192);
        internal Rectangle bgSummer = new Rectangle(256, 0, 128, 192);
        internal Rectangle bgFall = new Rectangle(384, 0, 128, 192);
        internal Rectangle bgWinter = new Rectangle(512, 0, 128, 192);
        internal Rectangle bgSpecial = new Rectangle(512, 192, 128, 192);

        // Basic UI Button Groups
        public List<ClickableComponent> labels = new();
        public List<ClickableComponent> leftSelectionButtons = new();
        public List<ClickableComponent> rightSelectionButtons = new();

        // Outfit buttons and offsets
        public List<ClickableComponent> outfitButtons = new();
        public List<OutfitSlot> outfitSlots = new();
        internal Rectangle outfitBox;

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
            xPositionOnScreen = (int)topLeft.X;
            yPositionOnScreen = (int)topLeft.Y;

            // Set up portrait and farmer
            _portraitBox = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + 64, 256, 384);
            _displayFarmer = Game1.player;
            _displayFarmer.faceDirection(2);
            _displayFarmer.FarmerSprite.StopAnimation();

            // Player display window movement buttons
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 42, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1.25f));
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X + 256 - 38, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1.25f));

            // Basic UI Functionality Buttons
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 1000,
                upNeighborID = 2000
            };

            // Generate outfit box positions, navigation, and components
            outfitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder + 342, yPositionOnScreen + 100, 592, 436);
            upArrow = new ClickableTextureComponent(new Rectangle(outfitBox.X + outfitBox.Width + 18, outfitBox.Y + 16, 44, 48), Game1.mouseCursors, new Rectangle(76, 72, 40, 44), 1f)
            {
                myID = 97865,
                downNeighborID = 106,
                leftNeighborID = 3546
            };
            downArrow = new ClickableTextureComponent(new Rectangle(outfitBox.X + outfitBox.Width + 18, outfitBox.Y + outfitBox.Height - 56, 44, 48), Game1.mouseCursors, new Rectangle(12, 76, 40, 44), 1f)
            {
                myID = 106,
                upNeighborID = 97865,
                leftNeighborID = 3546
            };
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 8, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, outfitBox.Height - 64 - upArrow.bounds.Height - 20);

            // Generate available player items
            GeneratePlayerOwnedItemList();

            // Generate outfit slots
            GenerateOutfitSlotList();

            // Generate scrollable outfit buttons
            GenerateOutfitButtons();

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
        }

        // Player owned items list generation (determines what is equippable at the moment)
        public void GeneratePlayerOwnedItemList()
        {
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
        }

        // Create list of possible wearable outfits for the chosen categories
        public void GenerateOutfitSlotList()
        {
            foreach (FavoriteOutfit outfit in favoritesData.Favorites)
            {
                outfitSlots.Add(new OutfitSlot(this, _displayFarmer, outfit, playerOwnedItems));
            }
            // Sort list by favorites

            // Sort list by availability
        }

        // Create list of currently displayed outfits as scrollable components
        public void GenerateOutfitButtons()
        {
            // Create buttons (two rows of up to 4 outfits possible to have displayed at one time, (j) is row, (i) is column)
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    outfitButtons.Add(new ClickableComponent(new Rectangle(outfitBox.X + 22 + (i * 128) + (i * 12), outfitBox.Y + 20 + (j * 192) + (j * 12), 128, 192), i.ToString() ?? "")
                    {
                        myID = i + 3546,
                        rightNeighborID = 97865,
                        fullyImmutable = true
                    });
                }
            }
            // Position buttons
            PositionOutfitButtons();
        }

        // Position outfit buttons
        public void PositionOutfitButtons()
        {
            // Only eight outfits can be displayed at a time
            for (int i = 0; i < 8; i++)
            {
                // Only try to position outfitslots which exist
                if (i < outfitSlots.Count)
                {
                    // Match slot draw positions to where the outfit buttons have been placed
                    outfitSlots[i].bgBox.X = outfitButtons[i].bounds.X;
                    outfitSlots[i].bgBox.Y = outfitButtons[i].bounds.Y;
                }
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
                //this.WearFavoriteOutfit(dresserObject, _displayFarmer, favoritesData.Favorites[0], playerOwnedItems);
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
        public override void draw(SpriteBatch b)
        {
            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }
            // General UI (title, background)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, "Favorite Outfits", xPositionOnScreen + width / 2, yPositionOnScreen - 64);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);

            // Outfit displays backdrop box
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 320, 60, 60), outfitBox.X, outfitBox.Y, outfitBox.Width, outfitBox.Height, Color.White, 2f, false);

            // Farmer portrait
            b.Draw(StardewOutfitManager.assetManager.customSprites, _portraitBox, outFitDisplayBG, Color.White);
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

            // Draw Outfit Display Slots (only draw 8 or fewer)
            for (int i = 0; i < 8; i++)
            {
                if (i < outfitSlots.Count)
                {
                    outfitSlots[i].Draw(b, i);
                }
            }

            // Draw navigation
            if (outfitSlots.Count > 8)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, false);
                scrollBar.draw(b);

                upArrow.draw(b);
                downArrow.draw(b);
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
