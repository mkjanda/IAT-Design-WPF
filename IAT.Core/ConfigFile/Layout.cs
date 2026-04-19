using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Windows.Media;
namespace IAT.Core.ConfigFile;

/// <summary>
/// THe Layout object contains information regarding the size and colors of diffreent elements of the test
/// </summary>
[XmlRoot("Layout")]
public sealed class Layout {
    /// <summary>
    /// The interior width represents the interior width of the test window
    /// </summary>
    [XmlElement("InteriorWidth", Form = XmlSchemaForm.Unqualified)]
    public int InteriorWidth { get; set; }

    /// <summary>
    /// The interior height represents the interior width of the test window
    /// </summary>
    [XmlElement("InteriorHeight", Form = XmlSchemaForm.Unqualified)]
    public int InteriorHeight { get; set; } = 0;

    /// <summary>
    /// The border width is the width of the border around the test window. If the border width is 0, then there is no border
    /// </summary>
    [XmlElement("BorderWidth", Form = XmlSchemaForm.Unqualified)]
    public int BorderWidth { get; set; } = 0;

    /// <summary>
    /// The response wiidth represents the width of the response area. If the response width is 0, then there is no response area
    /// </summary>
    [XmlElement("ResponseWidth", Form = XmlSchemaForm.Unqualified)]
    public int ResponseWidth { get; set; } = 0;

    /// <summary>
    /// The response height represents the height of the response area. If the response height is 0, then there is no response area
    /// </summary>
    [XmlElement("ResponseHeight", Form = XmlSchemaForm.Unqualified)]
    public int ResponseHeight { get; set; } = 0;

    /// <summary>
    /// Gets or sets the color used to draw the border.
    /// </summary>
    [XmlIgnore]
    public Color BorderColor { get; set; } = Colors.Black;

