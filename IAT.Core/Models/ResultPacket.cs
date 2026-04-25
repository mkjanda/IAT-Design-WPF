using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Security.Cryptography;
using System.IO;

namespace IAT.Core.Models;

/// <summary>
/// THe ResultSet class represents the structure of the result set returned by the IAT application. It includes properties for the result ID, administration time, 
/// result data, a table of contents (TOC) for the result data, and an optional token. The TOC is a list of entries that specify offsets and lengths for keys, 
/// initialization vectors (IVs), and data within the result data string. This class is designed to be serialized to and deserialized from 
/// XML format, with specific attributes to control the XML structure. Additionally, it includes a static RSA field for cryptographic operations related to the result set.
/// </summary>
[XmlRoot("ResultSet")]
public sealed class ResultPacket
{
    /// <summary>
    /// Gets or sets the unique identifier for the result.
    /// </summary>
    [XmlAttribute("ResultId", Form = XmlSchemaForm.Unqualified)]
    public string ResultId { get; set; }

    /// <summary>
    /// Gets or sets the administrative time value associated with the current instance.
    /// </summary>
    [XmlElement("AdminTime", Form = XmlSchemaForm.Unqualified)]
    public string AdminTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the result data as a string.
    /// </summary>
    [XmlElement("ResultData", Form = XmlSchemaForm.Unqualified)]
    public string ResultData { get; set; } = string.Empty;

    /// <summary>
    /// Represents a table of contents (TOC) entry that describes the location and length of key, initialization vector
    /// (IV), and data segments within a storage medium.
    /// </summary>
    /// <remarks>Each field specifies the offset and length of a segment, enabling efficient access to
    /// encrypted or structured data blocks. This struct is typically used in scenarios where precise positioning of
    /// cryptographic or data elements is required, such as in file formats or secure storage systems.</remarks>
    public struct TOCEntry
    {
        /// <summary>
        /// Specifies the byte offset of the key within the data source.
        /// </summary>
        public long KeyOffset;

        /// <summary>
        /// Specifies the length of the cryptographic key, in bits.
        /// </summary>
        public int KeyLength;

        /// <summary>
        /// Specifies the offset, in bytes, of the initialization vector (IV) within the data stream.
        /// </summary>
        public long IVOffset;

        /// <summary>
        /// Gets or sets the length, in bytes, of the initialization vector (IV) used for cryptographic operations.
        /// </summary>
        /// <remarks>The IV length must match the requirements of the cryptographic algorithm in use.
        /// Supplying an incorrect value may result in errors during encryption or decryption.</remarks>
        public int IVLength;

        /// <summary>
        /// Specifies the byte offset at which the data segment begins.
        /// </summary>
        public long DataOffset;

        /// <summary>
        /// Represents the length of the data, typically in bytes.
        /// </summary>
        public int DataLength;
    }

    /// <summary>
    /// Gets or sets the collection of table of contents entries for the result set.
    /// </summary>
    /// <remarks>Each entry in the collection represents a section or item in the table of contents. The order
    /// of entries in the list determines their sequence in the table of contents.</remarks>
    [XmlArray("TOC")]
    [XmlArrayItem("ResultTOCEntry", Form = XmlSchemaForm.Unqualified, Type = typeof(TOCEntry))]
    public List<TOCEntry> TOC { get; set; } = new List<TOCEntry>();

    /// <summary>
    /// Gets or sets the authentication token used to authorize requests.
    /// </summary>
    [XmlElement("Token", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
    public string? Token { get; set; }

    /// <summary>
    /// Represents a shared RSA cryptographic service provider instance.
    /// </summary>
    /// <remarks>This field is static and may be accessed concurrently from multiple threads. Ensure proper
    /// synchronization if modifying the instance. The field is ignored during XML serialization due to the XmlIgnore
    /// attribute.</remarks>
    [XmlIgnore] 
    public static RSA? rsa;

    /// <summary>
    /// Initializes a new instance of the ResultSet class.
    /// </summary>
    public ResultPacket() { }

}
