using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;
using StardewValley.Locations;
using StardewOutfitManager.Managers;
using StardewOutfitManager.Utils;
using StardewModdingAPI;

namespace StardewOutfitManager.Menus
{
    // Initialize a custom original dresser menu (basically had to clone the ShopMenu implementation because there are too many privates to extend correctly)
    /* This is an extremely hacky port of the game's internal ShopMenu I had to do for silly technical reasons related to how ShopMenu can't be extended without
     * losing functionality. It's ugly, don't mess with it.
     * 
     * Major changes are basically the rename to a new class, the insertion of my top tap switcher code, removing the close button, some refactoring out 
     * contexts that don't apply when it's always going to be the dresser, and adding in the variables StorageFurniture usually passes to the constructor for the same reason.
     * 
     * Otherwise this works just like the base dresser from decompiled SDV v1.5.6.
     */
    internal class NewDresserMenu : IClickableMenu
    {
        public const int region_shopButtonModifier = 3546;

        public const int region_upArrow = 97865;

        public const int region_downArrow = 97866;

        public const int region_tabStartIndex = 99999;

        public const int itemsPerPage = 4;

        private string hoverText = "";

        private string boldTitleText = "";

        public string purchaseSound = "purchaseClick";

        public string purchaseRepeatSound = "purchaseRepeat";

        public string storeContext = "";

        public InventoryMenu inventory;

        public ISalable heldItem;

        public ISalable hoveredItem;

        private TemporaryAnimatedSprite poof;

        private Rectangle scrollBarRunner;

        public List<ISalable> forSale = new List<ISalable>();

        public List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

        public List<int> categoriesToSellHere = new List<int>();

        public Dictionary<ISalable, int[]> itemPriceAndStock = new Dictionary<ISalable, int[]>();

        private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

        public int hoverPrice = -1;

        public int currency;

        public int currentItemIndex;

        public ClickableTextureComponent upArrow;

        public ClickableTextureComponent downArrow;

        public ClickableTextureComponent scrollBar;

        public StorageFurniture source;

        private bool scrolling;

        public Func<ISalable, Farmer, int, bool> onPurchase;

        public Func<ISalable, bool> onSell;

        public List<ClickableTextureComponent> tabButtons = new List<ClickableTextureComponent>();

        protected int currentTab;

        protected bool _isStorageShop;

        // My vars
        // Reference Top Tab Menu Manager
        internal MenuManager menuManager = StardewOutfitManager.playerManager.menuManager.Value;

        // Dumping these custom functions here because I don't wanna look at them and this is the object that uses them
        // Custom dresser functions needed for new dresser
        public virtual bool onDresserItemDeposited(ISalable deposited_salable)
        {
            if (deposited_salable is Item)
            {
                // Always deposit to primary dresser
                menuManager.primaryDresser.heldItems.Add(deposited_salable as Item);
                if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is NewDresserMenu)
                {
                    Dictionary<ISalable, int[]> contents = new Dictionary<ISalable, int[]>();
                    // Use combined inventory from all linked dressers
                    List<Item> list = menuManager.GetCombinedDresserItems();
                    list.Sort(menuManager.primaryDresser.SortItems);
                    foreach (Item item in list)
                    {
                        contents[item] = new int[2] { 0, 1 };
                    }
                    (Game1.activeClickableMenu as NewDresserMenu).setItemPriceAndStock(contents);
                    Game1.playSound("dwop");
                    return true;
                }
            }
            return false;
        }

        public virtual bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
        {
            if (salable is Item item)
            {
                // Find which dresser has the item and remove it from there
                DresserLinkingMethods.RemoveItemFromLinkedDressers(menuManager.linkedDressers, item);
                Game1.playSound("stoneStep");
            }
            return false;
        }


        // Begin jank OG code mess
        public NewDresserMenu(Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null)
            : this(itemPriceAndStock.Keys.ToList(), currency, who)
        {
            this.itemPriceAndStock = itemPriceAndStock;
        }

