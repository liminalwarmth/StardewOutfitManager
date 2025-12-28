using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Objects;

namespace StardewOutfitManager.Patches
{
    /// <summary>
    /// Harmony patches for Furniture class to prevent removal of locked dressers.
    /// This protects dressers that are part of a shared inventory cluster from being
    /// picked up while another player has the dresser menu open.
    /// </summary>
    [HarmonyPatch(typeof(Furniture), nameof(Furniture.canBeRemoved))]
    public static class FurniturePatches
    {
        /// <summary>
        /// Prefix patch: prevent removal of locked StorageFurniture (dressers).
        /// When any dresser is locked (either directly opened or as part of a shared cluster),
        /// it cannot be picked up or moved.
        /// </summary>
        /// <param name="__instance">The Furniture instance being checked.</param>
        /// <param name="__result">The result to return if we skip the original method.</param>
        /// <returns>False to skip original method, true to run it.</returns>
        public static bool Prefix(Furniture __instance, ref bool __result)
        {
            try
            {
                // Check if this is a StorageFurniture (dresser) and is locked
                if (__instance is StorageFurniture storage && storage.mutex.IsLocked())
                {
                    __result = false;  // Cannot be removed
                    return false;      // Skip original method
                }
            }
            catch (Exception ex)
            {
                StardewOutfitManager.ModMonitor?.Log($"Error in canBeRemoved patch: {ex.Message}", LogLevel.Error);
            }
            return true;  // Run original method
        }
    }
}
