using System;
using StardewModdingAPI;

namespace StardewOutfitManager
{
    /// <summary>
    /// API interface for Generic Mod Config Menu (spacechase0.GenericModConfigMenu).
    /// This allows the mod to register config options that can be edited in-game.
    /// </summary>
    public interface IGenericModConfigMenuApi
    {
        /// <summary>
        /// Register a mod whose config can be edited through the config UI.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="reset">Reset the mod's config to its default values.</param>
        /// <param name="save">Save the mod's current config to the config.json file.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        /// <summary>
        /// Add a boolean option at the current position in the form.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="fieldId">The unique field ID for use by other mods.</param>
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

        /// <summary>
        /// Add a section title at the current position in the form.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The title text shown in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the title.</param>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);

        /// <summary>
        /// Add a paragraph of text at the current position in the form.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The paragraph text to display.</param>
        void AddParagraph(IManifest mod, Func<string> text);

        /// <summary>
        /// Add a text option with optional dropdown of allowed values.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="allowedValues">The allowed values for the field, shown as a dropdown.</param>
        /// <param name="formatAllowedValue">Format a value for display in the dropdown.</param>
        /// <param name="fieldId">The unique field ID for use by other mods.</param>
        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);
    }
}
