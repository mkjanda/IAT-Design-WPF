using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Messages;
using IAT.Core.Services;
using IAT.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IAT.ViewModels.Controls
{
    /// <summary>
    /// Main ViewModel for the entire designer (Blocks + Layout + Stimuli + Trials + Deploy tabs).
    /// </summary>
    public partial class TestDesignerViewModel : ObservableObject
    {
        private readonly IProjectPackageService _packageService;
        private IatTest? _currentTest;

        public BlockEditViewModel BlockEditor { get; }
        public StimuliManagerViewModel StimuliManager { get; }

        /// <summary>
        /// Shared layout editor used by the Layout tab (and read-only preview on Blocks).
        /// </summary>
        public LayoutViewModel LayoutEditor { get; }

        [ObservableProperty] private bool _isStimuliSelected = false;
        [ObservableProperty] private bool _isBlocksSelected = false;

        [ObservableProperty] private ObservableCollection<StimulusEditViewModel> _stimuliLibrary = new();

        public TestDesignerViewModel(
            IProjectPackageService packageService,
            BlockEditViewModel blockEditor,
            StimuliManagerViewModel stimuliManager,
            LayoutViewModel layoutEditor)
        {
            _packageService = packageService;
            BlockEditor = blockEditor;
            StimuliManager = stimuliManager;
            LayoutEditor = layoutEditor;
        }

        [RelayCommand]
        private async Task LoadTestAsync(string packagePath)
        {
            try
            {
                _currentTest = await _packageService.LoadProjectAsync(packagePath, default);

                StimuliLibrary.Clear();

                foreach (var stim in _currentTest.AllStimuli)
                {
                    if (stim is TextStimulus textStim)
                        StimuliLibrary.Add(new StimulusEditViewModel(textStim, _currentTest));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorNotificationMessage("Load Failed", ex.Message, ex));
            }
        }

        [RelayCommand]
        private async Task OnStimuliTabSelected()
        {
            IsStimuliSelected = true;
        }

        [RelayCommand]
        private async Task OnBlocksTabSelected()
        {
            IsBlocksSelected = true;
        }
    }
}
