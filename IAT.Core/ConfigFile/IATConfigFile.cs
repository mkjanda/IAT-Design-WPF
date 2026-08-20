using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Text;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the configuration settings for an Implicit Association Test (IAT) application, including survey details, server information, response 
/// keys, and display settings. This class is designed to be serialized to and deserialized from XML format, allowing for easy storage and retrieval 
/// of configuration data. The ConfigFile class encapsulates all necessary parameters to configure the behavior and appearance of the IAT application, 
/// making it a central component for managing application settings in a structured manner.
/// </summary>
[XmlRoot("ConfigFile]")]
public class IATConfigFile
{
    /// <summary>
    /// Gets or sets the width of the slide, in pixels.
    /// </summary>
    [XmlIgnore]
    public int SlideWidth => 500;

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
    public string Name { get; set; } = string.Empty;

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
    [XmlElement("IsSevenBlock", Form = XmlSchemaForm.Unqualified)]
    public bool Is7Block { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL to which the user is redirected after the operation completes.
    /// </summary>
    [XmlElement("RedirectOnComplete", Form = XmlSchemaForm.Unqualified)]
    public string RedirectOnComplete { get; set; } = "https://iatsoftware.net";

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
    /// Gets or sets the identifier for the error mark associated with this instance.
    /// </summary>
    [XmlElement("ErrorMarkID", Form = XmlSchemaForm.Unqualified)]
    public int ErrorMarkID { get; set; } = 1;

    /// <summary>
    /// Gets or sets the identifier for the left key outline.
    /// </summary>
    [XmlElement("LeftKeyOutlineID", Form = XmlSchemaForm.Unqualified)] 
    public int LeftKeyOutlineID { get; set; } = 2;

    /// <summary>
    /// Gets or sets the identifier for the right key outline.
    /// </summary>
    [XmlElement("RightKeyOutlineID", Form = XmlSchemaForm.Unqualified)]
    public int RightKeyOutlineID { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether self-alternating surveys should be prefixed.
    /// </summary>
    [XmlElement("PrefixSelfAlternatingSurveys", Form = XmlSchemaForm.Unqualified)]
    public bool PrefixSelfAlternatingSurveys { get; set; } = false;

    /// <summary>
    /// Gets or sets the collection of surveys associated with this instance.
    /// </summary>
    [XmlArray("Surveys")]
    [XmlArrayItem("Survey", Form = XmlSchemaForm.Unqualified, Type = typeof(Survey))]
    public List<Survey> Surveys { get; set; } = new List<Survey>();

    /// <summary>
    /// Gets or sets the layout configuration for this instance.
    /// </summary>
    [XmlElement("Layout", Form = XmlSchemaForm.Unqualified, Type = typeof(Layout))]
    public Layout Layout { get; set; } = new Layout();

    /// <summary>
    /// Gets or sets the collection of events associated with this instance. Each event can be of various types, such as BeginIATBlock, EndIATBlock, 
    /// KeyedInstructionScreen, MockItemInstructionScreen, TextInstructionScreen, or Trial. This property is designed to hold a list of events that define the sequence and behavior of the IAT process.
    /// </summary>
    [XmlElement("BeginIATBlock", Form = XmlSchemaForm.Unqualified, Type = typeof(BeginIATBlock))]
    [XmlElement("EndIATBlock", Form = XmlSchemaForm.Unqualified, Type = typeof(EndIATBlock))]
    [XmlElement("KeyedInstructionScreen", Form = XmlSchemaForm.Unqualified, Type = typeof(KeyedInstructionScreen))]
    [XmlElement("MockItemInstructionScreen", Form = XmlSchemaForm.Unqualified, Type = typeof(MockItemInstructionScreen))]
    [XmlElement("TextInstructionScreen", Form = XmlSchemaForm.Unqualified, Type = typeof(TextInstructionScreen))]
    [XmlElement("Trial", Form = XmlSchemaForm.Unqualified, Type = typeof(Trial))]
    public List<Event> EventList { get; set; } = new List<Event>();

    /// <summary>
    /// Gets or sets the unique response item associated with this instance.
    /// </summary>
    [XmlElement("UniqueResponse", Form = XmlSchemaForm.Unqualified, Type = typeof(UniqueResponseItem), IsNullable = true)]
    public UniqueResponseItem? UniqueResponseItem { get; set; } = null;

    /// <summary>
    /// Gets or sets the collection of display items to be serialized or deserialized as part of the DisplayItemList XML
    /// element.
    /// </summary>
    [XmlArrayItem("DisplayItem", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(DisplayItem))]
    public List<DisplayItem> DisplayItems { get; set; } = new List<DisplayItem>();
}
