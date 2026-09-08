using System.ComponentModel.DataAnnotations;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ConfigFile;

/// <summary>
/// One survey row. The typed child name (Boolean, Likert, …) is what SurveyPage.xslt
/// dispatches on. <see cref="ResponseMarker"/> exists only so the Instruction-class
/// test (<c>Response/@Type</c>) has a node to read.
/// </summary>
public class SurveyItem
{
    [XmlAttribute("Optional")]
    public bool Optional { get; set; }

    [XmlAttribute("ItemNum")]
    public int ItemNum { get; set; }

    [XmlAttribute("QuestionNum")]
    public int QuestionNum { get; set; }

    [XmlElement("Format", Form = XmlSchemaForm.Unqualified)]
    public SurveyFormat Format { get; set; } = new();

    [XmlElement("Text", Form = XmlSchemaForm.Unqualified)]
    public string Text { get; set; } = string.Empty;
    
    [XmlElement("TrueFalse", typeof(TrueFalse), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("Likert", typeof(Likert), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("Date", typeof(Date), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("MultiChoice", typeof(MultiChoice), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("MultiSelect", typeof(MultiSelect), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("BoundedText", typeof(BoundedText), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("BoundedNumber", typeof(BoundedNumber), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("FixedDigit", typeof(FixedDigit), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("RegEx", typeof(RegEx), Form = XmlSchemaForm.Unqualified)]
    [XmlElement("Instruction", typeof(Instruction), Form = XmlSchemaForm.Unqualified)]
    public Response? Response { get; set; }

}
