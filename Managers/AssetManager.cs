using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace StardewOutfitManager.Managers
{
    internal class AssetManager
    {
        internal string assetFolderPath;

        // UI textures
        internal readonly Texture2D wardrobeTabTexture;
        internal readonly Texture2D favoritesTabTexture;
        internal readonly Texture2D dresserTabTexture;


        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Assets")).Name;

            // Load in the assets
            wardrobeTabTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "wardrobeTab.png"));
            favoritesTabTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "favoritesTab.png"));
            dresserTabTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "dresserTab.png"));
        }
    }
}
