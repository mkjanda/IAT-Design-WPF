using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>Download envelope from <c>GET /Download/Results</c>. Matches <c>GTestResults</c>.</summary>
[XmlRoot("TestResults")]
public sealed class TestResults
{
    [XmlElement("Descriptor", Form = XmlSchemaForm.Unqualified)]
    public ResultSetDescriptor Descriptor { get; set; } = new();

    [XmlElement("EncryptedResultSet", Form = XmlSchemaForm.Unqualified)]
    public List<EncryptedResultSet> EncryptedResultSets { get; set; } = new List<EncryptedResultSet>();

    [XmlIgnore]
    public List<ResultSet> ResultSets { get; set; } = new List<ResultSet>();
}
