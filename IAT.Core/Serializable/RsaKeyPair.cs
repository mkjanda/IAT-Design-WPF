using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Serializable;

class RsaKeyPair 
{
    [XmlElement("DataKey", Form = XmlSchemaForm.Unqualified, Type = typeof(EncryptedRSAKey)]
    private EncryptedRSAKey DataKey { get; init; } = new EncryptedRSAKey();

    public RsaKeyPair() { }

    public RsaKeyPair(EncryptedRSAKey dataKey)
    {
        DataKey = dataKey;
    }
}
