using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Messages;
using IAT.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IAT.ViewModels.Controls
{
    /// <summary>
    /// Main ViewModel for the entire designer (Blocks + Stimuli + Trials + Deploy tabs).
    /// Currently focused on the Stimuli tab to match your screenshot and XAML.
    /// </summary>
    public partial class TestDesignerViewModel : ObservableObject
    {
        private readonly IProjectPackageService _packageService;
        private IatTest? _currentTest;

        /// <summary>
        /// ViewModel for the Blocks tab. This is injected into the TestDesignerViewModel and can be shared across tabs if needed.
        /// </summary>
        public BlockEditViewModel BlockEditor { get; }

        /// <summary>
        /// ViewModel for the Stimuli tab. This is injected into the TestDesignerViewModel and can be shared across tabs if needed.
        /// </summary>
        public StimuliManagerViewModel StimuliManager { get; }

        [ObservableProperty] private bool _isStimuliSelected = false;
        [ObservableProperty] private bool _isBlocksSelected = false;

        [ObservableProperty] private ObservableCollection<StimulusEditViewModel> _stimuliLibrary = new();
        /// <summary>
        /// Collection of stimuli available in the library. This is what the user sees in the Stimuli tab and can drag into their test design.  
        /// </summary>
        /// <param name="packageService">Service for managing project packages.</param>
        /// <param name="blockEditor">ViewModel for the Blocks tab.</param>
        /// <param name="stimuliManager">ViewModel for the Stimuli tab.</param>
        public TestDesignerViewModel(IProjectPackageService packageService, BlockEditViewModel blockEditor,
            StimuliManagerViewModel stimuliManager)
        {
            _packageService = packageService;
            BlockEditor = blockEditor;
            StimuliManager = stimuliManager;
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
    }
}