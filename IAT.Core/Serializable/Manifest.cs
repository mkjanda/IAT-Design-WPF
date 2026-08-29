using IAT.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using IAT.Core.Enumerations;
using System.Xml.Serialization;
using MediatR;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents a request to execute a manifest operation and obtain a transaction result.
    /// </summary>
    /// <param name="manifest">The manifest to be executed as part of the transaction. Cannot be null.</param>
    public record ManifestCommand(Manifest manifest) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents an abstract file system entity, such as a file or directory, with common properties for path, size,
    /// and type.
    /// </summary>
    /// <remarks>This class serves as a base for file and directory representations, providing shared
    /// metadata. Derived types should implement additional behavior as needed. The class is intended for use in
    /// scenarios where file system entities need to be modeled or serialized.</remarks>
    [XmlInclude(typeof(ManifestFile))]
    [XmlInclude(typeof(ManifestDirectory))]
    public abstract class FileEntity
    {

        /// <summary>
        /// Gets or sets the name of the file entity, which can be used for identification or display purposes. The name should be unique within the 
        /// context of its parent directory or collection.
        /// </summary>
        [XmlElement("Name", Form = XmlSchemaForm.Unqualified)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file system or resource path associated with this instance.
        /// </summary>
        [XmlElement("Path", Form = XmlSchemaForm.Unqualified)]
        public virtual string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the file entity in bytes. For files, this represents the actual size of the file content. For directories, this may represent the cumulative size of all contained files and subdirectories. The property is ignored during XML serialization to avoid 
        /// including potentially large binary data directly in the XML representation.
        /// </summary>
        [XmlIgnore]
        public abstract int Size { get; set; }

        /// <summary>
        /// Gets the type of the file entity, indicating whether it is a file or a directory. This property is abstract and must be implemented by derived classes to specify the appropriate entity type. The value can be used to determine how to handle the entity in various operations, 
        /// such as serialization, processing, or display.
        /// </summary>
        public abstract FileEntityType FileEntityType { get; }
    }

    /// <summary>
    /// Represents a file entry in a manifest, including resource type, identifiers, and metadata relevant to the file's
    /// role within the manifest.
    /// </summary>
    /// <remarks>Use this class to describe files that are part of a manifest, such as resources,
    /// configuration files, or images. The properties provide information necessary for identifying and processing the
    /// file within the context of the manifest. Inherits from FileEntity, which may provide additional file-related
    /// functionality.</remarks>
    [XmlType("File")]
    public class ManifestFile : FileEntity
    {
        /// <summary>
        /// Gets or sets the type of the file entity.
        /// </summary>
        [XmlIgnore]
        public override FileEntityType FileEntityType => FileEntityType.File;

        /// <summary>
        /// Gets or sets the MIME type of the content.
        /// </summary>
        /// <remarks>The default value is "text/plain". Set this property to specify the media type of the
        /// data being represented, such as "application/json" or "image/png".</remarks>
        [XmlElement("MimeType", Form = XmlSchemaForm.Unqualified)]
        public string MimeType { get; set; } = "text/plain";

        /// <summary>
        /// Gets or sets the size of the file in bytes. The default value is 0, indicating an empty file. This property can be used to track the 
        /// file's size for validation, display, or processing purposes within the manifest context.
        /// </summary>
        [XmlElement("Size", Form = XmlSchemaForm.Unqualified, Type = typeof(int))]
        public override int Size { get; set; } = 0;

        /// <summary>
        /// Gets or sets the type of resource represented by this instance.
        /// </summary>
        [XmlElement("ResourceType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResourceType))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the associated resource.
        /// </summary>
        [XmlElement("ResourceId", Form = XmlSchemaForm.Unqualified, Type = typeof(int))]
        public int ResourceId { get; set; } = -1;

        /// <summary>
        /// The byte array representing the content of the file. This property is ignored during XML serialization, as it may 
        /// contain large binary data that is not suitable for direct inclusion in XML. Instead, the content can be stored or 
        /// transmitted separately, with references to it included in the manifest as needed.
        /// </summary>
        [XmlIgnore]
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Represents a directory in a manifest, containing a collection of file and directory entities.
    /// </summary>
    /// <remarks>Use this class to model hierarchical directory structures within a manifest. The directory
    /// can contain both files and subdirectories, accessible through the Contents collection or by index. Inherits from
    /// FileEntity, allowing directories to be treated uniformly with files in manifest operations.</remarks>
    [XmlType("Directory")]
    public class ManifestDirectory : FileEntity
    {
        /// <summary>
        /// Gets or sets the collection of subdirectories contained within this directory. Each item in the collection represents a ManifestDirectory instance, allowing for nested directory structures. 
        /// The collection may be empty if no subdirectories are present.
        /// </summary>
        [XmlElement("Directory", Type = typeof(ManifestDirectory))]
        public List<ManifestDirectory> Directories { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of files contained within this directory. Each item in the collection represents a ManifestFile instance, allowing for the inclusion of multiple files. 
        /// The collection may be empty if no files are present.
        /// </summary>
        [XmlElement("File", Type = typeof(ManifestFile))]
        public List<ManifestFile> Files { get; set; } = new();


        /// <summary>
        /// Returns Directory as the file entity type.
        /// </summary>
        [XmlIgnore]
        public override FileEntityType FileEntityType => FileEntityType.Directory;

        /// <summary>
        /// The size in bytes of the directory, calculated as the sum of the sizes of all contained file entities. 
        /// </summary>
        [XmlIgnore]
        public override int Size
        {
            get
            {
                int totalSize = 0;
                Directories.ToList().ForEach(fe => totalSize += fe.Size);
                Files.ToList().ForEach(fe => totalSize += fe.Size);
                return totalSize;
            }
            set;
        }
    }

    /// <summary>
    /// Represents a manifest that contains metadata for XML serialization, including the client identifier and IAT
    /// element name.
    /// </summary>
    [XmlRoot("Manifest")]
    public class Manifest : IWebSocketMessage
    {
        /// <summary>
        /// Gets or sets the type of the manifest, indicating whether it is a file manifest or an item slide manifest.
        /// </summary>
        [XmlAttribute("ManifestType", Form = XmlSchemaForm.Unqualified)]
        public ManifestType ManifestType { get; set; }

        /// <summary>
        /// Gets or sets the product key associated with the manifest, used for authentication or identification purposes.
        /// </summary>
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified, Order = 1)]
        public string ProductKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the IAT element for XML serialization.
        /// </summary>
        [XmlElement("IATName", Form = XmlSchemaForm.Unqualified, Order = 2)]
        public string IATName { get; set; } = string.Empty;

        /// <summary>
        /// The list of file entries in the manifest
        /// </summary>
        [XmlElement("File", Type=typeof(ManifestFile), Order = 3)]
        public List<ManifestFile> Files = new();

    }
}
