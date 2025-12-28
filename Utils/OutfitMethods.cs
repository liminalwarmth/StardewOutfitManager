using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;
using StardewOutfitManager.Utils;

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
                    farmer.changeShoeColor("12");
                    }
                else {
                    farmer.boots.Set(itemToEquip as Boots);
                    farmer.changeShoeColor(farmer.boots.Value.indexInColorSheet.Value.ToString());
                }
                if (itemLabel != null) itemLabel.name = (farmer.boots.Value != null) ? farmer.boots.Value.DisplayName : "None";
            }
            // Ring slots - only process if rings are included in outfits
            else if (StardewOutfitManager.Config.IncludeRingsInOutfits && category == "LeftRing")
            {
                // Put the current slot back in the dresser if it exists
                if (farmer.leftRing.Value != null) { dresserObject.heldItems.Add(farmer.leftRing.Value);}
                // Equip the new item or unequip the current slot if there is no new ring item given
                if (itemToEquip == null) { farmer.leftRing.Set(null); }
                else { farmer.leftRing.Set(itemToEquip as Ring); }
                if (itemLabel != null) itemLabel.name = (farmer.leftRing.Value != null) ? farmer.leftRing.Value.DisplayName : "None";
            }
            else if (StardewOutfitManager.Config.IncludeRingsInOutfits && category == "RightRing")
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
    }
    
    // Extension methods to IClickableMenu for farmer hair and accessory management
    public static class HairAndAccessoryMethods
    {
        // Vanilla hair range (indices 0-73 based on HairNames.json)
        public const int VANILLA_HAIR_MAX = 73;

        // Change to next or prior hair in the hairstyle indices
        public static void HairSwap(this IClickableMenu menu, string name, int change, Farmer farmer, ClickableComponent label = null)
        {
            // Get all valid hair indices
            List<int> all_hairs = Farmer.GetAllHairstyleIndices();

            // Filter to vanilla-only if modded hair is disabled in config
            if (!StardewOutfitManager.Config.IncludeModdedHairstyles)
            {
                all_hairs = all_hairs.Where(i => i <= VANILLA_HAIR_MAX).ToList();
            }

            int current_index = all_hairs.IndexOf(farmer.hair.Value);

            // If current hair not in filtered list, reset to first valid
            if (current_index < 0)
            {
                current_index = 0;
            }

            current_index += change;

            if (current_index >= all_hairs.Count)
            {
                current_index = 0;
            }
            else if (current_index < 0)
            {
                current_index = all_hairs.Count - 1;
            }

            // Update farmer hairstyle
            farmer.changeHairStyle(all_hairs[current_index]);

            // If a label was given, update the label with the correct hairstyle name
            if (label != null)
            {
                label.name = GetHairOrAccessoryName(menu, name, all_hairs[current_index]);
            }
            Game1.playSound("grassyStep");
        }

        // Change to next or prior accessory in the accessory indices
        public static void AccessorySwap(this IClickableMenu menu, string name, int change, Farmer farmer, ClickableComponent label = null)
        {
            // Get all valid accessory indices based on config
            List<int> validAccessories = AccessoryMethods.GetAllAccessoryIndices(
                StardewOutfitManager.Config.IncludeFacialHair,
                StardewOutfitManager.Config.IncludeModdedAccessories
            );

            int currentValue = farmer.accessory.Value;
            int currentIndex = validAccessories.IndexOf(currentValue);

            // If current accessory not in valid list, reset to first (none/-1)
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            // Move to next/previous
            currentIndex += change;

            // Wrap around
            if (currentIndex >= validAccessories.Count)
                currentIndex = 0;
            else if (currentIndex < 0)
                currentIndex = validAccessories.Count - 1;

            int newValue = validAccessories[currentIndex];
            farmer.accessory.Set(newValue);

            // If a label was given, update the label with the correct accessory name
            if (label != null)
            {
                label.name = GetHairOrAccessoryName(menu, name, newValue);
            }
            Game1.playSound("purchase");
        }

        // Get hair or accessory name given a type and index # value
        // JSON keys match the actual index values (e.g., key "-1" for None, key "0" for first accessory)
        public static string GetHairOrAccessoryName(this IClickableMenu menu, string stringType, int value)
        {
            string valueString = value.ToString();
            IDictionary<string, string> dictToCheck = (stringType == "Hair") ? StardewOutfitManager.assetManager.hairJSON : StardewOutfitManager.assetManager.accessoryJSON;
            if (dictToCheck.ContainsKey(valueString))
            {
                return dictToCheck[valueString];
            }
            else
            {
                // Fallback for unknown indices - use 1-based display for user-friendliness
                string prefix = (stringType == "Hair") ? "Hair " : "Accessory ";
                return prefix + (value + 1).ToString();
            }
        }
    }
}
