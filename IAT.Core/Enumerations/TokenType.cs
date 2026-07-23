using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Specifies the type of token used for encoding or representing data values.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the format or encoding of a token when processing or
    /// interpreting data. The values include standard representations such as hexadecimal and Base64, as well as
    /// specialized encodings like Base64 with UTF-8 support.</remarks>
    public enum TokenType
    {
        /// <summary>
        /// Indicates that no specific token type is defined or applicable.
        /// </summary>
        none,
        /// <summary>
        /// Indicates that the token represents a standard value.
        /// </summary>
        value,
        /// <summary>
        /// Indicates that the token is encoded in hexadecimal format.
        /// </summary>
        hex,
        /// <summary>
        /// Indicates that the token is encoded in Base64 format.
        /// </summary>
        base64,
        /// <summary>
        /// Indicates that the token is encoded in Base64 format with UTF-8 support.
        /// </summary>
        base64_utf8
    }
}
