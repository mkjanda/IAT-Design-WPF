using IAT.Core.Models;
using System.Windows;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a named item within a layout, including its description and an observer for its rectangular bounds.
    /// Serves as a base type for specific layout elements used in the application.
    /// </summary>
    /// <remarks>Predefined static instances are provided for common layout items. Use the static method to
    /// retrieve a layout item by name. This type is intended to be extended for specific layout elements.</remarks>
    /// <param name="Name">The unique name that identifies the layout item. This value is used to distinguish between different layout
    /// elements.</param>
    /// <param name="Description">A brief description of the layout item, providing context or purpose for its use within the layout.</param>
    /// <param name="RectangleObserver">An observer that tracks the rectangle representing the item's position and size within the layout.</param>
    public abstract record LayoutItem(String Name, String Description, ValueObserver<Rect> RectangleObserver)
    {
        public Rect BoundingRectangle => RectangleObserver.Value;

        /// <summary>
        /// Represents a layout item that indicates the absence of any layout configuration.
        /// </summary>
        /// <remarks>Use this field to specify that no layout should be applied. This can be useful as a
        /// default or sentinel value when a layout is optional.</remarks>
        public static readonly LayoutItem None = new _None();
        
        /// <summary>
        /// Represents a layout item that corresponds to a lambda expression.
        /// </summary>
        /// <remarks>Use this field to reference a predefined layout item for lambda expressions when
        /// constructing or analyzing layouts. This instance is immutable and can be shared safely across
        /// threads.</remarks>
        public static readonly LayoutItem Lambda = new _Lambda();

        /// <summary>
        /// Represents the layout item used for displaying a stimulus in the user interface.
        /// </summary>
        /// <remarks>This static instance can be used to reference the stimulus layout item when
        /// configuring or querying layouts. The specific behavior and appearance of the stimulus may depend on the
        /// implementation of the associated layout system.</remarks>
        public static readonly LayoutItem Stimulus = new _Stimulus();
        
        /// <summary>
        /// Represents the layout item corresponding to the left response key.
        /// </summary>
        public static readonly LayoutItem LeftResponseKey = new _LeftResponseKey();

        /// <summary>
        /// Represents the layout item corresponding to the right response key.
        /// </summary>
        public static readonly LayoutItem RightResponseKey = new _RightResponseKey();
        
        /// <summary>
        /// Represents the layout item used to display block instructions in the user interface.
        /// </summary>
        public static readonly LayoutItem BlockInstructions = new _BlockInstructions();

        /// <summary>
        /// Represents a layout item that indicates an error state or invalid content within a layout structure.
        /// </summary>
        /// <remarks>Use this value to mark positions in a layout where an error has occurred or where
        /// content is not valid. This can be useful for diagnostics, debugging, or rendering error indicators in user
        /// interfaces.</remarks>
        public static readonly LayoutItem ErrorMark = new _ErrorMark();
        
        /// <summary>
        /// Represents a layout item that displays instructions for continuing to the next step or action.
        /// </summary>
        public static readonly LayoutItem ContinueInstructions = new _ContinueInstructions();

        /// <summary>
        /// Represents a layout item for displaying a screen with text instructions.
        /// </summary>
        public static readonly LayoutItem TextInstructionScreen = new _TextInstructionScreen();

        /// <summary>
        /// Represents a layout item for a screen that displays instructions with key-based navigation.
        /// </summary>
        /// <remarks>Use this layout item to present instructional content where users interact using
        /// keyboard input. This member is typically used in UI frameworks that support customizable layouts.</remarks>
        public static readonly LayoutItem KeyedInstructionScreen = new _KeyedInstructionScreen();

        /// <summary>
        /// Represents a mock layout item containing instructions for testing or demonstration purposes.
        /// </summary>
        /// <remarks>This static instance can be used in scenarios where a placeholder or instructional
        /// layout item is required, such as in unit tests or sample UI configurations.</remarks>
        public static readonly LayoutItem MockItemInstructions = new _MockItemInstructions();

        /// <summary>
        /// Represents the layout item for the left response key outline.
        /// </summary>
        public static readonly LayoutItem LeftResponseKeyOutline = new _LeftResponseKeyOutline();

        /// <summary>
        /// Represents the layout item that outlines the right response key.
        /// </summary>
        public static readonly LayoutItem RightResponseKeyOutline = new _RightResponseKeyOutline();

        /// <summary>
        /// Represents a layout item that occupies the entire window area.
        /// </summary>
        /// <remarks>Use this field to specify that a UI element should fill the available window space.
        /// This is typically used when a single component or control is intended to take up the full client
        /// area.</remarks>
        public static readonly LayoutItem FullWindow = new _FullWindow();

        /// <summary>
        /// Returns the corresponding LayoutItem for the specified name, using a case-insensitive match.
        /// </summary>
        /// <param name="Name">The name of the layout item to retrieve. The comparison is case-insensitive.</param>
        /// <returns>The LayoutItem that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known layout item.</exception>
        static private LayoutItem FromName(string Name) =>
            Name.ToLowerInvariant() switch
            {
                "none" => None,
                "lambda" => Lambda,
                "stimulus" => Stimulus,
                "leftresponsekey" => LeftResponseKey,
                "rightresponsekey" => RightResponseKey,
                "blockinstructions" => BlockInstructions,
                "errormark" => ErrorMark,
                "continueinstructions" => ContinueInstructions,
                "textinstructionscreen" => TextInstructionScreen,
                "keyedinstructionscreen" => KeyedInstructionScreen,
                "mockiteminstructions" => MockItemInstructions,
                "leftresponsekeyoutline" => LeftResponseKeyOutline,
                "rightresponsekeyoutline" => RightResponseKeyOutline,
                "fullwindow" => FullWindow,
                _ => throw new ArgumentException($"Unknown layout item name: {Name}")
            };

        /// <summary>
        /// Represents a layout item that indicates the absence of any layout or value.
        /// </summary>
        /// <remarks>This type can be used as a sentinel or default value when no layout item is
        /// applicable. It is typically used to signify that no layout should be applied or that no value is
        /// present.</remarks>
        private sealed record _None() : LayoutItem("None", "No layout item.", new ValueObserver<Rect>()) { }

        /// <summary>
        /// Represents a layout item for lambda expressions within the layout system.
        /// </summary>
        /// <remarks>This type is used internally to identify and manage layout elements that correspond
        /// to lambda expressions. It is not intended for direct use in application code.</remarks>
        private sealed record _Lambda() : LayoutItem("Lambda", "Lambda layout item.", new ValueObserver<Rect>()) { }

        /// <summary>
        /// Represents a layout item for a stimulus within the layout system.
        /// </summary>
        /// <remarks>This type is used internally to define a stimulus element in the layout. It is not
        /// intended to be used directly in application code.</remarks>
        private sealed record _Stimulus() : LayoutItem("Stimulus", "Stimulus layout item.", new ValueObserver<Rect>())  { }

        /// <summary>
        /// Represents a layout item for the left response key, providing metadata and value observation for the
        /// associated rectangle.
        /// </summary>
        private sealed record _LeftResponseKey() : LayoutItem("LeftResponseKey", "Left response key layout item.", new ValueObserver<Rect>()) { }

        /// <summary>
        /// Represents a layout item for the right response key.
        /// </summary>
        private sealed record _RightResponseKey() : LayoutItem("RightResponseKey", "Right response key layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item for block instructions within the user interface.
        /// </summary>
        /// <remarks>This type is intended for internal use to define the structure and behavior of block
        /// instruction layout elements. It is not intended to be used directly in application code.</remarks>
        private sealed record _BlockInstructions() : LayoutItem("BlockInstructions", "Block instructions layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item used to indicate an error state within the layout system.
        /// </summary>
        /// <remarks>This type is intended for internal use to mark areas where an error has occurred
        /// during layout processing. It is not intended for direct use in application code.</remarks>
        private sealed record _ErrorMark() : LayoutItem("ErrorMark", "Error mark layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item for displaying continue instructions within the user interface.
        /// </summary>
        /// <remarks>This record is intended for use in UI layouts where a standardized continue
        /// instructions element is required. It derives from LayoutItem and is not intended to be instantiated directly
        /// by consumers.</remarks>
        private sealed record _ContinueInstructions() : LayoutItem("ContinueInstructions", "Continue instructions layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item for displaying a text instruction screen within the user interface.
        /// </summary>
        /// <remarks>This record is intended for use in UI layouts where a dedicated screen for text-based
        /// instructions is required. It provides a predefined configuration for such screens and is typically used as
        /// part of a larger layout composition.</remarks>
        private sealed record _TextInstructionScreen() : LayoutItem("TextInstructionScreen", "Text instruction screen layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item for a keyed instruction screen.
        /// </summary>
        /// <remarks>This record is used to define a specific layout item identified as a keyed
        /// instruction screen. It is intended for scenarios where a unique screen layout is required to display
        /// instructions associated with a particular key or context. This type is sealed and not intended for
        /// inheritance.</remarks>
        private sealed record _KeyedInstructionScreen() : LayoutItem("KeyedInstructionScreen", "Keyed instruction screen layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item used for mock item instructions in the UI.
        /// </summary>
        private sealed record _MockItemInstructions() : LayoutItem("MockItemInstructions", "Mock item instructions layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item for outlining the left response key.
        /// </summary>
        private sealed record _LeftResponseKeyOutline() : LayoutItem("LeftResponseKeyOutline", "Left response key outline layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item for outlining the right response key.
        /// </summary>
        /// <remarks>This record is used to define the layout and observation behavior for the right
        /// response key outline within the UI. It is intended for use in scenarios where the right response key's
        /// visual outline needs to be tracked or rendered.</remarks>
        private sealed record _RightResponseKeyOutline() : LayoutItem("RightResponseKeyOutline", "Right response key outline layout item.", new ValueObserver<Rect>()) { };

        /// <summary>
        /// Represents a layout item that occupies the entire window area.
        /// </summary>
        /// <remarks>Use this type to define a layout that spans the full available window space. This
        /// layout is typically used when no other layout constraints are required.</remarks>
        private sealed record _FullWindow() : LayoutItem("FullWindow", "Full window layout item.", new ValueObserver<Rect>()) { };
    }
}
