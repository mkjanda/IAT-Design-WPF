using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Messages;
using sun.security.x509;
using System;
using System.Windows.Media;

namespace IAT.ViewModels.Controls
{
    /// <summary>
    /// ViewModel for a single stimulus in the library and edit panel.
    /// One-to-one with the domain Stimulus but adds UI-specific properties and commands.
    /// </summary>
    public partial class StimulusEditViewModel : ObservableObject
    {
        [ObservableProperty] private Guid id;
        [ObservableProperty] private string displayName = string.Empty;
        [ObservableProperty] private string text = string.Empty;
        [ObservableProperty] private string fontFamily = "Arial";
        [ObservableProperty] private double fontSize = 24.0;
        [ObservableProperty] private Color textColor = Colors.Black;
        [ObservableProperty] private bool isImageStimulus; 
        [ObservableProperty] private bool isEditPanelVisible = true;
        [ObservableProperty] private byte[]? imageBytes; // for future image preview

        /// <summary>
        /// The brush used to draw the text preview in the library and edit panel. This is a UI-only property that 
        /// converts the Color to a SolidColorBrush.
        /// </summary>
        public SolidColorBrush PreviewBrush => new SolidColorBrush(TextColor);

        /// <summary>
        /// The default constructor is used when creating a new stimulus from the UI (e.g., clicking "Add Text Stimulus"). It generates a new ID and 
        /// sets default values.
        /// </summary>
        public StimulusEditViewModel()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StimulusEditViewModel"/> class from a text stimulus domain object.
        /// </summary>
        /// <param name="domainStimulus">The text stimulus domain object to map to the view model.</param>
        public StimulusEditViewModel(TextStimulus domainStimulus)
        {
            Id = domainStimulus.Id;
            DisplayName = domainStimulus.Text?.Length > 20
                ? domainStimulus.Text.Substring(0, 17) + "..."
                : domainStimulus.Text ?? "Text Stimulus";
            Text = domainStimulus.Text ?? string.Empty;
            FontFamily = domainStimulus.Style.FontFamily ?? "Arial";
            FontSize = domainStimulus.Style.FontSize;
            TextColor = Color.FromArgb(
                domainStimulus.Style.FontColor.A,
                domainStimulus.Style.FontColor.R,
                domainStimulus.Style.FontColor.G,
                domainStimulus.Style.FontColor.B);
        }

        /// <summary>
        /// Sets the text color based on the specified palette type.
        /// </summary>
        /// <param name="paletteType">The palette type. Supported values: 'black', 'white', 'pleasant', 'unpleasant' (case-insensitive).</param>
        [RelayCommand]
        private void ApplyPalette(string paletteType)
        {
            TextColor = paletteType.ToLower() switch
            {
                "black" => Colors.Black,
                "white" => Colors.White,
                "pleasant" => Color.FromRgb(0, 191, 165),   // teal from your screenshot
                "unpleasant" => Color.FromRgb(220, 50, 50),
                _ => TextColor
            };
        }

        [RelayCommand]
        private void Close()
        {
            IsEditPanelVisible = false;
        }

        /// <summary>
        /// Converts back to domain object when saving.
        /// </summary>
        public TextStimulus ToDomainTextStimulus()
        {
            return new TextStimulus
            {
                Id = Id,
                Text = Text,
                Style = new TextStyle
                {
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    FontColor = new Color
                    {
                        A = TextColor.A,
                        R = TextColor.R,
                        G = TextColor.G,
                        B = TextColor.B
                    }
                }
            };
        }

    }
}