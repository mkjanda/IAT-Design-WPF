using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;

namespace IAT.Core.Services
{
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
