using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// An abstract base class representing an event in the IAT (Implicit Association Test) application. This class serves as a 
/// foundation for specific event types, providing a common structure and properties that can be extended by derived classes 
/// to represent various events that occur during the IAT process. Each event type will have its own specific properties and 
/// behaviors, but they will all share the common EventType property defined in this base class, which is used to identify the 
/// type of event being represented.
/// </summary>
public abstract class Event
{
    /// <summary>
    /// The type of event that is being represented. This is used to determine how to process the event and what properties it may have.
    /// </summary>
    [XmlElement("EventType", Form = XmlSchemaForm.Unqualified)]
    public abstract EventType EventType { get; }

    /// <summary>
    /// No argument  constructor for IAT Event object
    /// </summary>
    public Event()
    {
    }

}
