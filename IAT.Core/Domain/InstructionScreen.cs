using IAT.Core.Enumerations;
using IAT.Core.Models;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a screen that displays instructions to the user, including information on how to continue past the
    /// instruction screen.
    /// </summary>
    /// <remarks>
    /// Concrete subtypes are <see cref="TextInstructionScreen"/>, <see cref="KeyedInstructionScreen"/>,
    /// and <see cref="MockItemInstructionScreen"/>. Polymorphism is required for package save/load —
    /// without the discriminator, System.Text.Json only persists base properties and reconstitutes
    /// every screen as the base type (losing keys, stimulus, direction, etc.).
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "InstructionType")]
    [JsonDerivedType(typeof(TextInstructionScreen), "Text")]
    [JsonDerivedType(typeof(KeyedInstructionScreen), "Keyed")]
    [JsonDerivedType(typeof(MockItemInstructionScreen), "MockItem")]
    public class InstructionScreen : IFormattedText
    {
        /// <summary>
        /// Gets or sets the unique identifier for the instructions resource associated with this instance.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets a value indicating whether this item is a header item.
        /// </summary>
        [JsonIgnore]
        public bool IsHeaderItem => false;

        /// <summary>
        /// Gets a value indicating whether the current item can be expanded to show additional content or children.
        /// </summary>
        [JsonIgnore]
        public bool IsExpandable => false;

        /// <summary>
        /// The key used to continue past this instruction screen.
        /// Currently fixed to Space (" ") for all screens — this matches standard IAT practice
        /// and avoids UX problems with invisible characters or free-text input.
        /// Must be a property (not a field) so System.Text.Json includes it in the package.
        /// </summary>
        public string ContinueKey { get; set; } = " ";

        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text style applied to the content.
        /// </summary>
        public TextStyle Style { get; set; } = new TextStyle()
        {
            FontFamily = "Arial",
            FontSize = 48.0,
            FontColor = Colors.White
        };

        /// <summary>
        /// Layout role for this instruction screen. Derived types override with the specific band
        /// (TextInstructions / KeyedInstructions / MockItemInstructions).
        /// </summary>
        public virtual LayoutItem LayoutItem { get; init; } = LayoutItem.Interior;

        /// <summary>
        /// Continue-prompt text shown at the bottom of the stage (e.g. "Press the spacebar to continue").
        /// Must be a property so package serialization persists the author-edited string.
        /// </summary>
        public FormattedText ContinueInstructions { get; set; } = new FormattedText()
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
        public Guid PreviewId { get; set; } = Guid.Empty;

        /// <summary>
        /// Validates the current instruction screen definition and returns the result of the validation.
        /// </summary>
        public virtual ValidationResult Validate()
        {
            if (ContinueInstructions is null || ContinueInstructions.Text == string.Empty)
                return ValidationResult.Fail("Continue instructions text cannot be empty");
            return ValidationResult.Success;
        }
    }
}
