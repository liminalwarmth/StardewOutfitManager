using Microsoft.Xna.Framework.Graphics;
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
            wardrobeBackgroundTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI/wardrobeBG.png"));

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
