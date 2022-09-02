using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;
using StardewOutfitManager.Utils;
using StardewOutfitManager.Managers;
using StardewOutfitManager.Data;
using System.Xml.Linq;

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
        public bool isFavorite { get; set; }

        // The string name of the outfit
        public string Name { get; set; }
       
        // The Category the outfit applies to ("Spring"/"Summer"/"Fall"/"Winter"/"Special1"/"Special2")
        public string Category { get; set; }

        public Dictionary<string, string> Items { get; set; }

        // The index and index position of the hair (index will come into play if there are multiple reference indexes)
        public string HairIndex { get; set; }
        public int Hair { get; set; }
       
        // The index and index position of the accessory (index will come into play if there are multiple reference indexes)
        public string AccessoryIndex { get; set; }
        public int Accessory { get; set; }

        // The time/day the player last equipped this outfit (for sorting by recency/usage)
        public int LastWorn { get; set; }

        // Define constructor for building a new favorite outfit object
        public FavoriteOutfit(Farmer player, string category = null, string name = null)
        {
            // Default to not a favorite favorite outfit
            isFavorite = false;

            // Name & Category
            Name = name;
            Category = category;

            // Set the string outfit tag values for this outfit loadout from the given player
            Items = new Dictionary<string, string>();
            Items.Add("Hat", this.tagItemAsFavorite(player.hat.Value));
            Items.Add("Shirt", this.tagItemAsFavorite(player.shirtItem.Value));
            Items.Add("Pants", this.tagItemAsFavorite(player.pantsItem.Value));
            Items.Add("Shoes", this.tagItemAsFavorite(player.boots.Value));
            Items.Add("LeftRing", this.tagItemAsFavorite(player.leftRing.Value));
            Items.Add("RightRing", this.tagItemAsFavorite(player.rightRing.Value));

            // TODO: In addition to storing the values I need to figure out how to store reference indexes for Hair and Accessory if not base
            Hair = player.hair.Value;
            HairIndex = "";
            Accessory = player.accessory.Value;
            AccessoryIndex = "";

            // Set last worn to now | TODO: update this when I know how I'm gonna track this
            LastWorn = 0;
        }
    }
}
