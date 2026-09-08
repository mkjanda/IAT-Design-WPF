using System.Security.RightsManagement;
using System.Windows.Media;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Extensions;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Per-item text format required by the survey XSD (<c>GFormat</c>) and by
/// <c>mine:textWidth</c> in SurveyPage.xslt.
/// </summary>
public class SurveyFormat
{
    [XmlElement("Font", Form = XmlSchemaForm.Unqualified)]
    public string Font { get; set; } = "Verdana";

    [XmlElement("FontSize", Form = XmlSchemaForm.Unqualified)]
    public string FontSize { get; set; } = "16";


    [XmlElement("Color", Form = XmlSchemaForm.Unqualified)]
    public string ColorHex { get { return $"#{Color.Red()}{Color.Green()}{Color.Blue()}"; } set; }

    [XmlIgnore]
    public Color Color { get; set; } = Colors.Black;

    [XmlElement("Bold", Form = XmlSchemaForm.Unqualified)]
    public bool Bold { get; set; }

    [XmlElement("Italic", Form = XmlSchemaForm.Unqualified)]
    public bool Italic { get; set; }

    public static SurveyFormat Default { get; } = new();
}