        public NewDresserMenu(List<ISalable> itemsForSale, int currency = 0, string who = null)
            : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            foreach (ISalable j in itemsForSale)
            {
                if (j is StardewValley.Object && (j as StardewValley.Object).IsRecipe)
                {
                    if (Game1.player.knowsRecipe(j.Name))
                    {
                        continue;
                    }
                    j.Stack = 1;
                }
                this.forSale.Add(j);
                this.itemPriceAndStock.Add(j, new int[2]
                {
                j.salePrice(),
                j.Stack
                });
            }
            this.updatePosition();
            this.currency = currency;
            // Inserting these as given in the constructor so I don't have to set them in MenuTabManager
            this.onPurchase = onDresserItemWithdrawn;
            this.onSell = onDresserItemDeposited;
            this.storeContext = "Dresser";
            //
            Game1.player.forceCanMove();
            this.inventory = new InventoryMenu(base.xPositionOnScreen + base.width, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
            {
                showGrayedOutSlots = true
            };
            this.inventory.movePosition(-this.inventory.width - 32, 0);
            this.currency = currency;
            this.upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
            {
                myID = 97865,
                downNeighborID = 106,
                leftNeighborID = 3546
            };
            this.downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
            {
                myID = 106,
                upNeighborID = 97865,
                leftNeighborID = 3546
            };
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);
            for (int i = 0; i < 4; i++)
            {
                this.forSaleButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 256) / 4), base.width - 32, (base.height - 256) / 4 + 4), i.ToString() ?? "")
                {
                    myID = i + 3546,
                    rightNeighborID = 97865,
                    fullyImmutable = true
                });
            }
            this.updateSaleButtonNeighbors();
            this.setUpStoreForContext();
            if (this.tabButtons.Count > 0)
            {
                foreach (ClickableComponent forSaleButton in this.forSaleButtons)
                {
                    forSaleButton.leftNeighborID = -99998;
                }
            }
            this.applyTab();
            foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
            {
                item.upNeighborID = -99998;
            }
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        public void updateSaleButtonNeighbors()
        {
            ClickableComponent last_valid_button = this.forSaleButtons[0];
            for (int i = 0; i < this.forSaleButtons.Count; i++)
            {
                ClickableComponent button = this.forSaleButtons[i];
                button.upNeighborImmutable = true;
                button.downNeighborImmutable = true;
                button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : (-7777));
                button.downNeighborID = ((i < 3 && i < this.forSale.Count - 1) ? (i + 3546 + 1) : (-7777));
                if (i >= this.forSale.Count)
                {
                    if (button == base.currentlySnappedComponent)
                    {
                        base.currentlySnappedComponent = last_valid_button;
                        if (Game1.options.SnappyMenus)
                        {
                            this.snapCursorToCurrentSnappedComponent();
                        }
                    }
                }
                else
                {
                    last_valid_button = button;
                }
            }
        }

        // Modified to always assume Dresser context
        public virtual void setUpStoreForContext()
        {
            this.tabButtons = new List<ClickableTextureComponent>();
            this.categoriesToSellHere.AddRange(new int[4] { -95, -100, -97, -96 });
            this._isStorageShop = true;
            ClickableTextureComponent tab3 = null;
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f)
            {
                myID = 99999 + this.tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            this.tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f)
            {
                myID = 99999 + this.tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            this.tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f)
            {
                myID = 99999 + this.tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            this.tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f)
            {
                myID = 99999 + this.tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            this.tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 64, 16, 16), 4f)
            {
                myID = 99999 + this.tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            this.tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 64, 16, 16), 4f)
            {
                myID = 99999 + this.tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            this.tabButtons.Add(tab3);
            menuManager.includeTopTabButtons(this);
            this.repositionTabs();
            this.purchaseSound = null;
            this.purchaseRepeatSound = null;
        }

        public void repositionTabs()
        {
            for (int i = 0; i < this.tabButtons.Count; i++)
            {
                if (i == this.currentTab)
                {
                    this.tabButtons[i].bounds.X = base.xPositionOnScreen - 56;
                }
                else
                {
                    this.tabButtons[i].bounds.X = base.xPositionOnScreen - 64;
                }
                this.tabButtons[i].bounds.Y = base.yPositionOnScreen + i * 16 * 4 + 16;
            }
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            switch (direction)
            {
                case 2:
                    {
                        if (this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
                        {
                            this.downArrowPressed();
                            break;
                        }
                        int emptySlot = -1;
                        for (int i = 0; i < 12; i++)
                        {
                            this.inventory.inventory[i].upNeighborID = oldID;
                            if (emptySlot == -1 && this.heldItem != null && this.inventory.actualInventory != null && this.inventory.actualInventory.Count > i && this.inventory.actualInventory[i] == null)
                            {
                                emptySlot = i;
                            }
                        }
                        base.currentlySnappedComponent = base.getComponentWithID((emptySlot != -1) ? emptySlot : 0);
                        this.snapCursorToCurrentSnappedComponent();
                        break;
                    }
                case 0:
                    if (this.currentItemIndex > 0)
                    {
                        this.upArrowPressed();
                        base.currentlySnappedComponent = base.getComponentWithID(3546);
                        this.snapCursorToCurrentSnappedComponent();
                    }
                    break;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(3546);
            this.snapCursorToCurrentSnappedComponent();
        }

        public bool highlightItemToSell(Item i)
        {
            if (this.heldItem != null)
            {
                return this.heldItem.canStackWith(i);
            }
            if (this.categoriesToSellHere.Contains(i.Category))
            {
                return true;
            }
            return false;
        }

        public static int getPlayerCurrencyAmount(Farmer who, int currencyType)
        {
            return currencyType switch
            {
                0 => who.Money,
                1 => who.festivalScore,
                2 => who.clubCoins,
                4 => who.QiGems,
                _ => 0,
            };
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (this.scrolling)
            {
                int y2 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upArrow.bounds.Height + 20));
                float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
                this.currentItemIndex = Math.Min(Math.Max(0, this.forSale.Count - 4), Math.Max(0, (int)((float)this.forSale.Count * percentage)));
                this.setScrollBarToCurrentIndex();
                this.updateSaleButtonNeighbors();
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

        private void downArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            this.currentItemIndex++;
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
        }

        private void upArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            this.currentItemIndex--;
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.heldItem != null && this.heldItem is Item)
            {
                Item item = this.heldItem as Item;
                this.heldItem = null;
                if (Utility.CollectOrDrop(item))
                {
                    Game1.playSound("stoneStep");
                }
                else
                {
                    Game1.playSound("throwDownITem");
                }
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.activeClickableMenu == null)
            {
                return;
            }
            Vector2 snappedPosition = this.inventory.snapToClickableComponent(x, y);
            menuManager.handleTopBarLeftClick(x, y);
            if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
            {
                this.downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
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
            for (int k = 0; k < this.tabButtons.Count; k++)
            {
                if (this.tabButtons[k].containsPoint(x, y))
                {
                    this.switchTab(k);
                }
            }
            this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
            if (this.heldItem == null)
            {
                Item toSell = this.inventory.leftClick(x, y, null, playSound: false);
                if (toSell != null)
                {
                    // Dresser uses onSell callback to deposit items
                    this.onSell(toSell);
                    this.updateSaleButtonNeighbors();
                }
            }
            else
            {
                this.heldItem = this.inventory.leftClick(x, y, this.heldItem as Item);
            }
            for (int i = 0; i < this.forSaleButtons.Count; i++)
            {
                if (this.currentItemIndex + i >= this.forSale.Count || !this.forSaleButtons[i].containsPoint(x, y))
                {
                    continue;
                }
                int index = this.currentItemIndex + i;
                if (this.forSale[index] != null)
                {
                    int toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / Math.Max(1, this.itemPriceAndStock[this.forSale[index]][0])), Math.Max(1, this.itemPriceAndStock[this.forSale[index]][1])));
                    toBuy = Math.Min(toBuy, this.forSale[index].maximumStackSize());
                    if (toBuy == -1)
                    {
                        toBuy = 1;
                    }
                    if (toBuy > 0 && this.tryToPurchaseItem(this.forSale[index], this.heldItem, toBuy, x, y, index))
                    {
                        this.itemPriceAndStock.Remove(this.forSale[index]);
                        this.forSale.RemoveAt(index);
                    }
                    else if (toBuy <= 0)
                    {
                        if (this.itemPriceAndStock[this.forSale[index]].Length != 0 && this.itemPriceAndStock[this.forSale[index]][0] > 0)
                        {
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        }
                        Game1.playSound("cancel");
                    }
                    if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus || (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.heldItem.maximumStackSize() == 1)) && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item))
                    {
                        this.heldItem = null;
                        DelayedAction.playSoundAfterDelay("coin", 100);
                    }
                }
                this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
                this.updateSaleButtonNeighbors();
                this.setScrollBarToCurrentIndex();
                return;
            }
        }

        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            if (direction == 1 && this.tabButtons.Contains(a) && this.tabButtons.Contains(b))
            {
                return false;
            }
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        public virtual void switchTab(int new_tab)
        {
            this.currentTab = new_tab;
            Game1.playSound("shwip");
            this.applyTab();
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                this.snapCursorToCurrentSnappedComponent();
            }
        }

        public virtual void applyTab()
        {
            // Dresser tab filtering by clothing type
            if (this.currentTab == 0)
            {
                this.forSale = this.itemPriceAndStock.Keys.ToList();
            }
            else
            {
                this.forSale.Clear();
                foreach (ISalable key in this.itemPriceAndStock.Keys)
                {
                    if (!(key is Item item))
                    {
                        continue;
                    }
                    if (this.currentTab == 1)
                    {
                        if (item.Category == -95) // Hats
                        {
                            this.forSale.Add(item);
                        }
                    }
                    else if (this.currentTab == 2)
                    {
                        if (item is Clothing && (item as Clothing).clothesType.Value == Clothing.ClothesType.SHIRT)
                        {
                            this.forSale.Add(item);
                        }
                    }
                    else if (this.currentTab == 3)
                    {
                        if (item is Clothing && (item as Clothing).clothesType.Value == Clothing.ClothesType.PANTS)
                        {
                            this.forSale.Add(item);
                        }
                    }
                    else if (this.currentTab == 4)
                    {
                        if (item.Category == -97) // Boots
                        {
                            this.forSale.Add(item);
                        }
                    }
                    else if (this.currentTab == 5 && item.Category == -96) // Rings
                    {
                        this.forSale.Add(item);
                    }
                }
            }
            this.currentItemIndex = 0;
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
        }

        public override bool readyToClose()
        {
            if (this.heldItem == null)
            {
                return this.animations.Count == 0;
            }
            return false;
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            if (this.heldItem != null)
            {
                Game1.player.addItemToInventoryBool(this.heldItem as Item);
                Game1.playSound("coin");
            }
        }

        public static void chargePlayer(Farmer who, int currencyType, int amount)
        {
            switch (currencyType)
            {
                case 0:
                    who.Money -= amount;
                    break;
                case 1:
                    who.festivalScore -= amount;
                    break;
                case 2:
                    who.clubCoins -= amount;
                    break;
                case 4:
                    who.QiGems -= amount;
                    break;
                case 3:
                    break;
            }
        }

        private bool tryToPurchaseItem(ISalable item, ISalable held_item, int numberToBuy, int x, int y, int indexInForSaleList)
        {
            if (held_item == null)
            {
                if (this.itemPriceAndStock[item][1] == 0)
                {
                    this.hoveredItem = null;
                    return true;
                }
                if (item.GetSalableInstance().maximumStackSize() < numberToBuy)
                {
                    numberToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
                }
                int price2 = this.itemPriceAndStock[item][0] * numberToBuy;
                int extraTradeItem2 = -1;
                int extraTradeItemCount2 = 5;
                if (this.itemPriceAndStock[item].Length > 2)
                {
                    extraTradeItem2 = this.itemPriceAndStock[item][2];
                    if (this.itemPriceAndStock[item].Length > 3)
                    {
                        extraTradeItemCount2 = this.itemPriceAndStock[item][3];
                    }
                    extraTradeItemCount2 *= numberToBuy;
                }
                if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= price2 && (extraTradeItem2 == -1 || Game1.player.Items.ContainsId(extraTradeItem2.ToString(), extraTradeItemCount2)))
                {
                    this.heldItem = item.GetSalableInstance();
                    this.heldItem.Stack = numberToBuy;
                    if (this.itemPriceAndStock[item][1] == int.MaxValue && item.Stack != int.MaxValue)
                    {
                        this.heldItem.Stack *= item.Stack;
                    }
                    if (!this.heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && (!(this.heldItem is StardewValley.Object) || !(this.heldItem as StardewValley.Object).IsRecipe))
                    {
                        Game1.playSound("smallSelect");
                        this.heldItem = null;
                        return false;
                    }
                    if (this.itemPriceAndStock[item][1] != int.MaxValue && !item.IsInfiniteStock())
                    {
                        this.itemPriceAndStock[item][1] -= numberToBuy;
                        this.forSale[indexInForSaleList].Stack -= numberToBuy;
                    }
                    ShopMenu.chargePlayer(Game1.player, this.currency, price2);
                    if (extraTradeItem2 != -1)
                    {
                        Game1.player.Items.ReduceId(extraTradeItem2.ToString(), extraTradeItemCount2);
                    }
                    // Dresser is storage shop - play purchase sounds
                    if (Game1.mouseClickPolling > 300)
                    {
                        if (this.purchaseRepeatSound != null)
                        {
                            Game1.playSound(this.purchaseRepeatSound);
                        }
                    }
                    else if (this.purchaseSound != null)
                    {
                        Game1.playSound(this.purchaseSound);
                    }
                    if (this.onPurchase != null && this.onPurchase(item, Game1.player, numberToBuy))
                    {
                        base.exitThisMenu();
                    }
                }
                else
                {
                    if (price2 > 0)
                    {
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                    }
                    Game1.playSound("cancel");
                }
            }
            else if (held_item.canStackWith(item))
            {
                numberToBuy = Math.Min(numberToBuy, held_item.maximumStackSize() - held_item.Stack);
                if (numberToBuy > 0)
                {
                    int price = this.itemPriceAndStock[item][0] * numberToBuy;
                    int extraTradeItem = -1;
                    int extraTradeItemCount = 5;
                    if (this.itemPriceAndStock[item].Length > 2)
                    {
                        extraTradeItem = this.itemPriceAndStock[item][2];
                        if (this.itemPriceAndStock[item].Length > 3)
                        {
                            extraTradeItemCount = this.itemPriceAndStock[item][3];
                        }
                        extraTradeItemCount *= numberToBuy;
                    }
                    int tmp = item.Stack;
                    item.Stack = numberToBuy + this.heldItem.Stack;
                    if (!item.CanBuyItem(Game1.player))
                    {
                        item.Stack = tmp;
                        Game1.playSound("cancel");
                        return false;
                    }
                    item.Stack = tmp;
                    if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= price && (extraTradeItem == -1 || Game1.player.Items.ContainsId(extraTradeItem.ToString(), extraTradeItemCount)))
                    {
                        int amountAddedToStack = numberToBuy;
                        if (this.itemPriceAndStock[item][1] == int.MaxValue && item.Stack != int.MaxValue)
                        {
                            amountAddedToStack *= item.Stack;
                        }
                        this.heldItem.Stack += amountAddedToStack;
                        if (this.itemPriceAndStock[item][1] != int.MaxValue && !item.IsInfiniteStock())
                        {
                            this.itemPriceAndStock[item][1] -= numberToBuy;
                            this.forSale[indexInForSaleList].Stack -= numberToBuy;
                        }
                        ShopMenu.chargePlayer(Game1.player, this.currency, price);
                        if (Game1.mouseClickPolling > 300)
                        {
                            if (this.purchaseRepeatSound != null)
                            {
                                Game1.playSound(this.purchaseRepeatSound);
                            }
                        }
                        else if (this.purchaseSound != null)
                        {
                            Game1.playSound(this.purchaseSound);
                        }
                        if (extraTradeItem != -1)
                        {
                            Game1.player.Items.ReduceId(extraTradeItem.ToString(), extraTradeItemCount);
                        }
                        if (this.onPurchase != null && this.onPurchase(item, Game1.player, numberToBuy))
                        {
                            base.exitThisMenu();
                        }
                    }
                    else
                    {
                        if (price > 0)
                        {
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        }
                        Game1.playSound("cancel");
                    }
                }
            }
            if (this.itemPriceAndStock[item][1] <= 0)
            {
                this.hoveredItem = null;
                return true;
            }
            return false;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            Vector2 snappedPosition = this.inventory.snapToClickableComponent(x, y);
            if (this.heldItem == null)
            {
                ISalable toSell = this.inventory.rightClick(x, y, null, playSound: false);
                if (toSell != null)
                {
                    // Dresser uses onSell callback to deposit items
                    this.onSell(toSell);
                }
            }
            else
            {
                this.heldItem = this.inventory.rightClick(x, y, this.heldItem as Item);
            }
            for (int i = 0; i < this.forSaleButtons.Count; i++)
            {
                if (this.currentItemIndex + i >= this.forSale.Count || !this.forSaleButtons[i].containsPoint(x, y))
                {
                    continue;
                }
                int index = this.currentItemIndex + i;
                if (this.forSale[index] == null)
                {
                    break;
                }
                int toBuy = 1;
                if (this.itemPriceAndStock[this.forSale[index]][0] > 0)
                {
                    toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / this.itemPriceAndStock[this.forSale[index]][0]), this.itemPriceAndStock[this.forSale[index]][1]));
                }
                if (toBuy > 0 && this.tryToPurchaseItem(this.forSale[index], this.heldItem, toBuy, x, y, index))
                {
                    this.itemPriceAndStock.Remove(this.forSale[index]);
                    this.forSale.RemoveAt(index);
                }
                if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus) && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item))
                {
                    this.heldItem = null;
                    DelayedAction.playSoundAfterDelay("coin", 100);
                }
                this.setScrollBarToCurrentIndex();
                break;
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverText = "";
            this.hoveredItem = null;
            this.hoverPrice = -1;
            this.boldTitleText = "";
            this.upArrow.tryHover(x, y);
            this.downArrow.tryHover(x, y);
            this.scrollBar.tryHover(x, y);
            if (this.scrolling)
            {
                return;
            }
            for (int i = 0; i < this.forSaleButtons.Count; i++)
            {
                if (this.currentItemIndex + i < this.forSale.Count && this.forSaleButtons[i].containsPoint(x, y))
                {
                    ISalable item = this.forSale[this.currentItemIndex + i];
                    this.hoverText = item.getDescription();
                    this.boldTitleText = item.DisplayName;
                    if (!this._isStorageShop)
                    {
                        this.hoverPrice = ((this.itemPriceAndStock != null && this.itemPriceAndStock.ContainsKey(item)) ? this.itemPriceAndStock[item][0] : item.salePrice());
                    }
                    this.hoveredItem = item;
                    this.forSaleButtons[i].scale = Math.Min(this.forSaleButtons[i].scale + 0.03f, 1.1f);
                }
                else
                {
                    this.forSaleButtons[i].scale = Math.Max(1f, this.forSaleButtons[i].scale - 0.03f);
                }
            }
            if (this.heldItem != null)
            {
                return;
            }
            foreach (ClickableComponent c in this.inventory.inventory)
            {
                if (!c.containsPoint(x, y))
                {
                    continue;
                }
                Item j = this.inventory.getItemFromClickableComponent(c);
                if (j == null || (this.inventory.highlightMethod != null && !this.inventory.highlightMethod(j)))
                {
                    continue;
                }
                // Dresser is always a storage shop
                this.hoverText = j.getDescription();
                this.boldTitleText = j.DisplayName;
                this.hoveredItem = j;
            }
            menuManager.handleTopBarOnHover(x, y, ref hoverText);
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (this.poof != null && this.poof.update(time))
            {
                this.poof = null;
            }
            this.repositionTabs();
        }

        public void drawCurrency(SpriteBatch b)
        {
            if (!this._isStorageShop)
            {
                if (this.currency != 0)
                {
                    _ = 1;
                }
                else
                {
                    Game1.dayTimeMoneyBox.drawMoneyBox(b, base.xPositionOnScreen - 36, base.yPositionOnScreen + base.height - this.inventory.height - 12);
                }
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b != Buttons.RightTrigger && b != Buttons.LeftTrigger)
            {
                return;
            }
            if (base.currentlySnappedComponent != null && base.currentlySnappedComponent.myID >= 3546)
            {
                int emptySlot = -1;
                for (int i = 0; i < 12; i++)
                {
                    this.inventory.inventory[i].upNeighborID = 3546 + this.forSaleButtons.Count - 1;
                    if (emptySlot == -1 && this.heldItem != null && this.inventory.actualInventory != null && this.inventory.actualInventory.Count > i && this.inventory.actualInventory[i] == null)
                    {
                        emptySlot = i;
                    }
                }
                base.currentlySnappedComponent = base.getComponentWithID((emptySlot != -1) ? emptySlot : 0);
                this.snapCursorToCurrentSnappedComponent();
            }
            else
            {
                this.snapToDefaultClickableComponent();
            }
            Game1.playSound("shiny4");
        }

        private string getHoveredItemExtraItemIndex()
        {
            if (this.itemPriceAndStock != null && this.hoveredItem != null && this.itemPriceAndStock.ContainsKey(this.hoveredItem) && this.itemPriceAndStock[this.hoveredItem].Length > 2)
            {
                return this.itemPriceAndStock[this.hoveredItem][2].ToString();
            }
            return null;
        }

        private int getHoveredItemExtraItemAmount()
        {
            if (this.itemPriceAndStock != null && this.hoveredItem != null && this.itemPriceAndStock.ContainsKey(this.hoveredItem) && this.itemPriceAndStock[this.hoveredItem].Length > 3)
            {
                return this.itemPriceAndStock[this.hoveredItem][3];
            }
            return 5;
        }

        public void updatePosition()
        {
            base.width = 1000 + IClickableMenu.borderWidth * 2;
            base.height = 600 + IClickableMenu.borderWidth * 2;
            base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (1000 + IClickableMenu.borderWidth * 2) / 2;
            base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
        }

        protected override void cleanupBeforeExit()
        {
            if (this.currency == 4)
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
            }
            base.cleanupBeforeExit();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.updatePosition();
            base.initializeUpperRightCloseButton();
            Game1.player.forceCanMove();
            this.inventory = new InventoryMenu(base.xPositionOnScreen + base.width, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
            {
                showGrayedOutSlots = true
            };
            this.inventory.movePosition(-this.inventory.width - 32, 0);
            this.upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            this.downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);
            this.forSaleButtons.Clear();
            for (int i = 0; i < 4; i++)
            {
                this.forSaleButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 256) / 4), base.width - 32, (base.height - 256) / 4 + 4), i.ToString() ?? ""));
            }
            if (this.tabButtons.Count > 0)
            {
                foreach (ClickableComponent forSaleButton in this.forSaleButtons)
                {
                    forSaleButton.leftNeighborID = -99998;
                }
            }
            this.repositionTabs();
            foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
            {
                item.upNeighborID = -99998;
            }
            // Reposition top menu tabs
            menuManager.repositionTopTabButtons(this);
        }

        public void setItemPriceAndStock(Dictionary<ISalable, int[]> new_stock)
        {
            this.itemPriceAndStock = new_stock;
            this.forSale = this.itemPriceAndStock.Keys.ToList();
            this.applyTab();
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }
            // Added a title to match the others - uses the actual dresser's display name
            SpriteText.drawStringWithScrollCenteredAt(b, menuManager.dresserDisplayName, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            // Removed full-size background box - the inventory and item list have their own backgrounds
            Texture2D purchase_texture = Game1.mouseCursors;
            Rectangle purchase_window_border = new Rectangle(384, 373, 18, 18);
            Rectangle purchase_item_rect = new Rectangle(384, 396, 15, 15);
            Color? purchase_item_text_color = null;
            bool purchase_draw_item_background = true;
            Rectangle purchase_item_background = new Rectangle(296, 363, 18, 18);
            Color purchase_selected_color = Color.Wheat;
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen + base.width - this.inventory.width - 32 - 24, base.yPositionOnScreen + base.height - 256 + 40, this.inventory.width + 56, base.height - 448 + 20, Color.White, 4f);
            IClickableMenu.drawTextureBox(b, purchase_texture, purchase_window_border, base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height - 256 + 32 + 4, Color.White, 4f);
            this.drawCurrency(b);
            for (int k = 0; k < this.forSaleButtons.Count; k++)
            {
                if (this.currentItemIndex + k >= this.forSale.Count)
                {
                    continue;
                }
                IClickableMenu.drawTextureBox(b, purchase_texture, purchase_item_rect, this.forSaleButtons[k].bounds.X, this.forSaleButtons[k].bounds.Y, this.forSaleButtons[k].bounds.Width, this.forSaleButtons[k].bounds.Height, (this.forSaleButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.scrolling) ? purchase_selected_color : Color.White, 4f, drawShadow: false);
                ISalable item = this.forSale[this.currentItemIndex + k];
                bool buyInStacks = item.Stack > 1 && item.Stack != int.MaxValue && this.itemPriceAndStock[item][1] == int.MaxValue;
                StackDrawType stackDrawType;
                if (this.itemPriceAndStock[item][1] == int.MaxValue)
                {
                    stackDrawType = StackDrawType.HideButShowQuality;
                }
                else
                {
                    stackDrawType = StackDrawType.Draw_OneInclusive;
                    if (this._isStorageShop)
                    {
                        stackDrawType = StackDrawType.Draw;
                    }
                }
                string displayName = item.DisplayName;
                if (buyInStacks)
                {
                    displayName = displayName + " x" + item.Stack;
                }
                if (this.forSale[this.currentItemIndex + k].ShouldDrawIcon())
                {
                    if (purchase_draw_item_background)
                    {
                        b.Draw(purchase_texture, new Vector2(this.forSaleButtons[k].bounds.X + 32 - 12, this.forSaleButtons[k].bounds.Y + 24 - 4), purchase_item_background, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    }
                    this.forSale[this.currentItemIndex + k].drawInMenu(b, new Vector2(this.forSaleButtons[k].bounds.X + 32 - 8, this.forSaleButtons[k].bounds.Y + 24), 1f, 1f, 0.9f, stackDrawType, Color.White, drawShadow: true);
                    SpriteText.drawString(b, displayName, this.forSaleButtons[k].bounds.X + 96 + 8, this.forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
                else
                {
                    SpriteText.drawString(b, displayName, this.forSaleButtons[k].bounds.X + 32 + 8, this.forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
                if (this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]][0] > 0)
                {
                    SpriteText.drawString(b, this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]][0] + " ", this.forSaleButtons[k].bounds.Right - SpriteText.getWidthOfString(this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]][0] + " ") - 60, this.forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]][0]) ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(this.forSaleButtons[k].bounds.Right - 52, this.forSaleButtons[k].bounds.Y + 40 - 4), new Rectangle(193 + this.currency * 9, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, 0.35f);
                }
                else if (this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]].Length > 2)
                {
                    int required_item_count = 5;
                    int requiredItem = this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]][2];
                    if (this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]].Length > 3)
                    {
                        required_item_count = this.itemPriceAndStock[this.forSale[this.currentItemIndex + k]][3];
                    }
                    bool hasEnoughToTrade = Game1.player.Items.ContainsId(requiredItem.ToString(), required_item_count);
                    float textWidth = SpriteText.getWidthOfString("x" + required_item_count);
                    Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)(this.forSaleButtons[k].bounds.Right - 88) - textWidth, this.forSaleButtons[k].bounds.Y + 28 - 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, requiredItem, 16, 16), Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, -1f, flipped: false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
                    SpriteText.drawString(b, "x" + required_item_count, this.forSaleButtons[k].bounds.Right - (int)textWidth - 16, this.forSaleButtons[k].bounds.Y + 44, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
            }
            if (this.forSale.Count == 0 && !this._isStorageShop)
            {
                SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), base.xPositionOnScreen + base.width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583")) / 2, base.yPositionOnScreen + base.height / 2 - 128);
            }
            this.inventory.draw(b);
            for (int j = this.animations.Count - 1; j >= 0; j--)
            {
                if (this.animations[j].update(Game1.currentGameTime))
                {
                    this.animations.RemoveAt(j);
                }
                else
                {
                    this.animations[j].draw(b, localPosition: true);
                }
            }
            if (this.poof != null)
            {
                this.poof.draw(b);
            }
            for (int i = 0; i < this.tabButtons.Count; i++)
            {
                this.tabButtons[i].draw(b);
            }
            menuManager.drawTopBar(b);
            // Only draw scroll arrows and bar when there are more items than fit on screen
            if (this.forSale.Count > 4)
            {
                this.upArrow.draw(b);
                this.downArrow.draw(b);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f);
                this.scrollBar.draw(b);
            }
            if (!this.hoverText.Equals(""))
            {
                if (this.hoveredItem is StardewValley.Object && (this.hoveredItem as StardewValley.Object).IsRecipe)
                {
                    IClickableMenu.drawToolTip(b, " ", this.boldTitleText, this.hoveredItem as Item, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), new CraftingRecipe(this.hoveredItem.Name.Replace(" Recipe", "")), (this.hoverPrice > 0) ? this.hoverPrice : (-1));
                }
                else
                {
                    IClickableMenu.drawToolTip(b, this.hoverText, this.boldTitleText, this.hoveredItem as Item, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), null, (this.hoverPrice > 0) ? this.hoverPrice : (-1));
                }
            }
            if (this.heldItem != null)
            {
                this.heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
            }
            base.drawMouse(b);
        }
    }
}
