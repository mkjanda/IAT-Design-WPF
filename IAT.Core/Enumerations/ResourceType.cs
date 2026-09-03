using System.Xml.Serialization;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Resource kinds that appear on a GFile. Tokens must match the Java XSD enumeration exactly.
    /// Legal wire values: Image | ItemSlide | Javascript | TestConfiguration | ErrorMark | KeyOutline.
    /// </summary>
    public enum ResourceType
    {
        [XmlEnum("Image")]
        Image,

        [XmlEnum("ItemSlide")]
        ItemSlide,

        /// <summary>XSD token is <c>Javascript</c> (no capital S).</summary>
        [XmlEnum("Javascript")]
        Javascript,

        [XmlEnum("TestConfiguration")]
        TestConfiguration,

        [XmlEnum("ErrorMark")]
        ErrorMark,

        [XmlEnum("KeyOutline")]
        KeyOutline,

        [XmlEnum("ResponseKey")]
        ResponseKey

    }
}
