using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentValidation;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Messages;
using IAT.Core.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Media;
// ErrorNotificationMessage lives in IAT.Core.Services (same namespace as the other service contracts).

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Blocks tab. Uses the shared singleton <see cref="IatTest"/> domain model so that
/// blocks created here appear immediately in the Trials tab (and vice-versa). Trials created on the
/// Trials tab appear in the bottom sequence grid because they are resolved through <see cref="Block.Trials"/>
/// and <see cref="Block.NotifyTrialsChanged"/> raises change notification.
/// Instruction screens assigned to the block appear at the top of the same grid with an "I#" indicator.
/// Selecting a row drives the layout preview (trial stimulus or instruction screen content).
/// </summary>
public partial class BlockEditViewModel : ObservableObject
{
    private readonly IProjectPackageService _packageService;
    private readonly ILayoutCalculatorService _layoutCalculator;
    private readonly IValidator<Block> _blockValidator;
    private readonly IatTest _currentTest;

    /// <summary>
    /// Once a standard 7-block structure has been generated from the two practice blocks,
    /// further structural changes (Add Block / re-generate) are locked so the IAT remains valid.
    /// </summary>
    [ObservableProperty]
    private bool isStandardStructureLocked;

    /// <summary>
    /// Live shared collection of blocks from the domain model.
    /// Changes here are visible to <see cref="TrialsManagerViewModel"/> automatically.
    /// </summary>
    public ObservableCollection<Block> Blocks => _currentTest.BlocksCollection;

    [ObservableProperty] private Block? selectedBlock;
    [ObservableProperty] private LayoutViewModel? layoutViewModel;

    /// <summary>
    /// Currently selected trial (kept for compatibility with RefreshLayoutPreview callers).
    /// Prefer <see cref="SelectedSequenceRow"/> for new code.
    /// </summary>
    [ObservableProperty] private Trial? selectedTrial;

    /// <summary>
    /// Unified list shown in the bottom grid: assigned instruction screens first, then trials.
    /// </summary>
    public ObservableCollection<BlockSequenceRow> SequenceRows { get; } = new();

    /// <summary>
    /// Currently selected row in the sequence grid. Drives the center layout preview.
    /// </summary>
    [ObservableProperty] private BlockSequenceRow? selectedSequenceRow;

    /// <summary>
    /// Bound to the Instructions Text editor. Mirrors <see cref="Block.BlockInstructions"/> /
    /// the linked <see cref="FormattedText"/> and pushes live updates into the layout preview.
    /// </summary>
    [ObservableProperty] private string blockInstructionsText = string.Empty;

    /// <summary>Font family for the selected block's instructions (FormattedText.Style).</summary>
    [ObservableProperty] private string instructionFontFamily = "Segoe UI";

    /// <summary>Font size for the selected block's instructions.</summary>
    [ObservableProperty] private double instructionFontSize = 24.0;

    /// <summary>Font color for the selected block's instructions.</summary>
    [ObservableProperty] private Color instructionTextColor = Colors.Black;

    /// <summary>Brush for a tiny live color swatch next to the palette.</summary>
    public SolidColorBrush InstructionPreviewBrush => new(InstructionTextColor);

    /// <summary>Shared font list used by text stimuli; kept identical for a consistent authoring experience.</summary>
    public ObservableCollection<string> AvailableFontFamilies { get; } = new()
    {
        "Segoe UI", "Arial", "Calibri", "Verdana", "Trebuchet MS", "Tahoma",
        "Georgia", "Times New Roman", "Cambria", "Garamond", "Palatino Linotype",
        "Consolas", "Courier New", "Segoe Script", "Impact"
    };

    public ObservableCollection<double> AvailableFontSizes { get; } =
        new() { 12, 16, 18, 20, 24, 28, 32, 36, 48, 54, 66, 72 };

    // Remember last-applied style so a newly created block starts with the author's preferred look.
    private string _lastInstructionFontFamily = "Segoe UI";
    private double _lastInstructionFontSize = 24.0;
    private Color _lastInstructionTextColor = Colors.Black;

    /// <summary>Suppresses write-back while the editor is being loaded from the domain model.</summary>
    private bool _loadingInstructionEditor;

    /// <summary>
    /// True when the Instructions Text box may be edited. False while an instruction-screen
    /// row is selected in the sequence grid — that editor is for <see cref="Block.BlockInstructions"/>
    /// only and must not rewrite the selected instruction screen's body.
    /// </summary>
    public bool IsBlockInstructionsEditable =>
        SelectedBlock is not null
        && SelectedSequenceRow is not { IsInstruction: true };

