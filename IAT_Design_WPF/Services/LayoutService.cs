using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using System;
using System.Windows;

namespace IAT_Design_WPF.Services
{
    public class LayoutService
    {
        private readonly Layout _layout;

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
        public void UpdateLayout()
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

        // Methods to set sizes and update layout
        public void SetInteriorSize(Size size)
        {
            _layout.InteriorSize = size;
            UpdateLayout();
        }

        public void SetKeyValueSize(Size size)
        {
            _layout.KeyValueSize = size;
            UpdateLayout();
        }

        public void SetStimulusSize(Size size)
        {
            _layout.StimulusSize = size;
            UpdateLayout();
        }

        public void SetInstructionsSize(Size size)
        {
            _layout.InstructionsSize = size;
            UpdateLayout();
        }

        public void SetErrorSize(Size size)
        {
            _layout.ErrorSize = size;
            UpdateLayout();
        }

        public void SetContinueInstructionsSize(Size size)
        {
            _layout.ContinueInstructionsSize = size;
            UpdateLayout();
        }
    }
}