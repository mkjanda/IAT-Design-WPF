using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using System.Collections.ObjectModel;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Blocks tab. Uses the shared IatTest domain model so that
/// blocks created here appear immediately in the Trials tab (and vice-versa).
/// </summary>
public partial class BlockEditViewModel : ObservableObject
{
    private readonly IProjectPackageService _packageService;
    private readonly ILayoutCalculatorService _layoutCalculator;
    private readonly IatTest _currentTest;

    /// <summary>
    /// Live shared collection of blocks from the domain model.
    /// Changes here are visible to TrialsManagerViewModel automatically.
    /// </summary>
    public ObservableCollection<Block> Blocks => _currentTest.BlocksCollection;

    [ObservableProperty] private Block? selectedBlock;
    [ObservableProperty] private int selectedBlockTrialCount;
    [ObservableProperty] private LayoutViewModel? layoutViewModel;

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
        // After load, just select the first block.
        var iat = await _packageService.LoadProjectAsync(packagePath);

        // The package service should have already populated _currentTest.
        // Force a re-selection so the UI refreshes.
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

        if (value != null && value.IatTest != null)
        {
            LayoutViewModel = new LayoutViewModel(_layoutCalculator, value.IatTest);
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
