using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// The base class to all survey item response types. This class serves as a common ancestor for various specific response types, 
/// such as Boolean, BoundedLength, BoundedNumber, FixedDigit, RegEx, WeightedMultiple, MultiBoolean, Date, Likert, and Multiple. 
/// Each of these derived classes represents a specific type of response that can be used in survey items to capture user input or 
/// system-generated responses. The Response class itself is abstract and cannot be instantiated directly; it provides a common 
/// interface and shared functionality for all response types in the survey configuration system.
/// </summary>
[XmlInclude(typeof(BoundedLength))]
[XmlInclude(typeof(BoundedNumber))]
[XmlInclude(typeof(FixedDigit))]
[XmlInclude(typeof(RegEx))]
[XmlInclude(typeof(WeightedMultiple))]
[XmlInclude(typeof(MultiBoolean))]
[XmlInclude(typeof(Date))]
[XmlInclude(typeof(Likert))]
[XmlInclude(typeof(Multiple))]
[XmlInclude(typeof(Boolean))]
public abstract class Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlIgnore]
    public abstract ResponseType ResponseType { get; }

    /// <summary>
    /// Initializes a new instance of the Response class.
    /// </summary>
    public Response() { }
}

/// <summary>
/// Represents a response that indicates a binary choice, typically between true and false, within a response processing
/// system.
/// </summary>
/// <remarks>Use the Boolean class to model responses where only two possible outcomes are valid, such as yes/no
/// or true/false decisions. This class provides properties to specify the statements associated with each outcome,
/// enabling clear handling of conditional logic in response workflows.</remarks>
public class Boolean : Response
{
    /// <summary>
    /// The response type for this class is always ResponseType.Boolean, indicating that the response is a binary choice between two options, 
    /// typically represented as "True" and "False". This property is overridden to return the specific response type associated with this class, 
    /// ensuring that any instance of Boolean will be correctly identified as a boolean response when processed or serialized.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.Boolean;

