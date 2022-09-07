using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewOutfitManager.Data;

namespace StardewOutfitManager.Managers
{
    public class AssetManager
    {
        internal string assetFolderPath;

        // UI textures
        public readonly Texture2D customSprites;
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
            customSprites = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/customSpriteSheet.png"));

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
