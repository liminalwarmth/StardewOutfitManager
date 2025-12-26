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
using static StardewValley.Menus.LoadGameMenu;
using StardewValley.Characters;

namespace StardewOutfitManager.Menus
{
    // This class defines the Favorites outfit selection menu
    internal class FavoritesMenu : IClickableMenu
    {
        // ** OUTFIT SLOT CLASS**

        // Outfit slot class definition
        public class OutfitSlot
        {
            // Base variables
            internal FavoritesMenu menu;
            internal bool isAvailable;
            internal bool isFavorite;
            internal bool isSelected;
            internal bool isVisible = false;
            public Dictionary<string, Item> outfitAvailabileItems;
            public Farmer modelFarmer;
            public FavoriteOutfit modelOutfit;
            public Rectangle bgSprite;
            public Rectangle bgBox;

            // Display name for outfit
            internal string outfitName;

            // ** CONSTRUCTOR **
            public OutfitSlot(FavoritesMenu menu, Farmer player, FavoriteOutfit outfit, Dictionary<string, Item> itemTagLookup)
            {
                // Set up background and fake model farmer
                this.menu = menu;
                modelOutfit = outfit;
                bgBox = new Rectangle(menu.xPositionOnScreen, menu.yPositionOnScreen, 128, 192);
                bgSprite = GetCategoryBackground(modelOutfit);
                modelFarmer = menu.CreateFakeModelFarmer(player);
                // Outfit card preview farmers always face forward regardless of main display farmer direction
                modelFarmer.faceDirection(2);

                // Set whether this is a favorited outfit among all outfits
                isFavorite = modelOutfit.isFavorite;

                // Establish equipment availability and dress the display farmer in what's available
                outfitAvailabileItems = modelOutfit.GetOutfitItemAvailability(itemTagLookup);
                modelOutfit.dressDisplayFarmerWithAvailableOutfitPieces(modelFarmer, outfitAvailabileItems);

                // Set outfit slot to unavailable if any necessary items are missing
                isAvailable = !outfitAvailabileItems.ContainsValue(null);

                // Set the display name
                outfitName = GetDisplayName();
            }

            // ** METHODS **

            // Activate the outfit slot and display the model outfit
            public void Select()
            {
                // Change this outfit box to selected and all others to unselected
                foreach (OutfitSlot outfit in menu.outfitSlots) { outfit.isSelected = false; }
                isSelected = true;

                // Update the menu reference to the selected outfit slot
                menu.outfitSlotSelected = this;

                // Set the display background to match the outfit background
                menu.outFitDisplayBG = bgSprite;

                // Dress the main display farmer in the favorites menu in this outfit
                modelOutfit.dressDisplayFarmerWithAvailableOutfitPieces(menu._displayFarmer, outfitAvailabileItems);
                Game1.playSound("dwop");
            }

            // Draw the outfit box
            public void Draw(SpriteBatch b)
            {
                // Draw selection indicator if this outfit is currently being displayed
                if (isSelected)
                {
                    b.Draw(Game1.staminaRect, new Rectangle(bgBox.X, bgBox.Y - 3, 128, 192 + 6), Color.Fuchsia);
                    b.Draw(Game1.staminaRect, new Rectangle(bgBox.X - 3, bgBox.Y, 128 + 6, 192), Color.Fuchsia);
                }

                // Draw background
                b.Draw(StardewOutfitManager.assetManager.customSprites, new Vector2(bgBox.X, bgBox.Y), bgSprite, Color.White);

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
                    b.Draw(Game1.menuTexture, new Rectangle(bgBox.X + 14, bgBox.Y + 15, 24, 24), new Rectangle(64, 772, 28, 28), Color.White);
                }
            }

            // Delete a favorite outfit
            public void Delete()
            {
                // Return the display farmer to the player defaults
                menu.ResetSelectedOutfit();
                // Remove the outfit from the favorites data model
                menu.favoritesData.DeleteOutfit(modelOutfit);
                // Remove the outfit slot from the outfitslots list
                menu.outfitSlots.Remove(this);
                // Re-filter, sort, and position the new outfit list
                menu.FilterOutfitSlotsByCategoryAndSort(menu.outfitSlots);
                menu.UpdateOutfitButtonsAndSlots();
            }

            // Toggle whether this is a favorited outfit in the favorites menu
            public void ToggleFavorite()
            {
                // Toggle both the slot and the data model status
                isFavorite = !isFavorite;
                modelOutfit.isFavorite = isFavorite;
            }

            // Get outfit category background
            public Rectangle GetCategoryBackground(FavoriteOutfit outfit)
            {
                Rectangle background = menu.bgDefault;
                // Check category and return appropriate background
                switch (outfit.Category)
                {
                    case "Spring":
                        background = menu.bgSpring;
                        break;

                    case "Summer":
                        background = menu.bgSummer;
                        break;

                    case "Fall":
                        background = menu.bgFall;
                        break;

                    case "Winter":
                        background = menu.bgWinter;
                        break;

                    case "Special":
                        background = menu.bgSpecial;
                        break;
                }
                return background;
            }

            // Get display name for outfit (custom name or auto-generated from roster position)
            private string GetDisplayName()
            {
                // If user set a custom name, use it
                if (!string.IsNullOrEmpty(modelOutfit.Name))
                    return modelOutfit.Name;

                // Otherwise, compute name based on position within category
                int position = 1;
                foreach (var fav in menu.favoritesData.Favorites)
                {
                    if (fav.Category == modelOutfit.Category)
                    {
                        if (fav == modelOutfit) break;
                        position++;
                    }
                }
                return $"{modelOutfit.Category} Outfit {position}";
            }
        }

