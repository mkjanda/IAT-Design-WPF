using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System;
using System.Windows;

namespace IAT_Design_WPF.Services
{
    /// <summary>
    /// Provides services for managing and updating layout properties and observables for UI elements within a layout.
    /// </summary>
    /// <remarks>The LayoutService coordinates updates to layout rectangles and sizes by subscribing to layout
    /// observables and applying changes when layout dimensions are modified. It is intended to be used in conjunction
    /// with a Layout instance to ensure UI elements are positioned and sized correctly in response to changes. This
    /// class is not thread-safe; all updates should be performed on the UI thread.</remarks>
    public class LayoutService
    {
        /// <summary>
        /// The layout object that this service manages, providing access to layout properties and observables for updating
        /// </summary>
        private readonly Layout _layout;


        /// <summary>
        /// Initializes a new instance of the LayoutService class using the specified layout.
        /// </summary>
        /// <param name="layout">The layout to be managed by the service. Cannot be null.</param>
        public LayoutService(Layout layout)
        {
            _layout = layout;
            WireLayoutItems();
        }

        /// <summary>
        /// Subscribes layout item rectangle observers to their corresponding observables to enable automatic updates of
        /// layout item positions and sizes.
        /// </summary>
        private void WireLayoutItems()
        {
            _layout.NoneObservable.Subscribe(LayoutItem.None.RectangleObserver);
            _layout.InteriorRectObservable.Subscribe(LayoutItem.FullWindow.RectangleObserver);
            _layout.BlockInstructionsRectObservable.Subscribe(LayoutItem.BlockInstructions.RectangleObserver);
            _layout.StimulusRectObservable.Subscribe(LayoutItem.Stimulus.RectangleObserver);
            _layout.LeftKeyValueRectObservable.Subscribe(LayoutItem.LeftResponseKey.RectangleObserver);
            _layout.LeftKeyValueRectObservable.Subscribe(LayoutItem.LeftResponseKeyOutline.RectangleObserver);
            _layout.RightKeyValueRectObservable.Subscribe(LayoutItem.RightResponseKey.RectangleObserver);
            _layout.RightKeyValueRectObservable.Subscribe(LayoutItem.RightResponseKeyOutline.RectangleObserver);
            _layout.ErrorRectObservable.Subscribe(LayoutItem.ErrorMark.RectangleObserver);
            _layout.TextInstructionScreenRectObservable.Subscribe(LayoutItem.TextInstructionScreen.RectangleObserver);
            _layout.KeyInstructionScreenTextRectObservable.Subscribe(LayoutItem.KeyedInstructionScreen.RectangleObserver);
            _layout.MockItemInstructionsRectObservable.Subscribe(LayoutItem.MockItemInstructions.RectangleObserver);
            _layout.ContinueInstructionsRectObservable.Subscribe(LayoutItem.ContinueInstructions.RectangleObserver);
        }

