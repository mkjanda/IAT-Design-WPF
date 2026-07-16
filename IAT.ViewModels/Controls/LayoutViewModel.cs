using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using System;
using System.Windows;

namespace IAT.ViewModels
{
    public partial class LayoutViewModel : ObservableObject
    {
        private readonly ILayoutCalculatorService _calculator;
        private readonly IatTest _test;

        [ObservableProperty] private double interiorWidth;
        [ObservableProperty] private double interiorHeight;
        [ObservableProperty] private double stimulusWidth;
        [ObservableProperty] private double stimulusHeight;
        [ObservableProperty] private double keyWidth;
        [ObservableProperty] private double keyHeight;
        [ObservableProperty] private double errorMarkWidth;
        [ObservableProperty] private double errorMarkHeight;
        [ObservableProperty] private double blockInstructionsWidth;
        [ObservableProperty] private double blockInstructionsHeight;
        [ObservableProperty] private double mockItemInstructionsWidth;
        [ObservableProperty] private double mockItemInstructionsHeight;
        [ObservableProperty] private double keyedInstructionsWidth;
        [ObservableProperty] private double keyedInstructionsHeight;
        [ObservableProperty] private double textInstructionsWidth;
        [ObservableProperty] private double textInstructionsHeight;
        [ObservableProperty] private double continueInstructionsWidth;
        [ObservableProperty] private double continueInstructionsHeight;
        [ObservableProperty] private bool isLayoutEditMode = true;
        [ObservableProperty] private double scaleFactor;
        [ObservableProperty] private string statusMessage = string.Empty;

        /// <summary>
        /// Last host size reported by the preview container. Used to re-fit the scale
        /// when InteriorWidth/Height change so the on-screen preview size stays stable
        /// while the logical stage (and aspect ratio) changes.
        /// </summary>
        private Size _lastAvailableSize;

