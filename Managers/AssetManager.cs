using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using System.Collections.Generic;
using System.IO;

namespace StardewOutfitManager.Managers
{
    public class AssetManager
    {
        private readonly IModHelper helper;
        internal string assetFolderPath;

        // UI textures
        public readonly Texture2D customSprites;

        // Hair Names
        internal readonly IDictionary<string, string> hairJSON;

        // Accessory Names
        internal readonly IDictionary<string, string> accessoryJSON;

        public AssetManager(IModHelper helper)
        {
            this.helper = helper;

            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Assets")).Name;

            // Load in custom UI image assets
            customSprites = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/customSpriteSheet.png"));

            // Load in JSON hair and accessory names for the base game
            hairJSON = helper.ModContent.Load<Dictionary<string, string>>(Path.Combine(assetFolderPath, "Hair/BaseGame/HairNames.json"));
            accessoryJSON = helper.ModContent.Load<Dictionary<string, string>>(Path.Combine(assetFolderPath, "Accessories/BaseGame/AccNames.json"));
        }

        /// <summary>
        /// Handle content API requests for custom furniture data and textures.
        /// </summary>
        public void HandleAssetRequested(AssetRequestedEventArgs e)
        {
            // Patch Data/Furniture to add custom dresser entries
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    // Mirror Dressers (7 variants) - 1x2 tile vanity style
                    // Pricing: Standard woods 7000g, Modern 8000g, Gold 9500g
                    // off_limits_for_random_sale controlled by TravelingMerchantSellsDressers config
                    string mirrorRandomSale = StardewOutfitManager.Config.TravelingMerchantSellsDressers ? "false" : "true";
                    data["LiminalWarmth.StardewOutfitManager_BirchMirrorDresser"] =
                        $"Birch Mirror Dresser/dresser/1 2/1 1/1/7000/0/Birch Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\BirchMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_GoldMirrorDresser"] =
                        $"Gold Mirror Dresser/dresser/1 2/1 1/1/9500/0/Gold Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\GoldMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_MahoganyMirrorDresser"] =
                        $"Mahogany Mirror Dresser/dresser/1 2/1 1/1/7000/0/Mahogany Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\MahoganyMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_ModernMirrorDresser"] =
                        $"Modern Mirror Dresser/dresser/1 2/1 1/1/8000/0/Modern Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\ModernMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_OakMirrorDresser"] =
                        $"Oak Mirror Dresser/dresser/1 2/1 1/1/7000/0/Oak Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\OakMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_WalnutMirrorDresser"] =
                        $"Walnut Mirror Dresser/dresser/1 2/1 1/1/7000/0/Walnut Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\WalnutMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_WhiteMirrorDresser"] =
                        $"White Mirror Dresser/dresser/1 2/1 1/1/7000/0/White Mirror Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\WhiteMirrorDresser/{mirrorRandomSale}/furniture_type_dresser";

                    // Small Dressers (7 variants) - 1x2 tile bedside table style
                    // Pricing: Standard dresser price - 2000g (Standard 3000g, Modern 4000g, Gold 5500g)
                    data["LiminalWarmth.StardewOutfitManager_SmallBirchDresser"] =
                        "Small Birch Dresser/dresser/1 2/1 1/1/3000/0/Small Birch Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallBirchDresser/true/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_SmallGoldDresser"] =
                        "Small Gold Dresser/dresser/1 2/1 1/1/5500/0/Small Gold Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallGoldDresser/true/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_SmallMahoganyDresser"] =
                        "Small Mahogany Dresser/dresser/1 2/1 1/1/3000/0/Small Mahogany Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallMahoganyDresser/true/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_SmallModernDresser"] =
                        "Small Modern Dresser/dresser/1 2/1 1/1/4000/0/Small Modern Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallModernDresser/true/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_SmallOakDresser"] =
                        "Small Oak Dresser/dresser/1 2/1 1/1/3000/0/Small Oak Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallOakDresser/true/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_SmallWalnutDresser"] =
                        "Small Walnut Dresser/dresser/1 2/1 1/1/3000/0/Small Walnut Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallWalnutDresser/true/furniture_type_dresser";
                    data["LiminalWarmth.StardewOutfitManager_SmallWhiteDresser"] =
                        "Small White Dresser/dresser/1 2/1 1/1/3000/0/Small White Dresser/0/LiminalWarmth.StardewOutfitManager\\Furniture\\SmallWhiteDresser/true/furniture_type_dresser";
                });
            }

            // Load custom furniture textures
            if (e.NameWithoutLocale.StartsWith("LiminalWarmth.StardewOutfitManager/Furniture/"))
            {
                string textureName = e.NameWithoutLocale.BaseName.Replace("LiminalWarmth.StardewOutfitManager/Furniture/", "");
                string texturePath = GetFurnitureTexturePath(textureName);

                if (texturePath != null)
                {
                    e.LoadFromModFile<Texture2D>(texturePath, AssetLoadPriority.Medium);
                }
            }

            // Patch Data/Shops to add custom dressers to Robin's Carpenter shop (if enabled)
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops") && StardewOutfitManager.Config.RobinSellsDressers)
            {
                e.Edit(asset =>
                {
                    var shops = asset.AsDictionary<string, ShopData>().Data;

                    if (shops.TryGetValue("Carpenter", out var carpenterShop))
                    {
                        // Add Mirror Dressers to Robin's rotation (spread across Mon/Tue/Wed)
                        // Monday: Oak, Birch, Gold Mirror Dressers
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_OakMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_OakMirrorDresser",
                            Price = 7000,
                            Condition = "DAY_OF_WEEK Monday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_BirchMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_BirchMirrorDresser",
                            Price = 7000,
                            Condition = "DAY_OF_WEEK Monday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_GoldMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_GoldMirrorDresser",
                            Price = 9500,
                            Condition = "DAY_OF_WEEK Monday"
                        });

                        // Tuesday: Walnut, Mahogany Mirror Dressers
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_WalnutMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_WalnutMirrorDresser",
                            Price = 7000,
                            Condition = "DAY_OF_WEEK Tuesday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_MahoganyMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_MahoganyMirrorDresser",
                            Price = 7000,
                            Condition = "DAY_OF_WEEK Tuesday"
                        });

                        // Wednesday: Modern, White Mirror Dressers
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_ModernMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_ModernMirrorDresser",
                            Price = 8000,
                            Condition = "DAY_OF_WEEK Wednesday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_WhiteMirrorDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_WhiteMirrorDresser",
                            Price = 7000,
                            Condition = "DAY_OF_WEEK Wednesday"
                        });

                        // Add Small Dressers to Robin's rotation (spread across Thu/Fri/Sat)
                        // Thursday: Oak, Birch, Gold Small Dressers
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallOakDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallOakDresser",
                            Price = 3000,
                            Condition = "DAY_OF_WEEK Thursday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallBirchDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallBirchDresser",
                            Price = 3000,
                            Condition = "DAY_OF_WEEK Thursday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallGoldDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallGoldDresser",
                            Price = 5500,
                            Condition = "DAY_OF_WEEK Thursday"
                        });

                        // Friday: Walnut, Mahogany Small Dressers
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallWalnutDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallWalnutDresser",
                            Price = 3000,
                            Condition = "DAY_OF_WEEK Friday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallMahoganyDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallMahoganyDresser",
                            Price = 3000,
                            Condition = "DAY_OF_WEEK Friday"
                        });

                        // Saturday: Modern, White Small Dressers
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallModernDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallModernDresser",
                            Price = 4000,
                            Condition = "DAY_OF_WEEK Saturday"
                        });
                        carpenterShop.Items.Add(new ShopItemData
                        {
                            Id = "LiminalWarmth.StardewOutfitManager_SmallWhiteDresser_Shop",
                            ItemId = "(F)LiminalWarmth.StardewOutfitManager_SmallWhiteDresser",
                            Price = 3000,
                            Condition = "DAY_OF_WEEK Saturday"
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Maps texture names to their file paths in the Assets folder.
        /// </summary>
        private string GetFurnitureTexturePath(string textureName)
        {
            return textureName switch
            {
                // Mirror Dressers
                "BirchMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/birch.png",
                "GoldMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/gold.png",
                "MahoganyMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/mahogany.png",
                "ModernMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/modern.png",
                "OakMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/oak.png",
                "WalnutMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/walnut.png",
                "WhiteMirrorDresser" => "Assets/Objects/Furniture/Mirror Dresser/white.png",
                // Small Dressers
                "SmallBirchDresser" => "Assets/Objects/Furniture/Small Dresser/birch.png",
                "SmallGoldDresser" => "Assets/Objects/Furniture/Small Dresser/gold.png",
                "SmallMahoganyDresser" => "Assets/Objects/Furniture/Small Dresser/mahogany.png",
                "SmallModernDresser" => "Assets/Objects/Furniture/Small Dresser/modern.png",
                "SmallOakDresser" => "Assets/Objects/Furniture/Small Dresser/oak.png",
                "SmallWalnutDresser" => "Assets/Objects/Furniture/Small Dresser/walnut.png",
                "SmallWhiteDresser" => "Assets/Objects/Furniture/Small Dresser/white.png",
                _ => null
            };
        }
    }
}
