using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.ConfigFile;

namespace IAT.Core.ResultData;

/// <summary>
/// Header on <c>TestResults</c>. Matches <c>GResultSetDescriptor</c> children.
/// Nested <c>ConfigFile</c> is left as raw XML so this folder does not take a
/// dependency on the designer ConfigFile types.
/// </summary>
public sealed class ResultSetDescriptor
{
    [XmlAttribute("DataVersion")]
    public int DataVersion { get; set; }

    [XmlElement("TestAuthor", Form = XmlSchemaForm.Unqualified)]
    public string TestAuthor { get; set; } = "";

    [XmlElement("ConfigFile")]
    public IATConfigFile ConfigFile { get; set; }

    [XmlElement("NumResults", Form = XmlSchemaForm.Unqualified)]
    public int NumResults { get; set; }

    [XmlElement("EncRsaParams", Form = XmlSchemaForm.Unqualified)]
    public RSACryptoParams RsaParams { get; set; } = new();
}
