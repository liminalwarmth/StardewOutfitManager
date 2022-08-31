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

namespace StardewOutfitManager.Managers
{
    // Definition of the class that handles player-specific data and menus per-screen
    public class PlayerManager
    {
        // Each local player can have an active menu manager while navigating menus
        public PerScreen<MenuManager> menuManager = new();
        // Each local player can have a different set of favorites data loaded
        public PerScreen<FavoritesData> favoritesData = new();

        // Clean exit when we want to close the menu for the active player
        public void cleanMenuExit(bool playSound = true)
        {
            if (menuManager.Value.activeManagedMenu != null)
            {
                // Unlock the dresser for other players to use
                menuManager.Value.dresserObject.mutex.ReleaseLock();
                // Exit the active menu
                menuManager.Value.activeManagedMenu.exitThisMenu(playSound);
                menuManager.Value = null;
            }
        }
    }
}
