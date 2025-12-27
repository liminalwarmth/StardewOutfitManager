using System.Collections.Generic;

namespace StardewOutfitManager.Models
{
    /// <summary>
    /// Per-save data tracked by the mod.
    /// </summary>
    public class ModSaveData
    {
        /// <summary>
        /// Set of farmer unique multiplayer IDs that have already received the starting dresser.
        /// Using a HashSet allows tracking multiple farmers in multiplayer.
        /// </summary>
        public HashSet<long> FarmersWithStartingDresser { get; set; } = new HashSet<long>();
    }
}
