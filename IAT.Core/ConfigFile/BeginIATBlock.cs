using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the event that marks the beginning of an IAT (Implicit Association Test) block. This event contains properties 
/// that define the characteristics of the IAT block, such as the number of presentations, alternation settings, block number, 
/// number of items, and display IDs for instructions and response options. This class is used to configure the structure and 
/// content of an IAT block within the application, allowing for flexible setup of the test parameters and presentation details. 
/// The properties are decorated with XML serialization attributes to facilitate easy serialization and deserialization from XML 
/// format when configuring the IAT blocks.
/// </summary>
[XmlType("BeginIATBlock")]
public sealed class BeginIATBlock : Event
{
    /// <summary>
    /// The type of event that is being represented. This is used to determine how to process the event and what properties it may have.
    /// </summary>
    [XmlIgnore]
    public override EventType EventType => EventType.BeginIATBlock;

    /// <summary>
    /// The number of presentations for the IAT block. This property specifies how many times the stimuli in the block will be presented to the participant.
    /// </summary>
    [XmlElement("NumPresentations", Form = XmlSchemaForm.Unqualified)]
    public int NumPresentations { get; set; }

    /// <summary>
    /// The block this block is alternated with. This property specifies the block number that this block will alternate with during the presentation of the IAT.
    /// </summary>
    [XmlElement("AlternatedWith", Form = XmlSchemaForm.Unqualified)]
    public int AlternatedWith { get; set; }

    /// <summary>
    /// The 1-based index of the block. This property specifies the block number for this IAT block, which is used to identify and organize the blocks within the test structure.
    /// </summary>
    [XmlElement("BlockNum", Form = XmlSchemaForm.Unqualified)]
    public int BlockNumber { get; set; }

    /// <summary>
    /// The number of instruction screens for the block. This property specifies how many instruction screens will be displayed to the participant before the stimuli presentations begin.
    /// </summary>
    [XmlElement("NumInstructionScreens", Form = XmlSchemaForm.Unqualified)]
    public int NumInstructionScreens { get; set; } = 0;

    /// <summary>
    /// The number of stimuli in the block
    /// </summary>
    [XmlElement("NumItems", Form = XmlSchemaForm.Unqualified)]
    public int NumItems { get; set; } = 0;

    /// <summary>
    /// The XML export form of the instructions display ID. This property is used to uniquely identify the instructions display in the exported configuration, allowing for proper mapping and retrieval of the instructions during the test execution.
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
    /// The Display ID of the instructions area of the block
    /// </summary>
    [XmlIgnore]
    public Guid InstructionsId { get; set; } = Guid.Empty;

    /// <summary>
    /// The XML export form of the left response display ID. This property is used to uniquely identify the left response display in the exported configuration, allowing for proper mapping and retrieval of the left response during the test execution.
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
    /// The Display ID of the left response area of the block
    /// </summary>
    [XmlIgnore]
    public Guid LeftResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// The XML export form of the right response display ID. This property is used to uniquely identify the right response display in the exported configuration, allowing for proper mapping and retrieval of the right response during the test execution.
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
    /// The Display ID of the right response area of the block
    /// </summary>
    [XmlIgnore]
    public Guid RightResponseId { get; set; } = Guid.Empty;
}
