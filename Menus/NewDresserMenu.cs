using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewOutfitManager.Menus
{
    // Initialize a custom original dresser menu (basically had to clone the ShopMenu implementation because there are too many privates to extend correctly)
    internal class NewDresserMenu : IClickableMenu
    {
        public const int region_shopButtonModifier = 3546;
        public const int region_upArrow = 97865;
        public const int region_downArrow = 97866;
        public const int region_tabStartIndex = 99999;
        public const int howManyRecipesFitOnPage = 28;
        public const int infiniteStock = int.MaxValue;
        public const int salePriceIndex = 0;
        public const int stockIndex = 1;
        public const int extraTradeItemIndex = 2;
        public const int extraTradeItemCountIndex = 3;
        public const int itemsPerPage = 4;                
        public const int numberRequiredForExtraItemTrade = 5;
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
        private float sellPercentage = 1f;
        private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();
        public int hoverPrice = -1;
        public int currency;
        public int currentItemIndex;
        public ClickableTextureComponent upArrow;
        public ClickableTextureComponent downArrow;
        public ClickableTextureComponent scrollBar;
        public NPC portraitPerson;
        public string potraitPersonDialogue;
        public StorageFurniture source;
        private bool scrolling;
        public Func<ISalable, Farmer, int, bool> onPurchase;
        public Func<ISalable, bool> onSell;
        public Func<int, bool> canPurchaseCheck;
        public List<ClickableTextureComponent> tabButtons = new List<ClickableTextureComponent>();
        protected int currentTab;
        protected bool _isStorageShop;
        public bool readOnly;
        public HashSet<ISalable> buyBackItems = new HashSet<ISalable>();
        public Dictionary<ISalable, ISalable> buyBackItemsToResellTomorrow = new Dictionary<ISalable, ISalable>();

        // Define Dresser Menus
        public NewDresserMenu(Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
            : this(itemPriceAndStock.Keys.ToList(), currency, who, on_purchase, on_sell, context)
        {
            this.itemPriceAndStock = itemPriceAndStock;
        }

        public NewDresserMenu(List<ISalable> itemsForSale, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
            : base(Game1.uiViewport.Width / 2 - (800 + borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + borderWidth * 2) / 2, 1000 + borderWidth * 2, 600 + borderWidth * 2, showUpperRightCloseButton: true)
        {
            foreach (ISalable j in itemsForSale)
            {
                forSale.Add(j);
                itemPriceAndStock.Add(j, new int[2]
                {
                j.salePrice(),
                j.Stack
                });
            }
            updatePosition();
            this.currency = currency;
            onPurchase = on_purchase;
            onSell = on_sell;
            Game1.player.forceCanMove();
            Game1.playSound("dwop");
            inventory = new InventoryMenu(xPositionOnScreen + width, yPositionOnScreen + spaceToClearTopBorder + borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
            {
                showGrayedOutSlots = true
            };
            inventory.movePosition(-inventory.width - 32, 0);
            upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
            {
                myID = 97865,
                downNeighborID = 106,
                leftNeighborID = 3546
            };
            downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
            {
                myID = 106,
                upNeighborID = 97865,
                leftNeighborID = 3546
            };
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
            for (int i = 0; i < 4; i++)
            {
                forSaleButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * ((height - 256) / 4), width - 32, (height - 256) / 4 + 4), i.ToString() ?? "")
                {
                    myID = i + 3546,
                    rightNeighborID = 97865,
                    fullyImmutable = true
                });
            }
            updateSaleButtonNeighbors();
            storeContext = context;

            // Added this in and eliminated "Set up store for context"
            tabButtons = new List<ClickableTextureComponent>();
            categoriesToSellHere.AddRange(new int[4] { -95, -100, -97, -96 });
            _isStorageShop = true;
            ClickableTextureComponent tab3 = null;
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f)
            {
                myID = 99999 + tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f)
            {
                myID = 99999 + tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f)
            {
                myID = 99999 + tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f)
            {
                myID = 99999 + tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 64, 16, 16), 4f)
            {
                myID = 99999 + tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            tabButtons.Add(tab3);
            tab3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 64, 16, 16), 4f)
            {
                myID = 99999 + tabButtons.Count,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546
            };
            tabButtons.Add(tab3);
            StardewOutfitManager.tabSwitcher.includeTopTabButtons(this);
            //
            repositionTabs();
            purchaseSound = null;
            purchaseRepeatSound = null;
            if (tabButtons.Count > 0)
            {
                foreach (ClickableComponent forSaleButton in forSaleButtons)
                {
                    forSaleButton.leftNeighborID = -99998;
                }
            }
            applyTab();
            foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Top))
            {
                item.upNeighborID = -99998;
            }
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        public void updateSaleButtonNeighbors()
        {
            ClickableComponent last_valid_button = forSaleButtons[0];
            for (int i = 0; i < forSaleButtons.Count; i++)
            {
                ClickableComponent button = forSaleButtons[i];
                button.upNeighborImmutable = true;
                button.downNeighborImmutable = true;
                button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : (-7777));
                button.downNeighborID = ((i < 3 && i < forSale.Count - 1) ? (i + 3546 + 1) : (-7777));
                if (i >= forSale.Count)
                {
                    if (button == currentlySnappedComponent)
                    {
                        currentlySnappedComponent = last_valid_button;
                        if (Game1.options.SnappyMenus)
                        {
                            snapCursorToCurrentSnappedComponent();
                        }
                    }
                }
                else
                {
                    last_valid_button = button;
                }
            }
        }

        public void repositionTabs()
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                if (i == currentTab)
                {
                    tabButtons[i].bounds.X = xPositionOnScreen - 56;
                }
                else
                {
                    tabButtons[i].bounds.X = xPositionOnScreen - 64;
                }
                tabButtons[i].bounds.Y = yPositionOnScreen + i * 16 * 4 + 16;
            }
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            switch (direction)
            {
                case 2:
                    {
                        if (currentItemIndex < Math.Max(0, forSale.Count - 4))
                        {
                            downArrowPressed();
                            break;
                        }
                        int emptySlot = -1;
                        for (int i = 0; i < 12; i++)
                        {
                            inventory.inventory[i].upNeighborID = oldID;
                            if (emptySlot == -1 && heldItem != null && inventory.actualInventory != null && inventory.actualInventory.Count > i && inventory.actualInventory[i] == null)
                            {
                                emptySlot = i;
                            }
                        }
                        currentlySnappedComponent = getComponentWithID((emptySlot != -1) ? emptySlot : 0);
                        snapCursorToCurrentSnappedComponent();
                        break;
                    }
                case 0:
                    if (currentItemIndex > 0)
                    {
                        upArrowPressed();
                        currentlySnappedComponent = getComponentWithID(3546);
                        snapCursorToCurrentSnappedComponent();
                    }
                    break;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(3546);
            snapCursorToCurrentSnappedComponent();
        }

        public bool highlightItemToSell(Item i)
        {
            if (heldItem != null)
            {
                return heldItem.canStackWith(i);
            }
            if (categoriesToSellHere.Contains(i.Category))
            {
                return true;
            }
            return false;
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                int y2 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
                float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                currentItemIndex = Math.Min(Math.Max(0, forSale.Count - 4), Math.Max(0, (int)((float)forSale.Count * percentage)));
                setScrollBarToCurrentIndex();
                updateSaleButtonNeighbors();
                if (y2 != scrollBar.bounds.Y)
                {
                    Game1.playSound("shiny4");
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false;
        }

        private void setScrollBarToCurrentIndex()
        {
            if (forSale.Count > 0)
            {
                scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, forSale.Count - 4 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                if (currentItemIndex == forSale.Count - 4)
                {
                    scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && currentItemIndex > 0)
            {
                upArrowPressed();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && currentItemIndex < Math.Max(0, forSale.Count - 4))
            {
                downArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        private void downArrowPressed()
        {
            downArrow.scale = downArrow.baseScale;
            currentItemIndex++;
            setScrollBarToCurrentIndex();
            updateSaleButtonNeighbors();
        }

        private void upArrowPressed()
        {
            upArrow.scale = upArrow.baseScale;
            currentItemIndex--;
            setScrollBarToCurrentIndex();
            updateSaleButtonNeighbors();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && heldItem != null && heldItem is Item)
            {
                Item item = heldItem as Item;
                heldItem = null;
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
            Vector2 snappedPosition = inventory.snapToClickableComponent(x, y);
            StardewOutfitManager.tabSwitcher.handleTopBarLeftClick(x, y);
            if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, forSale.Count - 4))
            {
                downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
            {
                upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (scrollBar.containsPoint(x, y))
            {
                scrolling = true;
            }
            else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
            {
                scrolling = true;
                leftClickHeld(x, y);
                releaseLeftClick(x, y);
            }
            for (int k = 0; k < tabButtons.Count; k++)
            {
                if (tabButtons[k].containsPoint(x, y))
                {
                    switchTab(k);
                }
            }
            currentItemIndex = Math.Max(0, Math.Min(forSale.Count - 4, currentItemIndex));
            
            // On Sell function
            if (heldItem == null && !readOnly)
            {
                Item toSell = inventory.leftClick(x, y, null, playSound: false);
                if (toSell != null)
                {
                    if (onSell != null)
                    {
                        if (onSell(toSell))
                        {
                            toSell = null;
                        }
                    }
                }
            }
            else
            {
                heldItem = inventory.leftClick(x, y, heldItem as Item);
            }
            for (int i = 0; i < forSaleButtons.Count; i++)
            {
                if (currentItemIndex + i >= forSale.Count || !forSaleButtons[i].containsPoint(x, y))
                {
                    continue;
                }
                int index = currentItemIndex + i;
                if (forSale[index] != null)
                {
                    int toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[index]][0])), Math.Max(1, itemPriceAndStock[forSale[index]][1])));
                    toBuy = Math.Min(toBuy, forSale[index].maximumStackSize());
                    if (toBuy == -1)
                    {
                        toBuy = 1;
                    }
                    if (canPurchaseCheck != null && !canPurchaseCheck(index))
                    {
                        return;
                    }
                    if (toBuy > 0 && tryToPurchaseItem(forSale[index], heldItem, toBuy, x, y, index))
                    {
                        itemPriceAndStock.Remove(forSale[index]);
                        forSale.RemoveAt(index);
                    }
                    else if (toBuy <= 0)
                    {
                        if (itemPriceAndStock[forSale[index]].Length != 0 && itemPriceAndStock[forSale[index]][0] > 0)
                        {
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        }
                        Game1.playSound("cancel");
                    }
                    if (heldItem != null && (_isStorageShop || Game1.options.SnappyMenus || (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && heldItem.maximumStackSize() == 1)) && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(heldItem as Item))
                    {
                        heldItem = null;
                        DelayedAction.playSoundAfterDelay("coin", 100);
                    }
                }
                currentItemIndex = Math.Max(0, Math.Min(forSale.Count - 4, currentItemIndex));
                updateSaleButtonNeighbors();
                setScrollBarToCurrentIndex();
                return;
            }
            if (readyToClose() && (x < xPositionOnScreen - 64 || y < yPositionOnScreen - 64 || x > xPositionOnScreen + width + 128 || y > yPositionOnScreen + height + 64))
            {
                exitThisMenu();
            }
        }

        public virtual bool CanBuyback()
        {
            return true;
        }

        public virtual void BuyBuybackItem(ISalable bought_item, int price, int stack)
        {
            Game1.player.totalMoneyEarned -= (uint)price;
            if (Game1.player.useSeparateWallets)
            {
                Game1.player.stats.IndividualMoneyEarned -= (uint)price;
            }
            if (buyBackItemsToResellTomorrow.ContainsKey(bought_item))
            {
                ISalable sold_tomorrow_item = buyBackItemsToResellTomorrow[bought_item];
                sold_tomorrow_item.Stack -= stack;
                if (sold_tomorrow_item.Stack <= 0)
                {
                    buyBackItemsToResellTomorrow.Remove(bought_item);
                    (Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Remove(sold_tomorrow_item as Item);
                }
            }
        }

        public virtual ISalable AddBuybackItem(ISalable sold_item, int sell_unit_price, int stack)
        {
            ISalable target = null;
            while (stack > 0)
            {
                target = null;
                foreach (ISalable buyback_item in buyBackItems)
                {
                    if (buyback_item.canStackWith(sold_item) && buyback_item.Stack < buyback_item.maximumStackSize())
                    {
                        target = buyback_item;
                        break;
                    }
                }
                if (target == null)
                {
                    target = sold_item.GetSalableInstance();
                    int amount_to_deposit2 = Math.Min(stack, target.maximumStackSize());
                    buyBackItems.Add(target);
                    itemPriceAndStock.Add(target, new int[2] { sell_unit_price, amount_to_deposit2 });
                    target.Stack = amount_to_deposit2;
                    stack -= amount_to_deposit2;
                }
                else
                {
                    int amount_to_deposit = Math.Min(stack, target.maximumStackSize() - target.Stack);
                    int[] stock_data = itemPriceAndStock[target];
                    stock_data[1] += amount_to_deposit;
                    itemPriceAndStock[target] = stock_data;
                    target.Stack = stock_data[1];
                    stack -= amount_to_deposit;
                }
            }
            forSale = itemPriceAndStock.Keys.ToList();
            return target;
        }

        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            if (direction == 1 && tabButtons.Contains(a) && tabButtons.Contains(b))
            {
                return false;
            }
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        public virtual void switchTab(int new_tab)
        {
            currentTab = new_tab;
            Game1.playSound("shwip");
            applyTab();
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                snapCursorToCurrentSnappedComponent();
            }
        }

        public virtual void applyTab()
        {
            if (currentTab == 0)
            {
                forSale = itemPriceAndStock.Keys.ToList();
            }
            else
            {
                forSale.Clear();
                foreach (ISalable key in itemPriceAndStock.Keys)
                {
                    if (!(key is Item item3))
                    {
                        continue;
                    }
                    if (currentTab == 1)
                    {
                        if (item3.Category == -95)
                        {
                            forSale.Add(item3);
                        }
                    }
                    else if (currentTab == 2)
                    {
                        if (item3 is Clothing && (item3 as Clothing).clothesType.Value == 0)
                        {
                            forSale.Add(item3);
                        }
                    }
                    else if (currentTab == 3)
                    {
                        if (item3 is Clothing && (item3 as Clothing).clothesType.Value == 1)
                        {
                            forSale.Add(item3);
                        }
                    }
                    else if (currentTab == 4)
                    {
                        if (item3.Category == -97)
                        {
                            forSale.Add(item3);
                        }
                    }
                    else if (currentTab == 5 && item3.Category == -96)
                    {
                        forSale.Add(item3);
                    }
                }
            }
            currentItemIndex = 0;
            setScrollBarToCurrentIndex();
            updateSaleButtonNeighbors();
        }

        public override bool readyToClose()
        {
            if (heldItem == null)
            {
                return animations.Count == 0;
            }
            return false;
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            if (heldItem != null)
            {
                Game1.player.addItemToInventoryBool(heldItem as Item);
                Game1.playSound("coin");
            }
        }

        private bool tryToPurchaseItem(ISalable item, ISalable held_item, int numberToBuy, int x, int y, int indexInForSaleList)
        {
            if (readOnly)
            {
                return false;
            }
            if (held_item == null)
            {
                if (itemPriceAndStock[item][1] == 0)
                {
                    hoveredItem = null;
                    return true;
                }
                if (item.GetSalableInstance().maximumStackSize() < numberToBuy)
                {
                    numberToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
                }
                int price2 = itemPriceAndStock[item][0] * numberToBuy;
                int extraTradeItem2 = -1;
                int extraTradeItemCount2 = 5;
                if (itemPriceAndStock[item].Length > 2)
                {
                    extraTradeItem2 = itemPriceAndStock[item][2];
                    if (itemPriceAndStock[item].Length > 3)
                    {
                        extraTradeItemCount2 = itemPriceAndStock[item][3];
                    }
                    extraTradeItemCount2 *= numberToBuy;
                }
                if (Game1.player.hasItemInInventory(extraTradeItem2, extraTradeItemCount2))
                {
                    heldItem = item.GetSalableInstance();
                    heldItem.Stack = numberToBuy;
                    if (itemPriceAndStock[item][1] == int.MaxValue && item.Stack != int.MaxValue)
                    {
                        heldItem.Stack *= item.Stack;
                    }
                    if (!heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && (!(heldItem is StardewValley.Object) || !(heldItem as StardewValley.Object).isRecipe))
                    {
                        Game1.playSound("smallSelect");
                        heldItem = null;
                        return false;
                    }
                    if (itemPriceAndStock[item][1] != int.MaxValue && !item.IsInfiniteStock())
                    {
                        itemPriceAndStock[item][1] -= numberToBuy;
                        forSale[indexInForSaleList].Stack -= numberToBuy;
                    }
                    if (CanBuyback() && buyBackItems.Contains(item))
                    {
                        BuyBuybackItem(item, price2, numberToBuy);
                    }
                    ShopMenu.chargePlayer(Game1.player, currency, price2);
                    if (extraTradeItem2 != -1)
                    {
                        Game1.player.removeItemsFromInventory(extraTradeItem2, extraTradeItemCount2);
                    }
                    if (heldItem != null && heldItem is StardewValley.Object && (heldItem as StardewValley.Object).ParentSheetIndex == 858)
                    {
                        Game1.player.team.addQiGemsToTeam.Fire(heldItem.Stack);
                        heldItem = null;
                    }
                    if (Game1.mouseClickPolling > 300)
                    {
                        if (purchaseRepeatSound != null)
                        {
                            Game1.playSound(purchaseRepeatSound);
                        }
                    }
                    else if (purchaseSound != null)
                    {
                        Game1.playSound(purchaseSound);
                    }
                    if (onPurchase != null && onPurchase(item, Game1.player, numberToBuy))
                    {
                        exitThisMenu();
                    }
                }
            }
            else if (held_item.canStackWith(item))
            {
                numberToBuy = Math.Min(numberToBuy, held_item.maximumStackSize() - held_item.Stack);
                if (numberToBuy > 0)
                {
                    int price = itemPriceAndStock[item][0] * numberToBuy;
                    int extraTradeItem = -1;
                    int extraTradeItemCount = 5;
                    if (itemPriceAndStock[item].Length > 2)
                    {
                        extraTradeItem = itemPriceAndStock[item][2];
                        if (itemPriceAndStock[item].Length > 3)
                        {
                            extraTradeItemCount = itemPriceAndStock[item][3];
                        }
                        extraTradeItemCount *= numberToBuy;
                    }
                    int tmp = item.Stack;
                    item.Stack = numberToBuy + heldItem.Stack;
                    if (!item.CanBuyItem(Game1.player))
                    {
                        item.Stack = tmp;
                        Game1.playSound("cancel");
                        return false;
                    }
                    item.Stack = tmp;
                    if (Game1.player.hasItemInInventory(extraTradeItem, extraTradeItemCount))
                    {
                        int amountAddedToStack = numberToBuy;
                        if (itemPriceAndStock[item][1] == int.MaxValue && item.Stack != int.MaxValue)
                        {
                            amountAddedToStack *= item.Stack;
                        }
                        heldItem.Stack += amountAddedToStack;
                        if (itemPriceAndStock[item][1] != int.MaxValue && !item.IsInfiniteStock())
                        {
                            itemPriceAndStock[item][1] -= numberToBuy;
                            forSale[indexInForSaleList].Stack -= numberToBuy;
                        }
                        if (CanBuyback() && buyBackItems.Contains(item))
                        {
                            BuyBuybackItem(item, price, amountAddedToStack);
                        }
                        if (Game1.mouseClickPolling > 300)
                        {
                            if (purchaseRepeatSound != null)
                            {
                                Game1.playSound(purchaseRepeatSound);
                            }
                        }
                        else if (purchaseSound != null)
                        {
                            Game1.playSound(purchaseSound);
                        }
                        if (extraTradeItem != -1)
                        {
                            Game1.player.removeItemsFromInventory(extraTradeItem, extraTradeItemCount);
                        }
                        if (!_isStorageShop && item.actionWhenPurchased())
                        {
                            heldItem = null;
                        }
                        if (onPurchase != null && onPurchase(item, Game1.player, numberToBuy))
                        {
                            exitThisMenu();
                        }
                    }
                }
            }
            if (itemPriceAndStock[item][1] <= 0)
            {
                if (buyBackItems.Contains(item))
                {
                    buyBackItems.Remove(item);
                }
                hoveredItem = null;
                return true;
            }
            return false;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            Vector2 snappedPosition = inventory.snapToClickableComponent(x, y);
            if (heldItem == null && !readOnly)
            {
                ISalable toSell = inventory.rightClick(x, y, null, playSound: false);
                if (toSell != null)
                {
                    if (onSell != null)
                    {
                        if (onSell(toSell))
                        {
                            toSell = null;
                        }
                    }               
                }
            }
            else
            {
                heldItem = inventory.rightClick(x, y, heldItem as Item);
            }
            for (int i = 0; i < forSaleButtons.Count; i++)
            {
                if (currentItemIndex + i >= forSale.Count || !forSaleButtons[i].containsPoint(x, y))
                {
                    continue;
                }
                int index = currentItemIndex + i;
                if (forSale[index] == null)
                {
                    break;
                }
                int toBuy = 1;
                if (itemPriceAndStock[forSale[index]][0] > 0)
                {
                    toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / itemPriceAndStock[forSale[index]][0]), itemPriceAndStock[forSale[index]][1]));
                }
                if (canPurchaseCheck == null || canPurchaseCheck(index))
                {
                    if (toBuy > 0 && tryToPurchaseItem(forSale[index], heldItem, toBuy, x, y, index))
                    {
                        itemPriceAndStock.Remove(forSale[index]);
                        forSale.RemoveAt(index);
                    }
                    if (heldItem != null && (_isStorageShop || Game1.options.SnappyMenus) && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(heldItem as Item))
                    {
                        heldItem = null;
                        DelayedAction.playSoundAfterDelay("coin", 100);
                    }
                    setScrollBarToCurrentIndex();
                }
                break;
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            descriptionText = "";
            hoverText = "";
            hoveredItem = null;
            hoverPrice = -1;
            boldTitleText = "";
            upArrow.tryHover(x, y);
            downArrow.tryHover(x, y);
            scrollBar.tryHover(x, y);
            if (scrolling)
            {
                return;
            }
            for (int i = 0; i < forSaleButtons.Count; i++)
            {
                if (currentItemIndex + i < forSale.Count && forSaleButtons[i].containsPoint(x, y))
                {
                    ISalable item = forSale[currentItemIndex + i];
                    if (canPurchaseCheck == null || canPurchaseCheck(currentItemIndex + i))
                    {
                        hoverText = item.getDescription();
                        boldTitleText = item.DisplayName;
                        if (!_isStorageShop)
                        {
                            hoverPrice = ((itemPriceAndStock != null && itemPriceAndStock.ContainsKey(item)) ? itemPriceAndStock[item][0] : item.salePrice());
                        }
                        hoveredItem = item;
                        forSaleButtons[i].scale = Math.Min(forSaleButtons[i].scale + 0.03f, 1.1f);
                    }
                }
                else
                {
                    forSaleButtons[i].scale = Math.Max(1f, forSaleButtons[i].scale - 0.03f);
                }
            }
            if (heldItem != null)
            {
                return;
            }
            foreach (ClickableComponent c in inventory.inventory)
            {
                if (!c.containsPoint(x, y))
                {
                    continue;
                }
                Item j = inventory.getItemFromClickableComponent(c);
                if (j == null || (inventory.highlightMethod != null && !inventory.highlightMethod(j)))
                {
                    continue;
                }
                if (_isStorageShop)
                {
                    hoverText = j.getDescription();
                    boldTitleText = j.DisplayName;
                    hoveredItem = j;
                    continue;
                }
                hoverText = j.DisplayName + " x" + j.Stack;
                if (j is StardewValley.Object hovered_object && hovered_object.needsToBeDonated())
                {
                    hoverText = hoverText + "\n\n" + j.getDescription() + "\n";
                }
                hoverPrice = (int)((j is StardewValley.Object) ? ((float)(j as StardewValley.Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(j.salePrice() / 2) * sellPercentage)) * j.Stack;
            }
            StardewOutfitManager.tabSwitcher.handleTopBarOnHover(x, y);
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b != Buttons.RightTrigger && b != Buttons.LeftTrigger)
            {
                return;
            }
            if (currentlySnappedComponent != null && currentlySnappedComponent.myID >= 3546)
            {
                int emptySlot = -1;
                for (int i = 0; i < 12; i++)
                {
                    inventory.inventory[i].upNeighborID = 3546 + forSaleButtons.Count - 1;
                    if (emptySlot == -1 && heldItem != null && inventory.actualInventory != null && inventory.actualInventory.Count > i && inventory.actualInventory[i] == null)
                    {
                        emptySlot = i;
                    }
                }
                currentlySnappedComponent = getComponentWithID((emptySlot != -1) ? emptySlot : 0);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                snapToDefaultClickableComponent();
            }
            Game1.playSound("shiny4");
        }

        private int getHoveredItemExtraItemIndex()
        {
            if (itemPriceAndStock != null && hoveredItem != null && itemPriceAndStock.ContainsKey(hoveredItem) && itemPriceAndStock[hoveredItem].Length > 2)
            {
                return itemPriceAndStock[hoveredItem][2];
            }
            return -1;
        }

        private int getHoveredItemExtraItemAmount()
        {
            if (itemPriceAndStock != null && hoveredItem != null && itemPriceAndStock.ContainsKey(hoveredItem) && itemPriceAndStock[hoveredItem].Length > 3)
            {
                return itemPriceAndStock[hoveredItem][3];
            }
            return 5;
        }

        public void updatePosition()
        {
            width = 1000 + borderWidth * 2;
            height = 600 + borderWidth * 2;
            xPositionOnScreen = Game1.uiViewport.Width / 2 - (800 + borderWidth * 2) / 2;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + borderWidth * 2) / 2;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            updatePosition();
            initializeUpperRightCloseButton();
            Game1.player.forceCanMove();
            inventory = new InventoryMenu(xPositionOnScreen + width, yPositionOnScreen + spaceToClearTopBorder + borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
            {
                showGrayedOutSlots = true
            };
            inventory.movePosition(-inventory.width - 32, 0);
            upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
            forSaleButtons.Clear();
            for (int i = 0; i < 4; i++)
            {
                forSaleButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * ((height - 256) / 4), width - 32, (height - 256) / 4 + 4), i.ToString() ?? ""));
            }
            if (tabButtons.Count > 0)
            {
                foreach (ClickableComponent forSaleButton in forSaleButtons)
                {
                    forSaleButton.leftNeighborID = -99998;
                }
            }
            repositionTabs();
            foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Top))
            {
                item.upNeighborID = -99998;
            }
        }

        public void setItemPriceAndStock(Dictionary<ISalable, int[]> new_stock)
        {
            itemPriceAndStock = new_stock;
            forSale = itemPriceAndStock.Keys.ToList();
            applyTab();
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }
            Texture2D purchase_texture = Game1.mouseCursors;
            Rectangle purchase_window_border = new Rectangle(384, 373, 18, 18);
            Rectangle purchase_item_rect = new Rectangle(384, 396, 15, 15);
            int purchase_item_text_color = -1;
            bool purchase_draw_item_background = true;
            Rectangle purchase_item_background = new Rectangle(296, 363, 18, 18);
            Color purchase_selected_color = Color.Wheat;
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen + width - inventory.width - 32 - 24, yPositionOnScreen + height - 256 + 40, inventory.width + 56, height - 448 + 20, Color.White, 4f);
            drawTextureBox(b, purchase_texture, purchase_window_border, xPositionOnScreen, yPositionOnScreen, width, height - 256 + 32 + 4, Color.White, 4f);
            for (int k = 0; k < forSaleButtons.Count; k++)
            {
                if (currentItemIndex + k >= forSale.Count)
                {
                    continue;
                }
                bool failedCanPurchaseCheck = false;
                if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + k))
                {
                    failedCanPurchaseCheck = true;
                }
                drawTextureBox(b, purchase_texture, purchase_item_rect, forSaleButtons[k].bounds.X, forSaleButtons[k].bounds.Y, forSaleButtons[k].bounds.Width, forSaleButtons[k].bounds.Height, (forSaleButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !scrolling) ? purchase_selected_color : Color.White, 4f, drawShadow: false);
                ISalable item = forSale[currentItemIndex + k];
                bool buyInStacks = item.Stack > 1 && item.Stack != int.MaxValue && itemPriceAndStock[item][1] == int.MaxValue;
                StackDrawType stackDrawType;
                if (itemPriceAndStock[item][1] == int.MaxValue)
                {
                    stackDrawType = StackDrawType.HideButShowQuality;
                }
                else
                {
                    stackDrawType = StackDrawType.Draw_OneInclusive;
                    if (_isStorageShop)
                    {
                        stackDrawType = StackDrawType.Draw;
                    }
                }
                string displayName = item.DisplayName;
                if (buyInStacks)
                {
                    displayName = displayName + " x" + item.Stack;
                }
                if (forSale[currentItemIndex + k].ShouldDrawIcon())
                {
                    if (purchase_draw_item_background)
                    {
                        b.Draw(purchase_texture, new Vector2(forSaleButtons[k].bounds.X + 32 - 12, forSaleButtons[k].bounds.Y + 24 - 4), purchase_item_background, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    }
                    forSale[currentItemIndex + k].drawInMenu(b, new Vector2(forSaleButtons[k].bounds.X + 32 - 8, forSaleButtons[k].bounds.Y + 24), 1f, 1f, 0.9f, stackDrawType, Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), drawShadow: true);
                    if (buyBackItems.Contains(forSale[currentItemIndex + k]))
                    {
                        b.Draw(Game1.mouseCursors2, new Vector2(forSaleButtons[k].bounds.X + 32 - 8, forSaleButtons[k].bounds.Y + 24), new Rectangle(64, 240, 16, 16), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
                    }
                    SpriteText.drawString(b, displayName, forSaleButtons[k].bounds.X + 96 + 8, forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
                else
                {
                    SpriteText.drawString(b, displayName, forSaleButtons[k].bounds.X + 32 + 8, forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
                if (itemPriceAndStock[forSale[currentItemIndex + k]][0] > 0)
                {
                    SpriteText.drawString(b, itemPriceAndStock[forSale[currentItemIndex + k]][0] + " ", forSaleButtons[k].bounds.Right - SpriteText.getWidthOfString(itemPriceAndStock[forSale[currentItemIndex + k]][0] + " ") - 60, forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, (ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) >= itemPriceAndStock[forSale[currentItemIndex + k]][0] && !failedCanPurchaseCheck) ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButtons[k].bounds.Right - 52, forSaleButtons[k].bounds.Y + 40 - 4), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, (!failedCanPurchaseCheck) ? 0.35f : 0f);
                }
                else if (itemPriceAndStock[forSale[currentItemIndex + k]].Length > 2)
                {
                    int required_item_count = 5;
                    int requiredItem = itemPriceAndStock[forSale[currentItemIndex + k]][2];
                    if (itemPriceAndStock[forSale[currentItemIndex + k]].Length > 3)
                    {
                        required_item_count = itemPriceAndStock[forSale[currentItemIndex + k]][3];
                    }
                    bool hasEnoughToTrade = Game1.player.hasItemInInventory(requiredItem, required_item_count);
                    if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + k))
                    {
                        hasEnoughToTrade = false;
                    }
                    float textWidth = SpriteText.getWidthOfString("x" + required_item_count);
                    Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)(forSaleButtons[k].bounds.Right - 88) - textWidth, forSaleButtons[k].bounds.Y + 28 - 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, requiredItem, 16, 16), Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, -1f, flipped: false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
                    SpriteText.drawString(b, "x" + required_item_count, forSaleButtons[k].bounds.Right - (int)textWidth - 16, forSaleButtons[k].bounds.Y + 44, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
            }
            inventory.draw(b);
            for (int j = animations.Count - 1; j >= 0; j--)
            {
                if (animations[j].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(j);
                }
                else
                {
                    animations[j].draw(b, localPosition: true);
                }
            }
            if (poof != null)
            {
                poof.draw(b);
            }
            upArrow.draw(b);
            downArrow.draw(b);
            for (int i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].draw(b);
            }
            StardewOutfitManager.tabSwitcher.drawTopBar(b);
            if (forSale.Count > 4)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
            }
            if (!hoverText.Equals(""))
            {
                if (hoveredItem is StardewValley.Object && (bool)(hoveredItem as StardewValley.Object).isRecipe)
                {
                    drawToolTip(b, " ", boldTitleText, hoveredItem as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex(), getHoveredItemExtraItemAmount(), new CraftingRecipe(hoveredItem.Name.Replace(" Recipe", "")), (hoverPrice > 0) ? hoverPrice : (-1));
                }
                else
                {
                    drawToolTip(b, hoverText, boldTitleText, hoveredItem as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex(), getHoveredItemExtraItemAmount(), null, (hoverPrice > 0) ? hoverPrice : (-1));
                }
            }
            if (heldItem != null)
            {
                heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
            }
            drawMouse(b);
        }
    }
}
