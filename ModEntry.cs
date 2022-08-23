using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewOutfitManager
{
    public class ModEntry : Mod
    {
        // Mod Entry
        public override void Entry(IModHelper helper)
        {
            /// Menu change event
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        // Look for the dresser display menu when a menu changes and insert the new Wardrobe menu instead
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.OldMenu is ShopMenu oldMenu)
            {
                if (oldMenu.storeContext == "Dresser")
                {
                    oldMenu.exitThisMenu();
                }
            }

            if (e.NewMenu is ShopMenu newMenu)
            {
                // Only modify the store menu when it's for a dresser object
                if (newMenu.storeContext == "Dresser")
                {
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

            internal const string FIRST_OPTION_BUTTON = "FirstOption";
            internal const string SECOND_OPTION_BUTTON = "SecondOption";
            internal const string THIRD_OPTION_BUTTON = "ThirdOption";
            internal const string SLEEVES_OPTION_BUTTON = "SleevesOption";
            internal const string SHOES_OPTION_BUTTON = "ShoesOption";

            private ClickableComponent descriptionLabel;
            private ClickableComponent hatLabel;
            private ClickableComponent shirtLabel;
            private ClickableComponent pantsLabel;
            private ClickableComponent shoesLabel;

            public List<ClickableComponent> labels = new List<ClickableComponent>();
            public List<ClickableComponent> itemLabels = new List<ClickableComponent>();
            public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
            public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();
            public List<ClickableComponent> optionButtons = new List<ClickableComponent>();
            public List<ClickableComponent> featureButtons = new List<ClickableComponent>();

            public ClickableTextureComponent okButton;
            public ShopMenu Dresser;
            public StorageFurniture menuSource;
            public List<ISalable> hatStock = new List<ISalable>();
            public List<Clothing> shirtStock = new List<Clothing>();
            public List<Clothing> pantsStock = new List<Clothing>();
            public List<ISalable> shoesStock = new List<ISalable>();
            public List<ISalable> ringsStock = new List<ISalable>();
            private int hatIndex = -1;
            private int shirtIndex = -1;
            private int pantsIndex = -1;
            private int shoesIndex = -1;
            private int priorItemIndex = -1;

            // Index Manager (Cycles through available clothes, -1 is nothing equipped)
            private static int IndexChange(int index, int listSize, int change)
            {
                if (listSize > 0)
                {
                    index += change;
                    if (index >= listSize) return -1;
                    else if (index < -1) return listSize - 1;
                    else return index;
                }
                else
                {
                    return -1;
                }
            }

            // Inventory Manager (Keeps dresser items and equipped items in proper sync when changing)
            private void InventorySwap(ISalable removeThing, ISalable addThing, ShopMenu Dresser, string itemCategory)
            {
                if (addThing != null)
                {
                    if (addThing is Item)
                    {
                        menuSource.heldItems.Add(addThing as Item);
                        if (Dresser != null && Dresser is ShopMenu)
                        {
                            Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
                            List<Item> list = menuSource.heldItems.ToList();
                            list.Sort(menuSource.SortItems);
                            foreach (Item item in list)
                            {
                                contents[item] = new int[2] { 0, 1 };
                            }
                            (Dresser as ShopMenu).setItemPriceAndStock(contents);
                        }
                    }
                }
                // If an item is being taken from the dresser, we need to equip it on the player
                if (removeThing != null)
                {
                    // Remove from dresser storage
                    menuSource.heldItems.Remove(removeThing as Item);
                    Dresser.forSale.Remove(removeThing);
                    Dresser.itemPriceAndStock.Remove(removeThing);
                    // Equip on player
                    Item equipThing = removeThing as Item;
                    if (equipThing.Category == -95)
                    {
                        _displayFarmer.hat.Set(hatStock[hatIndex] as StardewValley.Objects.Hat);
                        hatLabel.name = _displayFarmer.hat.Value.DisplayName;
                    }
                    else if (equipThing is Clothing && (equipThing as Clothing).clothesType.Value == 0)
                    {
                        _displayFarmer.shirtItem.Set(shirtStock[shirtIndex] as StardewValley.Objects.Clothing);
                        shirtLabel.name = _displayFarmer.shirtItem.Value.DisplayName;
                    }
                    else if (equipThing is Clothing && (equipThing as Clothing).clothesType.Value == 1)
                    {
                        _displayFarmer.pantsItem.Set(pantsStock[pantsIndex] as StardewValley.Objects.Clothing);
                        pantsLabel.name = _displayFarmer.pantsItem.Value.DisplayName;
                    }
                    else if (equipThing.Category == -97)
                    {
                        _displayFarmer.boots.Set(shoesStock[shoesIndex] as StardewValley.Objects.Boots);
                        shoesLabel.name = _displayFarmer.boots.Value.DisplayName;
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
                    }
                }
                _displayFarmer.UpdateClothing();
                Game1.playSound("pickUpItem");
            }

            // Main Wardrobe Menu Functionality
            public WardrobeMenu() : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
            {   
                /// WARDROBE DATA
                // Add the current farmer equipment to the wardrobe data
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
                // Get the item data from the base game dresser menu
                Dresser = (ShopMenu) Game1.activeClickableMenu;
                menuSource = Dresser.source as StorageFurniture;
                foreach (ISalable key in Dresser.itemPriceAndStock.Keys)
                {
                    if (!(key is Item item3))
                    {
                        continue;
                    }
                    if (item3.Category == -95)
                    {
                        hatStock.Add(item3);
                    }
                    else if (item3 is Clothing && (item3 as Clothing).clothesType.Value == 0)
                    {
                        shirtStock.Add(item3 as Clothing);
                    }
                    else if (item3 is Clothing && (item3 as Clothing).clothesType.Value == 1)
                    {
                        pantsStock.Add(item3 as Clothing);
                    }
                    else if (item3.Category == -97)
                    {
                        shoesStock.Add(item3);
                    }
                    else if (item3.Category == -96)
                    {
                        ringsStock.Add(item3);
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

                // Add appearance-related buttons
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

                yOffset += 40;
                leftSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 514,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 32, 1, 1), "Hat" ));
                rightSelectionButtons.Add(new ClickableTextureComponent("Hat", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 515,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                }); ;
                hatLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 64, 1, 1), _displayFarmer.hat.Value == null ? "None" : _displayFarmer.hat.Value.DisplayName);
                itemLabels.Add(hatLabel);
                
                yOffset += 84;
                leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 516,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 32, 1, 1), "Shirt" ));
                rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 517,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                }); ;
                shirtLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 64, 1, 1), _displayFarmer.shirtItem.Value == null ? "None" : _displayFarmer.shirtItem.Value.DisplayName);
                itemLabels.Add(shirtLabel);

                yOffset += 84;
                leftSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 518,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 32, 1, 1), "Pants"));
                rightSelectionButtons.Add(new ClickableTextureComponent("Pants", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 519,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                }); ;
                pantsLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 64, 1, 1), _displayFarmer.pantsItem.Value == null ? "None" : _displayFarmer.pantsItem.Value.DisplayName);
                itemLabels.Add(pantsLabel);

                yOffset += 84;
                leftSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
                {
                    myID = 520,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 32, 1, 1), "Shoes" ));
                rightSelectionButtons.Add(new ClickableTextureComponent("Shoes", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
                {
                    myID = 521,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                }); ;
                shoesLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 64, 1, 1), _displayFarmer.boots.Value == null ? "None" : _displayFarmer.boots.Value.DisplayName);
                itemLabels.Add(shoesLabel);

                // Add the option buttons
                optionButtons.Add(new ClickableTextureComponent(FIRST_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 130, _portraitBox.Y + yOffset, 32, 32), null, "enabled", null, new Rectangle(0, 0, 15, 15), 2f)
                {
                    myID = 611,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                optionButtons.Add(new ClickableTextureComponent(SECOND_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 82, _portraitBox.Y + yOffset, 32, 32), null, "disabled", null, new Rectangle(0, 0, 15, 15), 2f)
                {
                    myID = 612,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });
                optionButtons.Add(new ClickableTextureComponent(THIRD_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 34, _portraitBox.Y + yOffset, 32, 32), null, "disabled", null, new Rectangle(0, 0, 15, 15), 2f)
                {
                    myID = 613,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                });

                // Add the leftover buttons
                okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
                {
                    myID = 505,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };
            }
            private void selectionClick(string name, int change)
            {
                switch (name)
                {
                    case "Hat":
                        {
                            priorItemIndex = hatIndex;
                            hatIndex = (IndexChange(hatIndex, hatStock.Count, change));
                            if (hatIndex == -1)
                            {
                                _displayFarmer.hat.Set(null);
                                hatLabel.name = "None";
                                // If we move from a worn item to nothing, we have to put it back in the dresser 
                                if (priorItemIndex != hatIndex)
                                {
                                    InventorySwap(null, hatStock[priorItemIndex], Dresser);
                                }
                            }
                            else
                            {
                                _displayFarmer.hat.Set(hatStock[hatIndex] as StardewValley.Objects.Hat);
                                hatLabel.name = _displayFarmer.hat.Value.DisplayName;
                                if (priorItemIndex == -1)
                                {
                                    InventorySwap(hatStock[hatIndex], null, Dresser);
                                }
                                else
                                {
                                    InventorySwap(hatStock[hatIndex], hatStock[priorItemIndex], Dresser);
                                }
                            }
                            _displayFarmer.UpdateClothing();
                            Game1.playSound("pickUpItem");
                            break;
                        }                    
                    case "Shirt":
                        {
                            shirtIndex = (IndexChange(shirtIndex, shirtStock.Count, change));
                            if (shirtIndex == -1)
                            {
                                _displayFarmer.shirtItem.Set(null);
                                shirtLabel.name = "None";
                            }
                            else
                            {
                                _displayFarmer.shirtItem.Set(shirtStock[shirtIndex] as StardewValley.Objects.Clothing);
                                shirtLabel.name = _displayFarmer.shirtItem.Value.DisplayName;
                            }
                            _displayFarmer.UpdateClothing();
                            Game1.playSound("pickUpItem");
                            break;
                        }
                    case "Pants":
                        {
                            pantsIndex = (IndexChange(pantsIndex, pantsStock.Count, change));
                            if (pantsIndex == -1)
                            {
                                _displayFarmer.pantsItem.Set(null);
                                pantsLabel.name = "None";
                            }
                            else
                            {
                                _displayFarmer.pantsItem.Set(pantsStock[pantsIndex] as StardewValley.Objects.Clothing);
                                pantsLabel.name = _displayFarmer.pantsItem.Value.DisplayName;
                            }
                            _displayFarmer.UpdateClothing();
                            Game1.playSound("pickUpItem");
                            break;
                        }
                    case "Shoes":
                        {
                            shoesIndex = (IndexChange(shoesIndex, shoesStock.Count, change));
                            if (shoesIndex == -1)
                            {
                                _displayFarmer.boots.Set(null);
                                shoesLabel.name = "None";
                            }
                            else
                            {
                                _displayFarmer.boots.Set(shoesStock[shoesIndex] as StardewValley.Objects.Boots);
                                shoesLabel.name = _displayFarmer.boots.Value.DisplayName;
                            }
                            _displayFarmer.UpdateClothing();
                            Game1.playSound("pickUpItem");
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
            }

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

                // Draw the top side bar
                var sideBarPosition = new Vector2(base.xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder + 12, base.yPositionOnScreen + 24);
                b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 361, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

                // Draw the bottom side bar
                sideBarPosition.Y += 8 * 4;
                b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 377, 13, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

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
