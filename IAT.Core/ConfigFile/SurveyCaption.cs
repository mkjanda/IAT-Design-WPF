using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Windows.Media;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the visual style settings for a survey caption, including text color, background color, border color,
/// border width, and font size.
/// </summary>
/// <remarks>Use this class to configure the appearance of caption elements in a survey interface. All properties
/// must be set to valid color and size values to ensure correct rendering. This type is typically used when customizing
/// survey UI components for improved readability and visual consistency.</remarks>
public class SurveyCaption
{
    /// <summary>
    /// The color of the caption text. This property is used to specify the color of the caption text in a survey. It is 
    /// represented as a Color object, which can be defined using RGB values or named colors. The FontColor property allows 
    /// you to customize the appearance of the caption text to enhance readability and visual appeal in the survey interface.
    /// </summary>
    [XmlElement(ElementName = "FontColor", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
    public Color FontColor { get; set; }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    [XmlElement(ElementName = "BackColor", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
    public Color BackColor { get; set; }

    /// <summary>
    /// Gets or sets the color used to draw the border.
    /// </summary>
    [XmlElement(ElementName = "BorderColor", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
    public Color BorderColor { get; set; }

    /// <summary>
    /// Gets or sets the width of the border, in pixels.
    /// </summary>
    [XmlElement(ElementName = "BorderWidth", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int BorderWidth { get; set; }

    /// <summary>
    /// Gets or sets the font size to be used for text rendering.
    /// </summary>
    [XmlElement(ElementName = "FontSize", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int FontSize { get; set; }

    /// <summary>
    /// Initializes a new instance of the SurveyCaption class.
    /// </summary>
    public SurveyCaption() { }

}