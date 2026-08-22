using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Messages;
using IAT.Core.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Bound to the Instructions Text editor. Mirrors <see cref="Block.BlockInstructions"/> and
    /// pushes live updates into the layout preview.
    /// </summary>
    [ObservableProperty] private string blockInstructionsText = string.Empty;

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
        LayoutViewModel layoutViewModel,
        IatTest currentTest)
    {
        _packageService = packageService;
        _layoutCalculator = layoutCalculator;
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
            BlockNumber = nextNumber
        };

        // Add through the domain model so the block is fully registered
        // (cache, IatTest reference, etc.) and appears in every ViewModel
        // that binds to BlocksCollection.
        _currentTest.AddBlock(block);

        SelectedBlock = block;
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

        var key1L = _currentTest.GetKeyById(block1.LeftResponseId);
        var key1R = _currentTest.GetKeyById(block1.RightResponseId);
        var key2L = _currentTest.GetKeyById(block2.LeftResponseId);
        var key2R = _currentTest.GetKeyById(block2.RightResponseId);

        if (key1L is null || key1R is null || key2L is null || key2R is null)
        {
            WeakReferenceMessenger.Default.Send(new ErrorNotificationMessage(
                "Missing response keys",
                "Both practice blocks must have Left and Right response keys defined before generating the 7-block structure. Set them on the Trials tab."));
            return;
        }

        // Compatible combined keys (blocks 3 & 4): A or C  /  B or D
        var compatLeft = CreateCombinedKey(key1L, key2L, LayoutItem.LeftKey);
        var compatRight = CreateCombinedKey(key1R, key2R, LayoutItem.RightKey);
        _currentTest.AddKey(compatLeft);
        _currentTest.AddKey(compatRight);

        // Incompatible combined keys (blocks 6 & 7): A or D  /  B or C
        var incompatLeft = CreateCombinedKey(key1L, key2R, LayoutItem.LeftKey);
        var incompatRight = CreateCombinedKey(key1R, key2L, LayoutItem.RightKey);
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
        }

        // --- Block 5: attribute block with keys transposed; trials stay keyed to the term ---
        // Original block2: Left=C, Right=D  →  Left=D, Right=C
        // A trial that was Left (towards C) must become Right so it still points at C.
        {
            var block5 = CreateBlock(5, key2R.Id, key2L.Id);
            AppendTrialsFrom(block2, block5, flipDirection: true);
            block5.NotifyTrialsChanged();
            _currentTest.AddBlock(block5);
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
        }

        IsStandardStructureLocked = true;
        GenerateSevenBlockIatCommand.NotifyCanExecuteChanged();
        AddBlockCommand.NotifyCanExecuteChanged();

        // Select the newly created Block 3 so the user sees the result immediately.
        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault(b => b.BlockNumber == 3)
                        ?? Blocks.OrderBy(b => b.BlockNumber).LastOrDefault();

        WeakReferenceMessenger.Default.Send(TestModifiedMessage.Instance);
    }

    private static Block CreateBlock(int number, Guid leftKeyId, Guid rightKeyId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = $"Block {number}",
            BlockNumber = number,
            LeftResponseId = leftKeyId,
            RightResponseId = rightKeyId
        };

    /// <summary>
    /// Creates a combined response key whose display text is a vertical stack:
    /// <c>termA</c> / <c>or</c> / <c>termB</c>.
    /// </summary>
    private static Key CreateCombinedKey(Key first, Key second, LayoutItem layoutSlot)
    {
        var t1 = first.Text?.Trim() ?? string.Empty;
        var t2 = second.Text?.Trim() ?? string.Empty;
        return new Key
        {
            Id = Guid.NewGuid(),
            LayoutItem = layoutSlot,
            IsCombined = true,
            ComponentIds = new List<Guid> { first.Id, second.Id },
            Separator = " or ",
            LayoutMode = KeyLayoutMode.VerticalWithOr,
            // Multi-line so the Blocks preview TextBlock renders A / or / C in a column.
            Text = $"{t1}\nor\n{t2}",
            Style = new TextStyle(),
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
            var direction = srcTrial.KeyedDirection ?? KeyedDirection.None;
            if (flipDirection && direction != KeyedDirection.None)
                direction = direction.Opposite;

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

        // Sync instruction text editor + layout preview for this block.
        BlockInstructionsText = value?.BlockInstructions ?? string.Empty;
        LayoutViewModel?.ApplyBlockInstructions(BlockInstructionsText);
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
    }

    partial void OnBlockInstructionsTextChanged(string value)
    {
        // Instruction-screen rows own their body text on the Instructions tab.
        // Never write back or push preview changes from this editor while one is selected.
        if (SelectedSequenceRow is { IsInstruction: true })
            return;

        if (SelectedBlock is not null && SelectedBlock.BlockInstructions != value)
        {
            SelectedBlock.BlockInstructions = value ?? string.Empty;
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                IAT.Core.Messages.TestModifiedMessage.Instance);
        }

        LayoutViewModel?.ApplyBlockInstructions(value);
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
            LayoutViewModel.ApplyBlockInstructions(SelectedBlock?.BlockInstructions);
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
            LayoutViewModel.ApplyBlockInstructions(SelectedBlock?.BlockInstructions);
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

        LayoutViewModel.ApplyBlockInstructions(SelectedBlock?.BlockInstructions);
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
            DirectionDisplay = trial.KeyedDirection?.Name ?? "None"
        };
    }
}
