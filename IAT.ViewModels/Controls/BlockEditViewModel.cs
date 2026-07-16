using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using IAT.ViewModels;
using System.Collections.ObjectModel;

namespace IAT.ViewModels.Controls;

public partial class BlockEditViewModel : ObservableObject
{
    private readonly IProjectPackageService _packageService;
    private readonly ILayoutCalculatorService _layoutCalculator;

    [ObservableProperty] private ObservableCollection<Block> blocks = new();
    [ObservableProperty] private Block? selectedBlock;

    /// <summary>
    /// Shared layout view model (same instance used by the Layout tab).
    /// Blocks tab only reads it for a read-only preview.
    /// </summary>
    public LayoutViewModel LayoutViewModel { get; }

    public BlockEditViewModel(
        IProjectPackageService packageService,
        ILayoutCalculatorService layoutCalculator,
        LayoutViewModel layoutViewModel)
    {
        _packageService = packageService;
        _layoutCalculator = layoutCalculator;
        LayoutViewModel = layoutViewModel;
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

    [RelayCommand]
    private void AddBlock()
    {
        var block = new Block
        {
            Id = Guid.NewGuid(),
            Name = $"Block {Blocks.Count + 1}"
        };
        // BlockNumber if available
        try { block.GetType().GetProperty("BlockNumber")?.SetValue(block, Blocks.Count + 1); } catch { /* ignore */ }
        Blocks.Add(block);
        SelectedBlock = block;
    }
}
