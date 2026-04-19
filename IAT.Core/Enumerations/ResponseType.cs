using IAT.Core.Enumerations;
using org.omg.CORBA;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
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
        public static readonly ResponseType Boolean = new _Boolean();

        /// <summary>
        /// Represents a Likert scale response type, typically used for survey questions that measure attitudes or
        /// opinions on a graded scale.
        /// </summary>
        /// <remarks>Use this response type when you need to collect responses on a scale, such as
        /// "Strongly Disagree" to "Strongly Agree". The specific scale points and labels may vary depending on the
        /// survey design.</remarks>
        public static readonly ResponseType Likert = new _Likert();

        /// <summary>
        /// Represents a response type that handles date values.
        /// </summary>
        public static readonly ResponseType Date = new _Date();

        /// <summary>
        /// Represents a response type that allows multiple values to be returned.
        /// </summary>
        public static readonly ResponseType Multiple = new _Multiple();

        /// <summary>
        /// Represents a response type that allows selection of multiple options with assigned weights.
        /// </summary>
        /// <remarks>Use this response type when a question or input requires users to select more than
        /// one option and specify a weight or value for each selected option. This is commonly used in scenarios where
        /// the relative importance or contribution of each choice must be captured.</remarks>
        public static readonly ResponseType WeightedMultiple = new _WeightedMultiple();

        /// <summary>
        /// Represents a response type that uses regular expression matching.
        /// </summary>
        /// <remarks>Use this response type when pattern-based matching is required. The specific behavior
        /// depends on the implementation of the underlying _RegEx type.</remarks>
        public static readonly ResponseType RegEx = new _RegEx();

        /// <summary>
        /// Represents a response type that supports multiple boolean values.
        /// </summary>
        /// <remarks>Use this response type when a response may contain more than one boolean value, such
        /// as when handling multi-valued fields or batch operations.</remarks>
        public static readonly ResponseType MultiBoolean = new _MultiBoolean(); 

        /// <summary>
        /// Represents a response type that indicates a fixed digital value.
        /// </summary>
        public static readonly ResponseType FixedDig = new _FixedDig();

        /// <summary>
        /// Represents a response type that enforces numeric values within a specified range.
        /// </summary>
        /// <remarks>Use this response type when input or output values must be constrained to a defined
        /// numeric interval. The specific bounds and validation behavior depend on the implementation of the response
        /// type.</remarks>
        public static readonly ResponseType BoundedNum = new _BoundedNum();

        /// <summary>
        /// Represents a response type that enforces a bounded length constraint.
        /// </summary>
        /// <remarks>Use this value when a response must not exceed a specific length. The exact length
        /// limits and enforcement behavior depend on the context in which this response type is used.</remarks>
        public static readonly ResponseType BoundedLength = new _BoundedLength();

        /// <summary>
        /// Returns the ResponseType enumeration value that corresponds to the specified integer value.
        /// </summary>
        /// <remarks>Use this method to convert an integer value to its corresponding ResponseType. This
        /// is useful when working with serialized or external representations of ResponseType values.</remarks>
        /// <param name="value">The integer value representing a ResponseType enumeration member.</param>
        /// <returns>The ResponseType value that matches the specified integer.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified value does not correspond to a valid ResponseType enumeration member.</exception>
        public static ResponseType FromValue(int value) =>
            value switch
            {
                0 => None,
                1 => Instruction,
                2 => Image,
                3 => Boolean,
                4 => Likert,
                5 => Date,
                6 => Multiple,
                7 => WeightedMultiple,
                8 => RegEx,
                9 => MultiBoolean,
                10 => FixedDig,
                11 => BoundedNum,
                12 => BoundedLength,
                _ => throw new ArgumentException($"Invalid ResponseType value: {value}")
            };

        /// <summary>
        /// Returns the corresponding ResponseType value for the specified name.
        /// </summary>
        /// <remarks>The name comparison is performed in a case-insensitive manner using the invariant
        /// culture. Valid names include "none", "instruction", "image", "boolean", "likert", "date", "multiple",
        /// "weightedmultiple", "regex", "multiboolean", "fixeddig", "boundednum", and "boundedlength".</remarks>
        /// <param name="name">The name of the response type to retrieve. The comparison is case-insensitive.</param>
        /// <returns>A ResponseType value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known ResponseType value.</exception>
        public static ResponseType FromName(string name) =>
            name?.ToLowerInvariant() switch
            {
                "none" => None,
                "instruction" => Instruction,
                "image" => Image,
                "boolean" => Boolean,
                "likert" => Likert,
                "date" => Date,
                "multiple" => Multiple,
                "weightedmultiple" => WeightedMultiple,
                "regex" => RegEx,
                "multiboolean" => MultiBoolean,
                "fixeddig" => FixedDig,
                "boundednum" => BoundedNum,
                "boundedlength" => BoundedLength,
                _ => throw new ArgumentException($"Unknown ResponseType name: {name}")
            };

        private sealed record _None() : ResponseType(0, "None");
        private sealed record _Instruction() : ResponseType(1, "Instruction");
        private sealed record _Image() : ResponseType(2, "Image");
        private sealed record _Boolean() : ResponseType(3, "Boolean");  
        private sealed record _Likert() : ResponseType(4, "Likert");    
        private sealed record _Date() : ResponseType(5, "Date");    
        private sealed record _Multiple() : ResponseType(6, "Multiple");
        private sealed record _WeightedMultiple() : ResponseType(7, "WeightedMultiple");
        private sealed record _RegEx() : ResponseType(8, "RegEx");
        private sealed record _MultiBoolean() : ResponseType(9, "MultiBoolean");
        private sealed record _FixedDig() : ResponseType(10, "FixedDig");
        private sealed record _BoundedNum() : ResponseType(11, "BoundedNum");
        private sealed record _BoundedLength() : ResponseType(12, "BoundedLength");
    }
}
