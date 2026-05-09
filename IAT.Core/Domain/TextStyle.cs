using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace IAT.Core.Domain;

/// <summary>
/// A text style class that defines the font family, size, and color for text elements in the IAT application. This class is used 
/// to specify the visual appearance of text, allowing for customization of font properties to enhance readability and aesthetics. 
/// The FontFamily property specifies the typeface to be used, while the FontSize property determines the size of the text. The 
/// FontColor property defines the color of the text, enabling further customization to match the overall design and theme of the 
/// application. By using this TextStyle class, developers can create consistent and visually appealing text elements throughout Sthe application.    
/// </summary>
public class TextStyle
{
    /// <summary>
    /// THe font family name used for text rendering. This property allows you to specify the typeface that will be applied to 
    /// text elements in the IAT application. By setting the FontFamily, you can control the overall look and feel of the text, 
    /// ensuring that it aligns with the design and aesthetic preferences of your application. The default value is set to 
    /// "Segoe UI", which is a widely used and visually appealing font that provides good readability for most applications. 
    /// You can change this value to any valid font family name to customize the appearance of your text as needed.
    /// </summary>
    public string FontFamily { get; set; } = "Segoe UI";

    /// <summary>
    /// Gets or sets the font size used to display text.
    /// </summary>
    public double FontSize { get; set; } = 24.0;        
    
    /// <summary>
    /// Gets or sets the color used to display text.
    /// </summary>
    public Color FontColor { get; set; } = Colors.Black;   
}

