using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents a screen of text instructions that can be displayed to the user during the IAT process. This class inherits from the Event base class, 
/// indicating that it is a specific type of event that can occur within the application. The TextInstructionScreen class includes properties for defining 
/// the key code that allows the user to continue past the instruction screen, as well as identifiers for the displays used to show the instructions and any 
/// associated continue instructions. This class is designed to facilitate the presentation of textual instructions to users in a structured and configurable 
/// manner, allowing for flexibility in how instructions are displayed and interacted with during the IAT process.
/// </summary>
[XmlRoot("TextInstructionScreen")]
public class TextInstructionScreen : Event
{
    /// <summary>
    /// Gets the event type associated with this screen.
    /// </summary>
    public override EventType EventType => EventType.TextInstructionScreen;


    /// <summary>
    /// Gets or sets the ASCII key code that represents the 'Continue' action.
    /// </summary>
    /// <remarks>A value of -1 indicates that no key code is assigned. Set this property to the ASCII code of
    /// the desired key to enable the 'Continue' action via keyboard input.</remarks>
    [XmlElement("ContinueASCIIKeyCode", Form = XmlSchemaForm.Unqualified)]
    public int ContinueASCIIKeyCode { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier used to display continue instructions.
    /// </summary>
    [XmlElement("ContinueInstructionsDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int ContinueInstructionsDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier used to display instructions.
    /// </summary>
    [XmlElement("InstructionsDisplayID", Form = XmlSchemaForm.Unqualified)] 
    public int InstructionsDisplayID { get; set; } = -1;

    /// <summary>
    /// Initializes a new instance of the TextInstructionScreen class.
    /// </summary>
    public TextInstructionScreen() { }
}
