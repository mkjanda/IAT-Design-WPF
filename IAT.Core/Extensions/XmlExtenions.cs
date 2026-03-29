using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace IAT.Core.Extensions
{
    /// <summary>
    /// Provides extension methods and utilities for XML serialization using the XmlSerializer class.
    /// </summary>
    /// <remarks>This static class offers methods to serialize objects to XML and manages a thread-safe cache
    /// of XmlSerializer instances for improved performance. The utilities are designed for use with types compatible
    /// with the XmlSerializer and support concurrent access in multithreaded scenarios.</remarks>
    public static class XmlExtenions
    {
        /// <summary>
        /// Provides a thread-safe cache of XML serializers for specific types.
        /// </summary>
        /// <remarks>This dictionary enables efficient reuse of XmlSerializer instances, which can be
        /// expensive to create. Access to the dictionary is safe for concurrent read and write operations from multiple
        /// threads.</remarks>
        public static readonly ConcurrentDictionary<Type, XmlSerializer> _serializers = new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        /// Serializes the specified object to its XML representation as a string.
        /// </summary>
        /// <remarks>The resulting XML does not include an XML declaration and is not indented. The
        /// object's type must have a public parameterless constructor and be compatible with the XmlSerializer. This
        /// method uses a cached XmlSerializer instance for performance.</remarks>
        /// <param name="model">The object to serialize to XML. The object's type must be serializable by the XmlSerializer.</param>
        /// <returns>A string containing the XML representation of the specified object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is <see langword="null"/>.</exception>
        public static string ToXml(this object model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var type = model.GetType();
            var serializer = _serializers.GetOrAdd(model.GetType(), t => new XmlSerializer(t));
            using var stringWriter = new StringWriter();
            using var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, 
                new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true, Indent = false });
            serializer.Serialize(xmlWriter, model);
            return stringWriter.ToString();
        }
    }
}
