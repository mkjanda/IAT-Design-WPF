using System;
using System.Text;
using System.Text.Json.Serialization;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a named direction that can be used to indicate orientation or movement, such as left, right, or none.
    /// Provides predefined instances for common directions and supports retrieving the opposite direction.
    /// </summary>
    /// <remarks>
    /// Smart-enum style record. <see cref="Opposite"/> intentionally forms a Left↔Right cycle.
    /// The generated record <c>PrintMembers</c>/<c>ToString</c> would follow that cycle forever,
    /// so both are overridden to print only <see cref="Name"/>.
    /// </remarks>
    /// <param name="Name">The name of the direction ("Left", "Right", or "None").</param>
    public abstract record KeyedDirection(string Name)
    {
        /// <summary>
        /// Represents the left direction in a set of keyed directions.
        /// </summary>
        public static readonly KeyedDirection Left = new LeftDirection();

        /// <summary>
        /// Represents the right direction as a predefined instance of the KeyedDirection type.
        /// </summary>
        public static readonly KeyedDirection Right = new RightDirection();

        /// <summary>
        /// Represents a direction with no associated value.
        /// </summary>
        public static readonly KeyedDirection None = new NoneDirection();

        /// <summary>
        /// Returns the corresponding KeyedDirection value for the specified direction name.
        /// </summary>
        /// <param name="name">The name of the direction to convert. Valid values are "left", "right", or "none". Comparison is case-insensitive.</param>
        /// <returns>A KeyedDirection value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a valid direction.</exception>
        public static KeyedDirection FromName(string name) =>
            name?.ToLowerInvariant() switch
            {
                "left" => Left,
                "right" => Right,
                "none" => None,
                _ => throw new ArgumentException($"Unknown keyed direction: {name}")
            };

        /// <summary>
        /// Gets the direction that is opposite to the current direction.
        /// </summary>
        [JsonIgnore]
        public abstract KeyedDirection Opposite { get; }

        /// <summary>
        /// Returns only the direction name. Must not include <see cref="Opposite"/> —
        /// that property cycles Left↔Right and would overflow the stack in the generated printer.
        /// Sealed so nested records cannot regenerate a ToString that walks Opposite.
        /// </summary>
        public sealed override string ToString() => Name;

        /// <summary>
        /// Prints only <see cref="Name"/>. Excludes <see cref="Opposite"/> to break the
        /// Left→Right→Left recursion in the compiler-generated member printer.
        /// Sealed so nested records cannot append Opposite to the printout.
        /// </summary>
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append("Name = ");
            builder.Append(Name);
            return true;
        }

        /// <summary>
        /// Represents a direction corresponding to the left key input.
        /// </summary>
        public sealed record LeftDirection() : KeyedDirection("Left")
        {
            /// <inheritdoc />
            public override KeyedDirection Opposite => Right;
        }

        /// <summary>
        /// Represents a direction indicating movement to the right.
        /// </summary>
        public sealed record RightDirection() : KeyedDirection("Right")
        {
            /// <inheritdoc />
            public override KeyedDirection Opposite => Left;
        }

        /// <summary>
        /// Represents a direction that indicates the absence of any specific direction.
        /// </summary>
        public sealed record NoneDirection() : KeyedDirection("None")
        {
            /// <inheritdoc />
            public override KeyedDirection Opposite => None;
        }
    }
}