    public BlockEditViewModel(
        IProjectPackageService packageService,
        ILayoutCalculatorService layoutCalculator,
        IValidator<Block> blockValidator,
        LayoutViewModel layoutViewModel,
        IatTest currentTest)
    {
        _packageService = packageService;
        _layoutCalculator = layoutCalculator;
        _blockValidator = blockValidator ?? throw new ArgumentNullException(nameof(blockValidator));
        _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));
        LayoutViewModel = layoutViewModel;

        // Keep Generate / Add command CanExecute in sync with the live block list.
        Blocks.CollectionChanged += OnBlocksCollectionChanged;

        // Select first block if any exist
        if (Blocks.Count > 0)
            SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).First();
    }

    private void OnBlocksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        GenerateSevenBlockIatCommand.NotifyCanExecuteChanged();
        AddBlockCommand.NotifyCanExecuteChanged();
        DeleteBlockCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task LoadTestAsync(string packagePath)
    {
        // Loading is handled by the package service into the shared IatTest.
        // After load, just select the first block so the UI refreshes.
        await _packageService.LoadProjectAsync(packagePath);

        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault();
    }

    /// <summary>
    /// True while the user is still building the two practice blocks and has not yet
    /// locked the structure by generating the standard 7-block IAT.
    /// </summary>
    private bool CanAddBlock() => !IsStandardStructureLocked;

    [RelayCommand(CanExecute = nameof(CanAddBlock))]
    private void AddBlock()
    {
        var nextNumber = Blocks.Count + 1;

        var block = new Block
        {
            Id = Guid.NewGuid(),
            Name = $"Block {nextNumber}",
            BlockNumber = nextNumber,
            NumPresentations = DefaultPresentationsFor(nextNumber)
        };

        // Add through the domain model so the block is fully registered
        // (cache, IatTest reference, etc.) and appears in every ViewModel
        // that binds to BlocksCollection.
        _currentTest.AddBlock(block);

        // Own empty response keys — never share Block 1's Key instances.
        // Without dedicated ids the layout preview used to fall back to the first
        // LeftKey/RightKey in the test, which made a new block look like it had Block 1's labels.
        var leftKey = new Key
        {
            Id = Guid.NewGuid(),
            LayoutItem = LayoutItem.LeftKey,
            Style = new TextStyle(),
            Text = string.Empty
        };
        var rightKey = new Key
        {
            Id = Guid.NewGuid(),
            LayoutItem = LayoutItem.RightKey,
            Style = new TextStyle(),
            Text = string.Empty
        };
        _currentTest.AddKey(leftKey);
        _currentTest.AddKey(rightKey);
        block.LeftResponseId = leftKey.Id;
        block.RightResponseId = rightKey.Id;

        // Export and image generation resolve instructions via BlockInstructionsId → FormattedText.
        // Create the FormattedText up front (empty text) so the Id is valid; validation still
        // rejects blank / placeholder text until the author fills it in.
        _currentTest.EnsureBlockInstructions(
            block,
            text: string.Empty,
            style: new TextStyle
            {
                FontFamily = _lastInstructionFontFamily,
                FontSize = _lastInstructionFontSize,
                FontColor = _lastInstructionTextColor
            });

        SelectedBlock = block;
        WeakReferenceMessenger.Default.Send(TestModifiedMessage.Instance);
    }

    /// <summary>
    /// Delete is allowed only while the structure is still being authored
    /// (before Generate 7-Block locks it) and a block is selected.
    /// </summary>
    private bool CanDeleteBlock() =>
        !IsStandardStructureLocked && SelectedBlock is not null;

    /// <summary>
    /// Removes the selected block from the domain model and selects a neighbor.
    /// Disabled once the standard 7-block structure is locked so the IAT stays valid.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteBlock))]
    private void DeleteBlock()
    {
        if (SelectedBlock is null || IsStandardStructureLocked) return;

        var toRemove = SelectedBlock;
        var ordered = Blocks.OrderBy(b => b.BlockNumber).ToList();
        var index = ordered.IndexOf(toRemove);

        _currentTest.RemoveBlock(toRemove);

        SelectedBlock = null;
        SelectedSequenceRow = null;
        SequenceRows.Clear();

        if (Blocks.Count == 0)
        {
            BlockInstructionsText = string.Empty;
            LayoutViewModel?.ApplyBlockInstructions(null);
            LayoutViewModel?.ApplyBlockKeys(null);
            LayoutViewModel?.ApplyTrialPreview(null);
            LayoutViewModel?.ApplyInstructionPreview(null);
        }
        else
        {
            var nextIndex = Math.Min(Math.Max(index, 0), Blocks.Count - 1);
            SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).ElementAtOrDefault(nextIndex)
                            ?? Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault();
        }

        GenerateSevenBlockIatCommand.NotifyCanExecuteChanged();
        AddBlockCommand.NotifyCanExecuteChanged();
        DeleteBlockCommand.NotifyCanExecuteChanged();
        WeakReferenceMessenger.Default.Send(TestModifiedMessage.Instance);
    }

    /// <summary>
    /// Enabled only when exactly two blocks exist and the structure has not already been locked.
    /// </summary>
    private bool CanGenerateSevenBlockIat() =>
        !IsStandardStructureLocked && Blocks.Count == 2;

    /// <summary>
    /// Builds the classic 7-block IAT structure from the two existing practice blocks.
    /// <list type="bullet">
    ///   <item>Blocks 3 &amp; 4 — all trials from 1+2, compatible combined keys (A or C / B or D), keyed by origin side.</item>
    ///   <item>Block 5 — trials from block 2 with response keys transposed; trials stay keyed to the <em>term</em>.</item>
    ///   <item>Blocks 6 &amp; 7 — all trials from 1+2, incompatible combined keys (A or D / B or C), keyed by term.</item>
    /// </list>
    /// Combined key labels are rendered as a vertical stack (term / or / term). After success the
    /// block list is locked against further Add / re-generate so the structure stays valid.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateSevenBlockIat))]
    private void GenerateSevenBlockIat()
    {
        var ordered = Blocks.OrderBy(b => b.BlockNumber).ToList();
        if (ordered.Count != 2)
            return;

        var block1 = ordered[0];
        var block2 = ordered[1];

        // Validate both practice blocks fully before inventing blocks 3–7.
        // Failures surface as a single error banner so the author can fix them in place.
        if (!TryValidatePracticeBlocks(block1, block2, out var key1L, out var key1R, out var key2L, out var key2R))
            return;

        // Compatible combined keys (blocks 3 & 4): A or C  /  B or D
        var compatLeft = CreateCombinedKey(key1L!, key2L!, LayoutItem.LeftKey);
        var compatRight = CreateCombinedKey(key1R!, key2R!, LayoutItem.RightKey);
        _currentTest.AddKey(compatLeft);
        _currentTest.AddKey(compatRight);

        // Incompatible combined keys (blocks 6 & 7): A or D  /  B or C
        var incompatLeft = CreateCombinedKey(key1L!, key2R!, LayoutItem.LeftKey);
        var incompatRight = CreateCombinedKey(key1R!, key2L!, LayoutItem.RightKey);
        _currentTest.AddKey(incompatLeft);
        _currentTest.AddKey(incompatRight);

        // --- Blocks 3 & 4: compatible combined ---
        for (var n = 3; n <= 4; n++)
        {
            var block = CreateBlock(n, compatLeft.Id, compatRight.Id);
            AppendTrialsFrom(block1, block, flipDirection: false);
            AppendTrialsFrom(block2, block, flipDirection: false);
            block.NotifyTrialsChanged();
            _currentTest.AddBlock(block);
            AttachEmptyBlockInstructions(block);
        }

        // --- Block 5: attribute block with keys transposed; trials stay keyed to the term ---
        // Original block2: Left=C, Right=D  →  Left=D, Right=C
        // A trial that was Left (towards C) must become Right so it still points at C.
        {
            var block5 = CreateBlock(5, key2R.Id, key2L.Id);
            AppendTrialsFrom(block2, block5, flipDirection: true);
            block5.NotifyTrialsChanged();
            _currentTest.AddBlock(block5);
            AttachEmptyBlockInstructions(block5);
        }

        // --- Blocks 6 & 7: incompatible combined ---
        // Block1 terms keep their side (A stays left). Block2 terms flip (C moves to right).
        for (var n = 6; n <= 7; n++)
        {
            var block = CreateBlock(n, incompatLeft.Id, incompatRight.Id);
            AppendTrialsFrom(block1, block, flipDirection: false);
            AppendTrialsFrom(block2, block, flipDirection: true);
            block.NotifyTrialsChanged();
            _currentTest.AddBlock(block);
            AttachEmptyBlockInstructions(block);
        }

        // Practice blocks keep author trial pools but pick up standard presentation defaults
        // when still unset (legacy packages / empty authoring).
        foreach (var practice in new[] { block1, block2 })
        {
            if (practice.NumPresentations <= 0)
                practice.NumPresentations = DefaultPresentationsFor(practice.BlockNumber);
        }

        IsStandardStructureLocked = true;
        GenerateSevenBlockIatCommand.NotifyCanExecuteChanged();
        AddBlockCommand.NotifyCanExecuteChanged();
        DeleteBlockCommand.NotifyCanExecuteChanged();

        // Select the newly created Block 3 so the user sees the result immediately.
        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault(b => b.BlockNumber == 3)
                        ?? Blocks.OrderBy(b => b.BlockNumber).LastOrDefault();

        WeakReferenceMessenger.Default.Send(TestModifiedMessage.Instance);
    }

    /// <summary>
    /// Runs <see cref="IValidator{Block}"/> plus trial/key text checks on the two practice
    /// blocks. Returns false (and posts an error banner) when either block is incomplete.
    /// On success, <paramref name="key1L"/>…<paramref name="key2R"/> are non-null.
    /// </summary>
    private bool TryValidatePracticeBlocks(
        Block block1,
        Block block2,
        out Key? key1L,
        out Key? key1R,
        out Key? key2L,
        out Key? key2R)
    {
        key1L = key1R = key2L = key2R = null;
        var errors = new StringBuilder();

        foreach (var block in new[] { block1, block2 })
        {
            // Ensure BlockInstructionsId exists so BlockValidator's Guid rule is meaningful.
            _currentTest.EnsureBlockInstructions(block);

            var result = _blockValidator.Validate(block);
            foreach (var failure in result.Errors)
                errors.AppendLine($"{block.Name}: {failure.ErrorMessage}");

            if (block.TrialIds.Count == 0)
                errors.AppendLine($"{block.Name}: at least one trial is required before generating the 7-block structure.");
        }

        key1L = block1.LeftResponseId != Guid.Empty ? _currentTest.GetKeyById(block1.LeftResponseId) : null;
        key1R = block1.RightResponseId != Guid.Empty ? _currentTest.GetKeyById(block1.RightResponseId) : null;
        key2L = block2.LeftResponseId != Guid.Empty ? _currentTest.GetKeyById(block2.LeftResponseId) : null;
        key2R = block2.RightResponseId != Guid.Empty ? _currentTest.GetKeyById(block2.RightResponseId) : null;

        ValidateKeyText(block1.Name, "Left", key1L, errors);
        ValidateKeyText(block1.Name, "Right", key1R, errors);
        ValidateKeyText(block2.Name, "Left", key2L, errors);
        ValidateKeyText(block2.Name, "Right", key2R, errors);

        if (errors.Length > 0)
        {
            WeakReferenceMessenger.Default.Send(new ErrorNotificationMessage(
                "Practice blocks incomplete",
                errors.ToString().TrimEnd()));
            return false;
        }

        return true;
    }

    private static void ValidateKeyText(string blockName, string side, Key? key, StringBuilder errors)
    {
        if (key is null)
        {
            errors.AppendLine($"{blockName}: {side} response key is not defined. Set it on the Trials tab.");
            return;
        }

        var text = Key.FormatAuthoringDisplay(key.Text);
        if (string.IsNullOrWhiteSpace(text))
            errors.AppendLine($"{blockName}: {side} response key text is empty. Enter a label on the Trials tab.");
    }

    private Block CreateBlock(int number, Guid leftKeyId, Guid rightKeyId)
    {
        var block = new Block
        {
            Id = Guid.NewGuid(),
            Name = $"Block {number}",
            BlockNumber = number,
            LeftResponseId = leftKeyId,
            RightResponseId = rightKeyId,
            NumPresentations = DefaultPresentationsFor(number)
        };
        // FormattedText is registered after AddBlock (IatTest must be attached first).
        return block;
    }

    /// <summary>
    /// Classic IAT presentation counts: 20 on the long combined blocks (4 and 7), 10 elsewhere.
    /// </summary>
    private static int DefaultPresentationsFor(int blockNumber) =>
        blockNumber is 4 or 7 ? 20 : 10;

    /// <summary>
    /// After a generated block is added to the test, allocate its block-instructions FormattedText.
    /// Text stays empty so validation forces the author to supply real instructions.
    /// </summary>
    private void AttachEmptyBlockInstructions(Block block)
    {
        _currentTest.EnsureBlockInstructions(
            block,
            text: string.Empty,
            style: new TextStyle
            {
                FontFamily = _lastInstructionFontFamily,
                FontSize = _lastInstructionFontSize,
                FontColor = _lastInstructionTextColor
            });
    }

    /// <summary>
    /// Creates a combined response key for the Trials tab / domain model.
    /// Authoring text is a single line (<c>"Good or Flower"</c>); layout preview and
    /// participant slides stack it via <see cref="Key.FormatStackedDisplay"/>.
    /// </summary>
    private static Key CreateCombinedKey(Key first, Key second, LayoutItem layoutSlot)
    {
        var t1 = Key.FormatAuthoringDisplay(first.Text);
        var t2 = Key.FormatAuthoringDisplay(second.Text);
        return new Key
        {
            Id = Guid.NewGuid(),
            LayoutItem = layoutSlot,
            IsCombined = true,
            ComponentIds = new List<Guid> { first.Id, second.Id },
            Separator = " or ",
            LayoutMode = KeyLayoutMode.VerticalWithOr,
            // Single line so the Trials tab shows "Good or Flower", not a multi-line mess.
            Text = string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)
                ? string.Empty
                : $"{t1} or {t2}".Trim(),
            Style = new TextStyle
            {
                FontFamily = first.FontFamily ?? "Segoe UI",
                FontSize = first.FontSize > 0 ? first.FontSize : 24.0,
                FontColor = first.FontColor
            },
            FontFamily = first.FontFamily ?? "Segoe UI",
            FontSize = first.FontSize > 0 ? first.FontSize : 24.0,
            FontColor = first.FontColor
        };
    }

    /// <summary>
    /// Clones every trial from <paramref name="source"/> into <paramref name="target"/>,
    /// optionally flipping Left↔Right so the trial remains keyed toward the same term
    /// after response keys have been transposed or recombined.
    /// </summary>
    private void AppendTrialsFrom(Block source, Block target, bool flipDirection)
    {
        var nextNumber = target.TrialIds.Count + 1;
        foreach (var srcTrial in source.Trials.OrderBy(t => t.TrialNumber))
        {
            var direction = srcTrial.KeyedDirection;
            if (flipDirection && direction != KeyedDirection.None)
                direction = (KeyedDirection.Left == direction) ? KeyedDirection.Right : KeyedDirection.Left;

            var trial = new Trial
            {
                Id = Guid.NewGuid(),
                StimulusId = srcTrial.StimulusId,
                TrialNumber = nextNumber++,
                BlockNumber = target.BlockNumber,
                OriginatingBlock = source.BlockNumber,
                KeyedDirection = direction
            };

            _currentTest.AddTrial(trial);
            target.TrialIds.Add(trial.Id);
        }
    }

    partial void OnSelectedBlockChanged(Block? value)
    {
        // Layout is test-scoped, not block-scoped. Only create a LayoutViewModel once
        // (or when the underlying IatTest changes). Recreating it on every block selection
        // tears down/rebuilds the preview tree and re-fires SizeChanged → FitToWindow,
        // which was a primary source of stack overflows when switching back to this tab.
        if (value?.IatTest != null && LayoutViewModel is null)
        {
            LayoutViewModel = new LayoutViewModel(_layoutCalculator, value.IatTest, _packageService);
        }

        // Sync instruction text + style editor from the block's FormattedText (create if missing).
        LoadInstructionEditorFrom(value);
        PushBlockInstructionsToPreview(BlockInstructionsText);
        LayoutViewModel?.ApplyBlockKeys(value);

        RebuildSequenceRows();

        // Auto-select the first row (instruction if any, otherwise first trial).
        SelectedSequenceRow = SequenceRows.FirstOrDefault();
        if (SelectedSequenceRow is null)
        {
            SelectedTrial = null;
            LayoutViewModel?.ApplyTrialPreview(null);
            LayoutViewModel?.ApplyInstructionPreview(null);
        }

        OnPropertyChanged(nameof(IsBlockInstructionsEditable));
        DeleteBlockCommand.NotifyCanExecuteChanged();
    }

    partial void OnBlockInstructionsTextChanged(string value)
    {
        // Instruction-screen rows own their body text on the Instructions tab.
        // Never write back or push preview changes from this editor while one is selected.
        if (SelectedSequenceRow is { IsInstruction: true })
            return;

        PersistBlockInstructions(text: value);
        PushBlockInstructionsToPreview(value);
    }

    partial void OnInstructionFontFamilyChanged(string value)
    {
        PersistBlockInstructions(styleOnly: true);
        // Style-only edits must still refresh the live preview; text path already does this.
        if (!_loadingInstructionEditor && SelectedSequenceRow is not { IsInstruction: true })
            PushBlockInstructionsToPreview();
    }

    partial void OnInstructionFontSizeChanged(double value)
    {
        PersistBlockInstructions(styleOnly: true);
        if (!_loadingInstructionEditor && SelectedSequenceRow is not { IsInstruction: true })
            PushBlockInstructionsToPreview();
    }

    partial void OnInstructionTextColorChanged(Color value)
    {
        OnPropertyChanged(nameof(InstructionPreviewBrush));
        PersistBlockInstructions(styleOnly: true);
        if (!_loadingInstructionEditor && SelectedSequenceRow is not { IsInstruction: true })
            PushBlockInstructionsToPreview();
    }

    /// <summary>
    /// Current Instruction Style editor values as a <see cref="TextStyle"/> for domain + preview.
    /// </summary>
    private TextStyle CurrentInstructionStyle => new()
    {
        FontFamily = InstructionFontFamily ?? "Segoe UI",
        FontSize = InstructionFontSize > 0 ? InstructionFontSize : 24.0,
        FontColor = InstructionTextColor
    };

    /// <summary>
    /// Pushes block-instructions text + style into the layout live preview so font/size/color
    /// changes appear immediately (not only after save or block re-selection).
    /// </summary>
    private void PushBlockInstructionsToPreview(string? text = null)
    {
        LayoutViewModel?.ApplyBlockInstructions(
            text ?? BlockInstructionsText,
            CurrentInstructionStyle);
    }

    /// <summary>
    /// Loads the Instructions Text editor + style controls from the selected block's FormattedText.
    /// Migrates legacy blocks that only have the string property.
    /// </summary>
    private void LoadInstructionEditorFrom(Block? block)
    {
        _loadingInstructionEditor = true;
        try
        {
            if (block is null)
            {
                BlockInstructionsText = string.Empty;
                InstructionFontFamily = _lastInstructionFontFamily;
                InstructionFontSize = _lastInstructionFontSize;
                InstructionTextColor = _lastInstructionTextColor;
                OnPropertyChanged(nameof(InstructionPreviewBrush));
                return;
            }

            var ft = _currentTest.EnsureBlockInstructions(block);
            BlockInstructionsText = ft.Text ?? string.Empty;
            InstructionFontFamily = ft.Style?.FontFamily ?? "Segoe UI";
            InstructionFontSize = ft.Style?.FontSize > 0 ? ft.Style.FontSize : 24.0;
            InstructionTextColor = ft.Style?.FontColor ?? Colors.Black;
            OnPropertyChanged(nameof(InstructionPreviewBrush));
        }
        finally
        {
            _loadingInstructionEditor = false;
        }
    }

    /// <summary>
    /// Writes the current editor state into the block's FormattedText (and convenience string).
    /// </summary>
    private void PersistBlockInstructions(string? text = null, bool styleOnly = false)
    {
        if (_loadingInstructionEditor)
            return;
        if (SelectedSequenceRow is { IsInstruction: true })
            return;
        if (SelectedBlock is null)
            return;

        var effectiveText = styleOnly
            ? (SelectedBlock.BlockInstructions ?? BlockInstructionsText ?? string.Empty)
            : (text ?? BlockInstructionsText ?? string.Empty);

        var style = new TextStyle
        {
            FontFamily = InstructionFontFamily ?? "Segoe UI",
            FontSize = InstructionFontSize > 0 ? InstructionFontSize : 24.0,
            FontColor = InstructionTextColor
        };

        _currentTest.EnsureBlockInstructions(SelectedBlock, effectiveText, style);

        _lastInstructionFontFamily = style.FontFamily;
        _lastInstructionFontSize = style.FontSize;
        _lastInstructionTextColor = style.FontColor;

        WeakReferenceMessenger.Default.Send(TestModifiedMessage.Instance);
    }

    [RelayCommand]
    private void ApplyInstructionPalette(string paletteType)
    {
        InstructionTextColor = paletteType.ToLowerInvariant() switch
        {
            "black" => Colors.Black,
            "white" => Colors.White,
            "flame scarlet" => Color.FromRgb(205, 33, 42),
            "firefly" => Color.FromRgb(209, 206, 32),
            "silver sconce" => Color.FromRgb(161, 159, 165),
            "ultra violet" => Color.FromRgb(95, 75, 139),
            "knockout pink" => Color.FromRgb(255, 62, 165),
            "emerald" => Color.FromRgb(0, 148, 115),
            "sunset gold" => Color.FromRgb(247, 196, 148),
            "radiant orchid" => Color.FromRgb(174, 93, 153),
            "raspberry" => Color.FromRgb(255, 46, 94),
            "acid lime" => Color.FromRgb(187, 223, 50),
            "bluebird" => Color.FromRgb(0, 161, 180),
            "star sapphire" => Color.FromRgb(69, 104, 154),
            "angel blue" => Color.FromRgb(131, 198, 207),
            "ember glow" => Color.FromRgb(234, 103, 89),
            "pale gold" => Color.FromRgb(189, 152, 101),
            "blackened pearl" => Color.FromRgb(77, 75, 80),
            _ => InstructionTextColor
        };
    }

    partial void OnSelectedTrialChanged(Trial? value)
    {
        // Kept for callers that still set SelectedTrial directly.
        // Sequence selection is the primary path.
        if (value is not null && SelectedSequenceRow?.Trial != value)
        {
            var match = SequenceRows.FirstOrDefault(r => r.Trial == value);
            if (match is not null)
                SelectedSequenceRow = match;
        }
    }

    partial void OnSelectedSequenceRowChanged(BlockSequenceRow? value)
    {
        OnPropertyChanged(nameof(IsBlockInstructionsEditable));

        if (LayoutViewModel is null) return;

        if (value is null)
        {
            SelectedTrial = null;
            LayoutViewModel.ApplyTrialPreview(null);
            LayoutViewModel.ApplyInstructionPreview(null);
            PushBlockInstructionsToPreview(SelectedBlock?.BlockInstructions);
            LayoutViewModel.ApplyBlockKeys(SelectedBlock);
            return;
        }

        if (value.IsInstruction)
        {
            SelectedTrial = null;
            // Do not call ApplyTrialPreview(null) here — that restores the "Sample Stimulus"
            // placeholder. ApplyInstructionPreview fully owns the stage for instruction rows
            // (hides stimulus for Text/Keyed, fills it for Mock Item).
            LayoutViewModel.ApplyInstructionPreview(value.Instruction);
        }
        else
        {
            SelectedTrial = value.Trial;
            LayoutViewModel.ApplyInstructionPreview(null);
            LayoutViewModel.ApplyTrialPreview(value.Trial);
            // Restore block keys + block-instructions text/region (instruction preview overrode both).
            LayoutViewModel.ApplyBlockKeys(SelectedBlock);
            PushBlockInstructionsToPreview(SelectedBlock?.BlockInstructions);
        }
    }

    /// <summary>
    /// Rebuilds the bottom grid: instruction screens assigned to the block first, then trials.
    /// </summary>
    public void RebuildSequenceRows()
    {
        SequenceRows.Clear();
        if (SelectedBlock is null) return;

        // Instructions first, in the order they appear on the block.
        var instrIndex = 1;
        foreach (var id in SelectedBlock.InstructionsIds)
        {
            var screen = _currentTest.GetInstructionScreenById(id);
            if (screen is null) continue;
            SequenceRows.Add(BlockSequenceRow.FromInstruction(screen, instrIndex++));
        }

        // Then trials, in trial-number order.
        foreach (var trial in SelectedBlock.Trials.OrderBy(t => t.TrialNumber))
        {
            SequenceRows.Add(BlockSequenceRow.FromTrial(trial, _currentTest));
        }
    }

    /// <summary>
    /// Re-applies keys, instructions, and the selected sequence row to the shared layout preview.
    /// Called when the Blocks tab becomes visible so changes made on Trials / Instructions
    /// appear without requiring a block re-selection.
    /// </summary>
    public void RefreshLayoutPreview()
    {
        if (LayoutViewModel is null) return;

        PushBlockInstructionsToPreview(SelectedBlock?.BlockInstructions);
        LayoutViewModel.ApplyBlockKeys(SelectedBlock);

        RebuildSequenceRows();

        // Restore previous selection if still present; otherwise pick the first row.
        if (SelectedSequenceRow is not null)
        {
            var stillThere = SequenceRows.FirstOrDefault(r =>
                (r.IsInstruction && r.Instruction?.Id == SelectedSequenceRow.Instruction?.Id) ||
                (!r.IsInstruction && r.Trial?.Id == SelectedSequenceRow.Trial?.Id));
            SelectedSequenceRow = stillThere ?? SequenceRows.FirstOrDefault();
        }
        else
        {
            SelectedSequenceRow = SequenceRows.FirstOrDefault();
        }

        // Force preview refresh for the (possibly restored) selection.
        OnSelectedSequenceRowChanged(SelectedSequenceRow);
    }

    [RelayCommand]
    private void ToggleLayoutEditMode()
    {
        if (LayoutViewModel == null)
            return;
        LayoutViewModel.IsLayoutEditMode = !LayoutViewModel.IsLayoutEditMode;
    }

    /// <summary>
    /// Called by the shell after New/Open so selection and preview match the (possibly empty) document.
    /// Always ends by pushing the selected sequence row into the layout stage so the first
    /// trial is previewed immediately after open — not only when the user re-clicks it.
    /// </summary>
    public void OnDocumentReset()
    {
        IsStandardStructureLocked = false;
        GenerateSevenBlockIatCommand.NotifyCanExecuteChanged();
        AddBlockCommand.NotifyCanExecuteChanged();
        DeleteBlockCommand.NotifyCanExecuteChanged();

        SelectedTrial = null;
        SelectedSequenceRow = null;
        SequenceRows.Clear();
        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault();
        if (SelectedBlock is null)
        {
            BlockInstructionsText = string.Empty;
            LayoutViewModel?.ApplyBlockInstructions(null);
            LayoutViewModel?.ApplyBlockKeys(null);
            LayoutViewModel?.ApplyTrialPreview(null);
            LayoutViewModel?.ApplyInstructionPreview(null);
            return;
        }

        // If the loaded document already has the classic 7-block layout, treat it as locked.
        if (Blocks.Count == 7)
        {
            IsStandardStructureLocked = true;
            GenerateSevenBlockIatCommand.NotifyCanExecuteChanged();
            AddBlockCommand.NotifyCanExecuteChanged();
            DeleteBlockCommand.NotifyCanExecuteChanged();
        }

        // Rebuild rows, select first, and force the preview path even when the
        // SelectedSequenceRow reference did not change enough to raise PropertyChanged.
        RefreshLayoutPreview();
        if (SelectedSequenceRow is null && SequenceRows.Count > 0)
            SelectedSequenceRow = SequenceRows.FirstOrDefault();
        OnSelectedSequenceRowChanged(SelectedSequenceRow);
    }
}

