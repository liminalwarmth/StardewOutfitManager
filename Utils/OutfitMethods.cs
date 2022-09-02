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
using StardewOutfitManager.Menus;

namespace StardewOutfitManager.Utils
{
    // Extension methods to IClickable menu for farmer equipment management
    public static class EquipmentMethods
    {
        // Equips an item (or nothing) on the given farmer and puts whatever they were wearing, if anything, back into the given dresser furniture object
        public static void ItemExchange(this IClickableMenu m, StorageFurniture dresserObject, Farmer farmer, string category, Item itemToEquip, ClickableComponent itemLabel = null, bool playSound = true)
        {
            // Remove the item being equipped from the dresser
            if (itemToEquip != null) { dresserObject.heldItems.Remove(itemToEquip); }

            // Put anything being worn in this slot back in the dresser and equip the new item
            if (category == "Hat") 
            {
                // Put the current slot back in the dresser if it exists
                if (farmer.hat.Value != null) { dresserObject.heldItems.Add(farmer.hat.Value); }
                // Equip the new item or unequip the current slot if there is no new hat item given
                if (itemToEquip == null) { farmer.hat.Set(null); }
                else { farmer.hat.Set(itemToEquip as Hat); }
                if (itemLabel != null) itemLabel.name = (farmer.hat.Value != null) ? farmer.hat.Value.DisplayName : "None";
            }
            else if (category == "Shirt") 
            {
                // Put the current slot back in the dresser if it exists
                if (farmer.shirtItem.Value != null) { dresserObject.heldItems.Add(farmer.shirtItem.Value); }
                // Equip the new item or unequip the current slot if there is no new shirt item given
                if (itemToEquip == null) { farmer.shirtItem.Set(null); }
                else { farmer.shirtItem.Set(itemToEquip as Clothing); }
                if (itemLabel != null) itemLabel.name = (farmer.shirtItem.Value != null) ? farmer.shirtItem.Value.DisplayName : "None";
            }
            else if (category == "Pants") 
            { 
                // Put the current slot back in the dresser if it exists
                if (farmer.pantsItem.Value != null) { dresserObject.heldItems.Add(farmer.pantsItem.Value);}
                // Equip the new item or unequip the current slot if there is no new pants item given
                if (itemToEquip == null) { farmer.pantsItem.Set(null); }
                else { farmer.pantsItem.Set(itemToEquip as Clothing); }
                if (itemLabel != null) itemLabel.name = (farmer.pantsItem.Value != null) ? farmer.pantsItem.Value.DisplayName : "None";
            }
            else if (category == "Shoes") 
            { 
                // Put the current slot back in the dresser if it exists
                if (farmer.boots.Value != null) { dresserObject.heldItems.Add(farmer.boots.Value); }
                // Equip the new item or unequip the current slot if there is no new boots item given (requires special color handling)
                if (itemToEquip == null) {
                    farmer.boots.Set(null);
                    farmer.changeShoeColor(12);
                    }
                else {
                    farmer.boots.Set(itemToEquip as Boots); 
                    farmer.changeShoeColor(farmer.boots.Value.indexInColorSheet.Value);
                }
                if (itemLabel != null) itemLabel.name = (farmer.boots.Value != null) ? farmer.boots.Value.DisplayName : "None";
            }
            else if (category == "LeftRing") 
            { 
                // Put the current slot back in the dresser if it exists
                if (farmer.leftRing.Value != null) { dresserObject.heldItems.Add(farmer.leftRing.Value);}
                // Equip the new item or unequip the current slot if there is no new ring item given
                if (itemToEquip == null) { farmer.leftRing.Set(null); }
                else { farmer.leftRing.Set(itemToEquip as Ring); }
                if (itemLabel != null) itemLabel.name = (farmer.leftRing.Value != null) ? farmer.leftRing.Value.DisplayName : "None";
            }
            else if (category == "RightRing") 
            { 
                // Put the current slot back in the dresser if it exists
                if (farmer.rightRing.Value != null) { dresserObject.heldItems.Add(farmer.rightRing.Value);}
                // Equip the new item or unequip the current slot if there is no new ring item given
                if (itemToEquip == null) { farmer.rightRing.Set(null); }
                else { farmer.rightRing.Set(itemToEquip as Ring); }
                if (itemLabel != null) itemLabel.name = (farmer.rightRing.Value != null) ? farmer.rightRing.Value.DisplayName : "None";
            }
            
            // Update the clothing display
            farmer.UpdateClothing();
            farmer.completelyStopAnimatingOrDoingAction();

            // Play the item pickup sound
            if (playSound) Game1.playSound("pickUpItem");
        }
    
