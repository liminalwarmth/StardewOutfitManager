﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;

namespace StardewOutfitManager
{
    public class StardewOutfitManager : Mod
    {
        // Asset Manager
        internal static AssetManager assetManager;
        public static IClickableMenu priorShopMenu;

        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            // Set up asset manager
            assetManager = new AssetManager(helper);

            // Menu change event
            //helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Display.RenderingActiveMenu += this.OnMenuChanged;
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuChanged(object sender, RenderingActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu oldMenu)
            {
                if (oldMenu.storeContext == "Dresser")
                {
                    priorShopMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = new WardrobeMenu();
                }
            }
        }

        // Initialize the new Wardrobe menu
        private class WardrobeMenu : IClickableMenu
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
            public List<ClickableComponent> sidebarButtons = new List<ClickableComponent>();

            // Additional Buttons
            public ClickableTextureComponent okButton;
            public ClickableTextureComponent wardrobeButton;
            public ClickableTextureComponent favoritesButton;
            public ClickableTextureComponent dresserButton;

            // Menu, Inventory Lists, List Indexes
            public ShopMenu dresserMenu;
            public StorageFurniture dresserObject;
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

            // Character Customization Variables

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

            // Main Wardrobe Menu Functionality
            public WardrobeMenu() : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
            {   
                /// WARDROBE DATA
                // Add the current farmer equipment, if any, to the wardrobe stock lists
                if (Game1.player.hat.Value != null) {
                    hatStock.Add(Game1.player.hat.Value);
                    hatIndex = 0;
                }
                if (Game1.player.shirtItem.Value != null) {
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
                dresserMenu = priorShopMenu as ShopMenu;
                dresserObject = dresserMenu.source as StorageFurniture;
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
                _portraitBox = new Rectangle((int)topLeft.X, base.yPositionOnScreen + 64, 128, 192);
                _displayFarmer = Game1.player;
                _displayFarmer.faceDirection(2);
                _displayFarmer.FarmerSprite.StopAnimation();

                // Player display window movement buttons
                int yOffset = 160;
                leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 32, _portraitBox.Y + yOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 522,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.Right - 32, _portraitBox.Y + yOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 523,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                
                // Hat cycle buttons
                yOffset += 40;
                leftSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                { 
                    myID = 514,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 28, 1, 1), "Hat" ));
                rightSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 515,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                hatLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 69, 1, 1), _displayFarmer.hat.Value == null ? "None" : _displayFarmer.hat.Value.DisplayName);
                itemLabels.Add(hatLabel);
                
                // Shirt cycle buttons
                yOffset += 84;
                leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 516,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 28, 1, 1), "Shirt" ));
                rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 517,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                shirtLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 69, 1, 1), _displayFarmer.shirtItem.Value == null ? "None" : _displayFarmer.shirtItem.Value.DisplayName);
                itemLabels.Add(shirtLabel);
                
                // Pants cycle buttons
                yOffset += 84;
                leftSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 518,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 28, 1, 1), "Pants"));
                rightSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 519,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                pantsLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 69, 1, 1), _displayFarmer.pantsItem.Value == null ? "None" : _displayFarmer.pantsItem.Value.DisplayName);
                itemLabels.Add(pantsLabel);
                
                // Shoes Buttons
                yOffset += 84;
                leftSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 520,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 28, 1, 1), "Shoes" ));
                rightSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 521,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                shoesLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 69, 1, 1), _displayFarmer.boots.Value == null ? "None" : _displayFarmer.boots.Value.DisplayName);
                itemLabels.Add(shoesLabel);

                // Right Sidebar Buttons
                sidebarButtons.Add(wardrobeButton = new ClickableTextureComponent("Wardrobe", new Rectangle(base.xPositionOnScreen + base.width + IClickableMenu.spaceToClearSideBorder - 16, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder - 64, 64, 64), null, null, assetManager.wardrobeTabTexture, new Rectangle(0, 0, 16, 16), 4f)
                {
                    myID = 611,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                sidebarButtons.Add(favoritesButton = new ClickableTextureComponent("Favorites", new Rectangle(wardrobeButton.bounds.X, wardrobeButton.bounds.Y + 64, 64, 64), null, null, assetManager.favoritesTabTexture, new Rectangle(0, 0, 16, 16), 4f)
                {
                    myID = 612,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                sidebarButtons.Add(dresserButton = new ClickableTextureComponent("Dresser", new Rectangle(wardrobeButton.bounds.X, wardrobeButton.bounds.Y + 128, 64, 64), null, null, assetManager.dresserTabTexture, new Rectangle(0, 0, 16, 16), 4f)
                {
                    myID = 613,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });

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
                    }
                }
                if (okButton.containsPoint(x, y))
                {
                    okButton.scale -= 0.25f;
                    okButton.scale = Math.Max(0.75f, okButton.scale);
                    exitThisMenu();
                    Game1.playSound("coin");
                }
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
                foreach (ClickableTextureComponent c5 in sidebarButtons)
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
                if (okButton.containsPoint(x, y))
                {
                    okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
                }
                else
                {
                    okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
                }
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
                b.Draw(Game1.daybg, new Vector2(_portraitBox.X, _portraitBox.Y), Color.White);
                FarmerRenderer.isDrawingForUI = true;
                _displayFarmer.FarmerRenderer.draw(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(_portraitBox.Center.X - 32, _portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, _displayFarmer);
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
                foreach (ClickableTextureComponent sidebarButton in sidebarButtons)
                {
                    sidebarButton.draw(b);
                }

                // Draw the buttons
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
        }
    }
}
