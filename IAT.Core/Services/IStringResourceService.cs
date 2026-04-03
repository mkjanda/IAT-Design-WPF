using System;
using System.Collections.Generic;
using System.Text;

namespace IAT_Design_WPF.Services
{
    /// <summary>
    /// Provides methods for retrieving localized string resources by key.
    /// </summary>
    public interface IStringResourceService
    {
        /// <summary>
        /// Retrieves the localized string associated with the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The key that identifies the resource to retrieve. Cannot be null or empty.</param>
        /// <returns>The localized string corresponding to the specified resource key, or null if the key is not found.</returns>
        string GetString(string resourceKey);

        /// <summary>
        /// Gets the localized string associated with the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The key that identifies the resource to retrieve. Cannot be null.</param>
        /// <returns>The localized string corresponding to the specified resource key, or null if the key is not found.</returns>
        string this[string resourceKey] { get; }   // optional indexer for cleaner code
    }
}
