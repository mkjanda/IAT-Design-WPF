using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>
/// Plaintext payload stored inside an encrypted row. Matches <c>GResultSet</c>.
/// Unmarshal this after AES-GCM unwrap — not the <c>TestResults</c> envelope.
/// </summary>
[XmlRoot("ResultSet")]
public sealed class ResultSet
{
    [XmlElement("SurveyResult", Form = XmlSchemaForm.Unqualified)]
    public List<SurveyResult> SurveyResults { get; set; } = [];

    [XmlElement("IATResult", Form = XmlSchemaForm.Unqualified)]
    public IATResult IATResult { get; set; } = new();
}
