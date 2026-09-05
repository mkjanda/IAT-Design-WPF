using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents a mock item instruction screen, which is used for testing purposes to simulate the behavior of an instruction screen in the application.
/// </summary>
[XmlType("MockItemInstructionScreen")]
public class MockItemInstructionScreen : Event
{
    /// <summary>
    /// THe key the user must depress to continue past the instruction screen. This is used to test the functionality of 
    /// key-based interactions in the application, ensuring that the correct key press allows the user to proceed as expected.
    /// </summary>
    [XmlElement("ContinueASCIIKeyCode", Form = XmlSchemaForm.Unqualified)]
    public int ContinueASCIIKeyCode { get; set; } = 32;


    /// <summary>
    /// The XML export form of the continue instructions identifier, which is used to uniquely identify the continue instructions display in the exported configuration.
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
    /// Gets or sets the display identifier for continue instructions.
    /// </summary>
    [XmlIgnore]
    public Guid ContinueInstructionsId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets the XML export form of the left response identifier, which is used to uniquely identify the left response display in the exported configuration.
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
    /// Gets or sets the identifier used to display the left response.
    /// </summary>
    [XmlIgnore]
    public Guid LeftResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the identifier used to display the right response.
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
    /// Gets or sets the identifier used to display the right response.
    /// </summary>
    [XmlIgnore]
    public Guid RightResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets the XML export form of the stimulus identifier, which is used to uniquely identify the stimulus display in the exported configuration.
    /// </summary>
    [XmlElement("Stimulus", Form = XmlSchemaForm.Unqualified)]
    public string Stimulus
    {
        get
        {
            return StimulusId.ToString("N");
        }
        set
        {
            StimulusId = string.IsNullOrEmpty(value) ?
                Guid.Empty : Guid.ParseExact(value, "N");
        }
    }



        /// <summary>
        /// Gets or sets the identifier of the display used to present the stimulus.
        /// </summary>
        /// <remarks>A value of -1 typically indicates that no display has been assigned. Set this property to
        /// specify which display should be used for stimulus presentation in multi-display environments.</remarks>
        [XmlIgnore]
    public Guid StimulusId { get; set; } = Guid.Empty;


    /// <summary>
    /// Gets the XML export form of the instructions identifier, which is used to uniquely identify the instructions display in the exported configuration.
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
    /// The type of event associated with this instruction screen, which is used to identify the specific behavior and handling 
    /// logic for this type of screen within the application. This property is essential for ensuring that the correct processing 
    /// occurs when this event is triggered during testing scenarios.
    /// </summary>
    public override EventType EventType { get; } = EventType.MockItemInstructionScreen;

    /// <summary>
    /// Initializes a new instance of the MockItemInstructionScreen class.
    /// </summary>
    public MockItemInstructionScreen()
    {
    }
}

