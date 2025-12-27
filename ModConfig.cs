namespace StardewOutfitManager
{
    /// <summary>
    /// Configuration options for Stardew Outfit Manager.
    /// These can be edited via config.json or Generic Mod Config Menu.
    /// </summary>
    public sealed class ModConfig
    {
        /// <summary>
        /// If enabled, new players receive a Small Oak Dresser in their starting inventory.
        /// </summary>
        public bool StartingDresser { get; set; } = true;

        /// <summary>
        /// If enabled, Robin sells custom dressers at her Carpenter shop on a daily rotation.
        /// </summary>
        public bool RobinSellsDressers { get; set; } = true;

        /// <summary>
        /// If enabled, Mirror Dressers can appear in the Traveling Merchant's random furniture selection.
        /// </summary>
        public bool TravelingMerchantSellsDressers { get; set; } = true;
    }
}
