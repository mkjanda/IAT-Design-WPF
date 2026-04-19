using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.ConfigFile;

/// <summary>
/// KeyedInstructionScreen represents a specific type of instruction screen that includes key-based responses for user interaction.
/// </summary>
public sealed class KeyedInstructionScreen
{
    /// <summary>
    /// Gets or sets the ASCII key code that represents the 'Continue' action.
    /// </summary>
    [XmlElement("ContinueASCIIKeyCode", Form = XmlSchemaForm.Unqualified)]
    public int ContinueASCIIKeyCode { get; set; } = -1;

    /// <summary>
    /// Gets or sets the display identifier for the continue instructions.
    /// </summary>
    [XmlElement("ContinueInstructionsDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int ContinueInstructionsDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the display identifier for the left response.
    /// </summary>
    [XmlElement("LeftResponseDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int LeftResponseDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier for the right response display.
    /// </summary>
    [XmlElement("RightResponseDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int RightResponseDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier for the instructions display associated with this instance.
    /// </summary>
    [XmlElement("InstructionsDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int InstructionsDisplayID { get; set; } = -1;

    /// <summary>
    /// Initializes a new instance of the KeyedInstructionScreen class.
    /// </summary>
    public KeyedInstructionScreen() { }
}
