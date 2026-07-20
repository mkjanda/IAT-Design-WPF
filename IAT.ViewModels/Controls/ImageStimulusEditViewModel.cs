using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for editing an ImageStimulus. Inherits common Save/Delete/Close from StimulusEditViewModel
/// and overrides CreateDomainStimulus so Save persists an ImageStimulus into the shared IatTest singleton.
/// </summary>
public partial class ImageStimulusEditViewModel : StimulusEditViewModel
{
    private readonly IProjectPackageService _packageService;

    [ObservableProperty] private BitmapSource? previewImage;
    [ObservableProperty] private string fileName = string.Empty;
    [ObservableProperty] private string dimensions = string.Empty;
    [ObservableProperty] private string fileSize = string.Empty;
    [ObservableProperty] private string altText = string.Empty;
    private string filePath = string.Empty;

    public ImageStimulusEditViewModel(IatTest iatTest, IProjectPackageService packageService)
        : base(iatTest)
    {
        _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
    }

    public ImageStimulusEditViewModel(ImageStimulus stimulus, IatTest iatTest, IProjectPackageService packageService)
        : this(iatTest, packageService)
    {
        Id = stimulus.Id;
        FileName = string.IsNullOrEmpty(stimulus.FileName) ? "Image Stimulus" : Path.GetFileName(stimulus.FileName);
        filePath = stimulus.FileName ?? string.Empty;
        AltText = stimulus.AltText ?? string.Empty;

        // Load preview from package service if available
        try
        {
            var bytes = packageService.GetImageBytes(stimulus.Id);
            if (bytes is { Length: > 0 })
            {
                PreviewImage = BitmapFromBytes(bytes);
                UpdateFileInfo(bytes);
            }
        }
        catch
        {
            // Graceful – preview simply stays empty
        }
    }

    [RelayCommand]
    private async Task ChangeImageAsync()
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
            var newId = await _packageService.AddImageAsync(bytes, Path.GetFileName(dialog.FileName));

            Id = newId;
            FileName = Path.GetFileName(dialog.FileName);
            PreviewImage = BitmapFromBytes(bytes);
            UpdateFileInfo(bytes);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load image: {ex.Message}");
        }
    }

    private void UpdateFileInfo(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];

        Dimensions = $"{frame.PixelWidth} × {frame.PixelHeight}";
        FileSize = $"{bytes.Length / 1024} KB";
    }

    private static BitmapSource BitmapFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    /// <summary>
    /// Override so the shared SaveCommand on the base class produces an ImageStimulus.
    /// </summary>
    protected override Stimulus CreateDomainStimulus()
    {
        return new ImageStimulus
        {
            Id = Id,
            FileName = filePath,
            AltText = AltText
        };
    }
}