        /* stopping point, known bugs:
         * - Some dupe weirdness, possibly with identifying an item the player is already wearing (look at the item exchange script and isWearingThis)
         * - Favorites data isn't getting loaded correctly, throws errors
         * - If the game crashes or the player quits without saving, items won't have the mod tags they're supposed to (gotta save favorites at night with everything else)
         */

        // Equips a favorite outfit, if available, onto the player
        public static void WearFavoriteOutfit(this IClickableMenu m, StorageFurniture dresserObject, Farmer farmer, FavoriteOutfit outfit, List<Item> playerOwnedItems)
        {
            // If the outfit is available
            if (outfit.isAvailable(playerOwnedItems)) {
                // Check each item slot that the outfit should equip
                foreach (string itemSlot in outfit.Items.Keys)
                {
                    // Get the reference to the actual item we want from the player's existing items
                    Item equippingItem = outfit.GetItemByReferenceID(outfit.Items[itemSlot], playerOwnedItems);
                    // Need to check if the player is currently wearing this item (if so, no action required)
                    if (!isWearingThis(outfit.Items[itemSlot], equippingItem, farmer))
                    {
                        // If not, perform the necessary swap to wear the right item for this outfit (and don't play the sound)
                        ItemExchange(m, dresserObject, farmer, itemSlot, equippingItem, null, false);
                    }
                }
            }
        }

        internal static bool isWearingThis(string category, Item item, Farmer farmer)
        {
            if (category == "Hat")
            {
                if (farmer.hat.Value == item) { return true; }
            }
            else if (category == "Shirt")
            {
                if (farmer.shirtItem.Value == item) { return true; }
            }
            else if (category == "Pants")
            {
                if (farmer.pantsItem.Value == item) { return true; }
            }
            else if (category == "Shoes")
            {
                if (farmer.boots.Value == item) { return true; }
            }
            else if (category == "LeftRing")
            {
                if (farmer.leftRing.Value == item) { return true; }
            }
            else if (category == "RightRing")
            {
                if (farmer.rightRing.Value == item) { return true; }
            }
            return false;
        }
    }
    
    // Extension methods to IClickableMenu for farmer hair and accessory management
    public static class HairAndAccessoryMethods
    {
        // Change to next or prior hair in the hairstyle indices
        public static void HairSwap(this IClickableMenu menu, string name, int change, Farmer farmer, ClickableComponent label = null)
        {
            // Shuffle to prior or next hair in the hair index
            List<int> all_hairs = Farmer.GetAllHairstyleIndices();
            int current_index = all_hairs.IndexOf(farmer.hair.Value);
            current_index += change;
            if (current_index >= all_hairs.Count)
            {
                current_index = 0;
            }
            else if (current_index < 0)
            {
                current_index = all_hairs.Count() - 1;
            }
            // Update farmer hairstyle
            farmer.changeHairStyle(all_hairs[current_index]);
            // If a label was given, update the label with the correct hairstyle name
            if (label != null)
            {
                label.name = GetHairOrAccessoryName(menu, name, current_index);
            }
            Game1.playSound("grassyStep");
        }

        // Change to next or prior accessory in the accessory indices
        public static void AccessorySwap(this IClickableMenu menu, string name, int change, Farmer farmer, ClickableComponent label = null)
        {
            int newAccValue = (int)farmer.accessory.Value + change;
            // Taken from the game hardcoding -- these should be made dynamic if I want to add more or exclude beards
            // Taking out beards (0-6) and the duckbill (18) for now
            if (newAccValue < 6)
            {
                if (change == -1) { newAccValue = newAccValue < -1 ? 17 : -1; } // Hop down to 0 from 6 and then loop around if we're going left
                else { newAccValue = 6; } // Else hop up to 6 if right
            }
            if (newAccValue >= -1)
            {
                if (newAccValue >= 18)
                {
                    newAccValue = -1;
                }
                farmer.accessory.Set(newAccValue);
            }
            // If a label was given, update the label with the correct accesory name
            if (label != null)
            {
                label.name = GetHairOrAccessoryName(menu, name, newAccValue);
            }
            Game1.playSound("purchase");
        }

        // Get hair or accessory name given a type and index # value
        public static string GetHairOrAccessoryName(this IClickableMenu menu, string stringType, int value)
        {
            value++;
            string valueString = value.ToString();
            IDictionary<string, string> dictToCheck = (stringType == "Hair") ? StardewOutfitManager.assetManager.hairJSON : StardewOutfitManager.assetManager.accessoryJSON;
            if (dictToCheck.ContainsKey(valueString))
            {
                return dictToCheck[valueString];
            }
            else
            {
                string prefix = (stringType == "Hair") ? "Hair " : "Accessory ";
                return prefix + valueString;
            }
        }
    }
}
