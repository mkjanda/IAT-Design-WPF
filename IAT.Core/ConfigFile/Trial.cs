using IAT.Core.Enumerations;
using java.awt;using System;
using System.Collections;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents a single trial event in the IAT application, encapsulating information such as identifiers, block and
/// item numbers, stimulus display, and keyed direction.
/// </summary>
/// <remarks>The Trial class is used to model individual trial events within an IAT session. It provides
/// properties for uniquely identifying the trial, associating it with a block and item, and specifying the direction of
/// key input and stimulus display. Implements equality comparison based on item and block numbers, which is useful for
/// collection operations and deduplication scenarios.</remarks>
[XmlRoot("IATItem")]
public class Trial : Event, IEqualityComparer<Trial>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Returns the event type associated with this instance, which is used to identify the specific type of event being represented. 
    /// In this case, it returns EventType.Trial, indicating that this class represents a trial event in the IAT application.
    /// </summary>
    [XmlElement("EventType", Form = XmlSchemaForm.Unqualified)])]
    public override EventType EventType => EventType.Trial;

    /// <summary>
    /// Returns a hash code for the specified Trial object based on its ItemNum and BlockNum properties.
    /// </summary>
    /// <remarks>The hash code is computed using the ItemNum and BlockNum properties. This method is intended
    /// for use in hash-based collections.</remarks>
    /// <param name="obj">The Trial object for which to compute the hash code. Cannot be null.</param>
    /// <returns>An integer hash code representing the specified Trial object.</returns>
    public int GetHashCode(Trial obj)
    {
        int hash = 17;
        hash = hash * 23 + obj.ItemNum.GetHashCode();
        hash = hash * 23 + obj.BlockNum.GetHashCode();
        return hash;
    }

    /// <summary>
    /// Determines whether two Trial instances are equal based on their ItemNum and BlockNum values.
    /// </summary>
    /// <param name="A">The first Trial instance to compare.</param>
    /// <param name="B">The second Trial instance to compare.</param>
    /// <returns>true if both Trial instances have the same ItemNum and BlockNum values; otherwise, false.</returns>
    public bool Equals(Trial? A, Trial? B)
    {
        if (A is null || B is null) return false;
        return A.ItemNum == B.ItemNum && A.BlockNum == B.BlockNum;
    }

    /// <summary>
    /// Gets or sets the direction associated with the key input.
    /// </summary>
    [XmlElement("KeyedDir", Form = XmlSchemaForm.Unqualified)]
    public KeyedDirection KeyedDir { get; set; } = KeyedDirection.None;

    /// <summary>
    /// Gets or sets the item number associated with this instance.
    /// </summary>
    [XmlElement("ItemNum", Form = XmlSchemaForm.Unqualified)]
    public int ItemNum { get; set; } = 0;

    /// <summary>
    /// Gets or sets the block number associated with the current instance.
    /// </summary>
    [XmlElement("BlockNum", Form = XmlSchemaForm.Unqualified)]
    public int BlockNum { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the originating block associated with this entity.
    /// </summary>
    [XmlElement("OriginatingBlock", Form = XmlSchemaForm.Unqualified)]
    public int OriginatingBlock { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the stimulus display associated with this instance.
    /// </summary>
    [XmlElement("StimulusDisplayID", Form = XmlSchemaForm.Unqualified)]
    public int StimulusDisplayID { get; set; } = 0;
}
