using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents a single item in a survey, including its display text, response, and whether it is optional.
/// </summary>
/// <remarks>Use this class to define individual questions or prompts within a survey. Each SurveyItem contains
/// the text to display to the user, an associated response, and a flag indicating if a response is required.</remarks>
public class SurveyItem
{
    /// <summary>
    /// Gets or sets a value indicating whether the survey item is optional. If true, the respondent is not required to provide a response to this item.
    /// </summary>
    [XmlAttribute(AttributeName = "Optional", Form = XmlSchemaForm.Unqualified)]
    public bool Optional { get; set; }
    
    /// <summary>
    /// Gets or sets the text to display for this survey item.
    /// </summary>
    [XmlElement(ElementName = "Text", Form = XmlSchemaForm.Unqualified)]
    public String Text { get; set; }

    /// <summary>
    /// Gets or sets the response data associated with the current operation.
    /// </summary>
    [XmlElement(ElementName = "Response", Form = XmlSchemaForm.Unqualified, Type = typeof(Response))]
    public Response Response { get; set; }
}
