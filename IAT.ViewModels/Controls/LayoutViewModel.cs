using com.sun.org.apache.xml.@internal.resolver.helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
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
        [ObservableProperty] private bool isLayoutEditMode;
        [ObservableProperty] private double scaleFactor;

        [RelayCommand]
        public void FitToWindow(Size availableSize)
        {
            if (availableSize.Width <= 0 || availableSize.Height <= 0)
                return;
            
            // Design size comes from your layout (you already have InteriorWidth/Height)
            double scaleX = availableSize.Width / InteriorWidth;
            double scaleY = availableSize.Height / InteriorHeight;

            ScaleFactor = Math.Min(scaleX, scaleY) * 0.95; // 5% padding looks better
            OnPropertyChanged(nameof(DesignHeight));
            OnPropertyChanged(nameof(DesignWidth));
        }

        private double thickLayoutPadding => (InteriorHeight - KeyHeight - BlockInstructionsHeight - ErrorMarkHeight - StimulusHeight) / 3;
        private double thinLayoutPadding => (InteriorHeight - BlockInstructionsHeight - ErrorMarkHeight - StimulusHeight) / 3;

        public double StimulusX => InteriorWidth / 2 - StimulusWidth / 2;
        public double StimulusY => (InteriorWidth - StimulusWidth - KeyWidth * 2 < 0) ? thickLayoutPadding + KeyHeight : thinLayoutPadding;

        public double LeftKeyX => 0;

        public double LeftKeyY => 0;

        public double RightKeyX => InteriorWidth - KeyWidth;
        public double RightKeyY => 0;
        public double ErrorMarkX => InteriorWidth / 2 - ErrorMarkWidth / 2;
        public double ErrorMarkY => (InteriorWidth - StimulusWidth - KeyWidth * 2 < 0) ? StimulusY + StimulusHeight + thickLayoutPadding
            : StimulusY + StimulusHeight + thinLayoutPadding;

        public double BlockInstructionsX => InteriorWidth / 2 - BlockInstructionsWidth / 2;
        public double BlockInstructionsY => InteriorHeight - BlockInstructionsHeight;

        public double DesignWidth => InteriorWidth * ScaleFactor;
        public double DesignHeight => InteriorHeight * ScaleFactor;

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
        }

        partial void OnStimulusWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Size(value, StimulusHeight));
            OnPropertyChanged(nameof(StimulusX));
        }

        partial void OnStimulusHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Size(StimulusWidth, value));
            OnPropertyChanged(nameof(StimulusY));
            OnPropertyChanged(nameof(ErrorMarkY));
        }

        partial void OnKeyWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Size(value, KeyHeight));
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Size(value, KeyHeight));
            OnPropertyChanged(nameof(StimulusY));
            OnPropertyChanged(nameof(ErrorMarkY));
            OnPropertyChanged(nameof(RightKeyX));
        }

        partial void OnKeyHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Size(KeyWidth, value));
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Size(KeyWidth, value));
            OnPropertyChanged(nameof(StimulusY));
            OnPropertyChanged(nameof(ErrorMarkY));
        }
        
        partial void OnErrorMarkWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Size(value, ErrorMarkHeight));
            OnPropertyChanged(nameof(ErrorMarkX));
        }

        partial void OnErrorMarkHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Size(ErrorMarkWidth, value));
            OnPropertyChanged(nameof(StimulusY));
            OnPropertyChanged(nameof(ErrorMarkX));
        }

        partial void OnBlockInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Size(value, BlockInstructionsHeight));
            OnPropertyChanged(nameof(BlockInstructionsX));
        }

        partial void OnBlockInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Size(BlockInstructionsWidth, value));
            OnPropertyChanged(nameof(StimulusY));
            OnPropertyChanged(nameof(ErrorMarkY));
            OnPropertyChanged(nameof(BlockInstructionsY));
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
