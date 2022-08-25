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

namespace StardewOutfitManager.Menus
{
    // Initialize a custom original dresser menu
    internal class NewDresserMenu : ShopMenu
    {
        // Re-declare the private variables inherited from ShopMenu that draw needs to access
        //private bool scrolling;
        //private string hoverText = "";
        //private string boldTitleText = "";
        //private Rectangle scrollBarRunnerNew;
        //private TemporaryAnimatedSprite poof;
        //private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

        public NewDresserMenu(Dictionary<ISalable, int[]> itemPriceAndStock, StorageFurniture source, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null) 
            : base(itemPriceAndStock, 0, null, on_purchase, on_sell, context)
        {
            // Get rid of close button
            //upperRightCloseButton = null;
            // Add top tab buttons
            StardewOutfitManager.tabSwitcher.includeTopTabButtons(this);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            StardewOutfitManager.tabSwitcher.handleTopBarLeftClick(x, y);
        }
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            StardewOutfitManager.tabSwitcher.handleTopBarOnHover(x, y);
        }
        public override void draw(SpriteBatch b)
        {
            // Begin draw code
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
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen + base.width - inventory.width - 32 - 24, base.yPositionOnScreen + base.height - 256 + 40, inventory.width + 56, base.height - 448 + 20, Color.White, 4f);
            IClickableMenu.drawTextureBox(b, purchase_texture, purchase_window_border, base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height - 256 + 32 + 4, Color.White, 4f);
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
                IClickableMenu.drawTextureBox(b, purchase_texture, purchase_item_rect, forSaleButtons[k].bounds.X, forSaleButtons[k].bounds.Y, forSaleButtons[k].bounds.Width, forSaleButtons[k].bounds.Height, (forSaleButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !scrolling) ? purchase_selected_color : Color.White, 4f, drawShadow: false);
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
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
            }
            if (!hoverText.Equals(""))
            {
                IClickableMenu.drawToolTip(b, hoverText, boldTitleText, hoveredItem as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex(), getHoveredItemExtraItemAmount(), null, (hoverPrice > 0) ? hoverPrice : (-1));
            }
            if (heldItem != null)
            {
                heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
            }
            if (upperRightCloseButton != null && shouldDrawCloseButton())
            {
                upperRightCloseButton.draw(b);
            }
            drawMouse(b);
        }
        
        // Redefine private methods inherited from ShopMenu that draw needs to access
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
    }
}
