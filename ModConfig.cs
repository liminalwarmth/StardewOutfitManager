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

        /// <summary>
        /// If enabled, rings are included when saving and equipping outfits.
        /// When disabled, rings are excluded and ring slots are hidden from the UI.
        /// </summary>
        public bool IncludeRingsInOutfits { get; set; } = true;

        /// <summary>
        /// If enabled, facial hair (accessory indices 0-5 and 19-22) are included as selectable accessories.
        /// This includes beards, mustaches, and other facial hair options.
        /// When disabled, only non-facial-hair accessories are available in the accessory picker.
        /// </summary>
        public bool IncludeFacialHair { get; set; } = false;

        /// <summary>
        /// If enabled, modded hairstyles are included in the hair picker.
        /// When disabled, only vanilla hairstyles (0-73) are available.
        /// </summary>
        public bool IncludeModdedHairstyles { get; set; } = true;

        /// <summary>
        /// If enabled, modded accessories (beyond vanilla index 29) are included in the accessory picker.
        /// When disabled, only vanilla accessories are available.
        /// </summary>
        public bool IncludeModdedAccessories { get; set; } = true;
    }
}
