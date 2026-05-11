using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Models;
using IAT.Core.Enumerations;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents text content with associated formatting and layout information for rendering purposes.
    /// </summary>
    /// <remarks>Use the FormattedText class to encapsulate a string along with its text style and layout
    /// metadata. This type is typically used in scenarios where text needs to be displayed with specific formatting or
    /// positioned within a layout system. The Style property determines how the text appears, while the LayoutItem
    /// property provides layout context for rendering. Thread safety is not guaranteed; synchronize access if instances
    /// are shared across threads.</remarks>
    public class FormattedText : IFormattedText
    {
        /// <summary>
        /// The guid of the formatted text instance. This property serves as a unique identifier for each instance of FormattedText, 
        /// allowing for easy tracking and management of individual text elements within the application. The Id is automatically 
        /// generated when a new instance is created, ensuring that each FormattedText object has a distinct identifier that can be 
        /// used for referencing and manipulation throughout the application's lifecycle.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The text content to be rendered. This property can contain any string value, including plain text, formatted text with special characters, 
        /// or multiline text. The content of this property will be rendered according to the associated TextStyle and LayoutItem properties. It is 
        /// important to ensure that the text is properly formatted and does not contain unsupported characters that may affect rendering quality or performance.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text style applied to the content.
        /// </summary>
        public TextStyle Style { get; set; } = new TextStyle();

        /// <summary>
        /// Gets the layout item associated with this instance.
        /// </summary>
        public LayoutItem LayoutItem { get; init; }
    }
}
