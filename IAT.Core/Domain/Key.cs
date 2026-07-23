using IAT.Core.Enumerations;
using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;


namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a key with associated text, unique identifier, and formatting options for use within the application.
    /// </summary>
    /// <remarks>The Key class provides properties for managing key identity, display text, combination state,
    /// component relationships, and formatting. It supports customization of layout and text style, enabling flexible
    /// presentation and organization of keys in user interfaces. Instances of Key can be used to represent individual
    /// or combined keys, and are suitable for scenarios where keys need to be referenced, displayed, or
    /// grouped.</remarks>
    public class Key : IFormattedText
    {
        /// <summary>
        /// The id of the key object. This is used to uniquely identify the key and can be used to reference it in other parts of the application, 
        /// such as when defining combined keys or when associating keys with specific stimuli or responses. The Id is generated as a new Guid by 
        /// default, ensuring that each key has a unique identifier that can be used for tracking and management purposes within the IAT application.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the current instance represents a combined state.
        /// </summary>
        public bool IsCombined { get; set; }

        /// <summary>
        /// Gets or sets the layout item that determines the position or arrangement within the layout.
        /// </summary>
        public LayoutItem LayoutItem { get; init; } = LayoutItem.LeftKey;  // Default to LeftKey, can be overridden as needed

        /// <summary>
        /// Gets or sets the collection of unique identifiers for the associated components.
        /// </summary>
        public List<Guid> ComponentIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the string used to separate items in formatted output.
        /// </summary>
        public string Separator { get; set; } = " or ";

        /// <summary>
        /// Gets or sets the layout mode used to arrange keys within the control.
        /// </summary>
        /// <remarks>Use this property to specify how keys are visually organized, such as in a vertical
        /// stack or another supported arrangement. Changing the layout mode affects the appearance and positioning of
        /// keys.</remarks>
        public KeyLayoutMode LayoutMode { get; set; } = KeyLayoutMode.VerticalStack;

        /// <summary>
        /// Gets or sets the text style applied to the content.
        /// </summary>
        public required TextStyle Style { get; set; } 

        /// <summary>
        /// Gets or sets the font family name used for text rendering.
        /// </summary>
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Gets or sets the font size used to display text.
        /// </summary>
        public double FontSize { get; set; } = 24.0;

        /// <summary>
        /// Gets or sets the color used to display text.Specified as a System.Windows.Media.Color
        /// , allowing for a wide range of color customization options to enhance the visual appeal 
        /// and readability of text elements in the IAT application.
        /// </summary>
        public Color FontColor { get; set; } = Colors.Black;
    }

    /// <summary>
    /// Specifies the layout mode for arranging keys in a user interface.
    /// </summary>
    /// <remarks>Use this enumeration to control whether keys are displayed horizontally, stacked vertically,
    /// or arranged vertically with an 'or' separator. The selected mode affects the visual organization and user
    /// interaction with key controls.</remarks>
    public enum KeyLayoutMode
    {
        /// <summary>
        /// Specifies a horizontal orientation.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Specifies a vertical stack orientation.
        /// </summary>
        VerticalStack,
        
        /// <summary>
        /// Represents a vertical layout option that combines multiple elements using a logical OR operation.
        /// </summary>
        VerticalWithOr
    }
}
