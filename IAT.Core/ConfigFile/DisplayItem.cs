using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the metadata associated with an image for display purposes, including its filename and dimensions. 
/// This class is used to define how an image should be displayed in the application, specifying its position 
/// (X and Y coordinates) and size (Width and Height). The ID property serves as a unique identifier for each display 
/// item, allowing for easy reference and management within the application's configuration.
/// </summary>
[XmlType("DisplayItem")]
public sealed class DisplayItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the object.
    /// </summary>
    [XmlElement("ID", Form = XmlSchemaForm.Unqualified)]
    public int Id { get; set; } = -1;

    /// <summary>
    /// Gets or sets the XML export form of the unique identifier for the object. This property is used for serialization purposes, allowing the unique identifier to be represented as a string in XML format. The getter returns the GUID as a string in "N" format (32 digits), while the setter parses the string back into a GUID, defaulting to Guid.Empty if the value is null or empty.
    /// </summary>
    [XmlElement("Guid", Form = XmlSchemaForm.Unqualified)]
    public string GuidString
    {
        get
        {
            return Guid.ToString("N");
        }
        set
        {
            Guid = string.IsNullOrEmpty(value) ?
                Guid.Empty : Guid.ParseExact(value, "N");
        }
    }

    /// <summary>
    /// Gets or sets the unique identifier for the object.
    /// </summary>
    [XmlIgnore]
    public Guid Guid { get; set; } = Guid.Empty;


    /// <summary>
    /// Gets or sets the value of X.
    /// </summary>
    [XmlElement("X", Form = XmlSchemaForm.Unqualified)]
    public int X { get; set; } = 0;

    /// <summary>
    /// Gets or sets the Y-coordinate value.
    /// </summary>
    [XmlElement("Y", Form = XmlSchemaForm.Unqualified)]
    public int Y { get; set; } = 0;

    /// <summary>
    /// Gets or sets the width value.
    /// </summary>
    [XmlElement("Width", Form = XmlSchemaForm.Unqualified)]
    public int Width { get; set; } = 0;

    /// <summary>
    /// Gets or sets the height value.
    /// </summary>
    [XmlElement("Height", Form = XmlSchemaForm.Unqualified)]
    public int Height { get; set; } = 0;
}
