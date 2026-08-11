using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Serializable
{
    internal class ResultTOC
    {
        [XmlAttribute("NumEntries", Form = XmlSchemaForm.Unqualified)]
        public int NumEntries { get; set; } = 0;

        [XmlArray]
        [XmlArrayItem("ResultTOCEntry", Form = XmlSchemaForm.Unqualified)]
        public List<TOCEntry> Entries { get; set; } = new List<TOCEntry>();
    }

}
