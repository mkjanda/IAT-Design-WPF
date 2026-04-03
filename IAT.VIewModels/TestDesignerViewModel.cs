using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

        public TestDesignerViewModel()
        {
            // Register once for this exact message type — compiler-guaranteed
            WeakReferenceMessenger.Default.Register<ErrorNotificationMessage>(this,
                (recipient, msg) =>
                {
                    // msg is fully typed — Title, Message, Exception, etc.
                    ErrorBanner.Show(msg.Title, msg.Message, msg.Exception);

                    // Optional: log the full stack for your support team
                    if (msg.Exception is not null)
                        Log.Error(msg.Exception, "{Title}: {Message}", msg.Title, msg.Message);
                });
        }
    }
}