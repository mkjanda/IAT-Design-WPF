using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using Konscious.Security.Cryptography;
using MediatR;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.ResultData;

/// <summary>
/// Represents a command to process an encrypted RSA key as part of a request that returns a transaction result.
/// </summary>
/// <param name="Key">The encrypted RSA key to be processed. Cannot be null.</param>
public record DecryptorCommand(RsaParams decryptor) : IRequest<TransactionResult>;

/// <summary>
/// Contains the encrypted RSA key information, including the modulus (n), exponent (e), private exponent (d), prime factors (p and q), and other related parameters.
/// </summary>
public class RsaParams : IWebSocketMessage
{
    public string ProductKey { get; set; } = string.Empty;


    /// <summary>
    /// Gets or sets the string value associated with the NString XML element.
    /// </summary>
    [XmlElement("Modulus", Form = XmlSchemaForm.Unqualified)]
    public string Modulus { get; set; } = string.Empty;
        
    /// <summary>
    /// Gets or sets the value of the Exponent XML element.
    /// </summary>
    [XmlElement("Exponent", Form = XmlSchemaForm.Unqualified)]
    public string Exponent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted key value.
    /// </summary>
    [XmlElement("EncRsaParams", Form = XmlSchemaForm.Unqualified)]
    public string EncRSAParams { get; set; } = string.Empty;

    [XmlElement("Salt", Form = XmlSchemaForm.Unqualified)]
    public string Salt { get; set; } = string.Empty;

    [XmlElement("Tag", Form = XmlSchemaForm.Unqualified)]
    public string Tag { get; set; } = string.Empty;

    [XmlElement("Nonce", Form = XmlSchemaForm.Unqualified)]
    public string Nonce { get; set; } = string.Empty;
}


