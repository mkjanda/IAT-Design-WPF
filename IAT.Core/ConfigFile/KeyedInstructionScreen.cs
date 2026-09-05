using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// KeyedInstructionScreen represents a specific type of instruction screen that includes key-based responses for user interaction.
/// </summary>
[XmlType("KeyedInstructionScreen")]
public sealed class KeyedInstructionScreen : Event
{
    /// <summary>
    /// Gets or sets the ASCII key code that represents the 'Continue' action.
    /// </summary>
    [XmlElement("ContinueASCIIKeyCode", Form = XmlSchemaForm.Unqualified)]
    public int ContinueASCIIKeyCode { get; set; } = 32;

    /// <summary>
    /// Gets the XML export form of the continue instructions identifier.
    /// </summary>
    [XmlElement("ContinueInstructions", Form = XmlSchemaForm.Unqualified)]
    public string ContinueInstructions
    {
        get
        {
            return ContinueInstructionsId.ToString("N");
        }
        set
        {
            ContinueInstructionsId = string.IsNullOrEmpty(value) ?
                Guid.Empty : Guid.ParseExact(value, "N");
        }
    }

    /// <summary>
    /// Gets or sets the display identifier for the continue instructions.
    /// </summary>
    [XmlIgnore]
    public Guid ContinueInstructionsId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets the XML export form of the left response identifier.
    /// </summary>
    [XmlElement("LeftResponse", Form = XmlSchemaForm.Unqualified)]
    public string LeftResponse
    {
        get
        {
            return LeftResponseId.ToString("N");
        }
        set
        {
            LeftResponseId = string.IsNullOrEmpty(value) ?
                Guid.Empty : Guid.ParseExact(value, "N");
        }
    }

    /// <summary>
    /// Gets or sets the display identifier for the left response.
    /// </summary>
    [XmlIgnore]
    public Guid LeftResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets the XML export form of the right response identifier.
    /// </summary>
    [XmlElement("RightResponse", Form = XmlSchemaForm.Unqualified)]
    public string RightResponse
    {
        get
        {
            return RightResponseId.ToString("N");
        }
        set
        {
            RightResponseId = string.IsNullOrEmpty(value) ?
                Guid.Empty : Guid.ParseExact(value, "N");
        }
    }

    /// <summary>
    /// Gets or sets the identifier for the right response display.
    /// </summary>
    [XmlIgnore]
    public Guid RightResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets the XML export form of the instructions identifier.
    /// </summary>
    [XmlElement("Instructions", Form = XmlSchemaForm.Unqualified)]
    public string Instructions
    {
        get
        {
            return InstructionsId.ToString("N");
        }
        set
        {
            InstructionsId = string.IsNullOrEmpty(value) ?
                Guid.Empty : Guid.ParseExact(value, "N");
        }
    }

    /// <summary>
    /// Gets or sets the identifier for the instructions display associated with this instance.
    /// </summary>
    [XmlIgnore]
    public Guid InstructionsId { get; set; } = Guid.Empty;

    /// <summary>
    /// The type of event this class represents,
    /// </summary>
    public override EventType EventType { get; } = EventType.KeyedInstructionScreen;

    /// <summary>
    /// Initializes a new instance of the KeyedInstructionScreen class.
    /// </summary>
    public KeyedInstructionScreen() { }
}
