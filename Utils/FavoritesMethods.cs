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
    }
    
    //Extension methods to FavoriteOutfit data model for Outfit management
    public static class FavoriteOutfitMethods
    {
        // Build a lookup dictionary from item tags to items (call once, reuse for all outfit checks)
        public static Dictionary<string, Item> BuildItemTagLookup(List<Item> items)
        {
            var lookup = new Dictionary<string, Item>();
            foreach (Item item in items)
            {
                if (item.modData.TryGetValue("StardewOutfitManagerFavoriteItem", out string tag))
                {
                    // Only store first occurrence if duplicate tags exist
                    if (!lookup.ContainsKey(tag))
                    {
                        lookup[tag] = item;
                    }
                }
            }
            return lookup;
        }

        // TODO: Need to add checks for hair and Accessory validity when/if we're using custom hair and accessory indexes
        // Given a pre-built lookup dictionary, returns true if all pieces of this outfit are present
        public static bool isAvailable(this FavoriteOutfit f, Dictionary<string, Item> itemTagLookup)
        {
            // Check all items on the needed item list against the lookup dictionary
            foreach (string itemID in f.Items.Values)
            {
                // Only check for slots that have an item reference stored in that slot
                if (itemID != null)
                {
                    // If they're missing an item from this outfit, the ensemble is unavailable
                    if (!itemTagLookup.ContainsKey(itemID)) { return false; }
                }
            }

            // If all items in the outfit were found, return true
            return true;
        }

        // Given a pre-built lookup dictionary, returns the items for any non-null slots that are available and null for any that are not
        public static Dictionary<string, Item> GetOutfitItemAvailability(this FavoriteOutfit f, Dictionary<string, Item> itemTagLookup)
        {
            Dictionary<string, Item> itemAvailability = new();
            // Check all items on the needed item list against the lookup dictionary
            foreach (string itemKey in f.Items.Keys)
            {
                // Only check for slots that have an item reference stored in that slot
                if (f.Items[itemKey] != null)
                {
                    // Add the item to the available list (or add the slot as a null if it wasn't found)
                    itemTagLookup.TryGetValue(f.Items[itemKey], out Item foundItem);
                    itemAvailability.Add(itemKey, foundItem);
                }
            }
            // Return the available items
            return itemAvailability;
        }

        // Looks up the actual item in a pre-built lookup dictionary by tag reference ID (or null if not found)
        public static Item GetItemByReferenceID(this FavoriteOutfit f, string id, Dictionary<string, Item> itemTagLookup)
        {
            itemTagLookup.TryGetValue(id, out Item foundItem);
            return foundItem;
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

        // Put available items directly on a display farmer and unequip any slots that are not available (this will cause dupe/deletion if used on the player)
        public static void dressDisplayFarmerWithAvailableOutfitPieces(this FavoriteOutfit f, Farmer displayFarmer, Dictionary<string, Item> availability)
        {
            // Hat
            displayFarmer.hat.Set(null);
            if (availability.ContainsKey("Hat"))
            {
                if (availability["Hat"] != null) { displayFarmer.hat.Set(availability["Hat"] as Hat); }
            }
            // Shirt
            displayFarmer.shirtItem.Set(null);
            if (availability.ContainsKey("Shirt"))
            {
                if (availability["Shirt"] != null) 
                { 
                    displayFarmer.shirtItem.Set(availability["Shirt"] as Clothing);
                }
            }
            // Pants
            displayFarmer.pantsItem.Set(null);
            if (availability.ContainsKey("Pants"))
            {
                if (availability["Pants"] != null) { displayFarmer.pantsItem.Set(availability["Pants"] as Clothing); }
            }
            // Shoes
            displayFarmer.boots.Set(null);
            displayFarmer.changeShoeColor("12");
            if (availability.ContainsKey("Shoes"))
            {
                if (availability["Shoes"] != null) {
                    displayFarmer.boots.Set(availability["Shoes"] as Boots);
                    displayFarmer.changeShoeColor(displayFarmer.boots.Value.indexInColorSheet.Value.ToString());
                }
            }
            // LeftRing
            displayFarmer.leftRing.Set(null);
            if (availability.ContainsKey("LeftRing"))
            {
                if (availability["LeftRing"] != null) { displayFarmer.leftRing.Set(availability["LeftRing"] as Ring); }
            }
            // RightRing
            displayFarmer.rightRing.Set(null);
            if (availability.ContainsKey("RightRing"))
            {
                if (availability["RightRing"] != null) { displayFarmer.rightRing.Set(availability["RightRing"] as Ring); }
            }
            // Hair & Accessory
            displayFarmer.changeHairStyle(f.Hair);
            displayFarmer.accessory.Set(f.Accessory);
            // Finalize display settings
            displayFarmer.UpdateClothing();
            displayFarmer.faceDirection(2);
            displayFarmer.FarmerSprite.StopAnimation();
        }

        // Equips the available pieces of a favorite outfit onto the player, swapping items into the dresser (and unequips any slot that's supposed to be part of it and isn't available)
        public static void equipFavoriteOutfit(this FavoriteOutfit f, IClickableMenu menu, StorageFurniture dresserObject, Farmer farmer, Dictionary<string, Item> availability)
        {
            // First unequip all player slots and put whatever is in them back in the dresser
            foreach (string itemSlot in f.Items.Keys)
            {
                menu.ItemExchange(dresserObject, farmer, itemSlot, null, null, false);
            }
            // Then equip the available pieces of this outfit, removing them from the dresser
            foreach (string itemSlot in availability.Keys)
            {
                if (availability[itemSlot] != null)
                {
                    menu.ItemExchange(dresserObject, farmer, itemSlot, availability[itemSlot], null, false);
                }
            }
            // Change hair and accessory to match outfit settings
            farmer.changeHairStyle(f.Hair);
            farmer.accessory.Set(f.Accessory);
        }
    }
}
