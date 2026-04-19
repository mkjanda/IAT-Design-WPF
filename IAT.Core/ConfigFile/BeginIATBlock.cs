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
[XmlRoot(ElementName = "BeginIATBlock")]
public sealed class BeginIATBlock : Event
{
    /// <summary>
    /// The type of event that is being represented. This is used to determine how to process the event and what properties it may have.
    /// </summary>
    [XmlElement("EventType", Form = XmlSchemaForm.Unqualified, Type = typeof(EventType))]
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
    public int BlockNum { get; set; }

    /// <summary>
    /// The number of stimuli in the block
    /// </summary>
    [XmlElement("NumItems", Form = XmlSchemaForm.Unqualified)]
    public int NumItems { get; set; } = 0;

    /// <summary>
    /// The Display ID of the instructions area of the block
    /// </summary>
    [XmlElement("InstructionsDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int InstructionsDisplayID { get; set; } = 0;

    /// <summary>
    /// The display ID of the left response key
    /// </summary>
    [XmlElement("LeftResponseDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int LeftResponseDisplayID { get; set; } = 0;

    /// <summary>
    /// The display ID of the right response key
    /// </summary>
    [XmlElement("RightResponseDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int RightResponseDisplayID { get; set; } = 0;

    /// <summary>
    /// The no-arg consstructor for the object
    /// </summary>
    public BeginIATBlock() { }
}
