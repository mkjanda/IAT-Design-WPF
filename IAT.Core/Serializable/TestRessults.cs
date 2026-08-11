using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Serializable
{
    public class TestResults
    {
        [XmlElement("ResultSetDescriptor", Form = XmlSchemaForm.Unqualified, Type = typeof(ResultSetDescriptor))]
        public ResultSetDescriptor ResultDescriptor { get; set; } = new();

        [XmlArray]
        [XmlArrayItem("ResultSet", Form = XmlSchemaForm.Unqualified, Type = typeof(ResultSetEntry))]
        public List<ResultSetEntry> ResultSets { get; set; } = new List<ResultSetEntry>();
    }

    /// <summary>
    /// Represents an entry in the result set, containing information about the result data and its 
    /// associated table of contents (TOC) entry.
    /// </summary>
    public class ResultSetEntry
    {
        [XmlAttribute("ResultId", Form = XmlSchemaForm.Unqualified)]
        public int ResultId { get; set; } = 0;

        [XmlArray("TOC")]
        [XmlArrayItem("TOCEntry", Form = XmlSchemaForm.Unqualified, Type = typeof(TOCEntry))]
        public TOCEntry TOCEntry { get; set; } = new();

        [XmlElement("ResultData", Form = XmlSchemaForm.Unqualified)]
        public string ResultData { get; set; } = string.Empty;

        [XmlElement("AdminTime", Form = XmlSchemaForm.Unqualified)]
        public string AdminTime { get; set; } = string.Empty;
    }

    public class TOCEntry
    {
        [XmlElement("KeyOffset", Form = XmlSchemaForm.Unqualified)]
        public long KeyOffset { get; set; } = 0;

        [XmlElement("KeyLength", Form = XmlSchemaForm.Unqualified)]
        public int KeyLength { get; set; } = 0;

        [XmlElement("IVOffset", Form = XmlSchemaForm.Unqualified)]
        public long IVOffset { get; set; } = 0;

        [XmlElement("IVLength", Form = XmlSchemaForm.Unqualified)]
        public int IVLength { get; set; } = 0;

        [XmlElement("DataOffset", Form = XmlSchemaForm.Unqualified)]
        public long DataOffset { get; set; } = 0;

        [XmlElement("DataLength", Form = XmlSchemaForm.Unqualified)]
        public long DataLength { get; set; } = 0;
    }

}