    /// <summary>
    /// Gets or sets the background color for the control.
    /// </summary>
    [XmlIgnore]
    public Color BackColor { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the color used to draw the outline.
    /// </summary>
    [XmlIgnore]
    public Color OutlineColor { get; set; } = Colors.Black;

    /// <summary>
    /// Gets or sets the background color of the page.
    /// </summary>
    [XmlIgnore]
    public Color PageBackColor { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the red component of the border color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>When setting this property, the value must be a valid two-character hexadecimal string
    /// representing a byte (00 to FF). The green and blue components of the border color remain unchanged.</remarks>
    [XmlElement("BorderColorR", Form = XmlSchemaForm.Unqualified)]
    public string BorderColorR {
        get => string.Format("{0:X2}", BorderColor.R);
        set => BorderColor = Color.FromRgb(Convert.ToByte(value, 16), BorderColor.G, BorderColor.B);
    }

    /// <summary>
    /// Gets or sets the green component of the border color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the green channel of the border color in hexadecimal format (00 to FF).
    /// Setting this property updates only the green component of the border color, leaving the red and blue components
    /// unchanged.</remarks>
    [XmlElement("BorderColorG", Form = XmlSchemaForm.Unqualified)]
    public string BorderColorG {
        get => string.Format("{0:X2}", BorderColor.G);
        set => BorderColor = Color.FromRgb(BorderColor.R, Convert.ToByte(value, 16), BorderColor.B);
    }

    /// <summary>
    /// Gets or sets the blue component of the border color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the blue channel of the border color in uppercase hexadecimal format (00
    /// to FF). Setting this property updates only the blue component, preserving the red and green components of the
    /// border color.</remarks>
    [XmlElement("BorderColorB", Form = XmlSchemaForm.Unqualified)]
    public string BorderColorB {
        get => string.Format("{0:X2}", BorderColor.B);
        set => BorderColor = Color.FromRgb(BorderColor.R, BorderColor.G, Convert.ToByte(value, 16));
    }

    /// <summary>
    /// Gets or sets the red component of the background color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the red channel of the background color in uppercase hexadecimal format
    /// ("00" to "FF"). Setting this property updates only the red component, preserving the green and blue components
    /// of the background color.</remarks>
    [XmlElement("BackColorR", Form = XmlSchemaForm.Unqualified)]
    public string BackColorR {
        get => string.Format("{0:X2}", BackColor.R);
        set => BackColor = Color.FromRgb(Convert.ToByte(value, 16), BackColor.G, BackColor.B);
    }

    /// <summary>
    /// Gets or sets the green component of the background color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the green channel of the color in hexadecimal format (00 to FF). When
    /// setting, the input string must be a valid two-digit hexadecimal value.</remarks>
    [XmlElement("BackColorG", Form = XmlSchemaForm.Unqualified)]
    public string BackColorG {
        get => string.Format("{0:X2}", BackColor.G);
        set => BackColor = Color.FromRgb(BackColor.R, Convert.ToByte(value, 16), BackColor.B);
    }

    /// <summary>
    /// Gets or sets the blue component of the background color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the blue channel of the color in uppercase hexadecimal format ("00" to
    /// "FF"). Setting this property updates only the blue component of the background color, leaving the red and green
    /// components unchanged.</remarks>
    [XmlElement("BackColorB", Form = XmlSchemaForm.Unqualified)]
    public string BackColorB {
        get => string.Format("{0:X2}", BackColor.B);
        set => BackColor = Color.FromRgb(BackColor.R, BackColor.G, Convert.ToByte(value, 16));
    }

    /// <summary>
    /// Gets or sets the red component of the outline color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the red channel of the outline color in hexadecimal format ("00" to
    /// "FF"). Setting this property updates only the red component of the outline color, leaving the green and blue
    /// components unchanged.</remarks>
    [XmlElement("OutlineColorR", Form = XmlSchemaForm.Unqualified)]
    public string OutlineColorR {
        get => string.Format("{0:X2}", OutlineColor.R);
        set => OutlineColor = Color.FromRgb(Convert.ToByte(value, 16), OutlineColor.G, OutlineColor.B);
    }

    /// <summary>
    /// Gets or sets the green component of the outline color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the green channel of the outline color in hexadecimal format (00 to FF).
    /// Setting this property updates only the green component, preserving the red and blue components of the outline
    /// color.</remarks>
    [XmlElement("OutlineColorG", Form = XmlSchemaForm.Unqualified)]
    public string OutlineColorG {
        get => string.Format("{0:X2}", OutlineColor.G);
        set => OutlineColor = Color.FromRgb(OutlineColor.R, Convert.ToByte(value, 16), OutlineColor.B);
    }

    /// <summary>
    /// Gets or sets the blue component of the outline color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the blue channel of the outline color in hexadecimal format (00 to FF).
    /// Setting this property updates only the blue component of the outline color, leaving the red and green components
    /// unchanged.</remarks>
    [XmlElement("OutlineColorB", Form = XmlSchemaForm.Unqualified)]
    public string OutlineColorB {
        get => string.Format("{0:X2}", OutlineColor.B);
        set => OutlineColor = Color.FromRgb(OutlineColor.R, OutlineColor.G, Convert.ToByte(value, 16));
    }

    /// <summary>
    /// Gets or sets the red component of the page background color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the red channel of the color in uppercase hexadecimal format ("00" to
    /// "FF"). Setting this property updates only the red component of the page background color, leaving the green and
    /// blue components unchanged.</remarks>
    [XmlElement("PageBackColorR", Form = XmlSchemaForm.Unqualified)]
    public string PageBackColorR
    {
        get => string.Format("{0:X2}", PageBackColor.R);
        set => PageBackColor = Color.FromRgb(Convert.ToByte(value, 32), PageBackColor.G, PageBackColor.B);
    }

    /// <summary>
    /// Gets or sets the green component of the page background color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the green channel of the color in hexadecimal format (00 to FF). Setting
    /// this property updates only the green component of the page background color, leaving the red and blue components
    /// unchanged.</remarks>
    [XmlElement("PageBackColorG", Form = XmlSchemaForm.Unqualified)]
    public string PageBackColorG
    {
        get => string.Format("{0:X2}", PageBackColor.G);
        set => PageBackColor = Color.FromRgb(PageBackColor.R, Convert.ToByte(value, 16), PageBackColor.B);
    }

    /// <summary>
    /// Gets or sets the blue component of the page background color as a two-digit hexadecimal string.
    /// </summary>
    /// <remarks>The value represents the blue channel of the color in uppercase hexadecimal format ("00" to
    /// "FF"). Setting this property updates only the blue component of the page background color; the red and green
    /// components remain unchanged.</remarks>
    [XmlElement("PageBackColorB", Form = XmlSchemaForm.Unqualified)]
    public string PageBackColorB
    {
        get => string.Format("{0:X2}", PageBackColor.B);
        set => PageBackColor = Color.FromRgb(PageBackColor.R, PageBackColor.G, Convert.ToByte(value, 16));
    }

    /// <summary>
    /// Initializes a new instance of the Layout class.
    /// </summary>
    public Layout() { }
}
