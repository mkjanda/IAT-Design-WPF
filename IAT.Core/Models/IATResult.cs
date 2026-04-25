using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Models
{
    [XmlRoot("IATResultSetElement")]
    public class IATResult
    {
        [XmlElement("BlockNumber", Form = XmlSchemaForm.Unqualified)]
        public int BlockNumber { get; set; }
        [XmlElement("ItemNumber", Form = XmlSchemaForm.Unqualified)]    
        public int ItemNumber { get; set; }
        [XmlElement("ResponseTime", Form = XmlSchemaForm.Unqualified)]
        public int ResponseTime { get; set; }
        [XmlElement("Error", Form = XmlSchemaForm.Unqualified)]         
        public bool Error { get; set; } = false;
        [XmlElement("PresentationNumber", Form = XmlSchemaForm.Unqualified)]
        public int PresentationNumber { get; set; } = 0;

    }
}