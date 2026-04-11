using java.util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a type of image media used for displaying various visual elements within the application, each with
    /// specific sizing and display characteristics.
    /// </summary>
    /// <remarks>Use the predefined static instances to reference common image media types. The sizing
    /// parameters and associated DI types determine how and where each media type is used within the application's UI.
    /// This type is intended to be extended for specific media scenarios.</remarks>
    /// <param name="Name">The unique name that identifies the image media type.</param>
    /// <param name="Description">A description of the image media type and its intended usage.</param>
    /// <param name="InitialSize">The initial size, in pixels, used when displaying this media type.</param>
    /// <param name="IdealSize">The ideal size, in pixels, recommended for optimal display of this media type.</param>
    /// <param name="MaxSizee">The maximum allowable size, in pixels, for this media type.</param>
    /// <param name="GrowRate">The rate at which the media type can increase in size, in pixels per operation.</param>
    /// <param name="ShrinkRate">The rate at which the media type can decrease in size, in pixels per operation.</param>
    /// <param name="LayoutItem">Identifies the layout object the item is aassociated with</param>
    /// <param name="DITypes">An array of DIType values that specify the data item types associated with this media type.</param>
    public abstract record ImageMediaType(String Name, string Description, int InitialSize, int IdealSize, int MaxSizee, int GrowRate, int ShrinkRate, LayoutItem LayoutItem, DIType[] DITypes)
    {
        public static readonly ImageMediaType Unset = new _Unset();

        /// <summary>
        /// Represents an image media type that displays content in a full window layout.
        /// </summary>
        public static readonly ImageMediaType FullWindow = new _FullWindow();

        /// <summary>
        /// Gets the media type that represents the response key for image content.
        /// </summary>
        public static readonly ImageMediaType ResponseKey = new _ResponseKey();

        /// <summary>
        /// Represents the media type for a response key outline image. 
        /// </summary>
        /// <remarks>Use this field to specify or compare against the response key outline image media
        /// type in operations that require explicit media type handling.</remarks>
        public static readonly ImageMediaType ResponseKeyOutline = new _ResponseKeyOutline();

        /// <summary>
        /// Represents the image media type for stimulus images used in the application.
        /// </summary>
        /// <remarks>Use this field when specifying or comparing image media types that are designated as
        /// stimulus images. This value is immutable and can be used for equality checks or as a constant reference
        /// throughout the application.</remarks>
        public static readonly ImageMediaType Stimulus = new _Stimulus();

        /// <summary>
        /// Represents a special image media type used to indicate an error or invalid state.
        /// </summary>
        /// <remarks>This value can be used as a placeholder when an image media type cannot be determined
        /// or when an error occurs during processing. It is intended to signal error conditions in image handling
        /// workflows.</remarks>
        public static readonly ImageMediaType ErrorMark = new _ErrorMark();

        /// <summary>
        /// Represents the media type for block instructions images.
        /// </summary>
        public static readonly ImageMediaType BlockIstructions = new _BlockInstructions();

        /// <summary>
        /// Represents the media type for a text instruction screen image.
        /// </summary>
        /// <remarks>Use this value when specifying or comparing image media types that correspond to
        /// text-based instruction screens. This field is immutable and can be used for equality checks or as a constant
        /// reference throughout the application.</remarks>
        public static readonly ImageMediaType TextInstructionScreen = new _TextInstructionScreen();

        /// <summary>
        /// Represents the media type for a keyed instruction screen image.
        /// </summary>
        /// <remarks>Use this field to specify that an image is intended to be displayed as a keyed
        /// instruction screen. This value is typically used when categorizing or processing images by their intended
        /// usage within an application.</remarks>
        public static readonly ImageMediaType KeyedInstructionScreen = new _KeyedInstructionScreen();

        /// <summary>
        /// Represents the media type for mock item instructions used in testing or development scenarios.
        /// </summary>
        /// <remarks>This static instance can be used to identify or handle mock item instruction media
        /// types within the application. It is intended for use in non-production contexts such as automated tests or
        /// development tools.</remarks>
        public static readonly ImageMediaType MockItemInstructions = new _MockItemInstructions();

        /// <summary>
        /// Represents the media type for images that provide continue instructions.
        /// </summary>
        public static readonly ImageMediaType ContinueInstructions = new _ContinueInstructions();

        /// <summary>
        /// Represents an image media type with variable dimensions.
        /// </summary>
        /// <remarks>Use this field when the image size is not fixed and may vary depending on the context
        /// or source. This is useful for scenarios where image dimensions are determined at runtime or are not
        /// constrained by a predefined format.</remarks>
        public static readonly ImageMediaType VariableSize = new _VariableSize();

        /// <summary>
        /// Represents the media type for thumbnail images.
        /// </summary>
        public static readonly ImageMediaType Thumbnail = new _Thumbnail();

        /// <summary>
        /// Represents a null or uninitialized image media type instance.
        /// </summary>
        /// <remarks>Use this field to indicate the absence of a valid image media type. This can be
        /// useful when a method or property requires an explicit representation of 'no value' rather than
        /// null.</remarks>
        public static readonly ImageMediaType Null = new _Null();

        /// <summary>
        /// Returns the corresponding ImageMediaType value for the specified name.  
        /// </summary>
        /// <remarks>The method performs a case-insensitive match on the provided name. Use this method to
        /// convert a string representation of an image media type to its corresponding enumeration value.</remarks>
        /// <param name="name">The name of the image media type to retrieve. The comparison is case-insensitive.</param>
        /// <returns>The ImageMediaType value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if no ImageMediaType with the specified name exists.</exception>
        public static ImageMediaType FromName(String name)
        {
            return name.ToLowerInvariant() switch
            {
                "fullwindow" => FullWindow,
                "responsekey" => ResponseKey,
                "responsekeyoutline" => ResponseKeyOutline,
                "stimulus" => Stimulus,
                "errormark" => ErrorMark,
                "blockinstructions" => BlockIstructions,
                "textinstructionScreen" => TextInstructionScreen,
                "keyedinstructionScreen" => KeyedInstructionScreen,
                "mockiteminstructions" => MockItemInstructions,
                "continueinstructions" => ContinueInstructions,
                "variablesize" => VariableSize,
                "rhumbnail" => Thumbnail,
                "null" => Null,
                "unset" => Unset,
                _ => throw new ArgumentException($"No ImageMediaType with the name '{name}' exists.")
            };
        }

        /// <summary>
        /// Returns the corresponding ImageMediaType value for the specified type.
        /// </summary>
        /// <remarks>Use this method to obtain the ImageMediaType enumeration value that matches a given
        /// type. This is useful when working with types that represent different image media categories and a mapping
        /// to the enumeration is required.</remarks>
        /// <param name="type">The type to map to an ImageMediaType value. Must represent a supported image media type.</param>
        /// <returns>The ImageMediaType value associated with the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified type does not correspond to any supported ImageMediaType.</exception>
        public static ImageMediaType FromType(Type type)
        {
            if (type == typeof(_FullWindow))
                return FullWindow;
            else if (type == typeof(_ResponseKey))
                return ResponseKey;
            else if (type == typeof(_ResponseKeyOutline))
                return ResponseKeyOutline;
            else if (type == typeof(_Stimulus))
                return Stimulus;
            else if (type == typeof(_ErrorMark))
                return ErrorMark;
            else if (type == typeof(_BlockInstructions))
                return BlockIstructions;
            else if (type == typeof(_TextInstructionScreen))
                return TextInstructionScreen;
            else if (type == typeof(_KeyedInstructionScreen))
                return KeyedInstructionScreen;
            else if (type == typeof(_MockItemInstructions))
                return MockItemInstructions;
            else if (type == typeof(_ContinueInstructions))
                return ContinueInstructions;
            else if (type == typeof(_VariableSize))
                return VariableSize;
            else if (type == typeof(_Thumbnail))
                return Thumbnail;
            else if (type == typeof(_Null))
                return Null;
            else if (type == typeof(_Unset))
                return Unset;
            else
                throw new ArgumentException($"No ImageMediaType with the type '{type.Name}' exists.");
        }

        /// <summary>
        /// Represents a media type for displaying content that occupies the entire application window.
        /// </summary>
        /// <remarks>Use this type when content should be rendered to fill the full window area, without
        /// margins or borders. This is typically used for immersive or full-screen display scenarios.</remarks>
        private sealed record _FullWindow() : ImageMediaType("FullWindow", "Represents the full window media type, which is used for displaying content that occupies the entire application window.",
            10, 10, 100, 20, 5, LayoutItem.FullWindow, [ DIType.LambdaGenerated, DIType.Preview ]) { }

        /// <summary>
        /// Represents the response key media type used for displaying content related to response keys within the
        /// application.
        /// </summary>
        /// <remarks>This type is typically used to identify and handle response key elements in the
        /// application's user interface. It is a specialized media type that supports various display and interaction
        /// scenarios involving response keys.</remarks>
        private sealed record _ResponseKey() : ImageMediaType("ResponseKey", "Represents the response key media type, which is used for displaying content related to response keys in the application.",
            10, 0, 30, 5, 5, LayoutItem.LeftResponseKey, [ DIType.DualKey, DIType.ResponseKeyText, DIType.ResponseKeyImage, DIType.Conjunction ]) { }

        /// <summary>
        /// Represents the response key outline media type used for displaying outlines around response keys in the
        /// application.
        /// </summary>
        /// <remarks>This type is typically used to visually distinguish response keys by outlining them
        /// in the user interface. It is a specialized media type intended for scenarios where highlighting or
        /// emphasizing response keys is required.</remarks>
        private sealed record _ResponseKeyOutline() : ImageMediaType("ResponseKeyOutline", "Represents the response key outline media type, which is used for displaying outlines around response keys in the application.",
            2, 2, 10, 1, 1, LayoutItem.LeftResponseKey, [ DIType.LeftKeyValueOutline, DIType.RightKeyValueOutline]) { }

        /// <summary>
        /// Represents the stimulus media type used for displaying stimuli within the application.
        /// </summary>
        /// <remarks>This type defines the characteristics and layout for stimulus media, such as images
        /// or text, that are presented to users as part of the application's interactive content. It is intended for
        /// use in scenarios where stimuli need to be rendered according to specific layout and data integration
        /// requirements.</remarks>
        private sealed record _Stimulus() : ImageMediaType("Stimulus", "Represents the stimulus media type, which is used for displaying stimuli in the application.",
            25, 15, 100, 20, 10, LayoutItem.Stimulus, [ DIType.StimulusImage, DIType.StimulusText]) { }

        /// <summary>
        /// Represents the error mark media type used for displaying error indicators within the application.
        /// </summary>
        /// <remarks>This type is typically used to visually indicate errors in the user interface. It is
        /// intended for internal use when rendering error states or invalid elements. The error mark media type may be
        /// used in conjunction with other layout elements to provide clear feedback to users about issues that require
        /// attention.</remarks>
        private sealed record _ErrorMark() : ImageMediaType("ErrorMark", "Represents the error mark media type, which is used for displaying error marks in the application.",
            1, 1, 10, 1, 1, LayoutItem.ErrorMark, [ DIType.ErrorMark ]) { }   

        /// <summary>
        /// Represents the block instructions media type, which is used for displaying instructions related to blocks in
        /// the application.
        /// </summary>
        private sealed record _BlockInstructions() : ImageMediaType("BlockInstructions", "Represents the block instructions media type, which is used for displaying instructions related to blocks in the application.",
            10, 10, 25, 5, 5, LayoutItem.BlockInstructions, [ DIType.IatBlockInstructions ]) { }

        /// <summary>
        /// Represents the text instruction screen media type used for displaying text-based instructions on the
        /// instruction screen within the application.
        /// </summary>
        /// <remarks>This type is intended for scenarios where instructional content needs to be presented
        /// as text to users. It is typically used in user interfaces that require clear, readable instructions as part
        /// of the workflow.</remarks>
        private sealed record _TextInstructionScreen() : ImageMediaType("TextInstructionScreen", "Represents the text instruction screen media type, which is used for displaying text instructions on the instruction screen in the application.",
            5, 5, 25, 5, 5, LayoutItem.TextInstructionScreen, [ DIType.TextInstructionsScreen]) { }

        /// <summary>
        /// Represents the media type for the keyed instruction screen, which is used to display instructions related to
        /// keyed responses within the application's instruction screen.
        /// </summary>
        /// <remarks>This type is intended for scenarios where the application needs to present
        /// instructions that require user input via keyed responses. It is a specialized media type used internally to
        /// distinguish instruction screens that support keyed interactions from other instruction or media
        /// types.</remarks>
        private sealed record _KeyedInstructionScreen() : ImageMediaType("KeyedInstructionScreen", "Represents the keyed instruction screen media type, which is used for displaying instructions related to keyed responses on the instruction screen in the application.",

            5, 5, 25, 5, 5, LayoutItem.KeyedInstructionScreen, [ DIType.KeyedInstructionsScreen]) { }

        /// <summary>
        /// Represents the mock item instructions media type, which is used for displaying instructions related to mock
        /// items in the application.
        /// </summary>
        private sealed record _MockItemInstructions() : ImageMediaType("MockItemInstructions", "Represents the mock item instructions media type, which is used for displaying instructions related to mock items in the application.",
            5, 5, 25, 5, 5, LayoutItem.MockItemInstructions, [ DIType.MockItemInstructions ]) { }

        /// <summary>
        /// Represents the media type for continue instructions, used to display guidance or prompts for continuing
        /// within the application.
        /// </summary>
        /// <remarks>This type is typically used to render instructional content that assists users in
        /// proceeding to the next step or action. It is intended for scenarios where clear, actionable instructions are
        /// required to guide user flow.</remarks>
        private sealed record _ContinueInstructions() : ImageMediaType("ContinueInstructions", "Represents the continue instructions media type, which is used for displaying instructions related to continuing in the application.",
            5, 5, 25, 5, 5, LayoutItem.ContinueInstructions, [ DIType.ContinueInstructions ]) { }

        /// <summary>
        /// Represents a media type for images with variable dimensions, allowing content to be displayed at any size
        /// within the application.
        /// </summary>
        /// <remarks>This type is used when the image size is not fixed and can change dynamically based
        /// on the context or user input. It is typically associated with scenarios such as survey images where the
        /// dimensions are not predetermined.</remarks>
        private sealed record _VariableSize() : ImageMediaType("VariableSize", "Represents the variable size media type, which is used for displaying content that can have varying sizes in the application.",
            0, 0, 0, 0, 0, LayoutItem.None, [ DIType.SurveyImage ]) { }

        /// <summary>
        /// Represents the thumbnail image type used for previewing stimuli and instruction screens.
        /// </summary>
        /// <remarks>This type defines the characteristics and constraints for thumbnail images, including
        /// their recommended size and intended use as visual previews. It is typically used to provide quick,
        /// low-resolution representations of larger media assets within the application.</remarks>
        private sealed record _Thumbnail() : ImageMediaType("Thumbnail", "Represents the thumbnail preview of stimuli and instruction screens.",
            25, 15, 75, 5, 15, LayoutItem.None, []) { }

        /// <summary>
        /// Represents a media type that indicates the absence of any applicable or available media within the
        /// application.
        /// </summary>
        /// <remarks>Use this type when a media value is required by the API but no actual media is
        /// present or relevant. This can be useful for scenarios where a placeholder or sentinel value is needed to
        /// signify 'no media'.</remarks>
        private sealed record _Null() : ImageMediaType("Null", "Represents a null media type, which is used for cases where no media is applicable or available in the application.",
            1, 1, 3, 1, 1, LayoutItem.None, [ DIType.Null ]) { }

        private sealed record _Unset() : ImageMediaType("Unset", "Represents an unset media type, which is used as a default or uninitialized state for media types in the application.",
            0, 0, 0, 0, 0, LayoutItem.None, [ ]) { }

    }
}
