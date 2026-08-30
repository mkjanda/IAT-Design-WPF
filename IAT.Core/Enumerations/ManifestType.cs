using System.Xml.Serialization;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Enumeration representing the type of manifest. Tokens must match the Java XSD exactly.
    /// </summary>
    [Serializable]
    public enum ManifestType
    {
        [XmlEnum("FileManifest")]
        FileManifest,

        [XmlEnum("ItemSlideManifest")]
        ItemSlideManifest
    }
}
