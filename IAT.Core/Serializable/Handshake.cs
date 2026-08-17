using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Security.Cryptography;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents a command to initiate a cryptographic handshake, encapsulating the handshake data to be processed.
    /// </summary>
    /// <param name="inHand">The handshake data to be processed.</param>
    public record HandshakeCommand(Handshake inHand) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents the data exchanged during a cryptographic handshake, including public key, modulus, and encrypted or
    /// plain text values.
    /// </summary>
    /// <remarks>This class is typically used to serialize or deserialize handshake information in
    /// cryptographic protocols. The properties correspond to XML elements expected in handshake message
    /// formats.</remarks>
    public class Handshake : IWebSocketMessage
    {
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        public string ProductKey { get; set; } = String.Empty;


        /// <summary>
        /// Number used once
        /// </summary>
        [XmlElement("Nonce", Form = XmlSchemaForm.Unqualified)]
        public string Nonce { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the public key associated with the entity.
        /// </summary>
        [XmlElement("Challenge", Form = XmlSchemaForm.Unqualified)]
        public string Challenge { get; set; } = String.Empty;


        /// <summary>
        /// Tags the challenge
        /// </summary>
        [XmlElement("Tag", Form = XmlSchemaForm.Unqualified)]
        public string Tag { get; set; } = String.Empty;

        public Handshake() { }

        public Handshake(ILocalStorageService localStorage)
        {
            ProductKey = localStorage[Field.ProductKey];
        }
    }
}
