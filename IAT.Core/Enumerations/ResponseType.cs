using System;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a predefined set of response types used to categorize or identify the expected format or nature of a
    /// response in a survey or data collection context.
    /// </summary>
    /// <remarks>Use the static members of this type to reference specific response types, such as TrueFalse,
    /// Likert, or Date. This abstraction enables consistent handling and validation of different response formats
    /// across survey items or data entry fields.</remarks>
    /// <param name="Value">The integer value that uniquely identifies the response type.</param>
    /// <param name="Name">The display name or identifier for the response type.</param>
    public abstract record ResponseType(int Value, string Name)
    {
        /// <summary>
        /// Represents a response type that indicates no response is expected or required. This can be used for survey items that serve as
        /// instructions, separators, or informational text without requiring participant input.
        /// </summary>
        public static readonly ResponseType None = new _None();

        /// <summary>
        /// Represents a lack of a response type because the containing item is an instruction rather than a question. This type can be used
        /// to differentiate between items that require participant input and those that are purely informational or directive in nature.
        /// </summary>
        public static readonly ResponseType Instruction = new _Instruction();

        /// <summary>
        /// Represents a response type for image content.
        /// </summary>
        public static readonly ResponseType Image = new _Image();

        /// <summary>
        /// Represents a response type for Boolean values.
        /// </summary>
        public static readonly ResponseType TrueFalse = new _TrueFalse();

        /// <summary>
        /// Represents a Likert scale response type, typically used for survey questions that measure attitudes or
        /// opinions on a graded scale.
        /// </summary>
        public static readonly ResponseType Likert = new _Likert();

        /// <summary>
        /// Represents a response type that handles date values.
        /// </summary>
        public static readonly ResponseType Date = new _Date();

        /// <summary>
        /// Represents a single-select multiple-choice response type.
        /// </summary>
        public static readonly ResponseType MultiChoice = new _MultiChoice();

        /// <summary>
        /// Represents a response type that uses regular expression matching.
        /// </summary>
        public static readonly ResponseType RegEx = new _RegEx();

        /// <summary>
        /// Represents a response type that supports selecting multiple options.
        /// </summary>
        public static readonly ResponseType MultiSelect = new _MultiSelect();

        /// <summary>
        /// Represents a response type that indicates a fixed digital value.
        /// </summary>
        public static readonly ResponseType FixedDigit = new _FixedDigit();

        /// <summary>
        /// Represents a response type that enforces numeric values within a specified range.
        /// </summary>
        public static readonly ResponseType BoundedNumber = new _BoundedNumber();

        /// <summary>
        /// Represents a response type that enforces a bounded length constraint.
        /// </summary>
        public static readonly ResponseType BoundedText = new _BoundedText();

        /// <summary>
        /// Returns the ResponseType value that corresponds to the specified integer value.
        /// </summary>
        public static ResponseType FromValue(int value) =>
            value switch
            {
                0 => None,
                1 => Instruction,
                2 => Image,
                3 => TrueFalse,
                4 => Likert,
                5 => Date,
                6 => MultiChoice,
                7 => MultiSelect,
                8 => RegEx,
                10 => FixedDigit,
                11 => BoundedNumber,
                12 => BoundedText,
                _ => throw new ArgumentException($"Invalid ResponseType value: {value}")
            };

        /// <summary>
        /// Returns the corresponding ResponseType value for the specified name.
        /// Name comparison is case-insensitive. Legacy aliases (Boolean, Multiple, MultiBoolean, BoundedLength) are accepted.
        /// </summary>
        public static ResponseType FromName(string name) =>
            name?.ToLowerInvariant() switch
            {
                "none" => None,
                "instruction" => Instruction,
                "image" => Image,
                "truefalse" or "boolean" => TrueFalse,
                "likert" => Likert,
                "date" => Date,
                "multichoice" or "multiple" or "multiplechoice" => MultiChoice,
                "regex" or "regularexpression" => RegEx,
                "multiselect" or "multiboolean" => MultiSelect,
                "fixeddigit" or "fixeddig" => FixedDigit,
                "boundednumber" or "boundednum" => BoundedNumber,
                "boundedtext" or "boundedlength" => BoundedText,
                _ => throw new ArgumentException($"Unknown ResponseType name: {name}")
            };

        /// <summary>
        /// Returns the corresponding ResponseType value for the specified type. The type's name is used to determine the matching ResponseType value.
        /// Accepts both the private record names and the ConfigFile class names.
        /// </summary>
        public static ResponseType FromType(Type type) =>
            type.Name switch
            {
                nameof(_None) or "None" => None,
                nameof(_Instruction) or "Instruction" => Instruction,
                nameof(_Image) or "Image" => Image,
                nameof(_TrueFalse) or "TrueFalse" or "Boolean" => TrueFalse,
                nameof(_Likert) or "Likert" => Likert,
                nameof(_Date) or "Date" => Date,
                nameof(_MultiChoice) or "MultiChoice" or "Multiple" => MultiChoice,
                nameof(_MultiSelect) or "MultiSelect" or "MultiBoolean" => MultiSelect,
                nameof(_RegEx) or "RegEx" => RegEx,
                nameof(_FixedDigit) or "FixedDigit" => FixedDigit,
                nameof(_BoundedNumber) or "BoundedNumber" => BoundedNumber,
                nameof(_BoundedText) or "BoundedText" or "BoundedLength" => BoundedText,
                _ => throw new ArgumentException($"Unknown ResponseType type: {type.Name}")
            };

        private sealed record _None() : ResponseType(0, "None");
        private sealed record _Instruction() : ResponseType(1, "Instruction");
        private sealed record _Image() : ResponseType(2, "Image");
        private sealed record _TrueFalse() : ResponseType(3, "TrueFalse");
        private sealed record _Likert() : ResponseType(4, "Likert");
        private sealed record _Date() : ResponseType(5, "Date");
        private sealed record _MultiChoice() : ResponseType(6, "MultiChoice");
        private sealed record _MultiSelect() : ResponseType(7, "MultiSelect");
        private sealed record _RegEx() : ResponseType(8, "RegEx");
        private sealed record _FixedDigit() : ResponseType(10, "FixedDigit");
        private sealed record _BoundedNumber() : ResponseType(11, "BoundedNumber");
        private sealed record _BoundedText() : ResponseType(12, "BoundedText");
    }
}
