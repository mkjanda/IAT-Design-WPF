using System;
using System.Collections.Generic;
using System.IO;

namespace IAT.Core.Services
{
    public interface IXmlDeserializationService
    {
        /// <summary>
        /// Deserializes the specified XML string into an object of an unknown type.
        /// </summary>
        /// <remarks>Use this method when the type of the object represented by the XML is not known at
        /// compile time. The caller is responsible for casting the result to the expected type, if necessary.</remarks>
        /// <param name="xml">The XML string to deserialize. Must be a valid XML representation of an object.</param>
        /// <returns>An object deserialized from the provided XML string. The type of the returned object depends on the XML
        /// content.</returns>
        object DeserializeUnknownType(string xml);

        /// <summary>
        /// Deserializes data from the specified stream into an object of an unknown type.
        /// </summary>
        /// <remarks>The caller is responsible for ensuring that the stream contains data in a supported
        /// format. The method does not attempt to determine or validate the expected type in advance.</remarks>
        /// <param name="stream">The stream containing the serialized data to deserialize. The stream must be readable and positioned at the
        /// beginning of the data to deserialize.</param>
        /// <returns>An object representing the deserialized data. The actual type of the returned object depends on the content
        /// of the stream.</returns>
        object DeserializeUnknownType(Stream stream);
    }
}
