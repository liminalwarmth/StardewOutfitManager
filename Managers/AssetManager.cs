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
                else
                {
                    StardewOutfitManager.ModMonitor.Log($"Missing texture for furniture: {textureName}", LogLevel.Warn);
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
                        // Robin sells 2 different random dressers per day
                        // Uses SYNCED_CHOICE to pick consistently for all players, changes daily
                        // Robin is closed Tuesday and Sunday, so items only show on open days
                        // Dressers are split into two groups to guarantee variety (no duplicates)

                        string openDays = "!DAY_OF_WEEK Tuesday, !DAY_OF_WEEK Sunday";

                        // Slot A - picks from group 1 (4 mirror + 3 small dressers)
                        // 1=Oak Mirror, 2=Birch Mirror, 3=Walnut Mirror, 4=Mahogany Mirror
                        // 5=Oak Small, 6=Birch Small, 7=Walnut Small
                        AddDresserWithChoice(carpenterShop, "OakMirrorDresser", 7000, openDays, "SOM_DresserA", 1, 7);
                        AddDresserWithChoice(carpenterShop, "BirchMirrorDresser", 7000, openDays, "SOM_DresserA", 2, 7);
                        AddDresserWithChoice(carpenterShop, "WalnutMirrorDresser", 7000, openDays, "SOM_DresserA", 3, 7);
                        AddDresserWithChoice(carpenterShop, "MahoganyMirrorDresser", 7000, openDays, "SOM_DresserA", 4, 7);
                        AddDresserWithChoice(carpenterShop, "SmallOakDresser", 3000, openDays, "SOM_DresserA", 5, 7);
                        AddDresserWithChoice(carpenterShop, "SmallBirchDresser", 3000, openDays, "SOM_DresserA", 6, 7);
                        AddDresserWithChoice(carpenterShop, "SmallWalnutDresser", 3000, openDays, "SOM_DresserA", 7, 7);

                        // Slot B - picks from group 2 (3 mirror + 4 small dressers)
                        // 1=Modern Mirror, 2=White Mirror, 3=Gold Mirror
                        // 4=Mahogany Small, 5=Modern Small, 6=White Small, 7=Gold Small
                        AddDresserWithChoice(carpenterShop, "ModernMirrorDresser", 8000, openDays, "SOM_DresserB", 1, 7, "_B");
                        AddDresserWithChoice(carpenterShop, "WhiteMirrorDresser", 7000, openDays, "SOM_DresserB", 2, 7, "_B");
                        AddDresserWithChoice(carpenterShop, "GoldMirrorDresser", 9500, openDays, "SOM_DresserB", 3, 7, "_B");
                        AddDresserWithChoice(carpenterShop, "SmallMahoganyDresser", 3000, openDays, "SOM_DresserB", 4, 7, "_B");
                        AddDresserWithChoice(carpenterShop, "SmallModernDresser", 4000, openDays, "SOM_DresserB", 5, 7, "_B");
                        AddDresserWithChoice(carpenterShop, "SmallWhiteDresser", 3000, openDays, "SOM_DresserB", 6, 7, "_B");
                        AddDresserWithChoice(carpenterShop, "SmallGoldDresser", 5500, openDays, "SOM_DresserB", 7, 7, "_B");
                    }
                });
            }
        }

        /// <summary>
        /// Helper to add a dresser shop entry with SYNCED_CHOICE condition.
        /// </summary>
        private static void AddDresserWithChoice(ShopData shop, string dresserName, int price, string baseDayCondition, string choiceKey, int choiceValue, int maxChoice, string idSuffix = "")
        {
            shop.Items.Add(new ShopItemData
            {
                Id = $"LiminalWarmth.StardewOutfitManager_{dresserName}_Shop{idSuffix}",
                ItemId = $"(F)LiminalWarmth.StardewOutfitManager_{dresserName}",
                Price = price,
                Condition = $"{baseDayCondition}, SYNCED_CHOICE day {choiceKey} 1 {maxChoice} {choiceValue}"
            });
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
