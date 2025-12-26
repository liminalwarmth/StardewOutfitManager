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
        // Remember which tab the player last used (persists across dresser sessions)
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
            // Check if a local favorites file exists for this player and create a new FavoritesData model to store it does not        
            favoritesData.Value = modHelper.Data.ReadJsonFile<FavoritesData>($"favoritesData/{Game1.player.Name}_{Constants.SaveFolderName}_{Game1.player.UniqueMultiplayerID}.json") ?? new FavoritesData();
        }

        public void saveFavoritesDataToFile()
        {
            // Write favorites data model to local JSON save file
            modHelper.Data.WriteJsonFile($"favoritesData/{Game1.player.Name}_{Constants.SaveFolderName}_{Game1.player.UniqueMultiplayerID}.json", favoritesData.Value);
        }
    }
}
