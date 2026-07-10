using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace IAT.ViewModels.Controls;

public partial class StimulusEditViewModel : ObservableObject
{
    [ObservableProperty] private Guid id;
    [ObservableProperty] private string text = string.Empty;
    [ObservableProperty] private string fontFamily = "Arial";
    [ObservableProperty] private double fontSize = 24.0;
    [ObservableProperty] private Color textColor = Colors.Black;
    [ObservableProperty] private bool isEditPanelVisible = true;

    public ObservableCollection<string> AvailableFontFamilies { get; } = new() { "Arial", "Segoe UI", "Calibri", "Verdana", "Georgia" };
    public ObservableCollection<double> AvailableFontSizes { get; } = new() { 12, 16, 20, 24, 28, 32, 36, 48 };

    public SolidColorBrush PreviewBrush => new SolidColorBrush(TextColor);

    public StimulusEditViewModel()
    {
        Id = Guid.NewGuid();
    }

    public StimulusEditViewModel(TextStimulus stimulus)
    {
        Id = stimulus.Id;
        Text = stimulus.Text ?? string.Empty;
        FontFamily = stimulus.Style?.FontFamily ?? "Arial";
        FontSize = stimulus.Style?.FontSize ?? 24;

        if (stimulus.Style?.FontColor != null)
            TextColor = Color.FromArgb(
                stimulus.Style.FontColor.A,
                stimulus.Style.FontColor.R,
                stimulus.Style.FontColor.G,
                stimulus.Style.FontColor.B);
    }

    [RelayCommand]
    private void ApplyPalette(string paletteType)
    {
        TextColor = paletteType.ToLower() switch
        {
            "black" => Colors.Black,
            "white" => Colors.White,
            "pleasant" => Color.FromRgb(0, 191, 165),
            "unpleasant" => Color.FromRgb(220, 50, 50),
            _ => TextColor
        };
    }

    [RelayCommand]
    private void Close() => IsEditPanelVisible = false;

    // Override in derived classes
    public virtual TextStimulus ToDomainTextStimulus() => new()
    {
        Id = Id,
        Text = Text,
        Style = new TextStyle
        {
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontColor = new System.Windows.Media.Color
            {
                A = TextColor.A,
                R = TextColor.R,
                G = TextColor.G,
                B = TextColor.B
            }
        }
    };
}