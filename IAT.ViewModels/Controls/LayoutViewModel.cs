using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.ViewModels
{
    public partial class LayoutViewModel : ObservableObject
    {
        private readonly ILayoutCalculatorService _calculator;
        private readonly IatTest _test;
        private readonly IProjectPackageService? _packageService;

        [ObservableProperty] private double previewWidth;
        [ObservableProperty] private double previewHeight;
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
        [ObservableProperty] private bool isLayoutEditMode = false;
        [ObservableProperty] private double scaleFactor;
        [ObservableProperty] private string statusMessage = string.Empty;

        // ── Trial preview content (driven by Blocks-tab trial selection) ──────

        /// <summary>Text shown in the stimulus area when previewing a text stimulus (or as image fallback).</summary>
        [ObservableProperty] private string previewStimulusText = "Sample Stimulus";

        /// <summary>Font family for the stimulus text preview.</summary>
        [ObservableProperty] private string previewStimulusFontFamily = "Segoe UI";

        /// <summary>Font size for the stimulus text preview.</summary>
        [ObservableProperty] private double previewStimulusFontSize = 28.0;

        /// <summary>Foreground brush for the stimulus text preview.</summary>
        [ObservableProperty] private Brush previewStimulusBrush = Brushes.DimGray;

        /// <summary>Image source when previewing an image stimulus; null for text.</summary>
        [ObservableProperty] private ImageSource? previewStimulusImage;

        /// <summary>True when the preview should show text (not an image).</summary>
        [ObservableProperty] private bool isPreviewTextVisible = true;

        /// <summary>True when the preview should show an image.</summary>
        [ObservableProperty] private bool isPreviewImageVisible = false;

        /// <summary>Placeholder shown for the left key when no block is selected (or key text is empty).</summary>
        public const string DummyLeftKeyText = "Left Key (E)";

        /// <summary>Placeholder shown for the right key when no block is selected (or key text is empty).</summary>
        public const string DummyRightKeyText = "Right Key (I)";

        /// <summary>Placeholder shown in the Block Instructions rectangle when no text is available.</summary>
        public const string DummyBlockInstructionsText = "Block Instructions";

        /// <summary>Label for the left response key in the preview.</summary>
        [ObservableProperty] private string leftKeyPreviewText = DummyLeftKeyText;

        /// <summary>Label for the right response key in the preview.</summary>
        [ObservableProperty] private string rightKeyPreviewText = DummyRightKeyText;

        /// <summary>Block instructions text shown in the Block Instructions rectangle.</summary>
        [ObservableProperty] private string previewBlockInstructionsText = DummyBlockInstructionsText;

        /// <summary>Foreground for the left key label (highlighted when trial is left-keyed).</summary>
        [ObservableProperty] private Brush leftKeyPreviewBrush = Brushes.Black;

        /// <summary>Foreground for the right key label (highlighted when trial is right-keyed).</summary>
        [ObservableProperty] private Brush rightKeyPreviewBrush = Brushes.Black;

        /// <summary>Font weight for the left key label.</summary>
        [ObservableProperty] private FontWeight leftKeyPreviewFontWeight = FontWeights.Normal;

        /// <summary>Font weight for the right key label.</summary>
        [ObservableProperty] private FontWeight rightKeyPreviewFontWeight = FontWeights.Normal;

        /// <summary>
        /// Last host size reported by the preview container. Used to re-fit the scale
        /// when InteriorWidth/Height change so the on-screen preview size stays stable
        /// while the logical stage (and aspect ratio) changes.
        /// </summary>
        private Size _lastAvailableSize;

        /// <summary>
        /// True while the constructor is assigning initial property values.
        /// Suppresses OnInteriorWidth/HeightChanged side-effects (ApplyUserOverrides + FitToWindow)
        /// that would otherwise run against half-initialized state and can re-enter layout.
        /// </summary>
        private bool _isInitializing = true;

        /// <summary>
        /// Re-entrancy guard for FitToWindow. SizeChanged on the preview host can fire again
        /// when ScaleFactor updates the visual tree; without this guard the call stack overflows.
        /// </summary>
        private bool _isFitting;

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
            // Guard: invalid host size, zero stage, or re-entrant call from SizeChanged.
            if (_isFitting)
                return;
            if (availableSize.Width <= 0 || availableSize.Height <= 0)
                return;
            if (InteriorWidth <= 0 || InteriorHeight <= 0)
                return;

            _isFitting = true;
            try
            {
                _lastAvailableSize = availableSize;

                // Design size comes from InteriorWidth/Height (logical stage).
                // Uniform scale keeps aspect ratio visible as the white preview rectangle
                // changes shape when Interior aspect changes.
                double scaleX = availableSize.Width / InteriorWidth;
                double scaleY = availableSize.Height / InteriorHeight;
                double newScale = Math.Min(scaleX, scaleY) * 0.95; // 5% padding looks better

                // Only push a new ScaleFactor when it actually changes.
                // Tiny floating-point differences from repeated layout passes must not
                // retrigger SizeChanged → FitToWindow → SizeChanged (stack overflow).
                if (Math.Abs(ScaleFactor - newScale) > 0.0001)
                {
                    ScaleFactor = newScale;
                    OnPropertyChanged(nameof(DesignHeight));
                    OnPropertyChanged(nameof(DesignWidth));
                }
            }
            finally
            {
                _isFitting = false;
            }
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
            if (_isInitializing || value <= 0)
                return;

            // Stage size changed. Element positions stay absolute (user drags are preserved).
            // ScaleFactor is recomputed so the on-screen preview frame size remains stable.
            try
            {
                LayoutRects rects = _calculator.GetFinalRects(_test.Layout);
                double layoutWidth = rects.Interior.Width;
                if (layoutWidth <= 0)
                    layoutWidth = value; // avoid divide-by-zero on first override

                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Size(value, Math.Max(1, InteriorHeight)));
                LeftKeyX = ((LeftKeyX + (KeyWidth / 2)) / layoutWidth) * value - (KeyWidth / 2);
                StimulusX = ((StimulusX + (StimulusWidth / 2)) / layoutWidth) * value - (StimulusWidth / 2);
                RightKeyX = ((RightKeyX + (KeyWidth / 2)) / layoutWidth) * value - (KeyWidth / 2);
                ErrorMarkX = ((ErrorMarkX + (ErrorMarkWidth / 2)) / layoutWidth) * value - (ErrorMarkWidth / 2);
                BlockInstructionsX = ((BlockInstructionsX + (BlockInstructionsWidth / 2)) / layoutWidth) * value - (BlockInstructionsWidth / 2);
                FitToWindow(_lastAvailableSize); // recompute scale factor to keep preview size stable
            }
            catch
            {
                // Calculator may not yet expose Interior as an overridable item; preview still works.
            }
        }

        partial void OnInteriorHeightChanged(double value)
        {
            if (_isInitializing || value <= 0)
                return;

            OnPropertyChanged(nameof(DesignHeight));
            try
            {
                LayoutRects rects = _calculator.GetFinalRects(_test.Layout);
                double layoutHeight = rects.Interior.Height;
                if (layoutHeight <= 0)
                    layoutHeight = value;

                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Size(Math.Max(1, InteriorWidth), value));
                LeftKeyY = ((LeftKeyY + (KeyHeight / 2)) / layoutHeight) * value - (KeyHeight / 2);
                StimulusY = ((StimulusY + (StimulusHeight / 2)) / layoutHeight) * value - (StimulusHeight / 2);
                RightKeyY = ((RightKeyY + (KeyHeight / 2)) / layoutHeight) * value - (KeyHeight / 2);
                ErrorMarkY = ((ErrorMarkY + (ErrorMarkHeight / 2)) / layoutHeight) * value - (ErrorMarkHeight / 2);
                BlockInstructionsY = ((BlockInstructionsY + (BlockInstructionsHeight / 2)) / layoutHeight) * value - (BlockInstructionsHeight / 2);
                FitToWindow(_lastAvailableSize); // recompute scale factor to keep preview size stable
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
        /// 

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
        /// <param name="calculator">Layout geometry calculator.</param>
        /// <param name="test">Shared IAT test domain model.</param>
        /// <param name="packageService">Optional package service used to resolve image stimulus bytes for the live preview.</param>
        public LayoutViewModel(
            ILayoutCalculatorService calculator,
            IatTest test,
            IProjectPackageService? packageService = null)
        {
            _calculator = calculator;
            _test = test;
            _packageService = packageService;
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

            // Construction complete — allow InteriorWidth/Height change handlers to run.
            _isInitializing = false;

            // Non-zero default so the Blocks-tab preview is visible before the first
            // FitToWindow (which previously only ran after visiting the Layout tab).
            if (ScaleFactor <= 0)
                ScaleFactor = 0.4;
        }

        /// <summary>
        /// Re-reads geometry from the shared <see cref="IatTest.Layout"/> after New/Open.
        /// Call after the domain model has been reset or replaced in place.
        /// </summary>
        public void ReloadGeometry()
        {
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

            if (StimulusX == 0 && StimulusY == 0 && ErrorMarkX == 0)
                RecalculateDefaultPositions();

            ApplyBlockKeys(null);
            ApplyBlockInstructions(null);
            ApplyTrialPreview(null);
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

        // ── Trial / block preview API ─────────────────────────────────────────

        /// <summary>
        /// Updates the stimulus area to reflect the selected trial's stimulus
        /// (text with style, or image when available). Clears to a placeholder when trial is null.
        /// Also highlights the key rectangle matching the trial's keyed direction.
        /// </summary>
        public void ApplyTrialPreview(Trial? trial)
        {
            if (trial is null)
            {
                ClearStimulusPreview();
                ApplyKeyHighlight(KeyedDirection.None);
                return;
            }

            ApplyKeyHighlight(trial.KeyedDirection);

            var stimulus = _test.GetStimulusById(trial.StimulusId);
            if (stimulus is null)
            {
                PreviewStimulusText = "(missing stimulus)";
                PreviewStimulusFontFamily = "Segoe UI";
                PreviewStimulusFontSize = 20;
                PreviewStimulusBrush = Brushes.Gray;
                PreviewStimulusImage = null;
                IsPreviewTextVisible = true;
                IsPreviewImageVisible = false;
                return;
            }

            if (stimulus is TextStimulus textStim)
            {
                PreviewStimulusText = string.IsNullOrWhiteSpace(textStim.Text) ? "(empty)" : textStim.Text;
                PreviewStimulusFontFamily = textStim.Style?.FontFamily ?? "Segoe UI";
                PreviewStimulusFontSize = textStim.Style?.FontSize > 0 ? textStim.Style.FontSize : 28.0;
                var color = textStim.Style?.FontColor ?? Colors.Black;
                PreviewStimulusBrush = new SolidColorBrush(color);
                PreviewStimulusImage = null;
                IsPreviewTextVisible = true;
                IsPreviewImageVisible = false;
                return;
            }

            if (stimulus is ImageStimulus imageStim)
            {
                var image = TryLoadImage(imageStim);
                if (image is not null)
                {
                    PreviewStimulusImage = image;
                    PreviewStimulusText = string.Empty;
                    IsPreviewTextVisible = false;
                    IsPreviewImageVisible = true;
                }
                else
                {
                    // Bytes not in package cache and no PackageUri — filename fallback only
                    PreviewStimulusImage = null;
                    PreviewStimulusText = string.IsNullOrWhiteSpace(imageStim.FileName)
                        ? (imageStim.GetDisplayPreview() is { Length: > 0 } p ? p : "(image)")
                        : imageStim.FileName;
                    PreviewStimulusFontFamily = "Segoe UI";
                    PreviewStimulusFontSize = 16;
                    PreviewStimulusBrush = Brushes.DimGray;
                    IsPreviewTextVisible = true;
                    IsPreviewImageVisible = false;
                }
                return;
            }

            // Unknown stimulus type
            PreviewStimulusText = stimulus.GetDisplayPreview();
            PreviewStimulusFontFamily = "Segoe UI";
            PreviewStimulusFontSize = 20;
            PreviewStimulusBrush = Brushes.DimGray;
            PreviewStimulusImage = null;
            IsPreviewTextVisible = true;
            IsPreviewImageVisible = false;
        }

        /// <summary>
        /// Updates left/right key labels from the block's response key definitions.
        /// Prefer block-linked keys; fall back to any key registered with the matching LayoutItem.
        /// When no block is selected (or a key has no text), shows the dummy placeholders so the
        /// preview never looks empty.
        /// </summary>
        public void ApplyBlockKeys(Block? block)
        {
            if (block is null)
            {
                LeftKeyPreviewText = DummyLeftKeyText;
                RightKeyPreviewText = DummyRightKeyText;
                return;
            }

            var left = block.LeftResponseId != Guid.Empty
                ? _test.GetKeyById(block.LeftResponseId)
                : null;
            var right = block.RightResponseId != Guid.Empty
                ? _test.GetKeyById(block.RightResponseId)
                : null;

            // Fallback: scan keys collection by layout role if the block has no linked IDs yet
            if (left is null || right is null)
            {
                foreach (var key in _test.KeysCollection)
                {
                    if (left is null && key.LayoutItem == LayoutItem.LeftKey)
                        left = key;
                    if (right is null && key.LayoutItem == LayoutItem.RightKey)
                        right = key;
                }
            }

            var leftText = left?.Text?.Trim();
            var rightText = right?.Text?.Trim();
            LeftKeyPreviewText = string.IsNullOrEmpty(leftText) ? DummyLeftKeyText : leftText;
            RightKeyPreviewText = string.IsNullOrEmpty(rightText) ? DummyRightKeyText : rightText;
        }

        /// <summary>
        /// Updates the Block Instructions rectangle text (from the Blocks-tab editor).
        /// Null/whitespace falls back to the dummy placeholder so the rectangle stays readable
        /// when no block is selected or instructions have not been entered yet.
        /// </summary>
        public void ApplyBlockInstructions(string? text)
        {
            PreviewBlockInstructionsText = string.IsNullOrWhiteSpace(text)
                ? DummyBlockInstructionsText
                : text.Trim();
        }

        /// <summary>
        /// Highlights the key that matches the trial's keyed direction (bold + accent color).
        /// The opposite key stays normal black.
        /// </summary>
        public void ApplyKeyHighlight(KeyedDirection direction)
        {
            // Accent blue used elsewhere in the app for selection / primary actions
            var highlight = new SolidColorBrush(Color.FromRgb(0, 122, 204));

            if (direction == KeyedDirection.Left)
            {
                LeftKeyPreviewBrush = highlight;
                LeftKeyPreviewFontWeight = FontWeights.Bold;
                RightKeyPreviewBrush = Brushes.Black;
                RightKeyPreviewFontWeight = FontWeights.Normal;
            }
            else if (direction == KeyedDirection.Right)
            {
                RightKeyPreviewBrush = highlight;
                RightKeyPreviewFontWeight = FontWeights.Bold;
                LeftKeyPreviewBrush = Brushes.Black;
                LeftKeyPreviewFontWeight = FontWeights.Normal;
            }
            else
            {
                LeftKeyPreviewBrush = Brushes.Black;
                LeftKeyPreviewFontWeight = FontWeights.Normal;
                RightKeyPreviewBrush = Brushes.Black;
                RightKeyPreviewFontWeight = FontWeights.Normal;
            }
        }

        private void ClearStimulusPreview()
        {
            PreviewStimulusText = "Sample Stimulus";
            PreviewStimulusFontFamily = "Segoe UI";
            PreviewStimulusFontSize = 28;
            PreviewStimulusBrush = Brushes.DimGray;
            PreviewStimulusImage = null;
            IsPreviewTextVisible = true;
            IsPreviewImageVisible = false;
        }

        /// <summary>
        /// Loads the image for preview, preferring in-memory package cache bytes (same source the
        /// stimulus editor uses), then falling back to <see cref="ImageStimulus.PackageUri"/>.
        /// Decoded at roughly the stimulus rectangle size for crisp display without excess memory.
        /// </summary>
        private ImageSource? TryLoadImage(ImageStimulus imageStim)
        {
            // 1. Preferred path: bytes from IProjectPackageService (AddImageAsync cache)
            if (_packageService is not null)
            {
                try
                {
                    var bytes = _packageService.GetImageBytes(imageStim.Id);
                    if (bytes is { Length: > 0 })
                        return BitmapFromBytes(bytes);
                }
                catch
                {
                    // fall through to URI
                }
            }

            // 2. Fallback: PackageUri (OPC part URI when loaded from a saved package)
            if (imageStim.PackageUri is not null)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = imageStim.PackageUri;
                    if (StimulusWidth > 0)
                        bmp.DecodePixelWidth = Math.Max(1, (int)StimulusWidth);
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch
                {
                    // ignore
                }
            }

            return null;
        }

        private BitmapSource BitmapFromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            // Decode near the stimulus rectangle width so the preview stays sharp and light
            if (StimulusWidth > 0)
                image.DecodePixelWidth = Math.Max(1, (int)StimulusWidth);
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
