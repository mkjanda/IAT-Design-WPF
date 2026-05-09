using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Domain;
using IAT.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
        [ObservableProperty] private double leftKeyWidth;
        [ObservableProperty] private double leftKeyHeight;
        [ObservableProperty] private double rightKeyWidth;
        [ObservableProperty] private double rightKeyHeight;
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
            LeftKeyWidth = rects.LeftKey.Width;
            LeftKeyHeight = rects.LeftKey.Height;
            RightKeyWidth = rects.RightKey.Width;
            RightKeyHeight = rects.RightKey.Height;
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
        }

        partial void OnStimulusWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "Stimulus", new Size(value, StimulusHeight));
        }

        partial void OnStimulusHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "Stimulus", new Size(StimulusWidth, value));
        }

        partial void OnLeftKeyWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "LeftKey", new Size(value, LeftKeyHeight));
        }

        partial void OnLeftKeyHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "LeftKey", new Size(LeftKeyWidth, value));
        }

        partial void OnRightKeyWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "RightKey", new Size(value, RightKeyHeight));
        }

        partial void OnRightKeyHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "RightKey", new Size(RightKeyWidth, value));
        }
        
        partial void OnErrorMarkWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "ErrorMark", new Size(value, ErrorMarkHeight));
        }

        partial void OnErrorMarkHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "ErrorMark", new Size(ErrorMarkWidth, value));
        }

        partial void OnBlockInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "BlockInstructions", new Size(value, BlockInstructionsHeight));
        }

        partial void OnBlockInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "BlockInstructions", new Size(BlockInstructionsWidth, value));
        }

        partial void OnMockItemInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "MockItemInstructions", new Size(value, MockItemInstructionsHeight));
        }

        partial void OnMockItemInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "MockItemInstructions", new Size(MockItemInstructionsWidth, value));
        }

        partial void OnKeyedInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "KeyedInstructions", new Size(value, KeyedInstructionsHeight));
        }

        partial void OnKeyedInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "KeyedInstructions", new Size(KeyedInstructionsWidth, value));
        }

        partial void OnTextInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "TextInstructions", new Size(value, TextInstructionsHeight));
        }

        partial void OnTextInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "TextInstructions", new Size(TextInstructionsWidth, value));
        }

        partial void OnContinueInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "ContinueInstructions", new Size(value, ContinueInstructionsHeight));
        }

        partial void OnContinueInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, "ContinueInstructions", new Size(ContinueInstructionsWidth, value));
        }
    }
}
