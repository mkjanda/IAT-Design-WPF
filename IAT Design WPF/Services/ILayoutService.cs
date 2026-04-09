using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IAT_Design_WPF.Services
{
    /// <summary>
    /// Defines methods for configuring the sizes of various layout elements within a user interface or display context.
    /// </summary>
    /// <remarks>Implementations of this interface allow clients to set the dimensions of specific UI
    /// components, such as the interior area, key-value pairs, stimulus display, instructional text, error messages,
    /// and continuation instructions. The effect of these methods depends on the concrete implementation and the UI
    /// framework in use.</remarks>
    public interface ILayoutService
    {
        /// <summary>
        /// Sets the interior size of the element using the specified dimensions.
        /// </summary>
        /// <param name="size">The new interior size to apply. Specifies the width and height in device-independent units.</param>
        void SetInteriorSize(Size size);

        /// <summary>
        /// Sets the size of the key-value area using the specified dimensions.
        /// </summary>
        /// <param name="size">The new size to apply to the key-value area. The width and height must be non-negative.</param>
        void SetKeyValueSize(Size size);

        /// <summary>
        /// Sets the size of the stimulus to the specified dimensions.
        /// </summary>
        /// <param name="size">The new size of the stimulus, specified as a <see cref="Size"/> structure representing width and height in
        /// pixels.</param>
        void SetStimulusSz(Size size);

        /// <summary>
        /// Sets the size of the instruction to the specified value.
        /// </summary>
        /// <param name="size">The new size to apply to the instruction.</param>
        void SetInstructionSize(Size size);

        /// <summary>
        /// Sets the size of the error display area.
        /// </summary>
        /// <param name="size">The new size to apply to the error display area.</param>
        void SetErrorSize(Size size);   

        /// <summary>
        /// Sets the size to be used for the continue instruction.
        /// </summary>
        /// <param name="size">The new size to apply to the continue instruction.</param>
        void SetContinueInstructionSize(Size size); 

    }
}
