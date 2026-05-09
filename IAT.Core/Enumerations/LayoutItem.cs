using System;
using System.Collections.Generic;
using System.ComponentModel;  // For [Description]
using System.Linq;
using System.Reflection;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// An enumeration representing different layout items used in the Implicit Association Test (IAT) interface. Each layout item corresponds 
    /// to a specific area of the test interface, such as the stimulus area, response key areas, and instruction areas. This enumeration provides 
    /// a structured way to reference and manage the various layout components within the IAT application, allowing for consistent handling of 
    /// layout-related functionality across different parts of the codebase.
    /// </summary>
    public enum LayoutItem
    {
        /// <summary>
        /// Represents a layout item that defines the interior region.
        /// </summary>
        [Description("The interior layout area of the IAT interface.")]
        Interior,

        /// <summary>
        /// Represents a layout item that identifies the response key in a layout definition.
        /// </summary>
        [Description("The layout area for the left response key.")]
        LeftKey,

        /// <summary>
        /// Represents the layout item corresponding to the right response key.
        /// </summary>
        [Description("The layout area for the right response key.")]
        RightKey,

        /// <summary>
        /// Specifies the layout area used to display the error mark.
        /// </summary>
        [Description("The layout area for the error mark.")]
        ErrorMark,

        /// <summary>
        /// Represents a layout item for displaying a stimulus in the user interface.
        /// </summary>
        [Description("The layout area for the stimulus.")]
        Stimulus,

        /// <summary>
        /// Represents a layout item that displays block-level instructions in the user interface.
        /// </summary>
        [Description("The layout area for block instructions.")]
        BlockInstructions,

        /// <summary>
        /// Represents the layout item for the text instructions screen.
        /// </summary>
        [Description("The layout area for text instructions screens.")]
        TextInstructions,

        /// <summary>
        /// Represents the layout item for the keyed instructions screen.
        /// </summary>
        [Description("The layout area for keyed instructions screens.")]
        KeyedInstructions,

        /// <summary>
        /// Represents a mock layout item for the instructions screen, intended for use in testing or design-time
        /// scenarios.
        /// </summary>
        /// <remarks>This static member provides a predefined instance of a layout item that simulates the
        /// instructions screen. It can be used to facilitate UI testing, prototyping, or demonstration purposes without
        /// requiring actual runtime data.</remarks>
        [Description("The layout area for mock item instructions screens.")]
        MockItemInstructions,

        /// <summary>
        /// Represents the layout item that displays instructions for continuing to the next step.
        /// </summary>
        [Description("The layout area for continue instructions.")]
        ContinueInstructions
    }

    /// <summary>
    /// Extension methods for LayoutItem enum to access descriptions and names.
    /// </summary>
    public static class LayoutItemExtensions
    {
        /// <summary>
        /// Gets the description from the [Description] attribute of the enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The description string, or the enum name if no description is found.</returns>
        public static string GetDescription(this LayoutItem value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Gets the name of the enum value (equivalent to the old Name property).
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The enum name as a string.</returns>
        public static string GetName(this LayoutItem value) => value.ToString();
    }
}