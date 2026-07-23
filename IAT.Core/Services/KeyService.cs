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
            if (key == null || !key.IsCombined)
                return key?.Text ?? string.Empty;

            var parts = key.ComponentIds
                .Select(id => test.AllKeys.FirstOrDefault(k => k.Id == id)?.Text ?? "")
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            return string.Join(key.Separator, parts);   // or build multi-line
        }

        /// <summary>
        /// Resolves and returns the text for the specified key, combining component texts if the key is marked as
        /// combined.
        /// </summary>
        /// <remarks>If the specified key is marked as combined, the method concatenates the texts of its
        /// component keys using the defined separator. If any component key is missing, an empty string is used in its
        /// place.</remarks>
        /// <param name="test">The test instance containing the collection of keys to search and resolve.</param>
        /// <param name="keyId">The unique identifier of the key to resolve.</param>
        /// <returns>A Key object representing the resolved key. If the key is combined, its text is constructed by joining the
        /// texts of its component keys; otherwise, the original key is returned. If the key is not found, a new Key
        /// with the specified keyId and an empty text is returned.</returns>
        public Key GetResolvedKey(IatTest test, Guid keyId)
        {
            var key = test.AllKeys.FirstOrDefault(k => k.Id == keyId);
            if (key == null || !key.IsCombined)
                return key ?? new Key { Id = keyId, Style = new TextStyle(), Text = "" };
            var resolvedKey = new Key
            {
                Id = key.Id,
                Style = new TextStyle(),
                IsCombined = false,
                Text = string.Join(key.Separator, 
                    key.ComponentIds.Select(id => test.AllKeys.FirstOrDefault(k => k.Id == id)?.Text ?? ""))
            };
            return resolvedKey;
        }
    }
}
