using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Models;
using IAT.Core.Models.Enumerations;
using IAT.Core.Services;
using System.Collections.ObjectModel;

namespace IAT.ViewModels
{
    /// <summary>
    /// Main ViewModel for the test designer screen.
    /// Coordinates loading the package, blocks, and items for the UI.
    /// </summary>
    public partial class TestDesignerViewModel : ObservableObject
    {
        private readonly IPackageService _packageService;

        [ObservableProperty]
        private ObservableCollection<Core.Models.IATBlock> blocks = new();

        [ObservableProperty]
        private Core.Models.IATBlock? selectedBlock;

        public TestDesignerViewModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [RelayCommand]
        private async Task LoadTestAsync(string packagePath)
        {
            var ciat = await _packageService.LoadPackageAsync(packagePath);
            // TODO: populate Blocks from the package (we'll expand this as we port more)
            Blocks.Clear();
            // Example: Blocks.Add(new CIATBlock { Name = "Practice Block", Type = BlockType.Practice });
        }
    }
}