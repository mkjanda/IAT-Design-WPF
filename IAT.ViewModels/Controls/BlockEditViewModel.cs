using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Messages;
using IAT.Core.Services;
using System.Collections.ObjectModel;

namespace IAT.ViewModels.Controls;

public partial class BlockEditViewModel : ObservableObject
{
    private readonly IProjectPackageService _packageService;
    private readonly ILayoutCalculatorService _layoutCalculator;
    private readonly IatTest _currentTest;
    private readonly IDialogService _dialogService;

    /// <summary>
    /// Live collection from the domain. Changes made here (or on the Trials tab) stay in sync.
    /// </summary>
    public ObservableCollection<Block> Blocks => _currentTest.BlocksCollection;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveBlockCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteBlockCommand))]
    private Block? selectedBlock;

    [ObservableProperty]
    private LayoutViewModel? layoutViewModel;

    [ObservableProperty]
    private bool isLayoutEditMode;

    public BlockEditViewModel(
        IProjectPackageService packageService,
        ILayoutCalculatorService layoutCalculator,
        LayoutViewModel layoutViewModel,
        IatTest currentTest,
        IDialogService dialogService)
    {
        _packageService = packageService;
        _layoutCalculator = layoutCalculator;
        LayoutViewModel = layoutViewModel;
        _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));
        _dialogService = dialogService;

        // Select first block if the test already contains any
        if (Blocks.Count > 0)
            SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).First();
    }

    partial void OnSelectedBlockChanged(Block? value)
    {
        if (value != null && value.IatTest != null)
        {
            LayoutViewModel = new LayoutViewModel(_layoutCalculator, value.IatTest);
        }
        else
        {
            LayoutViewModel = null;
        }
        OnPropertyChanged(nameof(CanSaveOrDelete));
    }

    // ──────────────────────────────────────────────────────────────
    // Commands
    // ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AddBlock()
    {
        var nextNumber = Blocks.Count == 0
            ? 1
            : Blocks.Max(b => b.BlockNumber) + 1;

        var block = new Block
        {
            Id = Guid.NewGuid(),
            Name = $"Block {nextNumber}",
            BlockNumber = nextNumber,
            NumPresentations = 0
        };

        _currentTest.AddBlock(block);          // domain + cache
        SelectedBlock = block;                 // UI selects the new block
    }

    /// <summary>
    /// Validates the currently selected block and ensures it is correctly
    /// attached to the test.  Because the UI already mutates the domain
    /// object directly, "Save" is essentially a validate-and-confirm step.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveOrDelete))]
    private void SaveBlock()
    {
        if (SelectedBlock is null) return;

        // Guarantee the block is attached to the live test
        if (SelectedBlock.IatTest is null)
        {
            SelectedBlock.IatTest = _currentTest;
            if (_currentTest.GetBlockById(SelectedBlock.Id) is null)
                _currentTest.AddBlock(SelectedBlock);
        }

        var result = SelectedBlock.Validate();
        if (!result.IsValid)
        {
            WeakReferenceMessenger.Default.Send(
                new ErrorNotificationMessage(
                    "Block Validation Failed",
                    string.Join(Environment.NewLine, result.Errors)));
            return;
        }

        // Optional: keep BlockNumber contiguous after renames / reordering
        RenumberBlocks();

        WeakReferenceMessenger.Default.Send(
            new ErrorNotificationMessage(          // reuse the banner for success feedback
                "Block Saved",
                $"\"{SelectedBlock.Name}\" is valid and saved."));
    }

    /// <summary>
    /// Removes the selected block after confirmation. Also cleans up any
    /// trials that belonged exclusively to that block.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveOrDelete))]
    private async Task DeleteBlock()
    {
        if (SelectedBlock is null) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            $"Delete block \"{SelectedBlock.Name}\"?\n\n" +
            "All trials belonging to this block will also be removed.",
            "Confirm Delete");

        if (!confirmed) return;

        var blockToRemove = SelectedBlock;

        // 1. Remove every trial that belongs to this block
        var trialIds = blockToRemove.TrialIds.ToList();
        foreach (var id in trialIds)
        {
            var trial = _currentTest.GetTrialById(id);
            if (trial is not null)
                _currentTest.RemoveTrial(trial);
        }

        // 2. Remove the block itself
        _currentTest.RemoveBlock(blockToRemove);

        // 3. Select a sensible neighbour
        SelectedBlock = Blocks
            .OrderBy(b => b.BlockNumber)
            .FirstOrDefault();

        RenumberBlocks();
    }

    [RelayCommand]
    private void ToggleLayoutEditMode()
    {
        if (LayoutViewModel is null) return;
        LayoutViewModel.IsLayoutEditMode = !LayoutViewModel.IsLayoutEditMode;
        IsLayoutEditMode = LayoutViewModel.IsLayoutEditMode;
    }

    [RelayCommand]
    private async Task LoadTestAsync(string packagePath)
    {
        var iat = await _packageService.LoadProjectAsync(packagePath);
        // The domain already owns the blocks; just select the first one
        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault();
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private bool CanSaveOrDelete() => SelectedBlock is not null;

    /// <summary>
    /// Keeps BlockNumber values contiguous (1, 2, 3 …) after add/delete/reorder.
    /// </summary>
    private void RenumberBlocks()
    {
        int n = 1;
        foreach (var block in Blocks.OrderBy(b => b.BlockNumber))
        {
            block.BlockNumber = n++;
            // Keep the display name in sync if the user never customised it
            if (string.IsNullOrWhiteSpace(block.Name) ||
                block.Name.StartsWith("Block ", StringComparison.Ordinal))
            {
                block.Name = $"Block {block.BlockNumber}";
            }
        }
    }
}