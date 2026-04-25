using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Security.Cryptography;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents the data exchanged during a cryptographic handshake, including public key, modulus, and encrypted or
    /// plain text values.
    /// </summary>
    /// <remarks>This class is typically used to serialize or deserialize handshake information in
    /// cryptographic protocols. The properties correspond to XML elements expected in handshake message
    /// formats.</remarks>
    public class Handshake
    {
        /// <summary>
        /// Gets or sets the public key associated with the entity.
        /// </summary>
        [XmlElement("Text", Form = XmlSchemaForm.Unqualified)]
        public string Text { get; set; } = String.Empty;

        /// <summary>
        /// Initializes a new instance of the Handshake class.
        /// </summary>
        public Handshake()
        {

        }
    }
}
