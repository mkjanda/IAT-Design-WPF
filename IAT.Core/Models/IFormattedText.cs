using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace IAT.Core.Models
{
    /// <summary>
    /// Defines a contract for text content with associated formatting information.
    /// </summary>
    /// <remarks>Implementations of this interface allow consumers to access and modify both the textual
    /// content and its formatting style as a single unit. This is useful for scenarios such as rich text editing,
    /// document generation, or UI components that require styled text.</remarks>
    public interface IFormattedText
    {
        /// <summary>
        /// Gets the layout item associated with this instance.
        /// </summary>
        LayoutItem LayoutItem { get; init; }

        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets the text style applied to the content.
        /// </summary>
        TextStyle Style { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        Guid Id { get; set; }
    }
}
