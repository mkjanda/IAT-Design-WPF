using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Models;
using IAT.Core.Services;
using IAT.ViewModels.Controls;
using System.Collections.ObjectModel;

namespace IAT.ViewModels.Controls;

public partial class TestDesignerViewModel : ObservableObject
{
    private readonly IProjectPackageService _packageService;
    private readonly ILayoutCalculatorService _layoutCalculator;

    [ObservableProperty] private ObservableCollection<Block> blocks = new();
    [ObservableProperty] private Block? selectedBlock;
    [ObservableProperty] private bool isLayoutEditMode;
    [ObservableProperty] private LayoutViewModel? layoutViewModel;

    /// <summary>
    /// The constructor for the test designer view model, which facilitates the creating of IAT blocks and the assignment of trials and 
    /// stimuli to those blocks. It takes in services for loading project packages and calculating layouts, which are essential for managing 
    /// the test design process.
    /// </summary>
    /// <param name="packageService">The service responsible for loading project packages.</param>
    /// <param name="layoutCalculator">The service responsible for calculating layouts.</param>
    public TestDesignerViewModel(IProjectPackageService packageService, ILayoutCalculatorService layoutCalculator)
    {
        _packageService = packageService;
        _layoutCalculator = layoutCalculator;
    }

    [RelayCommand]
    private async Task LoadTestAsync(string packagePath)
    {
        var iat = await _packageService.LoadProjectAsync(packagePath);
        Blocks.Clear();
        foreach (var block in iat.AllBlocks.OrderBy(b => b.BlockNumber))
            Blocks.Add(block);

        if (Blocks.Any())
            SelectedBlock = Blocks.First();
    }

    partial void OnSelectedBlockChanged(Block? value)
    {
        if (value != null && value.IatTest != null)
        {
            LayoutViewModel = new LayoutViewModel(_layoutCalculator, value.IatTest);
        }
    }

    [RelayCommand]
    private void ToggleLayoutEditMode()
    {
        IsLayoutEditMode = !IsLayoutEditMode;
    }
 }