        /// <summary>
        /// Persists the current layout sizes and positions through the calculator
        /// (user overrides) and surfaces a short status message for the UI.
        /// </summary>
        [RelayCommand]
        private void SaveLayout()
        {
            try
            {
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Size(InteriorWidth, InteriorHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Size(StimulusWidth, StimulusHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Size(KeyWidth, KeyHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Size(KeyWidth, KeyHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Size(ErrorMarkWidth, ErrorMarkHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Size(BlockInstructionsWidth, BlockInstructionsHeight));

                StatusMessage = $"Layout saved — stage {InteriorWidth:0}×{InteriorHeight:0} px";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Save failed: {ex.Message}";
            }
        }

        [RelayCommand]
        public void FitToWindow(Size availableSize)
        {
            if (availableSize.Width <= 0 || availableSize.Height <= 0)
                return;

            _lastAvailableSize = availableSize;

            // Design size comes from InteriorWidth/Height (logical stage).
            // Uniform scale keeps aspect ratio visible as the white preview rectangle
            // changes shape when Interior aspect changes.
            double scaleX = availableSize.Width / InteriorWidth;
            double scaleY = availableSize.Height / InteriorHeight;

            ScaleFactor = Math.Min(scaleX, scaleY) * 0.95; // 5% padding looks better
            OnPropertyChanged(nameof(DesignHeight));
            OnPropertyChanged(nameof(DesignWidth));
        }

        /// <summary>
        /// Re-applies FitToWindow using the most recent host size.
        /// Call after InteriorWidth/Height change so the visual container size
        /// stays roughly constant while elements move raisin-bread style.
        /// </summary>
        private void RefitToLastAvailableSize()
        {
            if (_lastAvailableSize.Width > 0 && _lastAvailableSize.Height > 0)
                FitToWindow(_lastAvailableSize);
        }

        partial void OnInteriorWidthChanged(double value)
        {
            // Stage size changed. Element positions stay absolute (user drags are preserved).
            // ScaleFactor is recomputed so the on-screen preview frame size remains stable.
            RefitToLastAvailableSize();
            OnPropertyChanged(nameof(DesignWidth));
            try
            {
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Size(value, InteriorHeight));
            }
            catch
            {
                // Calculator may not yet expose Interior as an overridable item; preview still works.
            }
        }

        partial void OnInteriorHeightChanged(double value)
        {
            RefitToLastAvailableSize();
            OnPropertyChanged(nameof(DesignHeight));
            try
            {
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Size(InteriorWidth, value));
            }
            catch
            {
                // same as above
            }
        }

        // Positions are settable so elements can be freely dragged in the preview.
        // Initialized from the calculator; after a user drag they become absolute overrides.
        [ObservableProperty] private double stimulusX;
        [ObservableProperty] private double stimulusY;
        [ObservableProperty] private double leftKeyX;
        [ObservableProperty] private double leftKeyY;
        [ObservableProperty] private double rightKeyX;
        [ObservableProperty] private double rightKeyY;
        [ObservableProperty] private double errorMarkX;
        [ObservableProperty] private double errorMarkY;
        [ObservableProperty] private double blockInstructionsX;
        [ObservableProperty] private double blockInstructionsY;

        public double DesignWidth => InteriorWidth * ScaleFactor;
        public double DesignHeight => InteriorHeight * ScaleFactor;

        /// <summary>
        /// Re-applies the default layout rules (centering, edge alignment, padding) to all element positions.
        /// Called on construction and available for a future "Reset layout" command.
        /// </summary>
        public void RecalculateDefaultPositions()
        {
            StimulusX = InteriorWidth / 2 - StimulusWidth / 2;
            ErrorMarkX = InteriorWidth / 2 - ErrorMarkWidth / 2;
            BlockInstructionsX = InteriorWidth / 2 - BlockInstructionsWidth / 2;
            LeftKeyX = 0;
            RightKeyX = InteriorWidth - KeyWidth;

            double thickPad = (InteriorHeight - KeyHeight - BlockInstructionsHeight - ErrorMarkHeight - StimulusHeight) / 3.0;
            double thinPad = (InteriorHeight - BlockInstructionsHeight - ErrorMarkHeight - StimulusHeight) / 3.0;
            bool keysSideBySide = InteriorWidth - StimulusWidth - KeyWidth * 2 >= 0;

            LeftKeyY = 0;
            RightKeyY = 0;
            StimulusY = keysSideBySide ? thinPad : thickPad + KeyHeight;
            ErrorMarkY = StimulusY + StimulusHeight + (keysSideBySide ? thinPad : thickPad);
            BlockInstructionsY = InteriorHeight - BlockInstructionsHeight;
        }

        /// <summary>
        /// Constructs a new instance of the LayoutViewModel class, initializing layout properties based on the provided layout calculator 
        /// service and test configuration. The constructor retrieves the final layout rectangles from the calculator service using the test's 
        /// layout configuration and sets the corresponding width and height properties for each layout element (e.g., stimulus, keys, instructions). 
        /// This allows the view model to reflect the current layout settings and enables dynamic updates when properties are changed. The constructor 
        /// assumes that the provided calculator service and test are valid and properly initialized.
        /// </summary>
        /// <param name="calculator"></param>
        /// <param name="test"></param>
        public LayoutViewModel(ILayoutCalculatorService calculator, IatTest test)
        {
            _calculator = calculator;
            _test = test;
            var rects = _calculator.GetFinalRects(_test.Layout);
            InteriorWidth = rects.Interior.Width;
            InteriorHeight = rects.Interior.Height;
            StimulusWidth = rects.Stimulus.Width;
            StimulusHeight = rects.Stimulus.Height;
            KeyWidth = rects.LeftKey.Width;
            KeyHeight = rects.LeftKey.Height;
            ErrorMarkWidth = rects.ErrorMark.Width;
            ErrorMarkHeight = rects.ErrorMark.Height;
            BlockInstructionsWidth = rects.BlockInstructions.Width;
            BlockInstructionsHeight = rects.BlockInstructions.Height;
            MockItemInstructionsWidth = rects.MockItemInstructions.Width;
            MockItemInstructionsHeight = rects.MockItemInstructions.Height;
            KeyedInstructionsWidth = rects.KeyedInstructions.Width;
            KeyedInstructionsHeight = rects.KeyedInstructions.Height;
            TextInstructionsWidth = rects.TextInstructions.Width;
            TextInstructionsHeight = rects.TextInstructions.Height;
            ContinueInstructionsWidth = rects.ContinueInstructions.Width;
            ContinueInstructionsHeight = rects.ContinueInstructions.Height;

            // Prefer absolute positions from the calculator when available; otherwise apply default rules.
            StimulusX = rects.Stimulus.X;
            StimulusY = rects.Stimulus.Y;
            LeftKeyX = rects.LeftKey.X;
            LeftKeyY = rects.LeftKey.Y;
            RightKeyX = rects.RightKey.X;
            RightKeyY = rects.RightKey.Y;
            ErrorMarkX = rects.ErrorMark.X;
            ErrorMarkY = rects.ErrorMark.Y;
            BlockInstructionsX = rects.BlockInstructions.X;
            BlockInstructionsY = rects.BlockInstructions.Y;

            // If the calculator returned origin-only rects, fall back to rule-based placement.
            if (StimulusX == 0 && StimulusY == 0 && ErrorMarkX == 0)
                RecalculateDefaultPositions();
        }

        partial void OnStimulusWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Size(value, StimulusHeight));
        }

        partial void OnStimulusHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Size(StimulusWidth, value));
        }

        partial void OnKeyWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Size(value, KeyHeight));
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Size(value, KeyHeight));
        }

        partial void OnKeyHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Size(KeyWidth, value));
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Size(KeyWidth, value));
        }

        partial void OnErrorMarkWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Size(value, ErrorMarkHeight));
        }

        partial void OnErrorMarkHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Size(ErrorMarkWidth, value));
        }

        partial void OnBlockInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Size(value, BlockInstructionsHeight));
        }

        partial void OnBlockInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Size(BlockInstructionsWidth, value));
        }

        partial void OnMockItemInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.MockItemInstructions, new Size(value, MockItemInstructionsHeight));
        }

        partial void OnMockItemInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.MockItemInstructions, new Size(MockItemInstructionsWidth, value));
        }

        partial void OnKeyedInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.KeyedInstructions, new Size(value, KeyedInstructionsHeight));
        }

        partial void OnKeyedInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.KeyedInstructions, new Size(KeyedInstructionsWidth, value));
        }

        partial void OnTextInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.TextInstructions, new Size(value, TextInstructionsHeight));
        }

        partial void OnTextInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.TextInstructions, new Size(TextInstructionsWidth, value));
        }

        partial void OnContinueInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ContinueInstructions, new Size(value, ContinueInstructionsHeight));
        }

        partial void OnContinueInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ContinueInstructions, new Size(ContinueInstructionsWidth, value));
        }


    }
}
