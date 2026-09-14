using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>Latency payload for one administration. Matches <c>GIATResult</c>.</summary>
public sealed class IATResult
{
    [XmlAttribute("NumElements")]
    public int NumElements { get; set; }

    [XmlElement("Fragment", Form = XmlSchemaForm.Unqualified)]
    public List<IATResultFragment> Fragments { get; set; } = [];
}