    /// <summary>
    /// Gets or sets the statement that is executed when the associated condition evaluates to true.
    /// </summary>
    [XmlElement(ElementName = "TrueStatement", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public string TrueStatement { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the statement to be used when a condition evaluates to false.
    /// </summary>
    [XmlElement(ElementName = "FalseStatement", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public string FalseStatement { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the Boolean structure.
    /// </summary>
    public Boolean() { }

}

/// <summary>
/// Represents a response type that is defined by a minimum and maximum length constraint. This class is used to specify responses that must adhere to a certain length range,
/// </summary>
public class BoundedLength : Response
{
    /// <summary>
    /// Gets the response type for the current instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.BoundedLength;

    /// <summary>
    /// Gets or sets the minimum allowed length for the value.
    /// </summary>
    /// <remarks>Set this property to specify the smallest number of characters or elements permitted. Values
    /// less than this minimum may be considered invalid depending on the context in which the property is
    /// used.</remarks>
    [XmlElement(ElementName = "MinLength", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed length for the associated value.
    /// </summary>
    [XmlElement(ElementName = "MaxLength", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int MaxLength { get; set; }

    /// <summary>
    /// Initializes a new instance of the BoundedLength class.
    /// </summary>
    public BoundedLength() { }
}

public class BoundedNumber : Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.BoundedNum;

    /// <summary>
    /// Gets or sets the minimum allowable value.
    /// </summary>
    [XmlElement(ElementName = "MinValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowable value.
    /// </summary>
    [XmlElement(ElementName = "MaxValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal MaxValue { get; set; }

    /// <summary>
    /// Instantiates an object of type bounded Number
    /// </summary>
    public BoundedNumber() { }
}

public class FixedDigit : Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.FixedDig;

    /// <summary>
    /// Gets or sets the number of digits to use in the operation.
    /// </summary>
    [XmlElement(ElementName = "NumDigs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public int NumDigs { get; set; }

    /// <summary>
    /// Initializes a new instance of the FixedDigit class.
    /// </summary>
    public FixedDigit() { }
}

/// <summary>
/// Represents a response type for Likert-scale items, supporting reverse scoring and a customizable set of choices.
/// </summary>
public class Likert : Response
{
    /// <summary>
    /// Gets the response type for this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.Likert;

    /// <summary>
    /// Gets or sets a value indicating whether the item is reverse scored.
    /// </summary>
    /// <remarks>Set this property to <see langword="true"/> if higher raw values should be interpreted as
    /// lower scores, and vice versa. This is commonly used in scoring systems where some items are phrased
    /// negatively.</remarks>
    [XmlAttribute("ReverseScored", Form = XmlSchemaForm.Unqualified)]
    public bool ReverseScored { get; set; }

    /// <summary>
    /// Gets or sets the collection of available choices.
    /// </summary>
    [XmlArray]
    [XmlArrayItem("Text", Form = XmlSchemaForm.Unqualified)]
    public String[] Choices { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the Likert class.
    /// </summary>
    public Likert() { }

}

/// <summary>
/// Represents a response that allows selection of multiple boolean options from a predefined set of choices.
/// </summary>
/// <remarks>Use this class to define questions or prompts where the user can select more than one option, with
/// configurable minimum and maximum selection limits. The available choices are specified as an array of strings. The
/// selection constraints are enforced by the MinSelections and MaxSelections properties.</remarks>
public class MultiBoolean : Response
{
    /// <summary>
    /// Gets the response type for this operation.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.MultiBoolean;

    /// <summary>
    /// Gets or sets the minimum number of selections required.
    /// </summary>
    [XmlAttribute(AttributeName = "MinSelections")]
    public int MinSelections { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of selections allowed.
    /// </summary>
    [XmlAttribute(AttributeName = "MaxSelections")]
    public int MaxSelections { get; set; }

    /// <summary>
    /// Gets or sets the collection of available choices.
    /// </summary>
    [XmlArray]
    [XmlArrayItem(ElementName = "Text", Form = XmlSchemaForm.Unqualified)]
    public string[] Choices { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the MultiBoolean class.
    /// </summary>
    public MultiBoolean() { }
}

/// <summary>
/// Represents a response that contains multiple selectable text choices.
/// </summary>
/// <remarks>Use this class to model responses where a user or system can select from a predefined set of text
/// options. This type is commonly used in scenarios such as multiple-choice questions or selection-based
/// prompts.</remarks>
public class Multiple : Response
{
    /// <summary>
    /// Gets the response type for this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.Multiple;

    /// <summary>
    /// Gets or sets the collection of available choices.
    /// </summary>
    [XmlArray]
    [XmlArrayItem(ElementName = "Text", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public string[] Choices { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the Multiple class.
    /// </summary>
    public Multiple() { }
}

/// <summary>
/// Represents a response that contains a regular expression pattern.
/// </summary>
/// <remarks>Use this class to encapsulate a regular expression as part of a response. The regular expression can
/// be used for pattern matching or validation scenarios where a pattern needs to be communicated or processed as part
/// of a response payload.</remarks>
public class RegEx : Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.RegEx;

    /// <summary>
    /// Gets or sets the regular expression pattern used for validation or matching operations.
    /// </summary>
    [XmlElement(ElementName = "RegEx", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public string RegularExpression { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the RegEx class.
    /// </summary>
    /// <remarks>Use this constructor to create a RegEx object with default settings. To define a specific
    /// pattern or options, use an overloaded constructor that accepts parameters.</remarks>
    public RegEx(){ }
}

/// <summary>
/// Represents a selectable response option with an associated weight for use in weighted multiple choice scenarios.
/// </summary>
/// <remarks>Use this class to define individual choices where each option has a specific weight that influences
/// its likelihood of selection or scoring. The weight typically determines the relative importance or probability of
/// the choice when evaluated in the context of a weighted multiple choice response.</remarks>
public class WeightedChoice 
{
    /// <summary>
    /// Gets or sets the text content associated with this instance.
    /// </summary>
    [XmlElement(ElementName = "Text", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weight value.
    /// </summary>
    [XmlElement(ElementName = "Weight", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int Weight { get; set; } = 0;
    
    /// <summary>
    /// Initializes a new instance of the WeightedChoice class.
    /// </summary>
    public WeightedChoice() { }
}

/// <summary>
/// Represents a response that contains multiple weighted choices.
/// </summary>
public class WeightedMultiple : Response
{
    /// <summary>
    /// Gets the response type for this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.WeightedMultiple;

    /// <summary>
    /// Gets or sets the collection of weighted choices available for selection.
    /// </summary>
    [XmlArray]
    [XmlArrayItem("Choice", Form = XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(WeightedChoice))]
    public WeightedChoice[] Choices { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the WeightedMultiple class.
    /// </summary>
    public WeightedMultiple() { }
}

/// <summary>
/// Represents a date with year, month, and day components.
/// </summary>
/// <remarks>This class provides a simple structure for storing date information without time or timezone details.
/// It is commonly used for serialization scenarios where only the date is required.</remarks>
public class DateEntry
{
    /// <summary>
    /// Gets or sets the year associated with this instance.
    /// </summary>
    [XmlElement(ElementName = "Year", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int Year { get; set; } = 0;

    /// <summary>
    /// Gets or sets the month represented by this instance.
    /// </summary>
    /// <remarks>Valid values are typically in the range 1 (January) through 12 (December).</remarks>
    [XmlElement(ElementName = "Month", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int Month { get; set; } = 0;

    /// <summary>
    /// Gets or sets the day of the month represented by this instance.
    /// </summary>
    [XmlElement(ElementName = "Day", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public int Day { get; set; } = 0;
}

/// <summary>
/// Represents a response that contains date information, including optional start and end dates.
/// </summary>
/// <remarks>Use this class to encapsulate date-related response data, such as specifying a single date or a date
/// range. The presence of start and end dates is indicated by the HasStartDate and HasEndDate properties. Inherits from
/// Response.</remarks>
public class Date : Response
{
    /// <summary>
    /// Gets the response type for this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.Date;

    /// <summary>
    /// Gets or sets a value indicating whether a start date is specified.
    /// </summary>
    [XmlAttribute("HasStartDate", Form = XmlSchemaForm.Unqualified)]
    public bool HasStartDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an end date is specified.
    /// </summary>
    [XmlAttribute("HasEndDate", Form = XmlSchemaForm.Unqualified)]
    public bool HasEndDate { get; set; }

    /// <summary>
    /// Gets or sets the date value associated with this response.
    /// </summary>
    [XmlElement("StartDate", Form = XmlSchemaForm.Unqualified, IsNullable = true, Type = typeof(DateEntry))]
    public DateEntry DateValue { get; set; } = new DateEntry();

    /// <summary>
    /// Gets or sets the end date for the associated period or event.
    /// </summary>
    [XmlElement("EndDate", Form = XmlSchemaForm.Unqualified, IsNullable = true, Type = typeof(DateEntry))]
    public DateEntry EndDate{ get; set; } = new DateEntry();

    /// <summary>
    /// Initializes a new instance of the Date class.
    /// </summary>
    public Date() { }
}

/// <summary>
/// Represents a response item containing survey metadata and a collection of unique response strings for XML
/// serialization.
/// </summary>
/// <remarks>Use this class to encapsulate survey response data, including the survey name, item number, whether
/// the item is additive, and a list of unique responses. The class is designed for XML serialization scenarios where
/// each unique response is represented as an XML element. Property values should be set prior to serialization to
/// ensure correct output.</remarks>
public class UniqueResponseItem
{
    /// <summary>
    /// Gets or sets the collection of unique response strings to be serialized as an XML array.
    /// </summary>
    /// <remarks>Each element in the collection represents a distinct response. The property is serialized
    /// with the XML element name "UniqueResponses" for each item. Null values are allowed in the collection.</remarks>
    [XmlArray]
    [XmlArrayItem("UniqueResponses", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
    private List<string> UniqueResponses = new List<string>();

    /// <summary>
    /// Gets or sets the name of the survey.
    /// </summary>
    [XmlElement("SurveyName", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public string SurveyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item number associated with this instance.
    /// </summary>
    [XmlElement("ItemNum", Form = XmlSchemaForm.Unqualified, IsNullable = false)]   
    public int ItemNum { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the operation is additive.
    /// </summary>
    [XmlElement("Additive", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public bool Additive { get; set; } = false;
}