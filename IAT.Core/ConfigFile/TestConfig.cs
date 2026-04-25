using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Text;
using com.sun.tools.corba.se.idl;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the configuration settings for an Implicit Association Test (IAT) application, including survey details, server information, response 
/// keys, and display settings. This class is designed to be serialized to and deserialized from XML format, allowing for easy storage and retrieval 
/// of configuration data. The ConfigFile class encapsulates all necessary parameters to configure the behavior and appearance of the IAT application, 
/// making it a central component for managing application settings in a structured manner.
/// </summary>
[XmlRoot("ConfigFile]")]
public class TestConfig
{
    /// <summary>
    /// Gets or sets the number of surveys to be conducted before the main survey sequence begins.
    /// </summary>
    [XmlAttribute("NumBeforeSurveys")]
    public int NumBeforeSurveys { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of surveys to be conducted after the initial phase.
    /// </summary>
    [XmlAttribute("NumAfterSurveys")]
    public int NumAfterSurveys { get; set; } = 0;

    /// <summary>
    /// Gets or sets the version number of the result data format.
    /// </summary>
    [XmlAttribute("ResultDataVersion")]
    public int ResultDataVersion { get; set; } = 4;

    /// <summary>
    /// Gets or sets the name of the IAT (Item Analysis Tool) associated with this instance.
    /// </summary>
    [XmlElement("IATName", Form = XmlSchemaForm.Unqualified)]
    public string IATName { get; set; } = string.Empty;

    /// <summary>
    /// The author of the IAT, which can be used for display purposes or to provide metadata about the test configuration. 
    /// This property is optional and can be left empty if not applicable.
    /// </summary>
    [XmlElement("Author", Form = XmlSchemaForm.Unqualified)]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the domain name of the server to which the application connects.
    /// </summary>
    [XmlElement("ServerDomain", Form = XmlSchemaForm.Unqualified)]
    public string ServerDomain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server path associated with this instance.
    /// </summary>
    [XmlElement("ServerPath", Form = XmlSchemaForm.Unqualified)]
    public string ServerPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port number used by the server for incoming connections.
    /// </summary>
    [XmlElement("ServerPort", Form = XmlSchemaForm.Unqualified)]
    public int ServerPort { get; set; } = 80;

    /// <summary>
    /// Gets or sets the unique identifier for the client.
    /// </summary>
    [XmlElement("ClientID", Form = XmlSchemaForm.Unqualified)]
    public long ClientID { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of IAT items.
    /// </summary>
    [XmlElement("NumIATItems", Form = XmlSchemaForm.Unqualified)]
    public int NumIATItems { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the 7-block feature is enabled.
    /// </summary>
    [XmlElement("Is7Block", Form = XmlSchemaForm.Unqualified)]
    public bool Is7Block { get; set; } = false;

    /// <summary>
    /// Gets or sets the URL to which the user is redirected after the operation completes.
    /// </summary>
    [XmlElement("RedirectOnComplete", Form = XmlSchemaForm.Unqualified)]
    public string RedirectOnComplete { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character key used to indicate a left response.
    /// </summary>
    [XmlElement("LeftResponseKey", Form = XmlSchemaForm.Unqualified)]
    public char LeftResponseKey { get; set; } = 'E';

    /// <summary>
    /// Gets or sets the key character used to indicate a correct or affirmative response on the right side.
    /// </summary>
    [XmlElement("RightResponseKey", Form = XmlSchemaForm.Unqualified)]
    public char RightResponseKey { get; set; } = 'I';

    /// <summary>
    /// Gets or sets the type of randomization to apply.
    /// </summary>
    [XmlElement("RandomizationType", Form = XmlSchemaForm.Unqualified)]
    public string RandomizationType { get; set; } = "Full";

    /// <summary>
    /// Gets or sets the identifier for the error mark associated with this instance.
    /// </summary>
    [XmlElement("ErrorMarkID", Form = XmlSchemaForm.Unqualified)]
    public int ErrorMarkID { get; set; } = 0;

    /// <summary>
    /// Gets or sets the identifier for the left key outline.
    /// </summary>
    [XmlElement("LeftKeyOutlineID", Form = XmlSchemaForm.Unqualified)] 
    public int LeftKeyOutlineID { get; set; } = 0;

    /// <summary>
    /// Gets or sets the identifier for the right key outline.
    /// </summary>
    [XmlElement("RightKeyOutlineID", Form = XmlSchemaForm.Unqualified)]
    public int RightKeyOutlineID { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether self-alternating surveys should be prefixed.
    /// </summary>
    [XmlElement("PrefixSelfAlternatingSurveys", Form = XmlSchemaForm.Unqualified)]
    public bool PrefixSelfAlternatingSurveys { get; set; } = false;

    /// <summary>
    /// Gets or sets the layout configuration for this instance.
    /// </summary>
    [XmlElement("Layout", Form = XmlSchemaForm.Unqualified, Type = typeof(Layout))]
    public Layout Layout { get; set; }

    /// <summary>
    /// Gets or sets the collection of surveys as base64-encoded XML strings.
    /// </summary>
    /// <remarks>Each string in the collection represents a serialized survey object encoded in base64.
    /// Setting this property replaces the current collection of surveys with those deserialized from the provided
    /// base64-encoded XML strings.</remarks>
    [XmlArray]
    [XmlArrayItem("SurveyB64Xml", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public List<String> SurveyB64Xml
    {
        get
        {
            var results = new List<string>();
            XmlSerializer ser = new XmlSerializer(typeof(Survey));
            var strWriter = new StringWriter();
            Surveys.ForEach((survey) =>
            {
                ser.Serialize(strWriter, survey);
                results.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(strWriter.ToString())));
            });
            return results;
        }
        set
        {
            Surveys.Clear();
            XmlSerializer ser = new XmlSerializer(typeof(Survey));
            foreach (var b64Survey in value)
            {
                var xml = Encoding.UTF8.GetString(Convert.FromBase64String(b64Survey));
                using var strReader = new StringReader(xml);
                if (ser.Deserialize(strReader) is Survey survey)
                    Surveys.Add(survey);
            }
        }
    }

    /// <summary>
    /// Gets or sets the collection of surveys associated with this instance.
    /// </summary>
    [XmlIgnore]
    public List<Survey> Surveys { get; set; } = new List<Survey>();

    /// <summary>
    /// Gets or sets the unique response item associated with this instance.
    /// </summary>
    [XmlElement("UniqueResponse", Form = XmlSchemaForm.Unqualified, Type = typeof(UniqueResponseItem), IsNullable = true)]
    public UniqueResponseItem? UniqueResponseItem { get; set; } = null;

    /// <summary>
    /// Gets or sets the collection of events to be serialized or deserialized as part of the containing object.
    /// </summary>
    /// <remarks>The collection is serialized as an XML array named "EventList", with each event represented
    /// as an "Event" element. The property is initialized to an empty list by default.</remarks>
    [XmlArray("EventList")]
    [XmlArrayItem("Event", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Event))]
    public List<Event> EventList { get; set; } = new List<Event>();

    /// <summary>
    /// Gets or sets the collection of display items to be serialized or deserialized as part of the DisplayItemList XML
    /// element.
    /// </summary>
    [XmlArray("DisplayItemList")]
    [XmlArrayItem("DisplayItem", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(DisplayItem))]
    public List<DisplayItem> DisplayItemList { get; set; } = new List<DisplayItem>();

    /// <summary>
    /// 
    /// </summary>
    public TestConfig() { }
}
