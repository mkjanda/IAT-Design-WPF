using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents a mock item instruction screen, which is used for testing purposes to simulate the behavior of an instruction screen in the application.
/// </summary>
[XmlRoot("MockItemInstructionScreen")]
public class MockItemInstructionScreen 
{
    /// <summary>
    /// THe key the user must depress to continue past the instruction screen. This is used to test the functionality of 
    /// key-based interactions in the application, ensuring that the correct key press allows the user to proceed as expected.
    /// </summary>
    [XmlElement("ContinueASCIIKeyCode", Form = XmlSchemaForm.Unqualified)]
    public int ContinueASCIIKeyCode { get; set; } = -1;

    /// <summary>
    /// Gets or sets the display identifier for continue instructions.
    /// </summary>
    [XmlElement("ContinueInstructionsDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int ContinueInstructionsDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier used to display the left response.
    /// </summary>
    [XmlElement("LeftResponseDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int LeftResponseDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier used to display the right response.
    /// </summary>
    [XmlElement("RightResponseDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int RightResponseDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier of the display used to present the stimulus.
    /// </summary>
    /// <remarks>A value of -1 typically indicates that no display has been assigned. Set this property to
    /// specify which display should be used for stimulus presentation in multi-display environments.</remarks>
    [XmlElement("StimulusDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int StimulusDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets the identifier for the instructions display associated with this instance.
    /// </summary>
    [XmlElement("InstructionsDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int InstructionsDisplayID { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether an error mark is currently displayed.
    /// </summary>
    [XmlElement("ErrorMarkIsDisplayed", Form = XmlSchemaForm.Unqualified)]
    public bool ErrorMarkIsDisplayed { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the left outline response is enabled.
    /// </summary>
    [XmlElement("OutlineLeftResponse", Form = XmlSchemaForm.Unqualified)]
    public bool OutlineLeftResponse { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the right outline response is enabled.
    /// </summary>
    [XmlElement("OutlineRightResponse", Form = XmlSchemaForm.Unqualified)]
    public bool OutlineRightResponse { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the MockItemInstructionScreen class.
    /// </summary>
    public MockItemInstructionScreen()
    {
    }
}