/// <summary>
/// One row in the Blocks-tab sequence grid. Represents either an assigned instruction screen
/// (always listed first) or a trial.
/// </summary>
public sealed class BlockSequenceRow
{
    public bool IsInstruction { get; private init; }
    public InstructionScreen? Instruction { get; private init; }
    public Trial? Trial { get; private init; }

    /// <summary>"I1", "I2", … for instructions; "1", "2", … for trials.</summary>
    public string NumberDisplay { get; private init; } = string.Empty;

    /// <summary>Stimulus preview text or instruction body preview.</summary>
    public string Detail { get; private init; } = string.Empty;

    /// <summary>Direction for trials; screen type name for instructions.</summary>
    public string DirectionDisplay { get; private init; } = string.Empty;

    public static BlockSequenceRow FromInstruction(InstructionScreen screen, int index)
    {
        var typeName = screen switch
        {
            MockItemInstructionScreen => "Mock Item",
            KeyedInstructionScreen => "Keyed",
            _ => "Text"
        };

        var body = screen.Text ?? string.Empty;
        if (body.Length > 60) body = body[..57] + "…";

        return new BlockSequenceRow
        {
            IsInstruction = true,
            Instruction = screen,
            NumberDisplay = $"I{index}",
            Detail = string.IsNullOrWhiteSpace(body) ? "(empty instruction)" : body,
            DirectionDisplay = typeName
        };
    }

    public static BlockSequenceRow FromTrial(Trial trial, IatTest test)
    {
        var stim = test.GetStimulusById(trial.StimulusId);
        var preview = stim?.GetDisplayPreview() ?? "(none)";
        if (preview.Length > 60) preview = preview[..57] + "…";

        return new BlockSequenceRow
        {
            IsInstruction = false,
            Trial = trial,
            NumberDisplay = trial.TrialNumber.ToString(),
            Detail = preview,
            DirectionDisplay = trial.KeyedDirection.ToString()
        };
    }
}
