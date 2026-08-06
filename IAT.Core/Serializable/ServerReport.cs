using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;


namespace IAT.Core.Serializable;

[XmlRoot("IATReport")]
public class IATReport
{
    [XmlElement("Name", Form = XmlSchemaForm.Unqualified)] public string Name { get; set; } = string.Empty;

    [XmlElement("URL", Form = XmlSchemaForm.Unqualified)] public string URL { get; set; } = string.Empty;

    [XmlElement("NumAdministrations", Form = XmlSchemaForm.Unqualified)] public int NumAdministrations { get; set; } = 0;

    [XmlElement("NumResutSets", Form = XmlSchemaForm.Unqualified)] public int NumResultSets { get; set; } = 0;

    [XmlElement("TestSizeKB", Form = XmlSchemaForm.Unqualified)] public int TestSizeKB { get; set; } = 0;

    [XmlElement("LastDataRetrieval", Form = XmlSchemaForm.Unqualified)] public DateTime LastDataRetrieval { get; set; } = DateTime.MinValue;

    [XmlElement("AuthorTitle", Form = XmlSchemaForm.Unqualified)] public string Author { get; set; } = string.Empty;

    [XmlElement("AuthorFName", Form = XmlSchemaForm.Unqualified)] public string AuthorFName { get; set; } = string.Empty;

    [XmlElement("AuthorLName", Form = XmlSchemaForm.Unqualified)] public string AuthorLName { get; set; } = string.Empty;

    [XmlElement("AuthorEMail", Form = XmlSchemaForm.Unqualified)] public string AuthorEmail { get; set; } = string.Empty;

    [XmlElement("TestVersion", Form = XmlSchemaForm.Unqualified)] public string TestVersion { get; set; } = string.Empty;
}


[XmlRoot("ServerReport")]
public class ServerReport
{
    [XmlElement("ContactFName", Form = XmlSchemaForm.Unqualified)] public string ContactFName { get; set; } = string.Empty;

    [XmlElement("ContactLName", Form = XmlSchemaForm.Unqualified)] public string ContactLName { get; set; } = string.Empty;

    [XmlElement("Organization", Form = XmlSchemaForm.Unqualified)] public string Organization { get; set; } = string.Empty;

    [XmlElement("NumIATsAlotted", Form = XmlSchemaForm.Unqualified)] public int NumIATsAllotted { get; set; } = 0;

    [XmlElement("NumAdministrations", Form = XmlSchemaForm.Unqualified)] public int NumIATsUsed { get; set; } = 0;

    [XmlElement("NumAdministrationsRemaining", Form = XmlSchemaForm.Unqualified)] public int NumAdministrations { get; set; } = 0;

    [XmlElement("DiskAlottmentMB", Form = XmlSchemaForm.Unqualified)] public int DiskAlottmentMB { get; set; } = 0;

    [XmlElement("DiskAlottmentRemainingKB", Form = XmlSchemaForm.Unqualified)] public int DiskAlottmentRemainingKB { get; set; } = 0;

    [XmlArray]
    [XmlArrayItem("IATReport", IsNullable = true, Type = typeof(IATReport), Form = XmlSchemaForm.Unqualified)]
    public List<IATReport> ReportList { get; set; } = new();
}