        // ** FAVORITES MENU CLASS **

        // Layout constants
        private const int OUTFITS_PER_ROW = 4;
        private const int VISIBLE_ROWS = 2;

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
        private Item hoveredItem = null;
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

        // Category Selector Buttons
        internal ClickableComponent categorySelected;
        public Rectangle categoryShading;
        public List<ClickableComponent> categoryButtons = new();

        // Outfit buttons and offsets
        public List<ClickableComponent> outfitButtons = new();
        internal Rectangle outfitBox;

        // Master list of generated slots and filtered display list of slots
        public List<OutfitSlot> outfitSlots = new();
        public List<OutfitSlot> outfitSlotsFiltered = new();

        // Outfit Data
        internal int outfitDisplayIndex = 0;
        internal int currentOutfitIndex = 0;
        internal OutfitSlot outfitSlotSelected = null;
        internal OutfitSlot outfitSlotHovered = null;

        // Scroll bar and controls
        public ClickableTextureComponent upArrow;   // Start at 7000
        public ClickableTextureComponent downArrow;
        public ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private bool scrolling;

        // Available items for outfits
        public List<Item> playerOwnedItems = new();

        // Snap Regions
        internal const int OUTFITS = 10000;         // Start at 2000
        internal const int CATEGORIES = 20000;      // Start at 6000
        internal const int PORTRAIT = 30000;        // Start at 3000
        internal const int OUTFITSETTINGS = 40000;  // Start at 4000
        internal const int EQUIPMENT = 50000;       // Start at 5000

        // Empty slot icon indices (from Game1.menuTexture tile sheet)
        private const int EMPTY_HAT_ICON = 42;
        private const int EMPTY_SHIRT_ICON = 69;
        private const int EMPTY_PANTS_ICON = 68;
        private const int EMPTY_BOOTS_ICON = 40;
        private const int EMPTY_RING_ICON = 41;
        private const int FILLED_SLOT_ICON = 10;

        // Map equipment component names to outfit data keys
        private static string GetSlotKey(string componentName) => componentName switch
        {
            "Hat" => "Hat",
            "Shirt" => "Shirt",
            "Pants" => "Pants",
            "Boots" => "Shoes",
            "Left Ring" => "LeftRing",
            "Right Ring" => "RightRing",
            _ => null
        };

        // Get empty slot icon for a given equipment component
        private static int GetEmptySlotIcon(string componentName) => componentName switch
        {
            "Hat" => EMPTY_HAT_ICON,
            "Shirt" => EMPTY_SHIRT_ICON,
            "Pants" => EMPTY_PANTS_ICON,
            "Boots" => EMPTY_BOOTS_ICON,
            "Left Ring" or "Right Ring" => EMPTY_RING_ICON,
            _ => FILLED_SLOT_ICON
        };

        // Additional Buttons
        public ClickableTextureComponent leftRotationButton;
        public ClickableTextureComponent rightRotationButton;
        public ClickableTextureComponent okButton;

        // Equipment Icons (for selected outfit display)
        public List<ClickableComponent> equipmentIcons = new();

        // ** CONSTRUCTOR **

        // Favorites Menu
        public FavoritesMenu() : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            /// FAVORITES MENU
            // Set up menu structure
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            xPositionOnScreen = (int)topLeft.X;
            yPositionOnScreen = (int)topLeft.Y;

