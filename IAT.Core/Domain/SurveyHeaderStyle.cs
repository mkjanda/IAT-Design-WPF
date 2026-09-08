using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IAT.Core.Domain;

/// <summary>
/// Visual style for a survey header / caption. Maps 1:1 onto the SVG caption the
/// server XSLT renders: font face and size, text fill, banner background, and the
/// separator bar under the title (<c>BorderColor</c> / <c>BorderWidth</c> in ConfigFile).
/// </summary>
public partial class SurveyHeaderStyle : ObservableObject
{
    public const string DefaultFontFamily = "Segoe UI";
    public const double DefaultFontSize = 24.0;
    public const int DefaultSeparatorWidth = 8;

    [ObservableProperty]
    private string _fontFamily = DefaultFontFamily;

    [ObservableProperty]
    private double _fontSize = DefaultFontSize;

    /// <summary>Caption text fill. Serialized as RGB channels on export (FontColorR/G/B).</summary>
    [ObservableProperty]
    private Color _fontColor = Colors.Black;

    /// <summary>Banner background behind the SVG caption (BackColorR/G/B).</summary>
    [ObservableProperty]
    private Color _backColor = Colors.White;

    /// <summary>Separator bar under the caption (BorderColorR/G/B in the XSLT).</summary>
    [ObservableProperty]
    private Color _separatorColor = Colors.Black;

    /// <summary>
    /// Separator bar thickness in CSS pixels. XSLT uses this as <c>BorderWidth</c>
    /// and draws the bar at 1.25× that height.
    /// </summary>
    [ObservableProperty]
    private int _separatorWidth = DefaultSeparatorWidth;

    public SurveyHeaderStyle Clone() => new()
    {
        FontFamily = FontFamily,
        FontSize = FontSize,
        FontColor = FontColor,
        BackColor = BackColor,
        SeparatorColor = SeparatorColor,
        SeparatorWidth = SeparatorWidth
    };

    public void CopyFrom(SurveyHeaderStyle other)
    {
        ArgumentNullException.ThrowIfNull(other);
        FontFamily = string.IsNullOrWhiteSpace(other.FontFamily) ? DefaultFontFamily : other.FontFamily;
        FontSize = other.FontSize > 0 ? other.FontSize : DefaultFontSize;
        FontColor = other.FontColor;
        BackColor = other.BackColor;
        SeparatorColor = other.SeparatorColor;
        SeparatorWidth = other.SeparatorWidth > 0 ? other.SeparatorWidth : DefaultSeparatorWidth;
    }
}
