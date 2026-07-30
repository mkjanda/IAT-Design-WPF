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
        [ObservableProperty] private double keyedInstructionsHeight; [ObservableProperty] private double textInstructionsWidth;
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
        [ObservableProperty] private FontWeight leftKeyPreviewFontWeight = FontWeights.Bold;

        /// <summary>Font weight for the right key label.</summary>
        [ObservableProperty] private FontWeight rightKeyPreviewFontWeight = FontWeights.Bold;

        /// <summary>
        /// True when the left key should show the blue outline used by the Instructions-tab
        /// preview (Mock Item + Outline Correct Response + Left, or a left-keyed trial).
        /// </summary>
        [ObservableProperty] private bool isLeftKeyOutlined;

        /// <summary>
        /// True when the right key should show the blue outline used by the Instructions-tab
        /// preview (Mock Item + Outline Correct Response + Right, or a right-keyed trial).
        /// </summary>
        [ObservableProperty] private bool isRightKeyOutlined;

        /// <summary>True when the error-mark glyph should be visible (Mock Item with ShowErrorMark).</summary>
        [ObservableProperty] private bool isErrorMarkVisible = false;

        /// <summary>
        /// Continue-prompt text shown in the ContinueInstructions layout band while an
        /// instruction screen is selected on the Blocks tab. Empty / hidden for trial mode.
        /// </summary>
        [ObservableProperty] private string previewContinueText = string.Empty;

        /// <summary>True when the continue-instructions strip should be visible in the Blocks preview.</summary>
        [ObservableProperty] private bool isContinueInstructionsVisible;

        /// <summary>
        /// Controls the entire stimulus slot on the Blocks preview. False for Text / Keyed
        /// instruction screens so "Sample Stimulus" cannot appear under the body text.
        /// True for trial previews and Mock Item screens that have an assigned stimulus.
        /// </summary>
        [ObservableProperty] private bool isStimulusAreaVisible = true;

        /// <summary>
        /// Controls the left/right response key chrome on the Blocks preview.
        /// False for Text instruction screens (no keys on that stage). True for
        /// Keyed / Mock Item instruction previews and for trial mode.
        /// </summary>
        [ObservableProperty] private bool isResponseKeysVisible = true;

        /// <summary>
        /// Canvas position/size for the instruction-body border in the Blocks-tab preview.
        /// Switched by <see cref="ApplyInstructionPreview"/> to the layout rectangle that matches
        /// the screen type (TextInstructions / KeyedInstructions / MockItemInstructions),
        /// and restored to BlockInstructions when showing block/trial content.
        /// </summary>
        [ObservableProperty] private double activeInstructionsX;
        [ObservableProperty] private double activeInstructionsY;
        [ObservableProperty] private double activeInstructionsWidth;
        [ObservableProperty] private double activeInstructionsHeight;

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
        /// When true, geometry property setters must not write <see cref="Layout.UserSizeOverrides"/>.
        /// Used while applying calculator-derived instruction regions so they stay derived.
        /// </summary>
        private bool _suppressGeometryPush;

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
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Rect(0, 0, InteriorWidth, InteriorHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Rect(StimulusX, StimulusY, StimulusWidth, StimulusHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Rect(LeftKeyX, LeftKeyY, KeyWidth, KeyHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Rect(RightKeyX, RightKeyY, KeyWidth, KeyHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Rect(ErrorMarkX, ErrorMarkY, ErrorMarkWidth, ErrorMarkHeight));
                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Rect(BlockInstructionsX, BlockInstructionsY, BlockInstructionsWidth, BlockInstructionsHeight));

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

                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Rect(0, 0, value, Math.Max(1, InteriorHeight)));
                LeftKeyX = ((LeftKeyX + (KeyWidth / 2)) * value / layoutWidth) - (KeyWidth * value / layoutWidth / 2);
                RightKeyX = ((RightKeyX + (KeyWidth / 2)) * value / layoutWidth) - (KeyWidth * value / layoutWidth / 2);
                KeyWidth *= value / layoutWidth;
                StimulusX = ((StimulusX + (StimulusWidth / 2)) * value / layoutWidth) - (StimulusWidth * value / layoutWidth / 2);
                StimulusWidth *= value / layoutWidth;
                ErrorMarkX = ((ErrorMarkX + (ErrorMarkWidth / 2)) * value / layoutWidth) - (ErrorMarkWidth * value / layoutWidth / 2);
                ErrorMarkWidth *= value / layoutWidth;
                BlockInstructionsX = ((BlockInstructionsX + (BlockInstructionsWidth / 2)) * value / layoutWidth)  - (BlockInstructionsWidth * value / layoutWidth / 2);
                BlockInstructionsWidth *= value / layoutWidth;

                // Recompute instruction regions that depend on interior / keys / error mark.
                RefreshDerivedInstructionRects();

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

                _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Interior, new Rect(0, 0, Math.Max(1, InteriorWidth), value));
                LeftKeyY = ((LeftKeyY + (KeyHeight / 2)) / layoutHeight) * value - (KeyHeight * value / layoutHeight / 2);
                RightKeyY = ((RightKeyY + (KeyHeight / 2)) / layoutHeight) * value - (KeyHeight * value / layoutHeight / 2);
                KeyHeight *= value / layoutHeight;
                StimulusY = ((StimulusY + (StimulusHeight / 2)) / layoutHeight) * value - (StimulusHeight * value / layoutHeight / 2);
                StimulusHeight *= value / layoutHeight;
                ErrorMarkY = ((ErrorMarkY + (ErrorMarkHeight / 2)) / layoutHeight) * value - (ErrorMarkHeight * value / layoutHeight / 2);
                ErrorMarkHeight *= value / layoutHeight;
                BlockInstructionsY = ((BlockInstructionsY + (BlockInstructionsHeight / 2)) / layoutHeight) * value - (BlockInstructionsHeight * value / layoutHeight / 2);
                BlockInstructionsHeight *= value / layoutHeight;

                // Recompute instruction regions that depend on interior / keys / error mark.
                RefreshDerivedInstructionRects();

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
        [ObservableProperty] private double mockItemInstructionsX;
        [ObservableProperty] private double mockItemInstructionsY;
        [ObservableProperty] private double keyedInstructionsX;
        [ObservableProperty] private double keyedInstructionsY;
        [ObservableProperty] private double textInstructionsX;
        [ObservableProperty] private double textInstructionsY;
        [ObservableProperty] private double continueInstructionsX;
        [ObservableProperty] private double continueInstructionsY;


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
            MockItemInstructionsX = rects.MockItemInstructions.X;
            MockItemInstructionsY = rects.MockItemInstructions.Y;
            KeyedInstructionsX = rects.KeyedInstructions.X;
            KeyedInstructionsY = rects.KeyedInstructions.Y;
            TextInstructionsX = rects.TextInstructions.X;
            TextInstructionsY = rects.TextInstructions.Y;
            ContinueInstructionsX = rects.ContinueInstructions.X;
            ContinueInstructionsY = rects.ContinueInstructions.Y;

            // Default active body region is block instructions (trial / block mode).
            SetActiveInstructionsRect(rects.BlockInstructions);

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
            StimulusX = rects.Stimulus.X; StimulusY = rects.Stimulus.Y; 
            StimulusWidth = rects.Stimulus.Width; StimulusHeight = rects.Stimulus.Height;
            LeftKeyX = rects.LeftKey.X; LeftKeyY = rects.LeftKey.Y; 
            RightKeyX = rects.RightKey.X; RightKeyY = rects.RightKey.Y; 
            ErrorMarkX = rects.ErrorMark.X; ErrorMarkY = rects.ErrorMark.Y; 
            KeyWidth = rects.LeftKey.Width; KeyHeight = rects.LeftKey.Height;
            ErrorMarkX = rects.ErrorMark.X; ErrorMarkY = rects.ErrorMark.Y;
            ErrorMarkWidth = rects.ErrorMark.Width; ErrorMarkHeight = rects.ErrorMark.Height;
            BlockInstructionsX = rects.BlockInstructions.X; BlockInstructionsY = rects.BlockInstructions.Y;
            BlockInstructionsWidth = rects.BlockInstructions.Width; BlockInstructionsHeight = rects.BlockInstructions.Height;
            MockItemInstructionsX = rects.MockItemInstructions.X; MockItemInstructionsY = rects.MockItemInstructions.Y;
            MockItemInstructionsWidth = rects.MockItemInstructions.Width; MockItemInstructionsHeight = rects.MockItemInstructions.Height;
            KeyedInstructionsX = rects.KeyedInstructions.X; KeyedInstructionsY = rects.KeyedInstructions.Y;
            KeyedInstructionsWidth = rects.KeyedInstructions.Width; KeyedInstructionsHeight = rects.KeyedInstructions.Height;
            TextInstructionsX = rects.TextInstructions.X; TextInstructionsY = rects.TextInstructions.Y;
            TextInstructionsWidth = rects.TextInstructions.Width; TextInstructionsHeight = rects.TextInstructions.Height;
            ContinueInstructionsX = rects.ContinueInstructions.X; ContinueInstructionsY = rects.ContinueInstructions.Y;
            ContinueInstructionsWidth = rects.ContinueInstructions.Width; ContinueInstructionsHeight = rects.ContinueInstructions.Height;

            SetActiveInstructionsRect(rects.BlockInstructions);

            if (StimulusX == 0 && StimulusY == 0 && ErrorMarkX == 0)
                RecalculateDefaultPositions();

            ApplyBlockKeys(null);
            ApplyBlockInstructions(null);
            ApplyTrialPreview(null);
        }

        partial void OnStimulusWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Rect(StimulusX, StimulusY, value, StimulusHeight));
        }

        partial void OnStimulusHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.Stimulus, new Rect(StimulusX, StimulusY, StimulusWidth, value));
        }

        partial void OnKeyWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Rect(LeftKeyX, LeftKeyY, value, KeyHeight));
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Rect(RightKeyX, RightKeyY, value, KeyHeight));
            if (!_isInitializing) RefreshDerivedInstructionRects();
        }

        partial void OnKeyHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.LeftKey, new Rect(LeftKeyX, LeftKeyY, KeyWidth, value));
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.RightKey, new Rect(RightKeyX, RightKeyY, KeyWidth, value));
            if (!_isInitializing) RefreshDerivedInstructionRects();
        }

        partial void OnErrorMarkWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Rect(ErrorMarkX, ErrorMarkY, value, ErrorMarkHeight));
            if (!_isInitializing) RefreshDerivedInstructionRects();
        }

        partial void OnErrorMarkHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ErrorMark, new Rect(ErrorMarkX, ErrorMarkY, ErrorMarkWidth, value));
            if (!_isInitializing) RefreshDerivedInstructionRects();
        }

        partial void OnBlockInstructionsWidthChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Rect(BlockInstructionsX, BlockInstructionsY, value, BlockInstructionsHeight));
        }

        partial void OnBlockInstructionsHeightChanged(double value)
        {
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.BlockInstructions, new Rect(BlockInstructionsX, BlockInstructionsY, BlockInstructionsWidth, value));
        }

        partial void OnMockItemInstructionsWidthChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.MockItemInstructions, new Rect(MockItemInstructionsX, MockItemInstructionsY, value, MockItemInstructionsHeight));
        }

        partial void OnMockItemInstructionsHeightChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.MockItemInstructions, new Rect(MockItemInstructionsX, MockItemInstructionsY, MockItemInstructionsWidth, value));
        }

        partial void OnKeyedInstructionsWidthChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.KeyedInstructions, new Rect(KeyedInstructionsX, KeyedInstructionsY, value, KeyedInstructionsHeight));
        }

        partial void OnKeyedInstructionsHeightChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.KeyedInstructions, new Rect(KeyedInstructionsX, KeyedInstructionsY, KeyedInstructionsWidth, value));
        }

        partial void OnTextInstructionsWidthChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.TextInstructions, new Rect(TextInstructionsX, TextInstructionsY, value, TextInstructionsHeight));
        }

        partial void OnTextInstructionsHeightChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.TextInstructions, new Rect(TextInstructionsX, TextInstructionsY, TextInstructionsWidth, value));
        }

        partial void OnContinueInstructionsWidthChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ContinueInstructions, new Rect(ContinueInstructionsX, ContinueInstructionsY, value, ContinueInstructionsHeight));
        }

        partial void OnContinueInstructionsHeightChanged(double value)
        {
            if (_isInitializing || _suppressGeometryPush) return;
            _calculator.ApplyUserOverrides(_test.Layout, LayoutItem.ContinueInstructions, new Rect(ContinueInstructionsX, ContinueInstructionsY, ContinueInstructionsWidth, value));
        }

        // ── Trial / block preview API ─────────────────────────────────────────

        /// <summary>
        /// Updates the stimulus area to reflect the selected trial's stimulus
        /// (text with style, or image when available). Clears to a placeholder when trial is null.
        /// Also highlights the key rectangle matching the trial's keyed direction.
        /// </summary>
        public void ApplyTrialPreview(Trial? trial)
        {
            IsErrorMarkVisible = false;
            // Continue prompt is instruction-screen only.
            IsContinueInstructionsVisible = false;
            PreviewContinueText = string.Empty;
            IsStimulusAreaVisible = true;
            IsResponseKeysVisible = true;

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
                    // Bytes not in package cache and no PackageUri — show Text (leaf file name),
                    // never the full source path that may still sit in FileName on older packages.
                    PreviewStimulusImage = null;
                    var preview = imageStim.GetDisplayPreview();
                    PreviewStimulusText = string.IsNullOrWhiteSpace(preview) ? "(image)" : preview;
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
        /// Drives the layout preview from an instruction screen assigned to the current block.
        /// Body text is placed in the layout rectangle that matches the screen type:
        /// <list type="bullet">
        /// <item><see cref="TextInstructionScreen"/> → <c>TextInstructionsRect</c></item>
        /// <item><see cref="KeyedInstructionScreen"/> → <c>KeyedInstructionsRect</c></item>
        /// <item><see cref="MockItemInstructionScreen"/> → <c>MockItemInstructionsRect</c></item>
        /// </list>
        /// Keyed/Mock also populate keys; Mock fills the stimulus slot and optional error/outline.
        /// Pass null to clear instruction-specific state and restore the block-instructions region.
        /// </summary>
        /// <summary>
        /// Clears all stage chrome so the Instructions tab does not show leftover Blocks-tab
        /// trial content (stimulus, keys, error mark, body text) when no instruction screen is selected.
        /// Does not change geometry — only visibility and preview strings.
        /// </summary>
        public void ClearStageForInstructionsIdle()
        {
            IsErrorMarkVisible = false;
            IsContinueInstructionsVisible = false;
            PreviewContinueText = string.Empty;
            PreviewBlockInstructionsText = string.Empty;
            IsResponseKeysVisible = false;
            ApplyKeyHighlight(KeyedDirection.None);
            HideStimulusPreview();
            // Keep ActiveInstructions on the block band so the empty stage is stable if the user
            // switches back to Blocks without a sequence re-selection.
            SetActiveInstructionsRect(new Rect(
                BlockInstructionsX, BlockInstructionsY,
                BlockInstructionsWidth, BlockInstructionsHeight));
        }

        public void ApplyInstructionPreview(InstructionScreen? screen)
        {
            if (screen is null)
            {
                IsErrorMarkVisible = false;
                IsContinueInstructionsVisible = false;
                PreviewContinueText = string.Empty;
                ApplyKeyHighlight(KeyedDirection.None);
                // Restore the body region to BlockInstructions for trial / block mode.
                // Intentionally does NOT hide stimulus/keys — Blocks calls this when returning
                // to trial mode and then ApplyTrialPreview / ApplyBlockKeys. Instructions tab
                // with no selection must call <see cref="ClearStageForInstructionsIdle"/> instead.
                SetActiveInstructionsRect(new Rect(
                    BlockInstructionsX, BlockInstructionsY,
                    BlockInstructionsWidth, BlockInstructionsHeight));
                return;
            }

            // Fresh derived geometry so the body border always matches the layout rules.
            SyncDerivedInstructionGeometry();

            // Body text → type-specific instructions rectangle.
            var body = screen.Text?.Trim();
            PreviewBlockInstructionsText = string.IsNullOrWhiteSpace(body)
                ? "(empty instruction)"
                : body;

            // Continue prompt — always shown for instruction screens (Space is fixed).
            var continueText = screen.ContinueInstructions?.Text?.Trim();
            PreviewContinueText = string.IsNullOrWhiteSpace(continueText)
                ? "Press the spacebar to continue"
                : continueText;
            IsContinueInstructionsVisible = true;

            IsErrorMarkVisible = false;
            ApplyKeyHighlight(KeyedDirection.None);
            // Text / Keyed / empty Mock: no stimulus slot. Mock with a stimulus re-enables below.
            HideStimulusPreview();

            switch (screen)
            {
                case TextInstructionScreen:
                    // Full interior band — no keys, no stimulus; body text owns the stage.
                    IsResponseKeysVisible = false;
                    SetActiveInstructionsRect(new Rect(
                        TextInstructionsX, TextInstructionsY,
                        TextInstructionsWidth, TextInstructionsHeight));
                    break;

                case KeyedInstructionScreen keyed:
                    // Keyed body fills the stage below the keys; stimulus area stays collapsed.
                    IsResponseKeysVisible = true;
                    SetActiveInstructionsRect(new Rect(
                        KeyedInstructionsX, KeyedInstructionsY,
                        KeyedInstructionsWidth, KeyedInstructionsHeight));
                    ApplyInstructionKeys(keyed.LeftResponseId, keyed.RightResponseId);
                    break;

                case MockItemInstructionScreen mock:
                    IsResponseKeysVisible = true;
                    ApplyInstructionKeys(mock.LeftResponseId, mock.RightResponseId);
                    IsErrorMarkVisible = mock.ShowErrorMark;

                    if (mock.OutlineCorrectResponse && mock.KeyedDirection is not null
                        && mock.KeyedDirection != KeyedDirection.None)
                    {
                        ApplyKeyHighlight(mock.KeyedDirection);
                    }

                    // Body band = MockItemInstructions, but never higher than the stimulus bottom.
                    // Without this clamp a high/mis-scaled ErrorMark makes the derived rect span
                    // the whole stage so the body text runs straight through the stimulus.
                    var stimulusBottom = StimulusY + StimulusHeight;
                    var bodyTop = Math.Max(MockItemInstructionsY, stimulusBottom);
                    var bodyBottom = MockItemInstructionsY + MockItemInstructionsHeight;
                    if (bodyBottom <= bodyTop)
                    {
                        // Degenerate derived band (error mark at/below continue) — use a
                        // readable strip down to the continue line so the body still appears.
                        bodyBottom = Math.Max(bodyTop + 48, ContinueInstructionsY);
                    }
                    SetActiveInstructionsRect(new Rect(
                        0,
                        bodyTop,
                        InteriorWidth,
                        Math.Max(0, bodyBottom - bodyTop)));

                    // Stimulus slot — only for Mock Item (matches Instructions-tab stacking).
                    var stim = _test.GetStimulusById(mock.StimulusId);
                    if (stim is TextStimulus textStim)
                    {
                        PreviewStimulusText = string.IsNullOrWhiteSpace(textStim.Text) ? "(empty)" : textStim.Text;
                        PreviewStimulusFontFamily = textStim.Style?.FontFamily ?? "Segoe UI";
                        PreviewStimulusFontSize = textStim.Style?.FontSize > 0 ? textStim.Style.FontSize : 28.0;
                        PreviewStimulusBrush = new SolidColorBrush(textStim.Style?.FontColor ?? Colors.Black);
                        PreviewStimulusImage = null;
                        IsPreviewTextVisible = true;
                        IsPreviewImageVisible = false;
                        IsStimulusAreaVisible = true;
                    }
                    else if (stim is ImageStimulus imageStim)
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
                            PreviewStimulusImage = null;
                            PreviewStimulusText = imageStim.GetDisplayPreview();
                            PreviewStimulusFontFamily = "Segoe UI";
                            PreviewStimulusFontSize = 16;
                            PreviewStimulusBrush = Brushes.DimGray;
                            IsPreviewTextVisible = true;
                            IsPreviewImageVisible = false;
                        }
                        IsStimulusAreaVisible = true;
                    }
                    // else: leave stimulus hidden (no Sample Stimulus bleed under the body)
                    break;

                default:
                    // Unknown instruction subtype — treat like text (no keys).
                    IsResponseKeysVisible = false;
                    SetActiveInstructionsRect(new Rect(
                        TextInstructionsX, TextInstructionsY,
                        TextInstructionsWidth, TextInstructionsHeight));
                    break;
            }
        }

        /// <summary>
        /// Points the Blocks-tab instruction-body border at the given layout rectangle.
        /// </summary>
        private void SetActiveInstructionsRect(Rect rect)
        {
            ActiveInstructionsX = rect.X;
            ActiveInstructionsY = rect.Y;
            ActiveInstructionsWidth = rect.Width;
            ActiveInstructionsHeight = rect.Height;
        }

        /// <summary>
        /// Recomputes derived instruction regions and keeps the active body border in sync
        /// when it was already pointing at one of those regions.
        /// </summary>
        private void RefreshDerivedInstructionRects()
        {
            static bool ApproxEqual(double a, double b) => Math.Abs(a - b) < 0.5;

            bool Matches(double x, double y, double w, double h) =>
                ApproxEqual(ActiveInstructionsX, x)
                && ApproxEqual(ActiveInstructionsY, y)
                && ApproxEqual(ActiveInstructionsWidth, w)
                && ApproxEqual(ActiveInstructionsHeight, h);

            var wasText = Matches(TextInstructionsX, TextInstructionsY, TextInstructionsWidth, TextInstructionsHeight);
            var wasKeyed = Matches(KeyedInstructionsX, KeyedInstructionsY, KeyedInstructionsWidth, KeyedInstructionsHeight);
            var wasMock = Matches(MockItemInstructionsX, MockItemInstructionsY, MockItemInstructionsWidth, MockItemInstructionsHeight);

            SyncDerivedInstructionGeometry();

            if (wasText)
                SetActiveInstructionsRect(new Rect(TextInstructionsX, TextInstructionsY, TextInstructionsWidth, TextInstructionsHeight));
            else if (wasKeyed)
                SetActiveInstructionsRect(new Rect(KeyedInstructionsX, KeyedInstructionsY, KeyedInstructionsWidth, KeyedInstructionsHeight));
            else if (wasMock)
                SetActiveInstructionsRect(new Rect(MockItemInstructionsX, MockItemInstructionsY, MockItemInstructionsWidth, MockItemInstructionsHeight));
        }

        /// <summary>
        /// Pulls Text / Keyed / Mock / Continue instruction rectangles from the calculator
        /// (derived from interior + keys + error mark) into the ViewModel and domain layout.
        /// Does not write <see cref="Layout.UserSizeOverrides"/> so the regions stay derived.
        /// </summary>
        private void SyncDerivedInstructionGeometry()
        {
            // Drop any accidental overrides on calculated regions so GetFinalRects re-derives them.
            var overrides = _test.Layout.UserSizeOverrides;
            overrides.Remove(LayoutItem.TextInstructions);
            overrides.Remove(LayoutItem.KeyedInstructions);
            overrides.Remove(LayoutItem.MockItemInstructions);
            overrides.Remove(LayoutItem.ContinueInstructions);

            // Keep base geometry the calculator uses for keys / error mark in sync with the VM.
            _test.Layout.LeftKeyRect = new Rect(LeftKeyX, LeftKeyY, KeyWidth, KeyHeight);
            _test.Layout.RightKeyRect = new Rect(RightKeyX, RightKeyY, KeyWidth, KeyHeight);
            _test.Layout.ErrorMarkRect = new Rect(ErrorMarkX, ErrorMarkY, ErrorMarkWidth, ErrorMarkHeight);
            _test.Layout.InteriorRect = new Rect(0, 0, InteriorWidth, InteriorHeight);

            var rects = _calculator.GetFinalRects(_test.Layout);

            _suppressGeometryPush = true;
            try
            {
                TextInstructionsX = rects.TextInstructions.X;
                TextInstructionsY = rects.TextInstructions.Y;
                TextInstructionsWidth = rects.TextInstructions.Width;
                TextInstructionsHeight = rects.TextInstructions.Height;

                KeyedInstructionsX = rects.KeyedInstructions.X;
                KeyedInstructionsY = rects.KeyedInstructions.Y;
                KeyedInstructionsWidth = rects.KeyedInstructions.Width;
                KeyedInstructionsHeight = rects.KeyedInstructions.Height;

                MockItemInstructionsX = rects.MockItemInstructions.X;
                MockItemInstructionsY = rects.MockItemInstructions.Y;
                MockItemInstructionsWidth = rects.MockItemInstructions.Width;
                MockItemInstructionsHeight = rects.MockItemInstructions.Height;

                ContinueInstructionsX = rects.ContinueInstructions.X;
                ContinueInstructionsY = rects.ContinueInstructions.Y;
                ContinueInstructionsWidth = rects.ContinueInstructions.Width;
                ContinueInstructionsHeight = rects.ContinueInstructions.Height;
            }
            finally
            {
                _suppressGeometryPush = false;
            }

            _test.Layout.TextInstructionsRect = rects.TextInstructions;
            _test.Layout.KeyedInstructionsRect = rects.KeyedInstructions;
            _test.Layout.MockItemInstructionsRect = rects.MockItemInstructions;
            _test.Layout.ContinueInstructionsRect = rects.ContinueInstructions;
        }

        private void ApplyInstructionKeys(Guid leftId, Guid rightId)
        {
            var left = leftId != Guid.Empty ? _test.GetKeyById(leftId) : null;
            var right = rightId != Guid.Empty ? _test.GetKeyById(rightId) : null;

            var leftText = left?.Text?.Trim();
            var rightText = right?.Text?.Trim();
            LeftKeyPreviewText = string.IsNullOrEmpty(leftText) ? DummyLeftKeyText : leftText;
            RightKeyPreviewText = string.IsNullOrEmpty(rightText) ? DummyRightKeyText : rightText;
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
        /// Updates the Block Instructions rectangle text (from the Blocks-tab editor)
        /// and points the active body region at <c>BlockInstructionsRect</c>.
        /// Null/whitespace falls back to the dummy placeholder so the rectangle stays readable
        /// when no block is selected or instructions have not been entered yet.
        /// </summary>
        public void ApplyBlockInstructions(string? text)
        {
            PreviewBlockInstructionsText = string.IsNullOrWhiteSpace(text)
                ? DummyBlockInstructionsText
                : text.Trim();

            SetActiveInstructionsRect(new Rect(
                BlockInstructionsX, BlockInstructionsY,
                BlockInstructionsWidth, BlockInstructionsHeight));
        }

        /// <summary>
        /// Highlights the key that matches the trial's (or Mock Item's) keyed direction.
        /// Matches the Instructions-tab treatment: blue outline border on the correct side,
        /// both keys stay bold black text. Clears the outline when direction is None.
        /// </summary>
        public void ApplyKeyHighlight(KeyedDirection direction)
        {
            IsLeftKeyOutlined = direction == KeyedDirection.Left;
            IsRightKeyOutlined = direction == KeyedDirection.Right;

            // Text stays black + bold on both sides — the outline is the highlight signal,
            // identical to InstructionManagerControl's keyed/mock preview.
            LeftKeyPreviewBrush = Brushes.Black;
            RightKeyPreviewBrush = Brushes.Black;
            LeftKeyPreviewFontWeight = FontWeights.Bold;
            RightKeyPreviewFontWeight = FontWeights.Bold;
        }

        /// <summary>
        /// Resets the stimulus slot to the neutral sample placeholder used in trial / block mode.
        /// </summary>
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
        /// Hides the stimulus slot entirely. Used while previewing Text / Keyed instruction
        /// screens (and Mock Items with no assigned stimulus) so body text cannot run through
        /// a leftover sample or trial stimulus.
        /// </summary>
        private void HideStimulusPreview()
        {
            PreviewStimulusText = string.Empty;
            PreviewStimulusImage = null;
            IsPreviewTextVisible = false;
            IsPreviewImageVisible = false;
            IsStimulusAreaVisible = false;
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
        public void FlushToDomain()
        {
            LayoutRects rects = _calculator.GetFinalRects(_test.Layout);
            _test.Layout.ApplyRects(rects); 
        }
    }

}
