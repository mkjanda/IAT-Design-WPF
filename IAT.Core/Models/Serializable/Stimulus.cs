using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace IAT.Core.Models.Serializable
{
    public abstract class Stimulus
    {
        [XmlIgnore]
        protected static int ItemNumCounter = 0;
        [XmlElement] 
        protected int ItemNum { get; set; } = ++ItemNumCounter;
        [XmlElement] required public int BlockNum { get; set; }
        [XmlElement] required public int StimulusDisplayID { get; set; }
        [XmlElement] required public int OrignatingBlockNum { get; set; }



    }
}
