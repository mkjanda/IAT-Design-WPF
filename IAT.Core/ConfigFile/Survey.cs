using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ConfigFile;

/// <summary>
/// One questionnaire as a ConfigFile <c>Survey</c> element. Shape matches
/// <c>GIATSurvey</c> plus the caption / item / image siblings that SurveyPage.xslt walks.
/// </summary>
public class Survey
{
    /// <summary>Milliseconds. 0 = no auto-submit. SurveyScript compares this to "0".</summary>
    [XmlAttribute("TimeoutMillis")]
    public long TimeoutMillis { get; set; }

    [XmlElement("IATName", Form = XmlSchemaForm.Unqualified)]
    public string IATName { get; set; } = string.Empty;

    [XmlElement("ClientId", Form = XmlSchemaForm.Unqualified)]
    public long ClientId { get; set; }

    [XmlElement("SurveyName", Form = XmlSchemaForm.Unqualified)]
    public string SurveyName { get; set; } = string.Empty;

    [XmlElement("InitialPosition", Form = XmlSchemaForm.Unqualified)]
    public int InitialPosition { get; set; }

    /// <summary>
    /// Caption, items, and images in document order. Must stay a single polymorphic list
    /// so the header sits where SurveyPage expects <c>./Caption</c>.
    /// </summary>
    [XmlElement("Caption", typeof(SurveyCaption), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("SurveyItem", typeof(SurveyItem), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("SurveyImage", typeof(SurveyImageItem), Form = XmlSchemaForm.Unqualified)]
    public List<object> Contents { get; set; } = [];
}
