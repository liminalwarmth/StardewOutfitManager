namespace StardewOutfitManager
{
    /// <summary>
    /// Controls how dresser inventories are shared between multiple dressers.
    /// </summary>
    public enum DresserSharingMode
    {
        /// <summary>Each dresser has its own separate inventory.</summary>
        Individual,
        /// <summary>Dressers that are touching (8-way adjacent) share the same inventory.</summary>
        Touching,
        /// <summary>All dressers within the same house or cabin share the same inventory.</summary>
        SameBuilding
    }

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

        /// <summary>
        /// Controls how dresser inventories are shared.
        /// Individual: Each dresser has its own inventory.
        /// Touching: Adjacent dressers (8-way) share inventory.
        /// SameBuilding: All dressers in a house/cabin share inventory.
        /// </summary>
        public DresserSharingMode DresserInventorySharing { get; set; } = DresserSharingMode.Touching;
    }
}
