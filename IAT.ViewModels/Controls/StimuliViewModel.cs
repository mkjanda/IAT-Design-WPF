using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace IAT.ViewModels.Controls;

public partial class StimuliViewModel : ObservableObject
{
    private readonly IatTest _iatTest;
    private readonly IImageGenerationService _imageGenerationService;
    private readonly IProjectPackageService _packageService; // for image bytes
    private readonly ILayoutCalculatorService _layoutCalculatorService;

    [ObservableProperty] private ObservableCollection<Stimulus> stimuli = new();
    [ObservableProperty] private Stimulus? selectedStimulus;
    [ObservableProperty] private bool isTextMode; // true = editing text stimulus

    public StimuliViewModel(IatTest iatTest,
                           IImageGenerationService imageGenerationService,
                           IProjectPackageService packageService,
                           ILayoutCalculatorService layoutCalculatorService)
    {
        _iatTest = iatTest;
        _imageGenerationService = imageGenerationService;
        _packageService = packageService;
        _layoutCalculatorService = layoutCalculatorService;
        LoadStimuli();
    }

    private void LoadStimuli()
    {
        Stimuli.Clear();
        foreach (var stim in _iatTest.AllStimuli)
            Stimuli.Add(stim);
    }

    [RelayCommand]
    private void AddTextStimulus()
    {
        var newStim = new TextStimulus
        {
            Id = Guid.NewGuid(),
            Text = "New Text Stimulus",
            Style = new TextStyle()
            {
                FontFamily = "Arial",
                FontSize = 24,
                FontColor = System.Windows.Media.Colors.Black
            },
            LayoutItem = LayoutItem.Stimulus // reuse your existing layout item
        };
        _iatTest.AddStimulus(newStim);
        Stimuli.Add(newStim);
        SelectedStimulus = newStim;
        IsTextMode = true;
    }

    [RelayCommand]
    private async Task AddImageStimulusAsync()
    {
        // TODO: Hook up file dialog via DialogService (you already have one)
        // For now, placeholder – replace with actual file picker
        var newStim = new ImageStimulus { Id = Guid.NewGuid() };
        _iatTest.AddStimulus(newStim);
        Stimuli.Add(newStim);
        SelectedStimulus = newStim;
        IsTextMode = false;
    }

    [RelayCommand]
    private void DeleteStimulus()
    {
        if (SelectedStimulus != null)
        {
            _iatTest.RemoveStimulus(SelectedStimulus);
            Stimuli.Remove(SelectedStimulus);
            SelectedStimulus = null;
        }
    }

    // Live preview for binding
    public BitmapSource? LivePreview => SelectedStimulus switch
    {
        TextStimulus text => _imageGenerationService.RenderTextToBitmap(text, 
            _layoutCalculatorService.GetFinalRects(_iatTest.Layout).Stimulus),
        ImageStimulus img => _imageGenerationService.LoadEncodedBytesAsManipulableImage(
            _packageService.GetImageBytes(img.Id)), // your existing method
        _ => null
    };

    partial void OnSelectedStimulusChanged(Stimulus? value)
    {
        IsTextMode = value is TextStimulus;
    }
}