using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using IAT.Core.Enumerations;


namespace IAT.Core.Serializable
{
    /// <summary>
    /// Conntains the responses to a survey, represented as an array of strings. Each string in the array corresponds to an individual survey response.
    /// </summary>
    [XmlRoot("SurveyResults")]
    public class SurveyResponse
    {
        /// <summary>
        /// Gets or sets the collection of responses for individual survey items.
        /// </summary>
        [XmlArray]
        [XmlArrayItem("SurveyResult")]
        public string[] ItemResponses { get; set; } = Array.Empty<string>();
    }
}
