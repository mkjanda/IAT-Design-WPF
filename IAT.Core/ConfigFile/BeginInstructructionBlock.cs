using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the start of a block of instruction screens. This event contains properties that define the number of 
/// instruction screens in the block and the alternation settings for the instruction screens. This class is used to 
/// configure the structure and content of a block of instruction screens within the application, allowing for flexible 
/// setup of the instruction presentation details. The properties are decorated with XML serialization attributes to facilitate 
/// easy serialization and deserialization from XML format when configuring the instruction blocks.
/// </summary>
[XmlRoot("BeginInstructionBlock")]
public sealed class BeginInstructionBlock
{
    /// <summary>
    /// Represents the event type for the beginning of an instruction block. This property is used to identify the type of event and determine how to process it within the application.
    /// </summary>
    [XmlElement("EventType", Form = XmlSchemaForm.Unqualified, Type = typeof(EventType))]
    public EventType EventType => EventType.BeginInstructionBlock;

    /// <summary>
    /// Represents the number of instruction screens in the block. This property specifies how many instruction screens will be presented to the participant as part of this instruction block.
    /// </summary>
    [XmlElement("NumInstructionScreens", Form = XmlSchemaForm.Unqualified)]
    public int NumInstructionScreens { get; set; }

    /// <summary>
    /// The number of the block this block is alternated with. This property specifies the block number that this instruction block will 
    /// alternate with during the presentation of the instructions.
    /// </summary>
    [XmlElement("AlternatedWith", Form = XmlSchemaForm.Unqualified)]
    public int AlternaatedWidth { get; set; }
}
