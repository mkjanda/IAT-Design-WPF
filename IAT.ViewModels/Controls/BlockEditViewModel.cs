using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using System.Collections.ObjectModel;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Blocks tab. Uses the shared singleton <see cref="IatTest"/> domain model so that
/// blocks created here appear immediately in the Trials tab (and vice-versa). Trials created on the
/// Trials tab appear in the bottom Trials grid because they are resolved through <see cref="Block.Trials"/>
/// and <see cref="Block.NotifyTrialsChanged"/> raises change notification.
/// Selecting a trial in the grid drives the layout preview stimulus via <see cref="LayoutViewModel.ApplyTrialPreview"/>.
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
    [ObservableProperty] private int selectedBlockTrialCount;
    [ObservableProperty] private LayoutViewModel? layoutViewModel;

    /// <summary>
    /// Currently selected trial in the bottom trials grid. Drives the center layout preview.
    /// </summary>
    [ObservableProperty] private Trial? selectedTrial;

    /// <summary>
    /// Human-readable stimulus label for the selected trial (for the properties pane / grid helper).
    /// </summary>
    [ObservableProperty] private string selectedTrialStimulusPreview = string.Empty;

    /// <summary>
    /// Bound to the Instructions Text editor. Mirrors <see cref="Block.BlockInstructions"/> and
    /// pushes live updates into the layout preview.
    /// </summary>
    [ObservableProperty] private string blockInstructionsText = string.Empty;

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
        SelectedBlockTrialCount = value?.TrialIds?.Count ?? 0;

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

        SelectedTrial = null;

        // Auto-select the first trial so the preview is immediately meaningful.
        var firstTrial = value?.Trials?.FirstOrDefault();
        if (firstTrial is not null)
            SelectedTrial = firstTrial;
        else
            LayoutViewModel?.ApplyTrialPreview(null);
    }

    partial void OnBlockInstructionsTextChanged(string value)
    {
        if (SelectedBlock is not null && SelectedBlock.BlockInstructions != value)
            SelectedBlock.BlockInstructions = value ?? string.Empty;

        LayoutViewModel?.ApplyBlockInstructions(value);
    }

    partial void OnSelectedTrialChanged(Trial? value)
    {
        LayoutViewModel?.ApplyTrialPreview(value);

        if (value is null)
        {
            SelectedTrialStimulusPreview = string.Empty;
            return;
        }

        var stim = _currentTest.GetStimulusById(value.StimulusId);
        SelectedTrialStimulusPreview = stim?.GetDisplayPreview() ?? "(missing stimulus)";
    }

    /// <summary>
    /// Re-applies keys, instructions, and trial stimulus to the shared layout preview.
    /// Called when the Blocks tab becomes visible so changes made on Trials (e.g. key text)
    /// appear without requiring a block re-selection.
    /// </summary>
    public void RefreshLayoutPreview()
    {
        if (LayoutViewModel is null) return;

        LayoutViewModel.ApplyBlockInstructions(SelectedBlock?.BlockInstructions);
        LayoutViewModel.ApplyBlockKeys(SelectedBlock);

        if (SelectedTrial is not null)
            LayoutViewModel.ApplyTrialPreview(SelectedTrial);
        else if (SelectedBlock?.Trials?.FirstOrDefault() is Trial first)
        {
            SelectedTrial = first;
        }
        else
        {
            LayoutViewModel.ApplyTrialPreview(null);
        }
    }

    [RelayCommand]
    private void ToggleLayoutEditMode()
    {
        if (LayoutViewModel == null)
            return;
        LayoutViewModel.IsLayoutEditMode = !LayoutViewModel.IsLayoutEditMode;
    }
}
