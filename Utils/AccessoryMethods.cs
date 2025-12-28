using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace StardewOutfitManager.Utils
{
    /// <summary>
    /// Utility methods for dynamic accessory detection and validation.
    /// Handles both vanilla and modded accessories.
    /// </summary>
    public static class AccessoryMethods
    {
        // Vanilla accessory constants based on SDV 1.6 source code (FarmerRenderer.isAccessoryFacialHair)
        // Facial hair (uses hair color for tinting): 0-5 and 19-22
        // The game draws accessories dynamically from texture, no hardcoded max
        public const int ACCESSORY_NONE = -1;

        // Vanilla texture: 4 rows x 8 columns = 32 slots (indices 0-31)
        // However, indices 30 and 31 are empty slots with no visual
        // The actual last accessory is index 29
        // VANILLA_MAX_INDEX is used to distinguish vanilla vs modded when filtering
        public const int VANILLA_MAX_INDEX = 29;  // Last actual accessory in vanilla

        // Set of indices that are facial hair (excluded unless IncludeFacialHair is true)
        // Based on FarmerRenderer.isAccessoryFacialHair: returns true for 0-5 and 19-22
        // Includes beards, mustaches, and other facial hair options
        private static readonly HashSet<int> FacialHairIndices = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5,     // Original facial hair accessories
            19, 20, 21, 22        // SDV 1.6 facial hair additions
        };

        // Accessory sprite dimensions (from Characters/Farmer/accessories texture)
        private const int ACCESSORY_SPRITE_HEIGHT = 32;
        private const int ACCESSORIES_PER_ROW = 8;

        // Cache for performance
        private static List<int> _cachedAccessoryIndices = null;
        private static bool _cachedIncludeFacialHair = false;
        private static bool _cachedIncludeModded = true;
        private static int _cachedMaxIndex = -1;

        /// <summary>
        /// Get all valid accessory indices based on current config settings.
        /// Results are cached for performance.
        /// </summary>
        /// <param name="includeFacialHair">Whether to include facial hair accessories (indices 0-5 and 19-22)</param>
        /// <param name="includeModded">Whether to include modded accessories (beyond vanilla index 29)</param>
        /// <returns>List of valid accessory indices</returns>
        public static List<int> GetAllAccessoryIndices(bool includeFacialHair, bool includeModded)
        {
            // Return cached list if config unchanged
            if (_cachedAccessoryIndices != null &&
                _cachedIncludeFacialHair == includeFacialHair &&
                _cachedIncludeModded == includeModded)
            {
                return _cachedAccessoryIndices;
            }

            // Log when regenerating
            StardewOutfitManager.ModMonitor?.Log($"[AccessoryMethods] Regenerating accessory list: includeFacialHair={includeFacialHair}, includeModded={includeModded}", StardewModdingAPI.LogLevel.Debug);

            _cachedIncludeFacialHair = includeFacialHair;
            _cachedIncludeModded = includeModded;

            var indices = new List<int> { ACCESSORY_NONE }; // -1 = none

            // Get the maximum accessory index from texture dimensions
            int maxIndex = GetMaxAccessoryIndexFromTexture();

            // Add all accessories from 0 to max, filtering based on config
            for (int i = 0; i <= maxIndex; i++)
            {
                // Skip facial hair unless config allows it
                if (FacialHairIndices.Contains(i) && !includeFacialHair)
                    continue;

                // Skip modded accessories (beyond vanilla range) if not enabled
                if (!includeModded && i > VANILLA_MAX_INDEX)
                    continue;

                indices.Add(i);
            }

            StardewOutfitManager.ModMonitor?.Log($"[AccessoryMethods] Generated list with {indices.Count} items: [{string.Join(", ", indices)}]", StardewModdingAPI.LogLevel.Debug);

            _cachedAccessoryIndices = indices;
            return indices;
        }

        // Vanilla texture dimensions: 4 rows of 8 accessories = 32 slots (indices 0-31)
        private const int VANILLA_TEXTURE_ROWS = 4;

        /// <summary>
        /// Detect the maximum accessory index by checking texture dimensions.
        /// Modded accessories extend the texture beyond vanilla size.
        /// Returns VANILLA_MAX_INDEX (29) for vanilla, or higher for modded textures.
        /// </summary>
        /// <returns>Maximum valid accessory index</returns>
        public static int GetMaxAccessoryIndexFromTexture()
        {
            // Return cached value if available
            if (_cachedMaxIndex >= 0)
            {
                return _cachedMaxIndex;
            }

            try
            {
                Texture2D accessoryTexture = Game1.content.Load<Texture2D>("Characters/Farmer/accessories");
                int textureHeight = accessoryTexture.Height;
                int rows = textureHeight / ACCESSORY_SPRITE_HEIGHT;

                StardewOutfitManager.ModMonitor?.Log($"[AccessoryMethods] Texture height={textureHeight}, rows={rows}, VANILLA_TEXTURE_ROWS={VANILLA_TEXTURE_ROWS}", StardewModdingAPI.LogLevel.Debug);

                // Calculate max index based on texture dimensions
                if (rows > VANILLA_TEXTURE_ROWS)
                {
                    // Modded texture - calculate based on total slots
                    // Each row has 8 accessories, max index = total slots - 1
                    // Note: This assumes modded textures fill all slots. If a mod adds rows with
                    // empty trailing slots (like vanilla's 30-31), those will still be included.
                    // Mod authors should ensure their textures are fully populated.
                    int totalSlots = rows * ACCESSORIES_PER_ROW;
                    _cachedMaxIndex = totalSlots - 1;
                    StardewOutfitManager.ModMonitor?.Log($"[AccessoryMethods] Modded accessories detected: rows={rows}, maxIndex={_cachedMaxIndex}", StardewModdingAPI.LogLevel.Debug);
                }
                else
                {
                    // Vanilla texture - use known max (29, since slots 30-31 are empty)
                    _cachedMaxIndex = VANILLA_MAX_INDEX;
                    StardewOutfitManager.ModMonitor?.Log($"[AccessoryMethods] Vanilla texture, maxIndex={_cachedMaxIndex}", StardewModdingAPI.LogLevel.Debug);
                }

                return _cachedMaxIndex;
            }
            catch (Exception ex)
            {
                // Fallback to vanilla max if texture load fails
                StardewOutfitManager.ModMonitor?.Log($"[AccessoryMethods] Texture load failed: {ex.Message}, falling back to vanilla max", StardewModdingAPI.LogLevel.Warn);
                _cachedMaxIndex = VANILLA_MAX_INDEX;
                return _cachedMaxIndex;
            }
        }

        /// <summary>
        /// Check if an accessory index is valid (exists in current accessory range).
        /// This checks against the full detected range, not filtered by config.
        /// </summary>
        /// <param name="index">Accessory index to validate</param>
        /// <returns>True if the index is valid</returns>
        public static bool IsValidAccessoryIndex(int index)
        {
            if (index == ACCESSORY_NONE) return true;
            if (index < 0) return false;

            int maxIndex = GetMaxAccessoryIndexFromTexture();
            return index <= maxIndex;
        }

        /// <summary>
        /// Invalidate the cached accessory data.
        /// Call this when assets reload or config changes.
        /// </summary>
        public static void InvalidateCache()
        {
            _cachedAccessoryIndices = null;
            _cachedMaxIndex = -1;
        }
    }
}
