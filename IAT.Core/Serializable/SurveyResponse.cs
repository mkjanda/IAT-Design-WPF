using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;


namespace IAT.Core.Serializable
{
    public class SurveyResponse
    {
        [XmlArray("ItemResponses")]
        [XmlArrayItem("ItemResponse")]
        public List<string> ItemResponses { get; set; } =  new List<string>();
    }
}
