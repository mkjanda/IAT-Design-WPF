using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.IO;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a screen that displays instructions to the user, including information on how to continue past the
    /// instruction screen.
    /// </summary>
    /// <remarks>The InstructionsScreen class provides properties for configuring the instructional text, the
    /// key used to continue, and identifiers for the resource and preview instances. It also includes validation logic
    /// to ensure the instruction screen is properly defined. This class is typically used in user interfaces where
    /// clear instructions and a defined continuation action are required.</remarks>
    public class InstructionScreen : IFormattedText
    {
        /// <summary>
        /// Gets or sets the unique identifier for the instructions resource associated with this instance.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets a value indicating whether this item is a header item.
        /// </summary>
        public bool IsHeaderItem => false;

        /// <summary>
        /// Gets a value indicating whether the current item can be expanded to show additional content or children.
        /// </summary>
        public bool IsExpandable => false;

        /// <summary>
        /// Gets or sets the key used to continue an operation or process.
        /// </summary>
        public string ContinueKey = " ";

        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text style applied to the content.
        /// </summary>
        /// <remarks>Use this property to customize the appearance of the text, such as font family, size,
        /// and color. Changing the style will affect how the text is rendered.</remarks>
        public TextStyle Style { get; set; } = new TextStyle()
        {
            FontFamily = "Arial",
            FontSize = 48.0,
            FontColor = Colors.White
        };

        /// <summary>
        /// The layout item associated with this instruction screen. This property determines the position and arrangement of the 
        /// instruction screen within the overall user interface layout. The value of this property should correspond to a valid layout 
        /// item defined in the application's layout system, ensuring that the instruction screen is displayed correctly and consistently 
        /// with other UI elements. By setting this property, developers can control where the instruction screen appears on the screen, 
        /// allowing for a more organized and user-friendly interface design.
        /// </summary>
        public virtual LayoutItem LayoutItem { get; init; } = LayoutItem.Interior;


        /// <summary>
        /// The text that provides instructions to the user on how to continue past the instruction screen. This text is typically 
        /// displayed prominently on the screen and should clearly communicate the required action (e.g., "Press the spacebar to continue"). 
        /// It is important to ensure that the instructions are concise, easy to understand, and appropriately formatted for the target audience.
        /// </summary>
        public FormattedText ContinueInstructions = new FormattedText()
        {
            Text = "Press the spacebar to continue",
            Style = new TextStyle()
            {
                FontFamily = "Arial",
                FontSize = 48.0,
                FontColor = Colors.White
            },
            LayoutItem = LayoutItem.ContinueInstructions
        };

        /// <summary>
        /// Gets or sets the unique identifier for the preview instance.
        /// </summary>
        public Guid PreviewId = Guid.Empty;

        /// <summary>
        /// Validates the current instruction screen definition and returns the result of the validation.
        /// </summary>
        /// <returns>A ValidationResult indicating whether the instruction screen definition is valid. Returns
        /// ValidationResult.Success if validation passes.</returns>
        /// <exception cref="Exception">Thrown if the instruction screen type is blank or if the continue instructions text is empty.</exception>
        public virtual ValidationResult Validate()
        {
            if (ContinueInstructions.Text == string.Empty)
                return ValidationResult.Fail("Continue instructions text cannot be empty");
            return ValidationResult.Success;
        }
    }
}
