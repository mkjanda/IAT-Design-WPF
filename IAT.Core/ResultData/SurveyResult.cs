using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>One questionnaire's answers. Matches <c>GSurveyResult</c>.</summary>
public sealed class SurveyResult
{
    [XmlAttribute("SurveyName")]
    public string SurveyName { get; set; } = "";

    [XmlElement("Answer", Form = XmlSchemaForm.Unqualified)]
    public List<string> Answers { get; set; } = [];
}
