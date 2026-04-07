using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a named element within a layout, such as a region for stimuli, instructions, or user interaction
    /// areas.
    /// </summary>
    /// <remarks>Use the predefined static instances to refer to common layout elements, or create custom
    /// elements as needed. The class provides methods to retrieve layout elements by name or type. This type is
    /// intended to be used as a base for specific layout element records.</remarks>
    /// <param name="Name">The unique name that identifies the layout element. This value is used to distinguish between different types of
    /// layout regions.</param>
    /// <param name="Description">A description of the layout element's purpose or usage within the layout.</param>
    public abstract record LayoutElement(string Name, string Description)
    {

        /// <summary>
        /// Represents a sentinel value indicating that no layout has been set.
        /// </summary>
        /// <remarks>Use this value to distinguish between an explicitly set layout and an unset or
        /// default state. This field is typically used for comparison or initialization scenarios where the absence of
        /// a layout needs to be represented.</remarks>
        public static readonly LayoutElement Unset = new _Unset();

        /// <summary>
        /// Represents a layout element that defines the interior region within a container or layout structure.
        /// </summary>
        /// <remarks>Use this element to specify content that should be placed in the main interior area,
        /// excluding any borders, headers, or other peripheral regions. The exact behavior and appearance may depend on
        /// the layout system in use.</remarks>
        public static readonly LayoutElement Interior = new _Interior();
        
        /// <summary>
        /// Represents a layout element that displays a keyboard key symbol or label.
        /// </summary>
        /// <remarks>Use this element to visually indicate keyboard shortcuts or key presses in user
        /// interface layouts or documentation. The appearance and behavior may vary depending on the layout system or
        /// platform.</remarks>
        public static readonly LayoutElement Key = new _Key();

        /// <summary>
        /// Represents a layout element that defines the outline of a key.
        /// </summary>
        public static readonly LayoutElement KeyOutline = new _KeyOutline();

        /// <summary>
        /// Represents a layout element that defines the stimulus component in the user interface.
        /// </summary>
        public static readonly LayoutElement Stimulus = new _Stimulus();

        /// <summary>
        /// Represents a special layout element used to indicate an error state in the layout structure.
        /// </summary>
        /// <remarks>This element can be used as a placeholder or marker when an error occurs during
        /// layout processing. Consumers can check for this value to detect and handle layout errors
        /// appropriately.</remarks>
        public static readonly LayoutElement ErrorMark = new _ErrorMark();

        /// <summary>
        /// Represents a layout element that displays block-level instructions within the user interface.
        /// </summary>
        /// <remarks>Use this element to present instructional content that should be visually separated
        /// from other UI components. This element is typically used to guide users through multi-step processes or to
        /// provide contextual help.</remarks>
        public static readonly LayoutElement BlockInstructions = new _BlockInstructions();

        /// <summary>
        /// Represents a layout element that displays instructional text to the user.
        /// </summary>
        /// <remarks>Use this element to present guidance or instructions within a user interface layout.
        /// The content and formatting of the instructional text are defined by the implementation of the layout
        /// element.</remarks>
        public static readonly LayoutElement InstructionsText = new _InstructionsText();

        /// <summary>
        /// Represents the layout element that displays key instructions text in the user interface.
        /// </summary>
        public static readonly LayoutElement KeyInstructionsText = new _KeyInstructionsText();

        /// <summary>
        /// Represents a layout element used for displaying mock item text in the user interface.
        /// </summary>
        public static readonly LayoutElement MockItemText= new _MockItemText();

        /// <summary>
        /// Represents a layout element that displays instructions for continuing to the next step or screen.
        /// </summary>
        /// <remarks>Use this element to provide users with guidance on how to proceed within a workflow
        /// or user interface. The specific instructions and appearance are determined by the implementation of the
        /// layout element.</remarks>
        public static readonly LayoutElement ContinueInstructions = new _ContinueInstructions();

        /// <summary>
        /// Represents a layout element that displays a thumbnail image or preview.
        /// </summary>
        /// <remarks>Use this element to present a visual summary or preview of content, such as an image
        /// or video frame, within a layout. The specific appearance and behavior may depend on the layout system in
        /// use.</remarks>
        public static readonly LayoutElement Thumbnail = new _Thumbnail();

        /// <summary>
        /// Creates a LayoutElement value from its string representation.
        /// </summary>
        /// <param name="name">The name of the layout element to parse. The comparison is case-insensitive.</param>
        /// <returns>A LayoutElement value corresponding to the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known layout element.</exception>
        public static LayoutElement FromString(string name) =>
            name?.ToLowerInvariant() switch
            {
                "Unset" => Unset,
                "interior" => Interior,
                "key" => Key,
                "keyoutline" => KeyOutline,
                "stimulus" => Stimulus,
                "errormark" => ErrorMark,
                "blockinstructions" => BlockInstructions,
                "instructionstext" => InstructionsText,
                "keyinstructionstext" => KeyInstructionsText,
                "mockitemtext" => MockItemText,
                "continueinstructions" => ContinueInstructions,
                "thumbnail" => Thumbnail,
                _ => throw new ArgumentException($"Unknown layout element: {name}")
            };

        /// <summary>
        /// Returns the corresponding layout element for the specified type.
        /// </summary>
        /// <param name="type">The type whose name determines the layout element to return. The type name is matched case-insensitively
        /// against known layout element types.</param>
        /// <returns>The layout element associated with the specified type. If the type name does not match a known layout
        /// element, an exception is thrown.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified type does not correspond to a known layout element.</exception>
        public static LayoutElement FroomType(Type type) =>
            type.Name.ToLowerInvariant() switch
            {
                "unset" => Unset,
                "interior" => Interior,
                "key" => Key,
                "keyoutline" => KeyOutline,
                "stimulus" => Stimulus,
                "errormark" => ErrorMark,
                "blockinstructions" => BlockInstructions,
                "instructionstext" => InstructionsText,
                "keyinstructionstext" => KeyInstructionsText,
                "mockitemtext" => MockItemText,
                "continueinstructions" => ContinueInstructions,
                "thumbnail" => Thumbnail,
                _ => throw new ArgumentException($"Unknown layout element type: {type.Name}")
            };


        /// <summary>
        /// Represents a sentinel value indicating that no layout has been set.
        /// </summary>
        /// <remarks>This type is used internally to distinguish between an explicitly unset layout and
        /// other layout states. It is not intended for direct use in application code.</remarks>
        private sealed record _Unset() : LayoutElement("Unset", "A sentinel value indicating that no layout has been set.");

        /// <summary>
        /// Represents the interior area of a layout, typically used for displaying stimuli or primary content.
        /// </summary>
        /// <remarks>Use this element to define the main content region within a layout structure. It is
        /// commonly utilized as the central area for presenting information or interactive elements, distinct from
        /// headers, footers, or side panels.</remarks>
        private sealed record _Interior() : LayoutElement("Interior", "The interior area of the layout, typically used for displaying stimuli or content.");
        
        /// <summary>
        /// Represents a layout element that defines the area reserved for key responses or interactions.
        /// </summary>
        /// <remarks>Use this type to identify and work with regions in a layout where key-based input or
        /// actions are expected. This element is typically used in scenarios involving custom layouts that require
        /// explicit handling of key interactions.</remarks>
        private sealed record _Key() : LayoutElement("Key", "The area designated for key responses or interactions within the layout.");
        
        /// <summary>
        /// Represents the outline or border area surrounding the key response section in the layout.
        /// </summary>
        /// <remarks>Use this type to define or reference the visual boundary around the key response area
        /// when constructing or analyzing layout elements. This record is typically used in layout composition
        /// scenarios where distinguishing the key outline is necessary for rendering or interaction purposes.</remarks>
        private sealed record _KeyOutline() : LayoutElement("KeyOutline", "The outline or border area around the key response section of the layout.");

        /// <summary>
        /// Represents the layout element designated for presenting stimuli to the user.
        /// </summary>
        /// <remarks>Use this type to identify or configure the area of a layout where stimulus content
        /// should be displayed. This element is typically used in user interfaces that require a dedicated region for
        /// dynamic or interactive stimuli.</remarks>
        private sealed record _Stimulus() : LayoutElement("Stimulus", "The area of the layout where stimuli are presented to the user.");

        /// <summary>
        /// Represents a layout element that indicates an error state within the layout structure.
        /// </summary>
        /// <remarks>This type is used internally to mark locations in the layout where an error has
        /// occurred. It can be useful for diagnostic or validation scenarios where error tracking within the layout is
        /// required.</remarks>
        private sealed record _ErrorMark() : LayoutElement("ErrorMark", "A special layout element used to indicate an error state in the layout structure.");

        /// <summary>
        /// Represents a layout element used to display block-level instructions within the user interface.
        /// </summary>
        /// <remarks>Use this type to define areas in the UI where instructional content or guidance for a
        /// block of controls or content should be presented. This element is typically used to improve user
        /// understanding by providing context-specific instructions at the block level.</remarks>
        private sealed record _BlockInstructions() : LayoutElement("BlockInstructions", "The area designated for displaying block-level instructions within the user interface.");

        /// <summary>
        /// Represents a layout element used to display instructional text or guidance to the user within the interface.
        /// </summary>
        /// <remarks>Use this element to provide users with context-sensitive instructions or help
        /// messages. It is typically placed in areas where additional guidance is needed to assist user
        /// interaction.</remarks>
        private sealed record _InstructionsText() : LayoutElement("InstructionsText", "The area designated for displaying instructions or guidance to the user.");

        /// <summary>
        /// Represents a layout element for displaying instructions related to key responses or interactions.
        /// </summary>
        /// <remarks>Use this element to present guidance or prompts that inform users about keyboard
        /// actions or shortcuts within the user interface. This type is intended for scenarios where clear instructions
        /// about key-based interactions are required.</remarks>
        private sealed record _KeyInstructionsText() : LayoutElement("KeyInstructionsText", "The area for displaying instructions specifically related to key responses or interactions.");

        /// <summary>
        /// Represents a layout element used for displaying text associated with mock items or practice trials.
        /// </summary>
        /// <remarks>Use this element to present instructional or descriptive text relevant to mock items
        /// within a layout. This type is typically used in scenarios where users need guidance or context before
        /// engaging with actual items.</remarks>
        private sealed record _MockItemText() : LayoutElement("MockItemText", "The area used for displaying text related to mock items or practice trials within the layout.");
        
        /// <summary>
        /// Represents a layout element for displaying instructions or prompts that guide users to continue to the next
        /// section or trial.
        /// </summary>
        /// <remarks>Use this element to present context-specific guidance or actions required before
        /// proceeding. This is typically placed at transition points within a user interface layout.</remarks>
        private sealed record _ContinueInstructions() : LayoutElement("ContinueInstructions", 
            "The area designated for displaying instructions or prompts related to continuing to the next section or trial in the layout.");

        /// <summary>
        /// Represents a layout element used for displaying thumbnail images or visual representations within a layout.
        /// </summary>
        /// <remarks>Use this type to include thumbnail visuals, such as image previews or icons, as part
        /// of a larger layout structure. This element is typically used to provide a compact visual summary of content
        /// or objects within the user interface.</remarks>
        private sealed record _Thumbnail() : LayoutElement("Thumbnail", "A layout element used for displaying thumbnail images or representations within the layout.");
    }
}
