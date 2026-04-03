using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Validation
{
    /// <summary>
    /// Defines a contract for an item that can be validated and can report validation errors.
    /// </summary>
    /// <remarks>Implement this interface to provide custom validation logic for items. The validation results
    /// should be added to the provided error dictionary, associating each item with its corresponding validation
    /// error.</remarks>
    public interface IValidatedItem
    {
        /// <summary>
        /// Validates the current item and adds any validation errors to the specified dictionary.
        /// </summary>
        /// <remarks>If the item is valid, no entry is added to the dictionary. Existing entries in the
        /// dictionary are not removed or modified.</remarks>
        /// <param name="ErrorDictionary">A dictionary to which validation errors are added. The key is the item being validated, and the value is the
        /// associated validation error. Must not be null.</param>
        void Validate(Dictionary<IValidatedItem, ValidationError> ErrorDictionary);
    }
}
