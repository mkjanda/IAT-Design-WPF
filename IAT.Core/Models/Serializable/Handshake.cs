using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Security.Cryptography;

namespace IAT.Core.Models.Serializable
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
        [XmlElement("PublicKey", Form = XmlSchemaForm.Unqualified)]
        public string PublicKey { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the modulus value used in cryptographic operations.
        /// </summary>
        [XmlElement("Modulus", Form = XmlSchemaForm.Unqualified)]
        public string Modulus { get; set; } = String.Empty;
        
        /// <summary>
        /// Gets or sets the encrypted text represented as a base64-encoded string.
        /// </summary>
        [XmlElement("CipherText", Form = XmlSchemaForm.Unqualified)]
        public string CipherText { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the plain text content associated with this instance.
        /// </summary>
        [XmlIgnore]
        public string PlainText { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the default RSA algorithm instance used for cryptographic operations.
        /// </summary>
        /// <remarks>This property provides a globally accessible RSA instance for use in encryption,
        /// decryption, signing, and verification tasks. Modifying this property affects all consumers that rely on the
        /// default RSA instance. Thread safety is not guaranteed; callers should ensure appropriate synchronization if
        /// the property is accessed concurrently.</remarks>
        [XmlIgnore]
        static public RSA RSA { get; set; } = RSA.Create(2048);

        /// <summary>
        /// Initializes a new instance of the Handshake class.
        /// </summary>
        public Handshake()
        {   

        }
}
