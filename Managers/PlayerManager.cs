using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewOutfitManager.Data;
using StardewModdingAPI.Utilities;
using System.Threading;
using StardewValley.Objects;
using StardewValley.Monsters;

namespace StardewOutfitManager.Managers
{
    // Definition of the class that handles player-specific data and menus per-screen
    public class PlayerManager
    {
        // Each local player can have an active menu manager while navigating menus
        public PerScreen<MenuManager> menuManager = new();
        // Each local player can have a different set of favorites data loaded
        public PerScreen<FavoritesData> favoritesData = new();
        // Remember which tab the player last used (persists across dresser sessions within a play session)
        // Values: 0 = WardrobeMenu, 1 = FavoritesMenu, 2 = NewDresserMenu
        // Note: This resets when the game is closed - intentional as it's just session convenience
        public PerScreen<int> lastUsedTab = new(() => 0);
        // Store the mod helper
        internal IModHelper modHelper;

        public PlayerManager(IModHelper modHelper)
        {
            this.modHelper = modHelper;
        }

        // Clean exit when we want to close the menu for the active player (in this file because it has to check both players)
        public void cleanMenuExit(bool playSound = true)
        {
            if (menuManager.Value.activeManagedMenu != null)
            {
                // Remember which tab was last used for next dresser open
                lastUsedTab.Value = menuManager.Value.currentTab;
                // Unlock the dresser for other players to use
                menuManager.Value.dresserObject.mutex.ReleaseLock();
                // Exit the active menu
                menuManager.Value.activeManagedMenu.exitThisMenu(playSound);
                menuManager.Value = null;
            }
        }

        public void loadFavoritesDataFromFile()
        {
            string filePath = $"favoritesData/{Game1.player.Name}_{Constants.SaveFolderName}_{Game1.player.UniqueMultiplayerID}.json";
            try
            {
                // Try to load existing favorites data
                var loadedData = modHelper.Data.ReadJsonFile<FavoritesData>(filePath);

                // Validate the loaded data - if it's malformed, start fresh
                if (loadedData?.Favorites == null)
                {
                    StardewOutfitManager.ModMonitor.Log($"Favorites data was null or malformed, creating new data for {Game1.player.Name}", LogLevel.Info);
                    favoritesData.Value = new FavoritesData();
                    return;
                }

                // Filter out any malformed outfits (missing Items dictionary)
                int originalCount = loadedData.Favorites.Count;
                loadedData.Favorites.RemoveAll(outfit => outfit?.Items == null);
                int removedCount = originalCount - loadedData.Favorites.Count;

                if (removedCount > 0)
                {
                    StardewOutfitManager.ModMonitor.Log($"Removed {removedCount} malformed outfit(s) from favorites data", LogLevel.Warn);
                }

                favoritesData.Value = loadedData;
            }
            catch (System.Exception ex)
            {
                // If loading fails for any reason, start fresh
                StardewOutfitManager.ModMonitor.Log($"Failed to load favorites data: {ex.Message}. Creating new data.", LogLevel.Warn);
                favoritesData.Value = new FavoritesData();
            }
        }

        public void saveFavoritesDataToFile()
        {
            // Write favorites data model to local JSON save file
            modHelper.Data.WriteJsonFile($"favoritesData/{Game1.player.Name}_{Constants.SaveFolderName}_{Game1.player.UniqueMultiplayerID}.json", favoritesData.Value);
        }
    }
}
