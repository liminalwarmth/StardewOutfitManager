using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;
using StardewOutfitManager.Utils;
using StardewOutfitManager.Managers;
using StardewOutfitManager.Data;
using System.Xml.Linq;

namespace StardewOutfitManager.Utils
{
    // Extension methods to FavoritesData data model for player favorites management
    public static class FavoritesDataMethods
    {
        // Create a new favorite outfit in the data model--returns false if this outfit already exists
        public static bool SaveNewOutfit(this FavoritesData f, Farmer player, string category, string name)
        {
            FavoriteOutfit outfit = new FavoriteOutfit(player, category, name);
            if (!outfitExistsInFavorites(f, outfit)) {
                f.Favorites.Add(outfit);
                return true;
            }
            else return false;
        }

        // Given an outfit, delete it from the data model--returns true if successful
        public static bool DeleteOutfit(this FavoritesData f, FavoriteOutfit outfit)
        {
            if (outfitExistsInFavorites(f, outfit, true))
            {
                f.Favorites.Remove(outfit);
                return true;
            }
            else return false;
        }

        // Check if the Favorites model contains a given FavoriteOutfit, looking only at the category, hair, accessory, and equipment slots (name optional for delete)
        internal static bool outfitExistsInFavorites(this FavoritesData f, FavoriteOutfit outfit, bool checkName = false)
        {
            foreach (FavoriteOutfit favorite in f.Favorites)
            {
                if (outfit.Items["Hat"] == favorite.Items["Hat"] &&
                    outfit.Items["Shirt"] == favorite.Items["Shirt"] &&
                    outfit.Items["Pants"] == favorite.Items["Pants"] &&
                    outfit.Items["Shoes"] == favorite.Items["Shoes"] &&
                    outfit.Items["LeftRing"] == favorite.Items["LeftRing"] &&
                    outfit.Items["RightRing"] == favorite.Items["RightRing"] &&
                    outfit.Hair == favorite.Hair &&
                    outfit.Accessory == favorite.Accessory &&
                    outfit.Category == favorite.Category)
                {
                    if (checkName == true)
                    {
                        if (!outfit.Name.Equals(favorite.Name)) { return false; }
                    }
                    return true;
                }
            }
            return false;
        }

        // Sort a given list of outfits by different criteria and return sorted list
        public static List<FavoriteOutfit> SortOutfitList(this FavoritesData f, List<FavoriteOutfit> list, string sortType)
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
        public static List<FavoriteOutfit> FilterOutfitList(this FavoritesData f, List<FavoriteOutfit> list, string filterType, string categoryType = null)
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
    
    //Extension methods to FavoriteOutfit data model for Outfit management
    public static class FavoriteOutfitMethods
    {
        // Given a list of possible items to check against, returns true if all pieces of this outfit are present
        public static bool isAvailable(this FavoriteOutfit f, List<Item> playerOwnedItems)
        {
            // Check all items on the needed item list against the list of possible items the player could wear at this dresser
            foreach (string itemID in f.Items.Values)
            {
                // Only check for slots that have an item reference stored in that slot
                if (itemID != null)
                {
                    // If they're missing an item from this outfit, the ensemble is unavailable
                    if (GetItemByReferenceID(f, itemID, playerOwnedItems) == null) { return false; }
                }
            }
            // TODO: Need to add checks for hair and Accessory validity when/if we're using custom hair and accessory indexes

            // If all items in the outfit were found, return true
            return true;
        }

        // Looks up the actual item in the player's items which contains this outfit reference ID
        public static Item GetItemByReferenceID(this FavoriteOutfit f, string id, List<Item> playerOwnedItems)
        {
            foreach (Item item in playerOwnedItems)
            {
                if (item.modData.ContainsKey("StardewOutfitManagerFavoriteItem"))
                {
                   if (item.modData["StardewOutfitManagerFavoriteItem"] == id) { return item; }
                }
            }
            return null;
        }

        // Tag an item as a favorite Items for easy storage and retrieval
        internal static string tagItemAsFavorite(this FavoriteOutfit f, Item item)
        {
            if (item != null && item is Item)
            {
                // If this item is already tagged with a favorite item ID, just return the ID
                if (item.modData.ContainsKey("StardewOutfitManagerFavoriteItem"))
                {
                    return item.modData["StardewOutfitManagerFavoriteItem"];
                }
                // Otherwise we generate a new tag, tag it, and return the new ID
                else
                {
                    string tag = item.Name + "_" + Guid.NewGuid().ToString();
                    item.modData["StardewOutfitManagerFavoriteItem"] = tag;
                    return tag;
                }
            }
            else return null;
        }
    }
}
