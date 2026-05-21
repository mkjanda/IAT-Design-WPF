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


        public BlockEditViewModel BlockEditor { get; }


        [ObservableProperty] private ObservableCollection<StimulusEditViewModel> _stimuliLibrary = new();
        public TestDesignerViewModel(IProjectPackageService packageService, BlockEditViewModel blockEditor)
        {
            _packageService = packageService;
            BlockEditor = blockEditor;
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
                        StimuliLibrary.Add(new StimulusEditViewModel(textStim));
                    // TODO: else if (stim is ImageStimulus img) → add image support
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorNotificationMessage("Load Failed", ex.Message, ex));
            }
        }


    }
}