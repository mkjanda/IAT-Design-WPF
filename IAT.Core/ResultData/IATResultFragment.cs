using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>One administered trial. Matches <c>GIATResultFragment</c>.</summary>
public sealed class IATResultFragment
{
    [XmlElement("BlockNum", Form = XmlSchemaForm.Unqualified)]
    public int BlockNum { get; set; }

    [XmlElement("ItemNum", Form = XmlSchemaForm.Unqualified)]
    public int ItemNum { get; set; }

    [XmlElement("ResponseTime", Form = XmlSchemaForm.Unqualified)]
    public long ResponseTime { get; set; }

    [XmlElement("PresentationNum", Form = XmlSchemaForm.Unqualified)]
    public int PresentationNum { get; set; }

    [XmlElement("Error", Form = XmlSchemaForm.Unqualified)]
    public bool Error { get; set; }
}
