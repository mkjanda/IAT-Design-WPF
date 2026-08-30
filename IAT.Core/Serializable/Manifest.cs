using IAT.Core.Enumerations;
using MediatR;
using System.Xml.Schema;
using System.Xml.Serialization;

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
    [XmlInclude(typeof(ManifestFile))]
    [XmlInclude(typeof(ManifestDirectory))]
    public abstract class FileEntity
    {
        /// <summary>
        /// Gets or sets the name of the file entity, unique within its parent directory or collection.
        /// </summary>
        [XmlElement("Name", Form = XmlSchemaForm.Unqualified)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file system or resource path associated with this instance.
        /// </summary>
        [XmlElement("Path", Form = XmlSchemaForm.Unqualified)]
        public virtual string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the file entity in bytes. Ignored on the base so directories do not emit Size.
        /// </summary>
        [XmlIgnore]
        public abstract int Size { get; set; }

        /// <summary>
        /// Gets the type of the file entity (file or directory). Not part of the GFile/GDirectory wire contract.
        /// </summary>
        [XmlIgnore]
        public abstract FileEntityType FileEntityType { get; }
    }

    /// <summary>
    /// A file entry in a manifest. Wire name is <c>File</c> (GFile): Name, Path, MimeType, Size, ResourceType, ResourceId.
    /// </summary>
    [XmlType("File")]
    public class ManifestFile : FileEntity
    {
        [XmlIgnore]
        public override FileEntityType FileEntityType => FileEntityType.File;

        [XmlElement("MimeType", Form = XmlSchemaForm.Unqualified)]
        public string MimeType { get; set; } = "text/plain";

        [XmlElement("Size", Form = XmlSchemaForm.Unqualified)]
        public override int Size { get; set; } = 0;

        [XmlElement("ResourceType", Form = XmlSchemaForm.Unqualified)]
        public ResourceType ResourceType { get; set; }

        [XmlElement("ResourceId", Form = XmlSchemaForm.Unqualified)]
        public int ResourceId { get; set; } = -1;

        /// <summary>
        /// File bytes. Not part of the XML document — transmitted in a later upload packet.
        /// </summary>
        [XmlIgnore]
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// A directory in a manifest. Wire name is <c>Directory</c>. Used for nested trees, not as the Manifest root.
    /// </summary>
    [XmlType("Directory")]
    public class ManifestDirectory : FileEntity
    {
        [XmlElement("Directory", Type = typeof(ManifestDirectory))]
        public List<ManifestDirectory> Directories { get; set; } = new();

        [XmlElement("File", Type = typeof(ManifestFile))]
        public List<ManifestFile> Files { get; set; } = new();

        [XmlIgnore]
        public override FileEntityType FileEntityType => FileEntityType.Directory;

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
            set { }
        }
    }

    /// <summary>
    /// Deployment / item-slide manifest. Flattened (does not inherit <see cref="ManifestDirectory"/>)
    /// so XmlSerializer emits GManifest order: ProductKey, IATName, File*.
    /// </summary>
    [XmlRoot("Manifest")]
    public class Manifest : IWebSocketMessage
    {
        /// <summary>
        /// FileManifest or ItemSlideManifest. Token must match the XSD enumeration exactly.
        /// </summary>
        [XmlAttribute("ManifestType", Form = XmlSchemaForm.Unqualified)]
        public ManifestType ManifestType { get; set; }

        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified, Order = 1)]
        public string ProductKey { get; set; } = string.Empty;

        [XmlElement("IATName", Form = XmlSchemaForm.Unqualified, Order = 2)]
        public string IATName { get; set; } = string.Empty;

        /// <summary>
        /// Top-level files. Declared here (not inherited) so they serialize after ProductKey and IATName.
        /// </summary>
        [XmlElement("File", Type = typeof(ManifestFile), Order = 3)]
        public List<ManifestFile> Files { get; set; } = new();
    }
}
