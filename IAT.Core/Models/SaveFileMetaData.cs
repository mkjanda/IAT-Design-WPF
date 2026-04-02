using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Services;

namespace IAT.Core.Models
{
    internal class SaveFileMetaData
    {
        /// <summary>
        /// Gets the date and time, in Coordinated Universal Time (UTC), when the object was opened.
        /// </summary>
        [XmlElement("TimeOpened")]
        public DateTime TimeOpened { get; private set; } = DateTime.UtcNow;

        [XmlIgnore]
        public ConcurrentDictionary<String, List<int>> UriCounters { get; private set; } = new ConcurrentDictionary<String, List<int>>();

        /// <summary>
        /// Represents a serializable container for a URI and its associated consumed integer values.
        /// </summary>
        /// <remarks>This class is intended for XML serialization scenarios where a URI and a collection
        /// of related integer values need to be persisted or transferred together. The class is typically used as a
        /// data transfer object and is not intended for direct manipulation outside of serialization
        /// contexts.</remarks>
        public class UriCounterSerialiable {
            [XmlElement("Uri")]
            public String Uri { get; set; }

            [XmlArray("ConsumedValues")]
            [XmlArrayItem("ConsumedValue")]
            public List<int> ConsumedValues { get; set; }
        }

        /// <summary>
        /// Gets or sets the collection of serializable URI counters for persistence or data transfer.
        /// </summary>
        /// <remarks>This property is typically used to serialize or deserialize the state of URI
        /// counters, enabling storage or communication of usage data. Modifying this property updates the underlying
        /// URI counters accordingly.</remarks>
        [XmlArray("UriCountersSerializable")]
        [XmlArrayItem("UriCounterSerializable")]
        public List<UriCounterSerialiable> UriCountersSerializable {
            get
            {
                List<UriCounterSerialiable> list = new List<UriCounterSerialiable>();
                foreach (String key in UriCounters.Keys)
                {
                    list.Add(new UriCounterSerialiable() { Uri = key, ConsumedValues = UriCounters[key] });
                }
                return list;
            }
            set
            {
                if (value != null)
                {
                    foreach (UriCounterSerialiable item in value)
                    {
                        UriCounters[item.Uri] = item.ConsumedValues;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the collection of history entries associated with this instance.   
        /// </summary>
        /// <remarks>The returned list is read-only from outside the class. Entries are added internally
        /// to track changes or actions over time. The order of entries reflects the sequence in which they were
        /// recorded.</remarks>
        [XmlArray("HistoryEntries")]
        [XmlArrayItem("HistoryEntry")]
        public List<HistoryEntry> History { get; private set; } = new List<HistoryEntry>();

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Uri")]
        public Uri Uri { get; set; }
        public String MimeType { get { return "text/xml+" + typeof(SaveFileMetaData).ToString(); } }

        [XmlElement("IATRelId")]
        public String IATRelId { get; set; } = String.Empty;


        public required ISaveFileService _saveFileService;
        public required Version Version { get; init; } 
        public SaveFileMetaData(ISaveFileService saveFileService)
        {
            Version = new Version(saveFileService.CurrentVersion);
            Uri = PackUriHelper.CreatePartUri(new Uri(typeof(SaveFileMetaData).ToString() + "/" + typeof(SaveFileMetaData).ToString() + "1.xml", UriKind.Relative));
            saveFile.SavePackage.CreatePart(URI, MimeType, CompressionOption.Normal);
            UriCounters[typeof(SaveFileMetaData).ToString()] = new List<int>(new int[] { 1 });
            saveFile.CreatePackageLevelRelationship(URI, typeof(SaveFileMetaData));
            SaveFile = saveFile;
        }

        public SaveFileMetaData(SaveFile saveFile, Uri u)
        {
            SaveFile = saveFile;
            this.URI = u;
            UriCounters[typeof(SaveFileMetaData).ToString()] = new List<int>(new int[] { 1 });
            Load(SaveFile.SavePackage);
        }


        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("MetaData"));
            xDoc.Root.Add(new XElement("IATRelId", IATRelId));
            new HistoryEntry().AddToXml(xDoc.Root);
            foreach (HistoryEntry hist in History)
                hist.AddToXml(xDoc.Root);
            XElement xElem = new XElement("UriCounters");
            foreach (String t in UriCounters.Keys)
            {
                XElement uriElem = new XElement("UriCounter", new XAttribute("Type", t.ToString()));
                foreach (int i in UriCounters[t])
                    uriElem.Add(new XElement("ConsumedValue", i.ToString()));
                xElem.Add(uriElem);
            }
            xDoc.Root.Add(xElem);
            Stream s = Stream.Synchronized(SaveFile.GetWriteStream(this));
            try
            {
                xDoc.Save(s);
            }
            finally
            {
                s.Dispose();
                SaveFile.ReleaseWriteStreamLock();
            }
        }

        public void Load(Package savePackage)
        {
            Stream s = Stream.Synchronized(SaveFile.GetReadStream(this));
            XDocument xDoc = null;
            try
            {
                xDoc = XDocument.Load(s);
                s.Dispose();
            }
            finally
            {
                SaveFile.ReleaseReadStreamLock();
            }
            IATRelId = xDoc.Root.Element("IATRelId").Value;
            foreach (XElement elem in xDoc.Root.Elements("HistoryEntry"))
                History.Add(new HistoryEntry(elem));
            Version = new CVersion(History.First().Version);
            foreach (XElement elem in xDoc.Root.Element("UriCounters").Elements())
            {
                UriCounters[elem.Attribute("Type").Value] = new List<int>();
                foreach (XElement consumedVal in elem.Elements("ConsumedValue"))
                    UriCounters[elem.Attribute("Type").Value].Add(Convert.ToInt32(consumedVal.Value));
            }
        }

        public void Dispose()
        {
            UriCounters.Clear();
        }
    }

}
}