            // Set up portrait and farmer
            _portraitBox = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + 64, 256, 384);
            _displayFarmer = CreateFakeModelFarmer(Game1.player);
            _displayFarmer.faceDirection(menuManager.farmerFacingDirection);
            _displayFarmer.FarmerSprite.StopAnimation();

            // Player display window movement buttons
            leftRotationButton = new ClickableTextureComponent("LeftRotate", new Rectangle(_portraitBox.X - 42, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1.25f);
            rightRotationButton = new ClickableTextureComponent("RightRotate", new Rectangle(_portraitBox.X + 256 - 38, _portraitBox.Bottom - 24, 60, 60), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1.25f);

            // Equipment icons for selected outfit display (below portrait)
            // Layout: [Hat][Shirt][Left Ring]
            //         [Boots][Pants][Right Ring]
            int eqIconXOffset = _portraitBox.X + _portraitBox.Width / 2 - 81 - 16;
            int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64), "Hat")
            {
                myID = 5000,
                rightNeighborID = 5001,
                downNeighborID = 5003,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                region = EQUIPMENT
            });
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64), "Shirt")
            {
                myID = 5001,
                leftNeighborID = 5000,
                rightNeighborID = 5004,
                downNeighborID = 5002,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                region = EQUIPMENT
            });
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64), "Pants")
            {
                myID = 5002,
                leftNeighborID = 5003,
                rightNeighborID = 5005,
                upNeighborID = 5001,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                region = EQUIPMENT
            });
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64), "Boots")
            {
                myID = 5003,
                rightNeighborID = 5002,
                upNeighborID = 5000,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                region = EQUIPMENT
            });
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64), "Left Ring")
            {
                myID = 5004,
                leftNeighborID = 5001,
                downNeighborID = 5005,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                region = EQUIPMENT
            });
            equipmentIcons.Add(new ClickableComponent(new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64), "Right Ring")
            {
                myID = 5005,
                leftNeighborID = 5002,
                upNeighborID = 5004,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                region = EQUIPMENT
            });

            // Basic UI Functionality Buttons
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 9999,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC
            };

            // Generate outfit box positions, navigation, and components
            outfitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder + 342, yPositionOnScreen + 120, 592, 436);
            upArrow = new ClickableTextureComponent(new Rectangle(outfitBox.X + outfitBox.Width + 18, outfitBox.Y + 16, 44, 48), Game1.mouseCursors, new Rectangle(76, 72, 40, 44), 1f)
            {
                myID = 7000,
                upNeighborID = 6005,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC
            };
            downArrow = new ClickableTextureComponent(new Rectangle(outfitBox.X + outfitBox.Width + 18, outfitBox.Y + outfitBox.Height - 56, 44, 48), Game1.mouseCursors, new Rectangle(12, 76, 40, 44), 1f)
            {
                myID = 7001,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC
            };
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 8, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, outfitBox.Height - 64 - upArrow.bounds.Height - 20);

            // Generate Category Buttons
            int catYoffset = 78;
            categoryButtons.Add(new ClickableTextureComponent("All Outfits", new Rectangle(outfitBox.X, outfitBox.Y - catYoffset, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(124, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Spring", new Rectangle(outfitBox.X + 90, outfitBox.Y - catYoffset, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(14, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Summer", new Rectangle(outfitBox.X + 180, outfitBox.Y - catYoffset, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(36, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Fall", new Rectangle(outfitBox.X + 270, outfitBox.Y - catYoffset, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(58, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Winter", new Rectangle(outfitBox.X + 360, outfitBox.Y - catYoffset, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(80, 192, 22, 18), 4f));
            categoryButtons.Add(new ClickableTextureComponent("Special", new Rectangle(outfitBox.X + 450, outfitBox.Y - catYoffset, 88, 72), null, "", StardewOutfitManager.assetManager.customSprites, new Rectangle(102, 192, 22, 18), 4f));
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
            // If "All Outfits" was selected on this menu, keep it; otherwise find the matching season category
            categorySelected = categoryButtons.FirstOrDefault(c => c.name == menuManager.selectedCategory) ?? categoryButtons[1];

            // Generate available player items and build lookup dictionary for efficient outfit checks
            GeneratePlayerOwnedItemList();
            var itemTagLookup = FavoriteOutfitMethods.BuildItemTagLookup(playerOwnedItems);

            // Generate master outfit slot list, and create initial filtered list from it
            foreach (FavoriteOutfit outfit in favoritesData.Favorites)
            {
                outfitSlots.Add(new OutfitSlot(this, _displayFarmer, outfit, itemTagLookup));
            }
            FilterOutfitSlotsByCategoryAndSort(outfitSlots);

            // Create buttons (two rows of up to 4 outfits possible to have displayed at one time, (j) is row, (i) is column) and assign initial outfit slots to them
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    ClickableComponent btn = new ClickableComponent(new Rectangle(outfitBox.X + 22 + (i * 128) + (i * 12), outfitBox.Y + 20 + (j * 192) + (j * 12), 128, 192), i.ToString() ?? "")
                    {
                        myID = 2000 + i + (j * 4),
                        upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                        downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                        leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                        rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                        region = OUTFITS
                    };
                    outfitButtons.Add(btn);
                }
            }
            UpdateOutfitButtonsAndSlots();

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
                if (outfitSlotsFiltered.Count > 0)
                {
                    setCurrentlySnappedComponentTo(2000);
                    snapCursorToCurrentSnappedComponent();
                }
                else { snapToDefaultClickableComponent(); }
            }
        }

        // ** METHODS **

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
                else if (item is Clothing && (item as Clothing).clothesType.Value == Clothing.ClothesType.SHIRT)
                {
                    playerOwnedItems.Add(item as Clothing);
                }
                else if (item is Clothing && (item as Clothing).clothesType.Value == Clothing.ClothesType.PANTS)
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
        
        // Create outfit model for display
        public Farmer CreateFakeModelFarmer(Farmer player)
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
            // SDV 1.6: shirtColor removed - color now comes from Clothing item directly
            farmer.pantsColor.Set(player.pantsColor.Value);
            farmer.changeHairColor(player.hairstyleColor.Value);
            farmer.changeSkinColor(player.skin.Value);
            farmer.accessory.Set(player.accessory.Value);
            farmer.changeEyeColor(player.newEyeColor.Value);
            farmer.UpdateClothing();
            farmer.faceDirection(menuManager.farmerFacingDirection);
            farmer.FarmerSprite.StopAnimation();
            return farmer;
        }

        // Clears the selection and resets the display farmer to player defaults
        public void ResetSelectedOutfit()
        {
            // Change all outfits to unselected and update the selected reference
            foreach (OutfitSlot outfit in outfitSlots) { outfit.isSelected = false; }
            outfitSlotSelected = null;

            // Reset the background and restore the display model to player default equipment
            outFitDisplayBG = bgDefault;
            Farmer player = Game1.player;
            _displayFarmer.changeHairStyle(player.hair.Value);
            _displayFarmer.shirtItem.Set(player.shirtItem.Value);
            _displayFarmer.pantsItem.Set(player.pantsItem.Value);
            _displayFarmer.shirt.Set(player.shirt.Value);
            _displayFarmer.pants.Set(player.pants.Value);
            _displayFarmer.changeShoeColor(player.shoes.Value);
            _displayFarmer.boots.Set(player.boots.Value);
            _displayFarmer.leftRing.Set(player.leftRing.Value);
            _displayFarmer.rightRing.Set(player.rightRing.Value);
            _displayFarmer.hat.Set(player.hat.Value);
            // SDV 1.6: shirtColor removed - color now comes from Clothing item directly
            _displayFarmer.pantsColor.Set(player.pantsColor.Value);
            _displayFarmer.accessory.Set(player.accessory.Value);
            _displayFarmer.UpdateClothing();
            _displayFarmer.faceDirection(menuManager.farmerFacingDirection);
            _displayFarmer.FarmerSprite.StopAnimation();
        }

        public void FilterOutfitSlotsByCategoryAndSort(List<OutfitSlot> list)
        {
            if (list.Count > 0)
            {
                // Filter to the given category
                List<OutfitSlot> filtered = new();
                if (categorySelected.name == "All Outfits")
                {
                    filtered = list;
                }
                else
                {
                    foreach (OutfitSlot slot in list)
                    {
                        if (slot.modelOutfit.Category == categorySelected.name)
                        {
                            filtered.Add(slot);
                        }
                    }
                }

                // If this is all outfits, sort by season
                if (filtered.Count > 0 && categorySelected.name == "All Outfits")
                {
                    List<OutfitSlot> spring = new();
                    List<OutfitSlot> summer = new();
                    List<OutfitSlot> fall = new();
                    List<OutfitSlot> winter = new();
                    List<OutfitSlot> special = new();
                    foreach (OutfitSlot slot in filtered)
                    {
                        if (slot.modelOutfit.Category == "Spring") { spring.Add(slot); }
                        else if (slot.modelOutfit.Category == "Summer") { summer.Add(slot); }
                        else if (slot.modelOutfit.Category == "Fall") { fall.Add(slot); }
                        else if (slot.modelOutfit.Category == "Winter") { winter.Add(slot); }
                        else { special.Add(slot); }
                    }
                    filtered = spring.Concat(summer).Concat(fall).Concat(winter).Concat(special).ToList();
                }

                // Sort filtered list: favorited first, then regular, then unavailable (favorited unavailable before regular unavailable)
                if (filtered.Count > 0)
                {
                    List<OutfitSlot> favorited = new();
                    List<OutfitSlot> regular = new();
                    List<OutfitSlot> unavailableAndFavorite = new();
                    List<OutfitSlot> unavailable = new();
                    foreach (OutfitSlot slot in filtered)
                    {
                        if (!slot.isAvailable && slot.isFavorite) { unavailableAndFavorite.Add(slot); }
                        else if (!slot.isAvailable) { unavailable.Add(slot); }
                        else if (slot.isFavorite) { favorited.Add(slot); }
                        else { regular.Add(slot); }
                    }
                    filtered = favorited.Concat(regular).Concat(unavailableAndFavorite).Concat(unavailable).ToList();
                }

                // Set the filtered list to the ordered slots
                outfitSlotsFiltered = filtered;
                // Update the current outfit index
                currentOutfitIndex = 0;
            }
        }

        // Update the outfit buttons to the correct slots based on current outfit index
        public void UpdateOutfitButtonsAndSlots()
        {
            // First set all slots to invisible and clear their button position assignments
            foreach (OutfitSlot outfit in outfitSlots)
            {
                outfit.isVisible = false;
                outfit.bgBox.X = -1000;
                outfit.bgBox.Y = -1000;
            }
            
            // Then assign up to 8 outfits from the filtered list beginning at the current index to button positions
            for (int i = 0; i < 8; i++)
            {
                // Only try to position outfitslots which exist
                if (currentOutfitIndex + i < outfitSlotsFiltered.Count)
                {
                    // Match slot draw positions to where the 8 outfit buttons have been placed
                    outfitSlotsFiltered[currentOutfitIndex + i].bgBox.X = outfitButtons[i].bounds.X;
                    outfitSlotsFiltered[currentOutfitIndex + i].bgBox.Y = outfitButtons[i].bounds.Y;
                    outfitSlotsFiltered[currentOutfitIndex + i].isVisible = true;
                    outfitButtons[i].visible = true;
                }
                // Set any unused outfit buttons to invisible
                else { outfitButtons[i].visible = false; }
            }
            
            // Set navigation to visible if there are more outfits than fit on screen
            int visibleSlots = OUTFITS_PER_ROW * VISIBLE_ROWS;
            upArrow.visible = outfitSlotsFiltered.Count > visibleSlots;
            downArrow.visible = outfitSlotsFiltered.Count > visibleSlots;
            scrollBar.visible = outfitSlotsFiltered.Count > visibleSlots;

            // If the outfit index is 0 (top of outfits)...
            if (currentOutfitIndex == 0)
            {
                // Set the top row to auto-snap up
                for (int i = 0; i < OUTFITS_PER_ROW; i++) { outfitButtons[i].upNeighborID = ClickableComponent.SNAP_AUTOMATIC; }
                // If there are more than visible slots, set the bottom row to custom snap down
                if (outfitSlotsFiltered.Count > visibleSlots) { for (int i = OUTFITS_PER_ROW; i < visibleSlots; i++) { outfitButtons[i].downNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR; } }
                // Otherwise set them to auto-snap down
                else { for (int i = OUTFITS_PER_ROW; i < visibleSlots; i++) { outfitButtons[i].downNeighborID = ClickableComponent.SNAP_AUTOMATIC; } }
            }
            // If we're at the bottom of the index but it's not 0
            else if (currentOutfitIndex + OUTFITS_PER_ROW >= outfitSlotsFiltered.Count - OUTFITS_PER_ROW)
            {
                // Set the top row to custom snap up
                for (int i = 0; i < OUTFITS_PER_ROW; i++) { outfitButtons[i].upNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR; }
                // Set the bottom row to auto-snap down
                for (int i = OUTFITS_PER_ROW; i < visibleSlots; i++) { outfitButtons[i].downNeighborID = ClickableComponent.SNAP_AUTOMATIC; }
            }
            // Otherwise the top row should custom snap up and auto-snap down and the bottom row should custom snap down and auto-snap up
            else
            {
                for (int i = 0; i < OUTFITS_PER_ROW; i++)
                {
                    outfitButtons[i].upNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR;
                    outfitButtons[i].downNeighborID = ClickableComponent.SNAP_AUTOMATIC;
                }
                for (int i = OUTFITS_PER_ROW; i < visibleSlots; i++)
                {
                    outfitButtons[i].upNeighborID = ClickableComponent.SNAP_AUTOMATIC;
                    outfitButtons[i].downNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR;
                }
            }
        }

        // Change the category filter
        private void ChangeCategory(ClickableComponent newCategory)
        {
            // If we didn't click the category already active
            if (categorySelected.name != newCategory.name)
            {
                // Update the category setting
                categorySelected = newCategory;
                // Save to shared menu manager state (unless "All Outfits" which only exists here)
                // When switching to Wardrobe tab, "All Outfits" will fall back to current season
                menuManager.selectedCategory = newCategory.name;
                // Filter the list of favorite outfits to the correct category and sort it
                FilterOutfitSlotsByCategoryAndSort(outfitSlots);
                // Update the outfit buttons with the new slots
                UpdateOutfitButtonsAndSlots();
                Game1.playSound("smallSelect");
            }
        }
        
        // ** CONTROLS **

        // Default Snap
        public override void snapToDefaultClickableComponent()
        {
            setCurrentlySnappedComponentTo(6000); 
            snapCursorToCurrentSnappedComponent();
        }

        // Custom Snap Behavior
        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            ClickableComponent cmp = getCurrentlySnappedComponent();
            switch (cmp.region)
            {
                case OUTFITS:
                    // Move the visible outfit indexes if this isn't an automatic snap move
                    if (direction == 0) { upArrowPressed(); }
                    if (direction == 2) 
                    {
                        downArrowPressed();
                        // If we land on an invalid slot, shift to the first valid one in the row
                        if (!cmp.visible)
                        {
                            for (int i = 7; i > -1; i--) 
                            {
                                if (outfitButtons[i].visible)
                                {
                                    setCurrentlySnappedComponentTo(outfitButtons[i].myID);
                                    break;
                                }
                            }
                        }
                    }
                    snapCursorToCurrentSnappedComponent();
                    break;

                default:
                    break;
            }
        }

        // Key press
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
        }

        // Game pad buttons 
        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b == Buttons.RightShoulder)
            {
                rotationClick(-1);
            }
            else if (b == Buttons.LeftShoulder)
            {
                rotationClick(1);
            }
            if (b == Buttons.A)
            {
                ClickableComponent cmp = getCurrentlySnappedComponent();
                // A on outfits region moves you to the confirm button
                if (cmp.region == OUTFITS && outfitSlotSelected == outfitSlotsFiltered[cmp.myID - 2000 + currentOutfitIndex])
                {
                    setCurrentlySnappedComponentTo(9999);
                    snapCursorToCurrentSnappedComponent();
                }
            }
        }

        // Left click action
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // Rotation Buttons
            if (leftRotationButton.containsPoint(x, y))
            {
                rotationClick(-1);
                if (leftRotationButton.scale != 0f)
                {
                    leftRotationButton.scale -= 0.25f;
                    leftRotationButton.scale = Math.Max(0.75f, leftRotationButton.scale);
                }
            }
            if (rightRotationButton.containsPoint(x, y))
            {
                rotationClick(1);
                if (rightRotationButton.scale != 0f)
                {
                    rightRotationButton.scale -= 0.25f;
                    rightRotationButton.scale = Math.Max(0.75f, rightRotationButton.scale);
                }
            }

            // Scrollbar & Navigation Arrows 
            if (downArrow.containsPoint(x, y))
            {
                downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (upArrow.containsPoint(x, y))
            {
                upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (scrollBar.containsPoint(x, y))
            {
                scrolling = true;
            }
                // note: I don't know how this works with scrolling
            else if (!this.downArrow.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }

            // Outfit Slot Buttons
            for (int i = 0; i < 8; i++)
            {
                // Set this outfit to the selected outfit (with bounds check for safety)
                if (outfitButtons[i].containsPoint(x, y) && currentOutfitIndex + i < outfitSlotsFiltered.Count)
                {
                    outfitSlotsFiltered[currentOutfitIndex + i].Select();
                }
            }

            // Category buttons
            foreach (ClickableComponent c in categoryButtons)
            {
                if (c.containsPoint(x, y)) { ChangeCategory(c); }
            }

            // Outfit Customization
            
            // OK button
            if (okButton.containsPoint(x, y))
            {
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
                // Equip the outfit if not null and close the menu
                if (outfitSlotSelected != null)
                {
                    outfitSlotSelected.modelOutfit.equipFavoriteOutfit(this, dresserObject, Game1.player, outfitSlotSelected.outfitAvailabileItems);
                }
                StardewOutfitManager.playerManager.cleanMenuExit();
            }

            // Menu tab switching
            menuManager.handleTopBarLeftClick(x, y);
        }

        // Handle on-hover and resetting button states
        public override void performHoverAction(int x, int y)
        {
            hoverText = "";
            outfitSlotHovered = null;

            // Check outfit slot hover
            for (int i = 0; i < 8; i++)
            {
                if (outfitButtons[i].containsPoint(x, y) && currentOutfitIndex + i < outfitSlotsFiltered.Count)
                {
                    outfitSlotHovered = outfitSlotsFiltered[currentOutfitIndex + i];
                    break;
                }
            }

            // Rotation buttons
            if (leftRotationButton.containsPoint(x, y))
            {
                leftRotationButton.scale = Math.Min(leftRotationButton.scale + 0.02f, leftRotationButton.baseScale + 0.1f);
            }
            else
            {
                leftRotationButton.scale = Math.Max(leftRotationButton.scale - 0.02f, leftRotationButton.baseScale);
            }
            if (rightRotationButton.containsPoint(x, y))
            {
                rightRotationButton.scale = Math.Min(rightRotationButton.scale + 0.02f, rightRotationButton.baseScale + 0.1f);
            }
            else
            {
                rightRotationButton.scale = Math.Max(rightRotationButton.scale - 0.02f, rightRotationButton.baseScale);
            }
            
            // Category buttons
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
            
            // Other UI buttons
            if (okButton.containsPoint(x, y))
            {
                okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
            }
            else
            {
                okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
            }

            // Equipment icon hover - track hovered item for standard tooltip display
            hoveredItem = null;
            foreach (ClickableComponent c in equipmentIcons)
            {
                if (c.containsPoint(x, y))
                {
                    hoveredItem = GetEquipmentSlotItem(c);
                    if (hoveredItem == null)
                    {
                        hoverText = $"Empty {c.name} Slot";
                    }
                    break;
                }
            }

            menuManager.handleTopBarOnHover(x, y, ref hoverText);
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

            // Reposition portrait and farmer display
            _portraitBox = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, yPositionOnScreen + 64, 256, 384);
            leftRotationButton.bounds = new Rectangle(_portraitBox.X - 42, _portraitBox.Bottom - 24, 60, 60);
            rightRotationButton.bounds = new Rectangle(_portraitBox.X + 256 - 38, _portraitBox.Bottom - 24, 60, 60);

            // Reposition equipment icons
            int eqIconXOffset = _portraitBox.X + _portraitBox.Width / 2 - 81 - 16;
            int eqIconYOffset = _portraitBox.Y + _portraitBox.Height + 32;
            equipmentIcons[0].bounds = new Rectangle(eqIconXOffset, eqIconYOffset, 64, 64);           // Hat
            equipmentIcons[1].bounds = new Rectangle(eqIconXOffset + 64, eqIconYOffset, 64, 64);      // Shirt
            equipmentIcons[2].bounds = new Rectangle(eqIconXOffset + 64, eqIconYOffset + 64, 64, 64); // Pants
            equipmentIcons[3].bounds = new Rectangle(eqIconXOffset, eqIconYOffset + 64, 64, 64);      // Boots
            equipmentIcons[4].bounds = new Rectangle(eqIconXOffset + 128, eqIconYOffset, 64, 64);     // Left Ring
            equipmentIcons[5].bounds = new Rectangle(eqIconXOffset + 128, eqIconYOffset + 64, 64, 64);// Right Ring

            // Reposition OK button
            okButton.bounds = new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 28, 64, 64);

            // Reposition outfit box and navigation
            outfitBox = new Rectangle(xPositionOnScreen + borderWidth + spaceToClearSideBorder + 342, yPositionOnScreen + 120, 592, 436);
            upArrow.bounds = new Rectangle(outfitBox.X + outfitBox.Width + 18, outfitBox.Y + 16, 44, 48);
            downArrow.bounds = new Rectangle(outfitBox.X + outfitBox.Width + 18, outfitBox.Y + outfitBox.Height - 56, 44, 48);
            scrollBar.bounds = new Rectangle(upArrow.bounds.X + 8, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, outfitBox.Height - 64 - upArrow.bounds.Height - 20);

            // Reposition category buttons
            int catYoffset = 78;
            categoryButtons[0].bounds = new Rectangle(outfitBox.X, outfitBox.Y - catYoffset, 88, 72);
            categoryButtons[1].bounds = new Rectangle(outfitBox.X + 90, outfitBox.Y - catYoffset, 88, 72);
            categoryButtons[2].bounds = new Rectangle(outfitBox.X + 180, outfitBox.Y - catYoffset, 88, 72);
            categoryButtons[3].bounds = new Rectangle(outfitBox.X + 270, outfitBox.Y - catYoffset, 88, 72);
            categoryButtons[4].bounds = new Rectangle(outfitBox.X + 360, outfitBox.Y - catYoffset, 88, 72);
            categoryButtons[5].bounds = new Rectangle(outfitBox.X + 450, outfitBox.Y - catYoffset, 88, 72);

            // Reposition outfit buttons (2 rows x 4 columns)
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    int btnIndex = i + (j * 4);
                    outfitButtons[btnIndex].bounds = new Rectangle(outfitBox.X + 22 + (i * 128) + (i * 12), outfitBox.Y + 20 + (j * 192) + (j * 12), 128, 192);
                }
            }

            // Update outfit slot positions to match buttons
            UpdateOutfitButtonsAndSlots();
            setScrollBarToCurrentIndex();

            // Reposition top tabs
            menuManager.repositionTopTabButtons(this);
        }

        // Handle rotation actions
        private void rotationClick(int change)
        {
            int newDirection = (_displayFarmer.FacingDirection - change + 4) % 4;
            _displayFarmer.faceDirection(newDirection);
            menuManager.farmerFacingDirection = newDirection;
            _displayFarmer.FarmerSprite.StopAnimation();
            _displayFarmer.completelyStopAnimatingOrDoingAction();
            Game1.playSound("stoneStep");
        }
        
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                // Clamp scrollbar to runner bounds
                int minY = scrollBarRunner.Y;
                int maxY = scrollBarRunner.Y + scrollBarRunner.Height - scrollBar.bounds.Height;
                scrollBar.bounds.Y = Math.Min(maxY, Math.Max(minY, y - scrollBar.bounds.Height / 2));

                // Calculate percentage and update current index
                int scrollRange = scrollBarRunner.Height - scrollBar.bounds.Height;
                if (scrollRange > 0)
                {
                    float percentage = (float)(scrollBar.bounds.Y - scrollBarRunner.Y) / scrollRange;
                    int totalRows = (int)Math.Ceiling((float)outfitSlotsFiltered.Count / OUTFITS_PER_ROW);
                    int scrollableRows = Math.Max(0, totalRows - VISIBLE_ROWS);
                    int newIndex = (int)(scrollableRows * percentage) * OUTFITS_PER_ROW;
                    int maxIndex = Math.Max(0, outfitSlotsFiltered.Count - (OUTFITS_PER_ROW * VISIBLE_ROWS));
                    newIndex = Math.Max(0, Math.Min(newIndex, maxIndex));

                    if (newIndex != currentOutfitIndex)
                    {
                        currentOutfitIndex = newIndex;
                        UpdateOutfitButtonsAndSlots();
                    }
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            int visibleSlots = OUTFITS_PER_ROW * VISIBLE_ROWS;
            if (outfitSlotsFiltered.Count > visibleSlots)
            {
                if (direction > 0 && currentOutfitIndex > 0)
                {
                    this.upArrowPressed();
                    Game1.playSound("shiny4");
                }
                else if (direction < 0 && currentOutfitIndex + OUTFITS_PER_ROW < outfitSlotsFiltered.Count - OUTFITS_PER_ROW)
                {
                    this.downArrowPressed();
                    Game1.playSound("shiny4");
                }
            }
        }

        private void downArrowPressed()
        {
            downArrow.scale = downArrow.baseScale;
            if (currentOutfitIndex + OUTFITS_PER_ROW < outfitSlotsFiltered.Count - OUTFITS_PER_ROW)
            {
                currentOutfitIndex += OUTFITS_PER_ROW;
            }
            setScrollBarToCurrentIndex();
            UpdateOutfitButtonsAndSlots();
        }

        private void upArrowPressed()
        {
            upArrow.scale = upArrow.baseScale;
            if (currentOutfitIndex > 0)
            {
                currentOutfitIndex -= OUTFITS_PER_ROW;
            }
            setScrollBarToCurrentIndex();
            UpdateOutfitButtonsAndSlots();
        }

        private void setScrollBarToCurrentIndex()
        {
            int totalRows = (int)Math.Ceiling((float)outfitSlotsFiltered.Count / OUTFITS_PER_ROW);
            int scrollableRows = Math.Max(1, totalRows - VISIBLE_ROWS);
            int currentRow = currentOutfitIndex / OUTFITS_PER_ROW;
            float percentage = Math.Min(1f, (float)currentRow / scrollableRows);
            int scrollRange = scrollBarRunner.Height - scrollBar.bounds.Height;
            scrollBar.bounds.Y = scrollBarRunner.Y + (int)(scrollRange * percentage);
            // Clamp to valid bounds
            scrollBar.bounds.Y = Math.Min(scrollBarRunner.Y + scrollRange, Math.Max(scrollBarRunner.Y, scrollBar.bounds.Y));
        }

        // Get the item in an equipment slot (for mouse and gamepad hover)
        private Item GetEquipmentSlotItem(ClickableComponent c)
        {
            if (outfitSlotSelected != null)
            {
                // Get item from selected outfit
                string slotKey = GetSlotKey(c.name);
                if (slotKey != null && outfitSlotSelected.outfitAvailabileItems.TryGetValue(slotKey, out Item item))
                    return item;
                return null;
            }
            else
            {
                // Get player's currently equipped item
                return c.name switch
                {
                    "Hat" => Game1.player.hat.Value,
                    "Shirt" => Game1.player.shirtItem.Value,
                    "Pants" => Game1.player.pantsItem.Value,
                    "Boots" => Game1.player.boots.Value,
                    "Left Ring" => Game1.player.leftRing.Value,
                    "Right Ring" => Game1.player.rightRing.Value,
                    _ => null
                };
            }
        }

        // Draw outfit hover infobox with name and item grid
        private void DrawOutfitHoverInfobox(SpriteBatch b, OutfitSlot slot)
        {
            if (slot == null) return;

            // Infobox dimensions - use 64x64 slots like WardrobeMenu for proper item alignment
            int itemSize = 64;
            int padding = 16;
            int gridSpacing = 0;
            int gridWidth = 3 * itemSize + 2 * gridSpacing;
            int boxWidth = gridWidth + padding * 2;
            int boxHeight = padding + 32 + 2 * itemSize + gridSpacing + padding;

            // Position to the right of the hovered slot, or left if it would clip
            int boxX = slot.bgBox.Right + 8;
            int boxY = slot.bgBox.Y;

            // Check if it would clip off the right edge of the menu
            if (boxX + boxWidth > xPositionOnScreen + width - borderWidth)
            {
                boxX = slot.bgBox.Left - boxWidth - 8;
            }

            // Check if it would clip off the bottom
            if (boxY + boxHeight > yPositionOnScreen + height - borderWidth)
            {
                boxY = yPositionOnScreen + height - borderWidth - boxHeight;
            }

            // Draw background box
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), boxX, boxY, boxWidth, boxHeight, Color.White, 1f, false);

            // Draw outfit name
            string displayName = slot.outfitName;
            Vector2 nameSize = Game1.smallFont.MeasureString(displayName);
            float nameScale = Math.Min(1f, (boxWidth - padding * 2) / nameSize.X);
            Utility.drawTextWithShadow(b, displayName, Game1.smallFont, new Vector2(boxX + padding, boxY + padding), Game1.textColor, nameScale);

            // Draw 2x3 item grid below the name
            int gridX = boxX + padding;
            int gridY = boxY + padding + 32;

            // Slot keys in order: Hat, Shirt, Left Ring (top row), Boots, Pants, Right Ring (bottom row)
            string[] slotKeys = { "Hat", "Shirt", "LeftRing", "Shoes", "Pants", "RightRing" };
            int[] emptyIcons = { EMPTY_HAT_ICON, EMPTY_SHIRT_ICON, EMPTY_RING_ICON, EMPTY_BOOTS_ICON, EMPTY_PANTS_ICON, EMPTY_RING_ICON };

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int row = i / 3;
                int itemX = gridX + col * (itemSize + gridSpacing);
                int itemY = gridY + row * (itemSize + gridSpacing);

                Rectangle itemBounds = new Rectangle(itemX, itemY, itemSize, itemSize);
                slot.outfitAvailabileItems.TryGetValue(slotKeys[i], out Item slotItem);

                if (slotItem != null)
                {
                    b.Draw(Game1.menuTexture, itemBounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, FILLED_SLOT_ICON), Color.White);
                    slotItem.drawInMenu(b, new Vector2(itemX, itemY), 1f, 1f, 0.866f, StackDrawType.Hide);
                }
                else
                {
                    b.Draw(Game1.menuTexture, itemBounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, emptyIcons[i]), Color.White * 0.5f);
                }
            }
        }

        // ** DRAW **
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

            // Draw rotation buttons
            leftRotationButton.draw(b);
            rightRotationButton.draw(b);

            // Draw equipment icons - show selected outfit items, or current equipment if no outfit selected
            foreach (ClickableComponent c in equipmentIcons)
            {
                Item slotItem = GetEquipmentSlotItem(c);
                int emptySlotIcon = GetEmptySlotIcon(c.name);

                if (slotItem != null)
                {
                    b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, FILLED_SLOT_ICON), Color.White);
                    slotItem.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, 1f, 0.866f, StackDrawType.Hide);
                }
                else
                {
                    b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, emptySlotIcon), Color.White);
                }
            }

            // Draw category buttons
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
            
            // Draw other UI buttons
            okButton.draw(b);

            // Draw Outfit Display Slots (only draw 8 or fewer based on current index)
            foreach (OutfitSlot slot in outfitSlotsFiltered)
            {
                if (slot.isVisible) { slot.Draw(b); }
            }

            // Draw outfit hover infobox (for mouse hover or gamepad snap)
            OutfitSlot hoverSlot = outfitSlotHovered;
            if (hoverSlot == null && Game1.options.gamepadControls)
            {
                ClickableComponent snapped = getCurrentlySnappedComponent();
                if (snapped?.region == OUTFITS && currentOutfitIndex + snapped.myID - 2000 < outfitSlotsFiltered.Count)
                {
                    hoverSlot = outfitSlotsFiltered[currentOutfitIndex + snapped.myID - 2000];
                }
            }
            DrawOutfitHoverInfobox(b, hoverSlot);

            // Draw navigation (only if scrolling is available)
            if (scrollBar.visible)
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

            // If using gamepad and snapped to equipment icon, get the hovered item
            if (Game1.options.gamepadControls && hoveredItem == null && string.IsNullOrEmpty(hoverText))
            {
                ClickableComponent snapped = getCurrentlySnappedComponent();
                if (snapped?.region == EQUIPMENT)
                {
                    hoveredItem = GetEquipmentSlotItem(snapped);
                    if (hoveredItem == null)
                    {
                        hoverText = $"Empty {snapped.name} Slot";
                    }
                }
            }

            // Draw hover text or item tooltip
            if (hoveredItem != null)
            {
                IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
            }
            else if (!hoverText.Equals(""))
            {
                drawHoverText(b, hoverText, Game1.smallFont);
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}
