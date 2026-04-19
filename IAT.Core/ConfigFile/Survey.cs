using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;


namespace IAT.Core.ConfigFile;


public class Survey
{
    /// <summary>
    /// The number of items in the survey. This property is used to specify the total count of survey items included in a survey 
    /// configuration. It is represented as an integer value and is typically used to validate the completeness of the survey setup, 
    /// ensuring that all intended items are accounted for before deployment. The NumItems property helps maintain the integrity of 
    /// the survey structure and can be used for error checking during the survey creation process.
    /// </summary>
    [XmlAttribute(AttributeName = "NumItems", Form = XmlSchemaForm.Unqualified)]
    public int NumItems { get; set; }

    /// <summary>
    /// A flag that dictates whether the survey has a caption. This property is used to indicate whether a caption should be displayed for the survey. 
    /// It is represented as a boolean value, where true indicates that a caption is present and should be rendered, while false indicates that no 
    /// caption is associated with the survey. The HasCaption property allows for conditional rendering of the caption element in the survey interface, 
    /// enabling customization of the survey's visual presentation based on the presence or absence of a caption.
    /// </summary>
    [XmlAttribute(AttributeName = "HasCaption", Form = XmlSchemaForm.Unqualified)]
    public bool HasCaption { get; set; }

    /// <summary>
    /// Gets or sets the name associated with the current object.
    /// </summary>
    [XmlElement(ElementName = "Name", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public String Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout duration for the operation, in seconds.
    /// </summary>
    /// <remarks>A value of zero may indicate that no timeout is applied, depending on the consuming
    /// implementation. Ensure that the specified value is appropriate for the expected operation duration to avoid
    /// premature termination or excessive waiting.</remarks>
    [XmlElement(ElementName = "Timeout", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int Timeout { get; set; }

    /// <summary>
    /// Gets or sets the caption associated with the survey.
    /// </summary>
    [XmlElement(ElementName = "Caption", Form = XmlSchemaForm.Unqualified, Type = typeof(SurveyCaption))]
    public SurveyCaption Caption { get; set; } = new SurveyCaption();

    /// <summary>
    /// Gets or sets the collection of survey items associated with the survey.
    /// </summary>
    [XmlArray("SurveyItem")]
    [XmlArrayItem("SurveyItem", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(SurveyItem))]
    public SurveyItem[] SurveyItems { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the Survey class.
    /// </summary>
    public Survey() { }
}