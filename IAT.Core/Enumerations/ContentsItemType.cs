using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Domain;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// An enumeration of classifications for different types of content items that can be included in an IAT test. Each item type has associated 
    /// metadata such as a name, description, and flags indicating whether it is a header item or expandable. The AssociatedType property indicates 
    /// the .NET type that corresponds to each content item type, facilitating type-safe handling of content items in the application.
    /// </summary>
    /// <param name="Name">The name of the contents item</param>
    /// <param name="Description">A brief description of the contents item</param>
    /// <param name="IsHeaderItem">Denotes whether the contents item is a header item</param>
    /// <param name="IsExpandable">Denotes whether the contents item can be expanded</param>
    /// <param name="AssociatedType">The object type associated with the contents item</param>
    public abstract record ContentsItemType(string Name, string Description, bool IsHeaderItem, bool IsExpandable, Type AssociatedType)
    {
        /// <summary>
        /// Represents a contents item type for an IAT block.
        /// </summary>
        public static readonly ContentsItemType IATBlock = new _IATBlock();

        /// <summary>
        /// Represents a content item of type Trial.
        /// </summary>
        public static readonly ContentsItemType Trial = new _Trial();

        /// <summary>
        /// Represents a contents item type for a blank instruction screen.
        /// </summary>
        public static readonly ContentsItemType BlankInstructionScreen = new _BlankInstructionScreen();

        /// <summary>
        /// Represents a content item type for a screen that displays text instructions.
        /// </summary>
        public static readonly ContentsItemType TextInstructionScreen = new _TextInstructionScreen();

        /// <summary>
        /// Represents a content item type for a keyed instruction screen. This field is read-only.
        /// </summary>
        public static readonly ContentsItemType KeyedInstructionSreen = new _KeyedInstructionSreen();

        /// <summary>
        /// Represents a mock instruction screen item used for testing or demonstration purposes.
        /// </summary>
        public static readonly ContentsItemType MockItemInstructionScreen = new _MockItemInstructionScreen();

        /// <summary>
        /// Represents a content item type for surveys.
        /// </summary>
        /// <remarks>Use this field to identify or filter content items that are surveys within the
        /// application.</remarks>
        public static readonly ContentsItemType Survey = new _Survey();

        /// <summary>
        /// Returns the corresponding ContentsItemType value for the specified name.
        /// </summary>
        /// <remarks>The name comparison is performed using a case-insensitive match based on the
        /// invariant culture. Valid names include "iatblock", "trial", "blankinstructionscreen",
        /// "textinstructionscreen", "keyedinstructionscreen", "mockiteminstructionscreen", and "survey".</remarks>
        /// <param name="name">The name of the contents item type to retrieve. The comparison is case-insensitive.</param>
        /// <returns>The ContentsItemType value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known ContentsItemType.</exception>
        public static ContentsItemType FromName(String name) =>
            name.ToLowerInvariant() switch
            {
                "iatblock" => IATBlock,
                "trial" => Trial,
                "blankinstructionscreen" => BlankInstructionScreen,
                "textinstructionscreen" => TextInstructionScreen,
                "keyedinstructionscreen" => KeyedInstructionSreen,
                "mockiteminstructionscreen" => MockItemInstructionScreen,
                "survey" => Survey,
                _ => throw new ArgumentException($"Unknown ContentsItemType name: {name}")
            };


        private sealed record _IATBlock() : ContentsItemType("IATBlock", "A block containing trials for an Implicit Association Test.", true, true, typeof(Block));
        private sealed record _Trial() : ContentsItemType("Trial", "A single trial within a block, representing a stimulus presentation and response.", false, false, typeof(Trial));
        private sealed record _BlankInstructionScreen() : ContentsItemType("BlankInstructionScreen", "An instruction screen with no content, used for pacing or breaks.", false, false, 
            typeof(InstructionsScreen));
        private sealed record _TextInstructionScreen() : ContentsItemType("TextInstructionScreen", "An instruction screen containing text content for participant guidance.", false, false, 
            typeof(TextInstructionsScreen));
        private sealed record _KeyedInstructionSreen() : ContentsItemType("KeyedInstructionScreen", "An instruction screen that includes response key information for participant guidance.", false, false,
            typeof(KeyedInstructionsScreen));
        private sealed record _MockItemInstructionScreen() : ContentsItemType("MockItemInstructionScreen", "An instruction screen that simulates a trial item to demonstrate the task to participants.", false, false,
            typeof(MockItemInstructionsScreen));
        private sealed record _Survey() : ContentsItemType("Survey", "A survey item used to collect participant responses or demographic information.", true, false,
            typeof(Survey));
    }
}