        /// <summary>
        /// Updates the layout rectangles based on the current interior size and other dimensions.
        /// </summary>
        private void UpdateLayout()
        {
            var size = _layout.InteriorRectObservable.Value.Size;

            var rect = _layout.ContinueInstructionsRectObservable.Value;
            rect.X = (size.Width - rect.Width) / 2;
            _layout.ContinueInstructionsRectObservable.Value = rect;

            rect = _layout.InteriorRectObservable.Value;
            rect.Inflate(-25, -25);
            rect.Height -= _layout.ContinueInstructionsRectObservable.Value.Height;
            _layout.TextInstructionScreenRectObservable.Value = rect;

            rect = _layout.InteriorRectObservable.Value;
            rect.Inflate(-25, -25);
            rect.Y += _layout.KeyValueSize.Height;
            rect.Height -= _layout.KeyValueSize.Height + _layout.ContinueInstructionsRectObservable.Value.Height;
            _layout.KeyInstructionScreenTextRectObservable.Value = rect;

            rect = _layout.MockItemInstructionsRectObservable.Value;
            rect.Y += (size.Height - size.Height) / 6;
            rect.Height += (size.Height - size.Height) / 3;
            _layout.MockItemInstructionsRectObservable.Value = rect;

            rect = _layout.ErrorRectObservable.Value;
            rect.Y += (size.Height - size.Height) / 6;
            rect.X = (size.Width - rect.Width) / 2;
            _layout.ErrorRectObservable.Value = rect;

            rect = _layout.BlockInstructionsRectObservable.Value;
            rect.X = (size.Width - rect.Width) / 2;
            rect.Y += size.Height - size.Height;
            _layout.BlockInstructionsRectObservable.Value = rect;

            rect = _layout.RightKeyValueRectObservable.Value;
            rect.X = size.Width - rect.Width - 25;
            _layout.RightKeyValueRectObservable.Value = rect;

            rect = _layout.StimulusRectObservable.Value;
            if (rect.Width < size.Width - (_layout.KeyValueSize.Width * 2))
            {
                rect.X = (size.Width - rect.Width) / 2;
                rect.Y = (_layout.ErrorRectObservable.Value.Y - rect.Height) / 2;
            }
            else
            {
                rect.X = size.Width - rect.Width - _layout.KeyValueSize.Width;
                rect.Y = _layout.KeyValueSize.Height + (_layout.ErrorRectObservable.Value.Y - rect.Height) / 2;
            }
            _layout.StimulusRectObservable.Value = rect;
        }

        /// <summary>
        /// Sets the interior size of the layout area.
        /// </summary>
        /// <remarks>Call this method to update the layout's interior dimensions. The layout will be
        /// recalculated to reflect the new size.</remarks>
        /// <param name="size">The new size to apply to the layout's interior area.</param>
        public void SetInteriorSize(Size size)
        {
            _layout.InteriorSize = size;
            UpdateLayout();
        }

        /// <summary>
        /// Sets the size of the key-value display area.
        /// </summary>
        /// <remarks>Call this method to adjust the layout when the key-value area needs to be resized.
        /// The layout is updated immediately after the size is set.</remarks>
        /// <param name="size">The new size to apply to the key-value area.</param>
        public void SetKeyValueSize(Size size)
        {
            _layout.KeyValueSize = size;
            UpdateLayout();
        }

        /// <summary>
        /// Sets the size of the stimulus area for the layout.
        /// </summary>
        /// <remarks>Calling this method updates the layout to reflect the new stimulus size. The provided
        /// size should be valid for the intended layout context.</remarks>
        /// <param name="size">The new size to apply to the stimulus area.</param>
        public void SetStimulusSize(Size size)
        {
            _layout.StimulusSize = size;
            UpdateLayout();
        }

        /// <summary>
        /// Sets the size of the instructions area in the layout.
        /// </summary>
        /// <remarks>Calling this method updates the layout to reflect the new instructions size. This
        /// method should be called whenever the instructions area needs to be resized.</remarks>
        /// <param name="size">The new size to apply to the instructions area. Specifies the width and height in device-independent units.</param>
        public void SetInstructionsSize(Size size)
        {
            _layout.InstructionsSize = size;
            UpdateLayout();
        }

        /// <summary>
        /// Sets the error size for the layout and updates the layout accordingly.
        /// </summary>
        /// <param name="size">The new error size to apply to the layout.</param>
        public void SetErrorSize(Size size)
        {
            _layout.ErrorSize = size;
            UpdateLayout();
        }

        /// <summary>
        /// Sets the size of the continue instructions area in the layout.
        /// </summary>
        /// <remarks>Call this method to update the layout when the size of the continue instructions area
        /// needs to change. The layout is refreshed immediately after the size is set.</remarks>
        /// <param name="size">The new size to apply to the continue instructions area.</param>
        public void SetContinueInstructionsSize(Size size)
        {
            _layout.ContinueInstructionsSize = size;
            UpdateLayout();
        }
    }
}