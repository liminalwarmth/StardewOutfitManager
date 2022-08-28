﻿using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace StardewOutfitManager.Managers
{
    public class AssetManager
    {
        internal string assetFolderPath;

        // UI textures
        internal readonly Texture2D wardrobeTabTexture;
        internal readonly Texture2D favoritesTabTexture;
        internal readonly Texture2D dresserTabTexture;
        internal readonly Texture2D wardrobeBackgroundTexture;
        internal readonly Texture2D bgTextureSpring;
        internal readonly Texture2D bgTextureSummer;
        internal readonly Texture2D bgTextureFall;
        internal readonly Texture2D bgTextureWinter;

        // Hair Names
        internal readonly IDictionary<string, string> hairJSON;

        // Accessory Names
        internal readonly IDictionary<string, string> accessoryJSON;


        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Assets")).Name;

            // Load in custom UI image assets
            wardrobeTabTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/wardrobeTab.png"));
            favoritesTabTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/favoritesTab.png"));
            dresserTabTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/dresserTab.png"));

            // Backgrounds
            wardrobeBackgroundTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/wardrobeBG.png"));
            bgTextureSpring = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/spring_daybg.png"));
            bgTextureSummer = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/summer_daybg.png"));
            bgTextureFall = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/fall_daybg.png"));
            bgTextureWinter = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/winter_daybg.png"));

            // Load in JSON hair and accessory names for the base game
            hairJSON = helper.ModContent.Load<Dictionary<string, string>>(Path.Combine(assetFolderPath, "Hair/BaseGame/HairNames.json"));
            accessoryJSON = helper.ModContent.Load<Dictionary<string, string>>(Path.Combine(assetFolderPath, "Accessories/BaseGame/AccNames.json"));
        }

        // Load all in-game hair and any content pack hair into a single index
        internal void AssembleHairIndex()
        {
            //
        }
    }
}
