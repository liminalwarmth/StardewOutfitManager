using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace StardewOutfitManager.Managers
{
    /// <summary>
    /// Manages loading and retrieving random outfit names from seasonal JSON files.
    /// </summary>
    public class OutfitNameManager
    {
        private readonly Dictionary<string, List<string>> _namesByCategory;

        /// <summary>
        /// Data class for deserializing outfit name JSON files.
        /// </summary>
        public class OutfitNameData
        {
            public string Season { get; set; }
            public List<string> Names { get; set; }
        }

        public OutfitNameManager(IModHelper helper)
        {
            _namesByCategory = new Dictionary<string, List<string>>();
            LoadOutfitNames(helper);
        }

        private void LoadOutfitNames(IModHelper helper)
        {
            string[] categories = { "Spring", "Summer", "Fall", "Winter", "Special" };

            foreach (string category in categories)
            {
                // Use Path.Combine for cross-platform compatibility
                string path = Path.Combine("Assets", "OutfitNames", $"{category}OutfitNames.json");
                try
                {
                    var data = helper.ModContent.Load<OutfitNameData>(path);
                    if (data?.Names != null && data.Names.Count > 0)
                    {
                        _namesByCategory[category] = data.Names;
                        StardewOutfitManager.ModMonitor.Log($"Loaded {data.Names.Count} outfit names for {category}", LogLevel.Trace);
                    }
                    else
                    {
                        StardewOutfitManager.ModMonitor.Log($"No names found in {path}", LogLevel.Warn);
                        _namesByCategory[category] = new List<string> { $"{category} Outfit" };
                    }
                }
                catch (System.Exception ex)
                {
                    StardewOutfitManager.ModMonitor.Log($"Failed to load outfit names for {category}: {ex.Message}", LogLevel.Warn);
                    _namesByCategory[category] = new List<string> { $"{category} Outfit" };
                }
            }
        }

        /// <summary>
        /// Gets a random outfit name for the specified category.
        /// </summary>
        /// <param name="category">The season category (Spring, Summer, Fall, Winter, Special)</param>
        /// <returns>A random name from the category's pool, or a fallback name if not found</returns>
        public string GetRandomName(string category)
        {
            if (_namesByCategory.TryGetValue(category, out var names) && names.Count > 0)
            {
                return names[StardewValley.Game1.random.Next(names.Count)];
            }
            return $"{category} Outfit";
        }
    }
}
