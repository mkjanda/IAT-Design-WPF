using IAT.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Enumerations;
using MediatR;

namespace IAT.Core.Serializable
{

    public record ManifestCommand(Manifest manifest) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents an abstract file system entity, such as a file or directory, with common properties for path, size,
    /// and type.
    /// </summary>
    /// <remarks>This class serves as a base for file and directory representations, providing shared
    /// metadata. Derived types should implement additional behavior as needed. The class is intended for use in
    /// scenarios where file system entities need to be modeled or serialized.</remarks>
    public abstract class FileEntity
    {
        /// <summary>
        /// Specifies the type of a file system entity, such as a file or a directory.
        /// </summary>
        /// <remarks>Use this enumeration to distinguish between files and directories when working with
        /// file system operations. This can help determine the appropriate handling or processing logic based on the
        /// entity type.</remarks>
        public enum EFileEntityType { File, Directory };

        /// <summary>
        /// Gets or sets the size of the item in bytes.
        /// </summary>
        [XmlElement("Size", Form = XmlSchemaForm.Unqualified, Type = typeof(long))]
        public virtual long Size { get; set; } = 0;

        /// <summary>
        /// Gets or sets the file system or resource path associated with this instance.
        /// </summary>
        [XmlElement("Path", Form = XmlSchemaForm.Unqualified)]
        public virtual string Path { get; set; } = string.Empty;

        [XmlIgnore]
        public string _Name = string.Empty;

        [XmlElement("FileEntityType", Form = XmlSchemaForm.Unqualified, Type = typeof(EFileEntityType))]
        public EFileEntityType FileEntityType { get; set; }

        public FileEntity()
        {
        }
    }

    /// <summary>
    /// Represents a file entry in a manifest, including resource type, identifiers, and metadata relevant to the file's
    /// role within the manifest.
    /// </summary>
    /// <remarks>Use this class to describe files that are part of a manifest, such as resources,
    /// configuration files, or images. The properties provide information necessary for identifying and processing the
    /// file within the context of the manifest. Inherits from FileEntity, which may provide additional file-related
    /// functionality.</remarks>
    public class ManifestFile : FileEntity
    {
        public enum EResourceType { itemSlide, testConfiguration, updateFile, image, errorMark };

        /// <summary>
        /// Gets or sets the type of resource represented by this instance.
        /// </summary>
        [XmlElement("ResourceType", Form = XmlSchemaForm.Unqualified, Type = typeof(EResourceType))]
        public EResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the associated resource.
        /// </summary>
        [XmlElement("ResourceId", Form = XmlSchemaForm.Unqualified, Type = typeof(int))]
        public int ResourceId { get; set; } = -1;

        /// <summary>
        /// Gets the collection of reference identifiers associated with this instance.
        /// </summary>
        /// <remarks>The collection is read-only from outside the class. Items can be added or removed
        /// only within the class implementation.</remarks>
        [XmlArray("ReferenceIds", Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("ReferenceId", Form = XmlSchemaForm.Unqualified, Type = typeof(int))]
        public List<int> ReferenceIds { get; private set; } = new List<int>();

        /// <summary>
        /// Gets or sets the MIME type of the content.
        /// </summary>
        /// <remarks>The default value is "text/plain". Set this property to specify the media type of the
        /// data being represented, such as "application/json" or "image/png".</remarks>
        [XmlElement("MimeType", Form = XmlSchemaForm.Unqualified)]
        public String MimeType { get; set; } = "text/plain";

        /// <summary>
        /// Gets or sets the size of the content, in bytes.
        /// </summary>
        [XmlElement("Size", Form = XmlSchemaForm.Unqualified, Type = typeof(long))]
        public long Size { get; set; } = 0;

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
    public class ManifestDirectory : FileEntity
    {
        /// <summary>
        /// Gets or sets the collection of file entities contained within this object.
        /// </summary>
        /// <remarks>Each item in the collection represents a file entity. The collection may be empty if
        /// no file entities are present.</remarks>
        [XmlArray]
        [XmlArrayItem("FileEntity", Type = typeof(FileEntity))]
        public List<FileEntity> Contents;

        [XmlIgnore]        
        public FileEntity this[int ctr]
        {
            get
            {
                return Contents[ctr];
            }
        }

        /// <summary>
        /// Gets or sets the file system path associated with this instance.
        /// </summary>
        public override string Path { get; set; } = string.Empty;

        /// <summary>
        /// The size in bytes of the directory, calculated as the sum of the sizes of all contained file entities. 
        /// </summary>
        [XmlElement("Size", Form = XmlSchemaForm.Unqualified)]
        public override long Size
        {
            get
            {
                long totalSize = 0;
                Contents.Where(fe => fe.FileEntityType == EFileEntityType.Directory).Cast<ManifestDirectory>().ToList().ForEach(fe => totalSize += fe.Size);
                Contents.Where(fe => fe.FileEntityType == EFileEntityType.File).Cast<ManifestFile>().ToList().ForEach(fe => totalSize += fe.Size);
                return totalSize;
            }
            set;
        }
    }

    /// <summary>
    /// Represents a manifest that contains metadata for XML serialization, including the client identifier and IAT
    /// element name.
    /// </summary>
    public class Manifest : ManifestDirectory
    {
        /// <summary>
        /// Gets or sets the name of the IAT element for XML serialization.
        /// </summary>
        [XmlElement("IATName", Form = XmlSchemaForm.Unqualified)]
        private string IATName = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the client.
        /// </summary>
        [XmlElement("ClientId", Form = XmlSchemaForm.Unqualified, Type = typeof(long))]
        public long ClientId { get; set; } = 0;

    }
}
