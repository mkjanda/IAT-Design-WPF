using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Enumeration representing the type of manifest.
    /// </summary>
    [Serializable]
    public enum ManifestType
    {
        /// <summary>
        /// Manifest of deployment files
        /// </summary>
        [Description("Manifest of deployment files")]
        [XmlEnum("FileManifest")]
        FileManifest,

        /// <summary>
        /// Manifest of item slide files
        /// </summary>
        [Description("Manifest of item slide files")]
        [XmlEnum("ItemSlideManifest")]
        ItemSlideManifest

    }
}
