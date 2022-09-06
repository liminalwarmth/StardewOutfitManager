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
            // Create outfit object
            FavoriteOutfit outfit = new FavoriteOutfit();

            // Name & Category
            outfit.Name = name;
            outfit.Category = category;

            // Set the string outfit tag values for this outfit loadout from the given player
            outfit.Items.Add("Hat", outfit.tagItemAsFavorite(player.hat.Value));
            outfit.Items.Add("Shirt", outfit.tagItemAsFavorite(player.shirtItem.Value));
            outfit.Items.Add("Pants", outfit.tagItemAsFavorite(player.pantsItem.Value));
            outfit.Items.Add("Shoes", outfit.tagItemAsFavorite(player.boots.Value));
            outfit.Items.Add("LeftRing", outfit.tagItemAsFavorite(player.leftRing.Value));
            outfit.Items.Add("RightRing", outfit.tagItemAsFavorite(player.rightRing.Value));

            // TODO: In addition to storing the values I need to figure out how to store reference indexes for Hair and Accessory if not base
            outfit.Hair = player.hair.Value;
            outfit.HairIndex = "";
            outfit.Accessory = player.accessory.Value;
            outfit.AccessoryIndex = "";

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
                    Item foundItem = GetItemByReferenceID(f, itemID, playerOwnedItems);
                    // If they're missing an item from this outfit, the ensemble is unavailable
                    if ( foundItem == null) { return false; }
                }
            }
            // TODO: Need to add checks for hair and Accessory validity when/if we're using custom hair and accessory indexes

            // If all items in the outfit were found, return true
            return true;
        }

        // Looks up the actual item in a list of item objects which contains the given item tag reference ID (or null if not found
        public static Item GetItemByReferenceID(this FavoriteOutfit f, string id, List<Item> itemListToCheck)
        {
            foreach (Item item in itemListToCheck)
            {
                if (item.modData.ContainsKey("StardewOutfitManagerFavoriteItem"))
                {
                   if (item.modData["StardewOutfitManagerFavoriteItem"] == id) { 
                        return item; 
                   }
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

        // Check if a given farmer is currently wearing an item with the given tag
        public static bool isWearingThis(this FavoriteOutfit f, string category, string itemTag, Farmer farmer)
        {
            if (category == "Hat")
            {
                if (favoriteItemTagMatches(f, farmer.hat.Value, itemTag)) { return true; }
            }
            else if (category == "Shirt")
            {
                if (favoriteItemTagMatches(f, farmer.shirtItem.Value, itemTag)) { return true; }
            }
            else if (category == "Pants")
            {
                if (favoriteItemTagMatches(f, farmer.pantsItem.Value, itemTag)) { return true; }
            }
            else if (category == "Shoes")
            {
                if (favoriteItemTagMatches(f, farmer.boots.Value, itemTag)) { return true; }
            }
            else if (category == "LeftRing")
            {
                if (favoriteItemTagMatches(f, farmer.leftRing.Value, itemTag)) { return true; }
            }
            else if (category == "RightRing")
            {
                if (favoriteItemTagMatches(f, farmer.rightRing.Value, itemTag)) { return true; }
            }
            return false;
        }

        // Compare two given items and return true only if they both have moddata outfit tags and those tags are the same
        internal static bool favoriteItemTagMatches(this FavoriteOutfit f, Item item, string itemTag)
        {
            if (item != null && item is Item)
            {
                if (item.modData.ContainsKey("StardewOutfitManagerFavoriteItem"))
                {
                    if (item.modData["StardewOutfitManagerFavoriteItem"] == itemTag)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
