using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models.Enumerations
{
    /// <summary>
    /// Represents a named direction that can be used to indicate orientation or movement, such as left, right, or none.
    /// Provides predefined instances for common directions and supports retrieving the opposite direction.
    /// </summary>
    /// <remarks>Use the predefined static instances to avoid creating new objects for common directions. This
    /// type is typically used in scenarios where directional input, orientation, or navigation logic is required. The
    /// meaning of each direction and its opposite is determined by the specific derived type.</remarks>
    /// <param name="Name">The name of the direction represented by this instance. Common values include "Left", "Right", or "None". The
    /// value is used to identify and compare directions.</param>
    public abstract record KeyedDirection(string Name) 
    {
        /// <summary>
        /// Represents the left direction in a set of keyed directions.
        /// </summary>
        public static readonly KeyedDirection Left = new LeftDirection();

        /// <summary>
        /// Represents the right direction as a predefined instance of the KeyedDirection type.
        /// </summary>
        /// <remarks>Use this field to specify or compare against the right direction without creating a
        /// new instance. This instance is typically used in scenarios where directional input or orientation is
        /// required.</remarks>
        public static readonly KeyedDirection Right = new RightDirection();

        /// <summary>
        /// Represents a direction with no associated value.
        /// </summary>
        /// <remarks>Use this field to indicate the absence of a direction when a valid direction is not
        /// applicable.</remarks>
        public static readonly KeyedDirection None = new NoneDirection();

        /// <summary>
        /// Returns the corresponding KeyedDirection value for the specified direction name.
        /// </summary>
        /// <param name="name">The name of the direction to convert. Valid values are "left", "right", or "none". Comparison is
        /// case-insensitive.</param>
        /// <returns>A KeyedDirection value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a valid direction.</exception>
        private static KeyedDirection FromName(string name) =>
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
        /// <remarks>Use this property to retrieve the direction that represents the logical opposite of
        /// the current instance. The specific meaning of "opposite" depends on the implementation of the derived
        /// class.</remarks>
        public abstract KeyedDirection Opposite { get; }

        /// <summary>
        /// Represents a direction corresponding to the left key input.
        /// </summary>
        public sealed record LeftDirection() : KeyedDirection("Left") {
            /// <summary>
            /// Gets the direction that is opposite to the current direction.
            /// </summary>
            public override KeyedDirection Opposite => Right;
        }

        /// <summary>
        /// Represents a direction indicating movement to the right.
        /// </summary>
        public sealed record RightDirection() : KeyedDirection("Right") { 
            /// <summary>
            /// Gets the direction that is opposite to the current direction.
            /// </summary>
            public override KeyedDirection Opposite => Left;
        }

        /// <summary>
        /// Represents a direction that indicates the absence of any specific direction.
        /// </summary>
        /// <remarks>Use this type when a direction is required by the API but no actual direction is
        /// applicable. This can be useful as a default or placeholder value in scenarios where directional information
        /// is optional or not relevant.</remarks>
        public sealed record NoneDirection() : KeyedDirection("None") { 
            /// <summary>
            /// Gets the direction that is opposite to the current direction.
            /// </summary>
            public override KeyedDirection Opposite => None;
        }
    }
}
