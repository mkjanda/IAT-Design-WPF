using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace IAT.ViewModels.Controls;

public partial class StimuliManagerViewModel : ObservableObject
{
    private readonly IatTest _currentTest;
    private readonly ProjectPackageService _packageService;
    private readonly IImageGenerationService _imageGenService;

    [ObservableProperty]
    private ObservableCollection<StimulusListItemViewModel> stimuliItems = new();

    [ObservableProperty]
    private StimulusListItemViewModel? selectedItem;

    [ObservableProperty]
    private string searchText = string.Empty;

    public StimuliManagerViewModel(IatTest currentTest,
                                  ProjectPackageService packageService,
                                  IImageGenerationService imageGenService)
    {
        _currentTest = currentTest;
        _packageService = packageService;
        _imageGenService = imageGenService;

        LoadStimuli();
    }

    private void LoadStimuli()
    {
        StimuliItems.Clear();
        foreach (var stimulus in _currentTest.AllStimuli)
        {
            StimuliItems.Add(new StimulusListItemViewModel(stimulus, _imageGenService));
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        // Real-time filtering (you can also use CollectionViewSource if you prefer)
        var filtered = _currentTest.AllStimuli
            .Where(s => string.IsNullOrEmpty(value) ||
                        (s is TextStimulus ts && ts.Text.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .Select(s => new StimulusListItemViewModel(s, _imageGenService))
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

        _currentTest.AddStimulus(newStimulus);
        StimuliItems.Add(new StimulusListItemViewModel(newStimulus, _imageGenService));
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
            var bytes = await File.ReadAllBytesAsync(dialog.FileName);
            var imageId = await _packageService.AddImageAsync(bytes, Path.GetFileName(dialog.FileName));

            var newStimulus = new ImageStimulus
            {
                FileName = dialog.FileName
            };

            _currentTest.AddStimulus(newStimulus);
            StimuliItems.Add(new StimulusListItemViewModel(newStimulus, _imageGenService));
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

public partial class StimulusListItemViewModel : ObservableObject
{
    public Stimulus Stimulus { get; }
    public string DisplayName { get; }
    public string StimulusType { get; }
    public BitmapSource? Preview { get; private set; }

    public StimulusListItemViewModel(Stimulus stimulus, IImageGenerationService imageGenService)
    {
        Stimulus = stimulus;
       

        if (stimulus is TextStimulus ts)
        {
            DisplayName = ts.Text.Length > 30 ? ts.Text[..27] + "..." : ts.Text;
            StimulusType = "Text";
            Preview = imageGenService.RenderTextToBitmap(ts, );
        }
        else if (stimulus is ImageStimulus img)
        {
            DisplayName = string.IsNullOrEmpty(img.FileName) ? "Image Stimulus" : Path.GetFileName(img.FileName);
            StimulusType = "Image";
            // TODO: Add thumbnail generation in ImageGenerationService if needed
            Preview = null; // placeholder – you can extend LoadEncodedBytesAsManipulableImage for thumbs
        }
    }
}