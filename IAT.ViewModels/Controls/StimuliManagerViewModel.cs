using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace IAT.ViewModels.Controls;

/// <summary>
/// Manages the collection of stimuli for an IAT test, providing functionality to add, delete, and filter text and image
/// stimuli.
/// </summary>
public partial class StimuliManagerViewModel : ObservableObject
{
    private readonly IatTest _currentTest;
    private readonly IProjectPackageService _packageService;
    private readonly IImageGenerationService _imageGenService;
    private readonly ILayoutCalculatorService _layoutCalculatorService;

    [ObservableProperty]
    private ObservableCollection<StimulusListItemViewModel> stimuliItems = new();

    [ObservableProperty]
    private StimulusListItemViewModel? selectedItem;

    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="StimuliManagerViewModel"/> class.
    /// </summary>
    /// <param name="currentTest">The current IAT test.</param>
    /// <param name="packageService">The service for managing project packages.</param>
    /// <param name="imageGenService">The image generation service.</param>
    /// <param name="rectCalculator">The layout calculator service.</param>
    public StimuliManagerViewModel(IatTest currentTest,
                                  IProjectPackageService packageService,
                                  IImageGenerationService imageGenService,
                                  ILayoutCalculatorService rectCalculator)
    {
        _currentTest = currentTest;
        _packageService = packageService;
        _imageGenService = imageGenService;
        _layoutCalculatorService = rectCalculator;
        LoadStimuli();
    }

    private void LoadStimuli()
    {
        StimuliItems.Clear();
        LayoutRects rects = _layoutCalculatorService.GetFinalRects(_currentTest.Layout);
        foreach (var stimulus in _currentTest.AllStimuli)
        {
            StimuliItems.Add(new StimulusListItemViewModel(stimulus, _packageService,
                _imageGenService, rects.Stimulus));
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        // Real-time filtering (you can also use CollectionViewSource if you prefer)
        LayoutRects rects = _layoutCalculatorService.GetFinalRects(_currentTest.Layout);
        var filtered = _currentTest.AllStimuli
            .Where(s => string.IsNullOrEmpty(value) ||
                        (s is TextStimulus ts && ts.Text.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .Select(s => new StimulusListItemViewModel(s, _packageService,
                _imageGenService, rects.Stimulus))
            .ToList();

        StimuliItems.Clear();
        foreach (var item in filtered) StimuliItems.Add(item);
    }

    [RelayCommand]
    private void AddTextStimulus()
    {
        var newStimulus = new TextStimulus
        {
            Id = Guid.NewGuid(),
            Text = "New Text Stimulus"
        };
        LayoutRects rects = _layoutCalculatorService.GetFinalRects(_currentTest.Layout);

        _currentTest.AddStimulus(newStimulus);
        StimuliItems.Add(new StimulusListItemViewModel(newStimulus, _packageService, _imageGenService, rects.Stimulus));
        SelectedItem = StimuliItems.Last(); 
    }

    [RelayCommand]
    private async Task AddImageStimulusAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            LayoutRects rects = _layoutCalculatorService.GetFinalRects(_currentTest.Layout);

            var bytes = await File.ReadAllBytesAsync(dialog.FileName);
            var imageId = await _packageService.AddImageAsync(bytes, Path.GetFileName(dialog.FileName));

            var newStimulus = new ImageStimulus
            {
                FileName = dialog.FileName
            };

            _currentTest.AddStimulus(newStimulus);
            StimuliItems.Add(new StimulusListItemViewModel(newStimulus, _packageService, _imageGenService, rects.Stimulus));
            SelectedItem = StimuliItems.Last();
        }
        catch (Exception ex)
        {
            // Your existing UserNotificationService would be injected here in production
            System.Windows.MessageBox.Show($"Failed to add image: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteStimulus(StimulusListItemViewModel? item)
    {
        if (item?.Stimulus == null) return;

        // TODO: Production check – is this stimulus used in any trial?
        _currentTest.RemoveStimulus(item.Stimulus);
        StimuliItems.Remove(item);
    }
}

/// <summary>
/// Represents a view model for a stimulus list item, providing display-friendly properties and a preview image.
/// </summary>
public partial class StimulusListItemViewModel : ObservableObject
{
    /// <summary>
    /// Gets the stimulus.  
    /// </summary>
    public Stimulus Stimulus { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the stimulus type
    /// </summary>
    public string StimulusType { get; }

    /// <summary>
    /// Gets the preview bitmap image.
    /// </summary>
    public BitmapSource? Preview { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StimulusListItemViewModel"/> class with the specified stimulus and
    /// generates preview content.
    /// </summary>
    /// <param name="stimulus">The stimulus to display in the list item.</param>
    /// <param name="pack">The package service used to retrieve image data.</param>
    /// <param name="imageGenService">The service used to generate preview images.</param>
    /// <param name="boundingRect">The bounding rectangle for rendering text previews.</param>
    public StimulusListItemViewModel(Stimulus stimulus, IProjectPackageService pack, IImageGenerationService imageGenService, Rect boundingRect)
    {
        Stimulus = stimulus;
       

        if (stimulus is TextStimulus ts)
        {
            DisplayName = ts.Text.Length > 30 ? ts.Text[..27] + "..." : ts.Text;
            StimulusType = "Text";
            Preview = imageGenService.RenderTextToBitmap(ts, boundingRect);
        }
        else if (stimulus is ImageStimulus img)
        {
            DisplayName = string.IsNullOrEmpty(img.FileName) ? "Image Stimulus" : Path.GetFileName(img.FileName);
            StimulusType = "Image";
            Preview = imageGenService.BitmapFromBytes(pack.GetImageBytes(img.Id));
        }
    }
}