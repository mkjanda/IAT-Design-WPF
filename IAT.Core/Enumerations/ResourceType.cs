using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Specifies the types of resources that can be managed or referenced within the application.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the kind of resource being handled, such as slides,
    /// configuration settings, files, images, error markers, or key outlines. The meaning and usage of each value
    /// depend on the application context where the resource is required.</remarks>
    public enum ResourceType
    {
        /// <summary>
        /// Gets or sets the image associated with this instance.
        /// </summary>
        [Description("Image")]
        [XmlEnum("Image")]
        Image,

        /// <summary>
        /// Gets or sets the slide item associated with this instance.
        /// </summary>
        [Description("Item slide")]
        [XmlEnum("ItemSlide")]
        ItemSlide,

        /// <summary>
        /// Gets or sets the JavaScript file associated with this instance.
        /// </summary>
        [Description("JavaScript")]
        [XmlEnum("Javascript")]
        Javascript,

        /// <summary>
        /// Updates the contents of a file with new data.
        /// </summary>
        [Description("Update file")]
        [XmlEnum("UpdateFile")]
        UpdateFile,

        /// <summary>
        /// Gets or sets the test configuration settings used by the application.
        /// </summary>
        [Description("Test configuration")]
        [XmlEnum("TestConfiguration")]
        TestConfiguration,

        /// <summary>
        /// Gets or sets the error marker associated with the current operation.
        /// </summary>
        [Description("Error marker")]
        [XmlEnum("ErrorMark")]
        ErrorMark,

        /// <summary>
        /// Gets or sets the outline of the key, typically used to define the visual shape or border of a key element.
        /// </summary>
        [Description("Key outline")]
        [XmlEnum("KeyOutline")]
        KeyOutline
    }
}
