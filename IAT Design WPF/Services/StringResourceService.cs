using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using IAT.Core.Services;

namespace IAT_Design_WPF.Services
{
    /// <summary>
    /// Provides access to localized string resources by key, enabling retrieval of application-defined text for
    /// localization and globalization purposes.
    /// </summary>
    /// <remarks>This service offers a consistent way to obtain localized strings from application resources.
    /// If a resource key is not found, a fallback string indicating the missing resource is returned instead of
    /// throwing an exception. This behavior helps prevent application crashes due to missing resources.</remarks>
    public sealed class StringResourceService : IStringResourceService
    {
        /// <summary>
        /// Retrieves the localized string resource associated with the specified resource key.
        /// </summary>
        /// <remarks>This method provides a graceful fallback for missing resources, ensuring that the
        /// application does not throw an exception if the resource key is not found. The returned placeholder string
        /// includes the missing resource key for easier debugging.</remarks>
        /// <param name="resourceKey">The key of the resource to retrieve. Cannot be null.</param>
        /// <returns>The localized string corresponding to the specified resource key. If the resource is not found, returns a
        /// placeholder string indicating the missing resource.</returns>
        public string GetString(string resourceKey)
        {
            try
            {
                return (string)Application.Current.FindResource(resourceKey);
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                // Graceful fallback so the app never crashes over a missing string
                return $"[MISSING RESOURCE: {resourceKey}]";
            }
        }

        /// <summary>
        /// Gets the localized string associated with the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The key of the resource to retrieve. Cannot be null.</param>
        /// <returns>The localized string corresponding to the specified resource key, or null if the key is not found.</returns>
        public string this[string resourceKey] => GetString(resourceKey);
    }
}