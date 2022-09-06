﻿using StardewModdingAPI;
using System.Collections.Generic;


namespace StardewOutfitManager.Data
{
    // Defines the data structure for a list of favorite outfits for a player
    public class FavoritesData
    {
        public List<FavoriteOutfit> Favorites { get; set; }

        // Construct a new favorite outfits list for the player
        public FavoritesData()
        {
            Favorites = new();
        }
    }

    // Defines the data structure for a favorite outfit that the player has stored
    public class FavoriteOutfit
    {
        // The favorite outfit itself can be a favorite within a list of favorites for positioning at the top of the list
        public bool isFavorite { get; set; } = false;

        // The string name of the outfit
        public string Name { get; set; } = "";

        // The Category the outfit applies to ("Spring"/"Summer"/"Fall"/"Winter"/"Special1"/"Special2")
        public string Category { get; set; } = "";

        public Dictionary<string, string> Items { get; set; } = new Dictionary<string, string>();

        // The index and index position of the hair (index will come into play if there are multiple reference indexes)
        public string HairIndex { get; set; } = "";
        public int Hair { get; set; } = 0;
       
        // The index and index position of the accessory (index will come into play if there are multiple reference indexes)
        public string AccessoryIndex { get; set; } = "";
        public int Accessory { get; set; } = 0;

        // The time/day the player last equipped this outfit (for sorting by recency/usage)
        public int LastWorn { get; set; } = 0;

        public FavoriteOutfit() { }

    }
}
