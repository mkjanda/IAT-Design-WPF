using net.sf.saxon.ma.map;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using IAT.Core.Serializable;

namespace IAT.Core.Services
{
    /// <summary>
    /// Provides functionality to deserialize XML data into objects of known types based on the XML element name.
    /// </summary>
    /// <remarks>This service determines the target object type for deserialization by mapping the root XML
    /// element name to a registered .NET type. It supports deserialization from XML strings, streams, or XML readers.
    /// The set of supported types is defined internally and may be extended as needed. If the XML element name does not
    /// correspond to a registered type, deserialization will fail with an exception.</remarks>
    public class XmlDeserializationService : IXmlDeserializationService
    {
        private static readonly Dictionary<string, Type> _elementToType = new(StringComparer.OrdinalIgnoreCase)
        {
            { typeof(ActivationRequest).Name, typeof(ActivationRequest) },
            { typeof(ActivationResponse).Name, typeof(ActivationResponse) },
            { typeof(AlternationGroup).Name, typeof(AlternationGroup)  },
            { typeof(Block).Name, typeof(Block) },
            { typeof(HistoryEntry).Name, typeof(HistoryEntry) },
            { typeof(Key).Name, typeof(Key)  },
            { typeof(SaveFileMetaData).Name, typeof(SaveFileMetaData) },
            { typeof(Test).Name, typeof(Test)  },
            { typeof(TransactionRequest).Name, typeof(TransactionRequest) },
            { typeof(Trial).Name, typeof(Trial) },
            { typeof(Serializable.Version).Name, typeof(Serializable.Version) }
        };

        /// <summary>
        /// Deserializes an XML string into an object of an unknown type.
        /// </summary>
        /// <param name="xml">The XML string to deserialize. Cannot be null.</param>
        /// <returns>An object representing the deserialized data. The type of the returned object depends on the XML content.</returns>
        public object DeserializeUnknownType(string xml)
        {
            using var stringReader = new StringReader(xml);
            return DeserializeUnknownType(XmlReader.Create(stringReader));
        }

        /// <summary>
        /// Deserializes an object of unknown type from the specified XML stream.
        /// </summary>
        /// <param name="stream">The stream containing the XML data to deserialize. The stream must be readable and positioned at the start
        /// of the XML content.</param>
        /// <returns>An object representing the deserialized data. The type of the returned object depends on the XML content.</returns>
        public object DeserializeUnknownType(Stream stream)
        {
            using var xmlReader = XmlReader.Create(stream);
            return DeserializeUnknownType(xmlReader);
        }

        /// <summary>
        /// Deserializes an XML element of unknown type using the provided XML reader.
        /// </summary>
        /// <remarks>The method determines the target type for deserialization based on the local name of
        /// the current XML element and an internal mapping. Ensure that the XML element name matches a registered type
        /// mapping before calling this method.</remarks>
        /// <param name="xmlReader">The XML reader positioned at the element to deserialize. Cannot be null.</param>
        /// <returns>An object representing the deserialized XML element. The object's type is determined by the element name and
        /// the internal mapping.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="xmlReader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no type mapping exists for the XML element name, or if deserialization results in a null object.</exception>
        public object DeserializeUnknownType(XmlReader xmlReader)
        {
            if (xmlReader == null) throw new ArgumentNullException(nameof(xmlReader));
            xmlReader.MoveToContent();
            var RootName = xmlReader.LocalName;
            if (!_elementToType.TryGetValue(RootName, out var targetType))
                throw new InvalidOperationException($"No mapping found for XML element '{RootName}'. Unable to determine type for deserialization.");
            var serializer = new XmlSerializer(targetType);
            return serializer.Deserialize(xmlReader) ?? throw new InvalidOperationException($"Deserialization of XML element '{RootName}' resulted in a null object.");
        }
    }
}
