using System.Windows.Media;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Extensions;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Survey caption / header as consumed by <c>SurveyPage.xslt</c> <c>GenerateCaption</c>.
/// Channel values are two uppercase hex digits with no prefix.
/// </summary>
public class SurveyCaption
{
    [XmlElement("Text", Form = XmlSchemaForm.Unqualified)]
    public string Text { get; set; } = string.Empty;

    [XmlElement("TextWidth", Form = XmlSchemaForm.Unqualified)]
    public string TextWidth { get; set; } = "0";

    [XmlElement("BorderWidth", Form = XmlSchemaForm.Unqualified)]
    public string BorderWidth { get; set; } = "8";

    [XmlElement("FontName", Form = XmlSchemaForm.Unqualified)]
    public string FontName { get; set; } = "Segoe UI";

    [XmlElement("LineHeight", Form = XmlSchemaForm.Unqualified)]
    public string LineHeight { get; set; } = "24";

    [XmlElement("FontSize", Form = XmlSchemaForm.Unqualified)]
    public string FontSize { get; set; } = "24";

    [XmlElement("FontColorR", Form = XmlSchemaForm.Unqualified)]
    public string FontColorR
    {
        get { return $"{FontColor.Red()}"; }
        set { FontColor = Color.FromRgb(Convert.ToByte(value, 16), FontColor.G, FontColor.B); }
    }

    [XmlElement("FontColorG", Form = XmlSchemaForm.Unqualified)]
    public string FontColorG
    {
        get { return $"{FontColor.Green()}"; }
        set { FontColor = Color.FromRgb(FontColor.R, Convert.ToByte(value, 16), FontColor.B); }
    }

    [XmlElement("FontColorB", Form = XmlSchemaForm.Unqualified)]
    public string FontColorB
    {
        get { return $"{FontColor.Blue()}"; }
        set { FontColor = Color.FromRgb(FontColor.R, FontColor.G, Convert.ToByte(value, 16)); }
    }

    [XmlIgnore]
    public Color FontColor { get; set; }

    [XmlElement("BackColorR", Form = XmlSchemaForm.Unqualified)]
    public string BackColorR
    {
        get { return $"{BackColor.Red()}"; }
        set { BackColor = Color.FromRgb(Convert.ToByte(value, 16), BackColor.G, BackColor.B); }
    }

    [XmlElement("BackColorG", Form = XmlSchemaForm.Unqualified)]
    public string BackColorG
    {
        get { return $"{BackColor.Green()}"; }
        set { BackColor = Color.FromRgb(BackColor.R, Convert.ToByte(value, 16), BackColor.B); }
    }

    [XmlElement("BackColorB", Form = XmlSchemaForm.Unqualified)]
    public string BackColorB
    {
        get { return $"{BackColor.Blue()}"; }
        set { BackColor = Color.FromRgb(BackColor.R, BackColor.G, Convert.ToByte(value, 16)); }
    }

    [XmlIgnore]
    public Color BackColor { get; set; }

    [XmlElement("BorderColorR", Form = XmlSchemaForm.Unqualified)]
    public string BorderColorR
    {
        get { return $"{BorderColor.Red()}"; }
        set { BorderColor = Color.FromRgb(Convert.ToByte(value, 16), BorderColor.G, BorderColor.B); }
    }

    [XmlElement("BorderColorG", Form = XmlSchemaForm.Unqualified)]
    public string BorderColorG
    {
        get { return $"{BorderColor.Green()}"; }
        set { BorderColor = Color.FromRgb(BorderColor.R, Convert.ToByte(value, 16), BorderColor.B); }
    }

    [XmlElement("BorderColorB", Form = XmlSchemaForm.Unqualified)]
    public string BorderColorB
    {
        get { return $"{BorderColor.Blue()}"; }
        set { BorderColor = Color.FromRgb(BorderColor.R, BorderColor.G, Convert.ToByte(value, 16)); }
    }

    [XmlIgnore]
    public Color BorderColor { get; set; }
}
