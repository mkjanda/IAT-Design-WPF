using IAT.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services
{
    /// <summary>
    /// The interface for the KeyService, which provides methods for resolving the display text and DI text for keys in an IAT test. 
    /// The GetResolvedDisplayText method takes an IatTest and a key ID, and returns the resolved display text for that key, handling 
    /// combined keys by concatenating the texts of their components. The GetResolvedDIText method similarly resolves the DI text for a key, 
    /// returning a Key object with the resolved text. This service is essential for ensuring that the correct text is displayed for 
    /// each key during the test, especially when dealing with combined keys that require special handling to display their components properly.
    /// </summary>
    public interface IKeyService
    {
        /// <summary>
        /// Resolves and returns the display text associated with the specified test and key identifier.
        /// </summary>
        /// <param name="test">The test instance for which to retrieve the display text. Cannot be null.</param>
        /// <param name="keyId">The unique identifier of the key whose display text is to be resolved.</param>
        /// <returns>A string containing the resolved display text for the specified test and key. Returns an empty string if no
        /// display text is found.</returns>
        string GetResolvedDisplayText(IatTest test, Guid keyId);
        
        /// <summary>
        /// Resolves and retrieves the data item text associated with the specified key identifier for the given IAT
        /// test.
        /// </summary>
        /// <param name="test">The IAT test instance from which to resolve the data item text. Cannot be null.</param>
        /// <param name="keyId">The unique identifier of the key whose associated data item text is to be retrieved.</param>
        /// <returns>A Key object containing the resolved data item text for the specified key identifier. Returns null if the
        /// key is not found.</returns>
        Key GetResolvedKey(IatTest test, Guid keyId);   // for your image gen
    }

    /// <summary>
    /// Provides methods for resolving and retrieving display text and data for keys within an IAT test.
    /// </summary>
    /// <remarks>The KeyService class offers functionality to obtain resolved representations of keys,
    /// including handling combined keys by aggregating their component texts. This service is intended to be used in
    /// scenarios where key display or data needs to be dynamically constructed based on test definitions.</remarks>
    public class KeyService : IKeyService
    {
        /// <summary>
        /// Gets the resolved display text for the specified key, combining component texts if the key is a combined
        /// key.
        /// </summary>
        /// <remarks>If the key is marked as combined, the method concatenates the display texts of its
        /// component keys using the specified separator. Only non-empty component texts are included in the
        /// result.</remarks>
        /// <param name="test">The test instance containing the collection of keys to search.</param>
        /// <param name="keyId">The unique identifier of the key for which to retrieve the display text.</param>
        /// <returns>A string containing the display text for the specified key. If the key is a combined key, returns the
        /// concatenated display texts of its components separated by the key's separator. Returns an empty string if
        /// the key is not found.</returns>
        public string GetResolvedDisplayText(IatTest test, Guid keyId)
        {
            var key = test.AllKeys.FirstOrDefault(k => k.Id == keyId);
            if (key == null)
                return string.Empty;

            // Prefer the stored authoring text (already "A or B" for combined keys).
            // Fall back to component join when Text was never set.
            if (!string.IsNullOrWhiteSpace(key.Text))
                return Key.FormatAuthoringDisplay(key.Text);

            if (!key.IsCombined)
                return string.Empty;

            var parts = key.ComponentIds
                .Select(id => test.AllKeys.FirstOrDefault(k => k.Id == id)?.Text ?? "")
                .Select(Key.FormatAuthoringDisplay)
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            return string.Join(key.Separator, parts);
        }

        /// <summary>
        /// Resolves and returns a <see cref="Key"/> suitable for image generation.
        /// Combined (or "X or Y") keys are stacked to three rows so participant slides match
        /// the classic IAT column layout.
        /// </summary>
        public Key GetResolvedKey(IatTest test, Guid keyId)
        {
            var key = test.AllKeys.FirstOrDefault(k => k.Id == keyId);
            if (key == null)
                return new Key { Id = keyId, Style = new TextStyle(), Text = "" };

            string authoring;
            if (!string.IsNullOrWhiteSpace(key.Text))
            {
                authoring = Key.FormatAuthoringDisplay(key.Text);
            }
            else if (key.IsCombined)
            {
                authoring = string.Join(
                    key.Separator,
                    key.ComponentIds
                        .Select(id => test.AllKeys.FirstOrDefault(k => k.Id == id)?.Text ?? "")
                        .Select(Key.FormatAuthoringDisplay)
                        .Where(t => !string.IsNullOrEmpty(t)));
            }
            else
            {
                authoring = string.Empty;
            }

            var style = key.Style ?? new TextStyle
            {
                FontFamily = key.FontFamily ?? "Segoe UI",
                FontSize = key.FontSize > 0 ? key.FontSize : 24.0,
                FontColor = key.FontColor
            };

            return new Key
            {
                Id = key.Id,
                Style = style,
                FontFamily = style.FontFamily,
                FontSize = style.FontSize,
                FontColor = style.FontColor,
                IsCombined = key.IsCombined || authoring.Contains(" or ", StringComparison.OrdinalIgnoreCase),
                LayoutMode = KeyLayoutMode.VerticalWithOr,
                // Stacked for slides / bitmap render.
                Text = Key.FormatStackedDisplay(authoring)
            };
        }
    }
}
