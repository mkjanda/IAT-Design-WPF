using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace IAT.ViewModels.Controls;

public partial class ImageStimulusEditViewModel : StimulusEditViewModel
{
    private readonly IProjectPackageService _packageService;

    [ObservableProperty] private BitmapSource? previewImage;
    [ObservableProperty] private string fileName = string.Empty;
    [ObservableProperty] private string dimensions = string.Empty;
    [ObservableProperty] private string fileSize = string.Empty;
    [ObservableProperty] private string altText = string.Empty;

    public ImageStimulusEditViewModel(IProjectPackageService packageService)
    {
        _packageService = packageService;
    }

    public ImageStimulusEditViewModel(ImageStimulus stimulus, IProjectPackageService packageService) : this(packageService)
    {
        Id = stimulus.Id;
        FileName = Path.GetFileName(stimulus.FileName);
        AltText = stimulus.AltText ?? string.Empty;

        // Load preview from package service
        var bytes = packageService.GetImageBytes(stimulus.Id);
        if (bytes.Length > 0)
        {
            PreviewImage = BitmapFromBytes(bytes);
            UpdateFileInfo(bytes);
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
            // Use your UserNotificationService here in production
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

    public ImageStimulus ToDomainImageStimulus() => new()
    {
        Id = Id,
        FileName = FileName,
        AltText = AltText
    };
}