using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewOutfitManager.Managers;

namespace StardewOutfitManager.Data
{
    public class LocalPlayers
    {
        public Dictionary<Farmer, LocalPlayerContext> Players = new();

        public void AddPlayerContext(Farmer player)
        {
            LocalPlayerContext playerContext = new LocalPlayerContext(player);
            Players.Add(player, playerContext);
        }

        public LocalPlayerContext GetCurrentPlayerContext()
        {
            if (Players[Game1.player] != null) return Players[Game1.player];
            else return null;
        }
    }

    // Defines the per-player data access structure for a given player who loaded a save (this is to manage favorites and menus in local co-op)
    public class LocalPlayerContext
    {
        public FavoritesData FavoriteOutfits { get; set; }
        public MenuManager MenuManager { get; set; }

        // Construct a new local player context to define per-player Favorites and menu managers
        public LocalPlayerContext(Farmer player)
        {
            // Load favorite outfits
        }
    }

}
