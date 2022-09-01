using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        // Create a new favorite outfit in the data model
        public void SaveNewOutfit(Farmer player, string category, string name)
        {
            Favorites.Add(new FavoriteOutfit(player, category, name));
        }

        // Given an outfit, delete it from the data model
        public void DeleteOutfit(FavoriteOutfit outfit)
        {
            Favorites.Remove(outfit);
        }

        // Sort a given list of outfits by different criteria and return sorted list
        public List<FavoriteOutfit> SortOutfitList(List<FavoriteOutfit> list, string sortType)
        {   
            List<FavoriteOutfit> sorted = new List<FavoriteOutfit>();
            foreach (FavoriteOutfit outfit in list)
            {
                switch (sortType)
                {
                    // Sort by when the player last wore the outfit
                    case "lastWorn":
                        // some sorting code to compare wear dates
                            // insert
                        break;

                    // Sort by the outfit's primary category
                    case "category":
                        // some sorting code to compare categories
                            // insert
                        break;
                }
            }
            return sorted;
        }

        // Filter a list of outfits by different criteria and return filtered list
        public List<FavoriteOutfit> FilterOutfitList(List<FavoriteOutfit> list, string filterType, string categoryType = null)
        {
            List<FavoriteOutfit> filtered = new List<FavoriteOutfit>();
            foreach (FavoriteOutfit outfit in list)
            {
                switch (filterType)
                {
                    case "category":
                        if (categoryType == outfit.Category) { filtered.Add(outfit); }
                        break;

                    case "favorites":
                        if (outfit.isFavorite) { filtered.Add(outfit); }
                        break;

                    case "notFavorites":
                        if (!outfit.isFavorite) { filtered.Add(outfit); }
                        break;
                }
            }
            return filtered;
        }
    }

    // Defines the data structure for a favorite outfit that the player has stored
    public class FavoriteOutfit
    {
        // The favorite outfit itself can be a favorite within a list of favorites for positioning at the top of the list
        public bool isFavorite { get; set; }
       
        // The name of the outfit (defaults to "Category #" and then sticks)
        public string Name { get; set; }
       
        // The Category the outfit applies to ("Spring"/"Summer"/"Fall"/"Winter"/"Special1"/"Special2")
        public string Category { get; set; }

        // The in-game item references the outfit contains
        public Item Hat { get; set; }
        public Item Shirt { get; set; }
        public Item Pants { get; set; }
        public Item Boots { get; set; }
        
        // Rings included in case I decide to allow setting rings as part of a loadout
        public Item LeftRing { get; set; }
        public Item RightRing { get; set; }
        
        // The index and index position of the hair (index will come into play if there are multiple reference indexes)
        public string HairIndex { get; set; }
        public int Hair { get; set; }
       
        // The index and index position of the accessory (index will come into play if there are multiple reference indexes)
        public string AccessoryIndex { get; set; }
        public int Accessory { get; set; }

        // The time/day the player last equipped this outfit (for sorting by recency/usage)
        public int LastWorn { get; set; }

        // Define constructor for building a new favorite outfit object
        public FavoriteOutfit(Farmer player, string category, string name)
        {
            // Default to not a favorite favorite outfit
            isFavorite = false;

            // Name and category strings
            Name = name;
            Category = category;

            // Set the values for this outfit loadout from the given player
            Hat = player.hat.Value;
            Shirt = player.shirtItem.Value;
            Pants = player.pantsItem.Value;
            Boots = player.boots.Value;
            LeftRing = player.leftRing.Value;
            RightRing = player.rightRing.Value;

            // TODO: In addition to storing the values I need to figure out how to store reference indexes for Hair and Accessory if not base
            Hair = player.hair.Value;
            HairIndex = "";
            Accessory = player.accessory.Value;
            AccessoryIndex = "";

            // Set last worn to now | TODO: update this when I know how I'm gonna track this
            LastWorn = 0;
        }

        // Given a list of possible items to check against, returns true if all pieces of this outfit are present
        public bool isAvailable(List<Item> playerOwnedItems)
        {   
            // Check all items on the needed item list against the list of possible items the player could wear
            foreach (Item item in getNeededItemsList())
            {
                // Only check for slots that have an item stored in that slot
                if (item != null)
                {
                    // If they're missing an item from this outfit, the ensemble is unavailable
                    if (!playerOwnedItems.Contains(item))
                    {
                        // TODO: Need special handling for rings (there can be two of the same ring!)
                        return false;
                    }
                }
            }

            // TODO: Need to add checks for hair and Accessory validity when/if we're using custom hair and accessory indexes

            // If all items in the outfit were found, return true
            return true;
        }

        // Given a list of possible items to wear, returns a list of needed items for this outfit and whether they're available to equip
        public Dictionary<Item, bool> getItemsAndAvailability(List<Item> playerOwnedItems)
        {
            Dictionary<Item, bool> outfitItemDict = new();

            // Check all items on the needed item list against the list of possible items the player could wear
            foreach (Item item in getNeededItemsList())
            {
                // Only check for slots that have an item stored in that slot
                if (item != null)
                {
                    // If they're missing an item from this outfit, mark it as unavailable
                    if (!playerOwnedItems.Contains(item))
                    {
                        // TODO: Need special handling for rings (there can be two of the same ring!)
                        outfitItemDict.Add(item, false);
                    }
                    // Otherwise add it to the list as valid
                    else outfitItemDict.Add(item, true);
                }
            }
            // Return the outfit item and availability dictionary
            return outfitItemDict;
        }

        // Assemble equippable items needed for outfit
        internal List<Item> getNeededItemsList()
        {
            return new List<Item>() { Hat, Shirt, Pants, Boots, LeftRing, RightRing };
        }
    }
}
