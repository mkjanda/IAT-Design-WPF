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

        none, value, hex, base64, base64_utf8
    }
}
