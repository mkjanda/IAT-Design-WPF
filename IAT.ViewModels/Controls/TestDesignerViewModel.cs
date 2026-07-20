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
    /// Holds references to the per-tab ViewModels that share the same singleton <see cref="IatTest"/>
    /// and the shared singleton <see cref="LayoutViewModel"/>.
    /// </summary>
    public partial class TestDesignerViewModel : ObservableObject
    {
        private readonly IProjectPackageService _packageService;
        private IatTest? _currentTest;

        /// <summary>
        /// ViewModel for the Blocks tab.
        /// </summary>
        public BlockEditViewModel BlockEditor { get; }

        /// <summary>
        /// Shared layout editor used by the Layout tab (and the read-only preview on Blocks).
        /// Same DI singleton instance injected into <see cref="BlockEditViewModel"/>.
        /// </summary>
        public LayoutViewModel LayoutEditor { get; }

        /// <summary>
        /// ViewModel for the Stimuli tab.
        /// </summary>
        public StimuliManagerViewModel StimuliManager { get; }

        /// <summary>
        /// ViewModel for the Trials tab.
        /// </summary>
        public TrialsManagerViewModel TrialsManager { get; }

        [ObservableProperty] private bool _isStimuliSelected = false;
        [ObservableProperty] private bool _isBlocksSelected = false;
        [ObservableProperty] private bool _isLayoutSelected = false;
        [ObservableProperty] private bool _isTrialsSelected = false;

        [ObservableProperty] private ObservableCollection<StimulusEditViewModel> _stimuliLibrary = new();

        public TestDesignerViewModel(
            IProjectPackageService packageService,
            BlockEditViewModel blockEditor,
            LayoutViewModel layoutEditor,
            StimuliManagerViewModel stimuliManager,
            TrialsManagerViewModel trialsManager)
        {
            _packageService = packageService;
            BlockEditor = blockEditor;
            LayoutEditor = layoutEditor;
            StimuliManager = stimuliManager;
            TrialsManager = trialsManager;
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
                    // TODO: else if (stim is ImageStimulus img) → add image support
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

        [RelayCommand]
        private async Task OnLayoutTabSelected()
        {
            IsLayoutSelected = true;
            // Ensure edit-mode affordances (resize thumbs) are available on the Layout tab.
            if (LayoutEditor is not null)
                LayoutEditor.IsLayoutEditMode = true;
        }

        [RelayCommand]
        private async Task OnTrialsTabSelected()
        {
            IsTrialsSelected = true;
        }
    }
}
