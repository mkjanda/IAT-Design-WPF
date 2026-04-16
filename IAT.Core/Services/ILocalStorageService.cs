using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;

namespace IAT.Core.Services
{
    /// <summary>
    /// The interface ILocalStorageService defines a contract for a service that provides access to local storage, allowing for the retrieval and storage of string 
    /// values associated with specific fields. This service can be used to manage application settings, user preferences, or any other data that needs to be persisted 
    /// locally. The implementation of this interface may vary depending on the platform (e.g., file system, database, in-memory storage) and should handle cases where 
    /// fields may not exist gracefully.
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        /// Gets or sets the value associated with the specified field.
        /// </summary>
        /// <remarks>If the specified field does not exist, getting the value may return null or throw an
        /// exception, depending on the implementation. Setting a value for a non-existent field may add a new entry or
        /// update an existing one.</remarks>
        /// <param name="field">The field for which to get or set the value.</param>
        /// <returns></returns>
        string this[Field field] { get; set; }
    }
}
