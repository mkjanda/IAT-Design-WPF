using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>One ciphertext row on the wire. Matches <c>GEncryptedResultSet</c>.</summary>
public sealed class EncryptedResultSet
{
    [XmlElement("Results", Form = XmlSchemaForm.Unqualified)]
    public string Results { get; set; } = "";

    [XmlElement("Cipher", Form = XmlSchemaForm.Unqualified)]
    public string EncryptedCipher { get; set; } = "";

    [XmlElement("Tag", Form = XmlSchemaForm.Unqualified)]
    public string Tag { get; set; } = "";

    [XmlElement("Nonce", Form = XmlSchemaForm.Unqualified)]
    public string Nonce { get; set; } = "";
}
