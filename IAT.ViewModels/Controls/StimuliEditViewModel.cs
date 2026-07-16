using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace IAT.ViewModels.Controls;

/// <summary>
/// Base ViewModel for editing a stimulus (text or image). Handles common Save / Delete / Close logic
/// against the singleton IatTest domain model. Derived classes supply the concrete domain conversion
/// via CreateDomainStimulus().
/// </summary>
public partial class StimulusEditViewModel : ObservableObject
{
    protected readonly IatTest _iatTest;

    [ObservableProperty] private Guid id;
    [ObservableProperty] private string text = string.Empty;
    [ObservableProperty] private string fontFamily = "Arial";
    [ObservableProperty] private double fontSize = 24.0;
    [ObservableProperty] private Color textColor = Colors.Black;
    [ObservableProperty] private bool isEditPanelVisible = true;

    public ObservableCollection<string> AvailableFontFamilies { get; } = new() { "Arial", "Segoe UI", "Calibri", "Verdana", "Georgia", "Segoe Print," };
    public ObservableCollection<double> AvailableFontSizes { get; } = new() { 12, 16, 20, 24, 28, 32, 36, 48, 54, 66, 72, 96 };

    public SolidColorBrush PreviewBrush => new SolidColorBrush(TextColor);

    /// <summary>
    /// Raised after a successful Save so the owning manager can refresh selection / list item previews if needed.
    /// </summary>
    public event Action? Saved;
    public event Action? Deleted;
    public event Action? Closed;
    public event Action? AddNewRequested;

    public StimulusEditViewModel(IatTest iatTest)
    {
        _iatTest = iatTest ?? throw new ArgumentNullException(nameof(iatTest));
        Id = Guid.NewGuid();
    }

    public StimulusEditViewModel(TextStimulus stimulus, IatTest iatTest) : this(iatTest)
    {
        Id = stimulus.Id;
        Text = stimulus.Text ?? string.Empty;
        FontFamily = stimulus.Style?.FontFamily ?? "Arial";
        FontSize = stimulus.Style?.FontSize ?? 24.0;

        if (stimulus.Style?.FontColor != null)
        {
            TextColor = Color.FromArgb(
                stimulus.Style.FontColor.A,
                stimulus.Style.FontColor.R,
                stimulus.Style.FontColor.G,
                stimulus.Style.FontColor.B);
        }
    }

    [RelayCommand]
    private void ApplyPalette(string paletteType)
    {
        TextColor = paletteType.ToLowerInvariant() switch
        {
            "black" => Colors.Black,
            "white" => Colors.White,
            "flame scarlet" => Color.FromRgb(205, 33, 42),
            "silver sconce" => Color.FromRgb(220, 50, 50),
            "ultra violet" => Color.FromRgb(95, 75, 139),
            "knockout pink" => Color.FromRgb(255, 62, 165),
            "emerald" => Color.FromRgb(0, 148, 115),
            "sunset gold" => Color.FromRgb(247, 196, 148),
            "radiant orchid" => Color.FromRgb(174, 93, 153),
            "raspberry" => Color.FromRgb(227, 11, 93),
            "acid lime" => Color.FromRgb(187, 223, 50),
            "bluebird" => Color.FromRgb(0, 161, 180),
            "star sapphire" => Color.FromRgb(69, 104, 154),
            "angel blue" => Color.FromRgb(131, 198, 207),
            "ember glow" => Color.FromRgb(234, 103, 89),
            "pale gold" => Color.FromRgb(238, 232, 170),
            "blackened pearl" => Color.FromRgb(77, 75, 80),

            _ => TextColor
        };
        OnPropertyChanged(nameof(PreviewBrush));
    }

    [RelayCommand]
    private void Close()
    {
        IsEditPanelVisible = false;
        Closed?.Invoke();
    }

    /// <summary>
    /// Creates the concrete domain Stimulus from the current edit state.
    /// Override in ImageStimulusEditViewModel (and any future derived types).
    /// </summary>
    protected virtual Stimulus CreateDomainStimulus()
    {
        return new TextStimulus
        {
            Id = Id,
            Text = Text,
            Style = new TextStyle
            {
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontColor = TextColor
            }
        };
    }

    /// <summary>
    /// Saves the current edit state into the singleton IatTest.Stimuli ObservableCollection
    /// (adds if new, updates if the Id already exists).
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        var domain = CreateDomainStimulus();
        if (_iatTest.GetStimulusById(domain.Id) is null)
        {
            _iatTest.AddStimulus(domain);
        }
        else
        {
            _iatTest.UpdateStimulus(domain);
        }
        Saved?.Invoke();
    }

    [RelayCommand]
    private void Delete()
    {
        var existing = _iatTest.GetStimulusById(Id);
        if (existing is not null)
        {
            _iatTest.RemoveStimulus(existing);
        }
        IsEditPanelVisible = false;
        Deleted?.Invoke();
    }

    [RelayCommand]
    private void AddNew()
    {
        // Let the manager decide how to create a fresh blank editor
        AddNewRequested?.Invoke();
    }
}
