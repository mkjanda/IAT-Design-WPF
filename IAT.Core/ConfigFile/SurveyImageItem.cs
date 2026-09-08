using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Embedded survey image. XSLT reads <c>MimeType</c>, <c>ImageData</c> (base64), and <c>Id</c>.
/// </summary>
public class SurveyImageItem
{

    [XmlElement("MimeType", Form = XmlSchemaForm.Unqualified)]
    public string MimeType { get; set; } = "image/png";

    [XmlIgnore]
    public string ImageData { get; set; } = string.Empty;

    [XmlElement("ResourceId", Form = XmlSchemaForm.Unqualified)]
    public long ResourceId { get; set; }

    [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
    public string Id { get; set; } = string.Empty;

    [XmlIgnore]
    public string FileName { get; set; } = string.Empty;
}
