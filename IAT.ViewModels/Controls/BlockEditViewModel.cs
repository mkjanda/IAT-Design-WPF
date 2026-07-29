using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Services;
using System.Collections.ObjectModel;

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

        // Select first block if any exist
        if (Blocks.Count > 0)
            SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).First();
    }

    [RelayCommand]
    private async Task LoadTestAsync(string packagePath)
    {
        // Loading is handled by the package service into the shared IatTest.
        // After load, just select the first block so the UI refreshes.
        await _packageService.LoadProjectAsync(packagePath);

        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault();
    }

    [RelayCommand]
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
    /// </summary>
    public void OnDocumentReset()
    {
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
        }
        else
        {
            RefreshLayoutPreview();
        }
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
