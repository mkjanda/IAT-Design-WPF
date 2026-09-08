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
[XmlInclude(typeof(TrueFalse))]
[XmlInclude(typeof(BoundedText))]
[XmlInclude(typeof(BoundedNumber))]
[XmlInclude(typeof(Date))]
[XmlInclude(typeof(FixedDigit))]
[XmlInclude(typeof(Likert))]
[XmlInclude(typeof(MultiSelect))]
[XmlInclude(typeof(MultiChoice))]
[XmlInclude(typeof(RegEx))]
[XmlInclude(typeof(Instruction))]
public abstract class Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlIgnore]
    public abstract ResponseType ResponseType { get; }

    [XmlElement("Format", Form = XmlSchemaForm.Unqualified)]
    public SurveyFormat Format { get; set; } = new();

}

/// <summary>Instruction / header-less text row. No participant input.</summary>
public class Instruction : Response
{
    [XmlIgnore]
    public override ResponseType ResponseType => ResponseType.Instruction;


}

/// <summary>
/// Represents a response that indicates a binary choice, typically between true and false, within a response processing
/// system.
/// </summary>
/// <remarks>Use the TrueFalse class to model responses where only two possible outcomes are valid, such as yes/no
/// or true/false decisions. This class provides properties to specify the statements associated with each outcome,
/// enabling clear handling of conditional logic in response workflows.</remarks>
public class TrueFalse : Response
{
    /// <summary>
    /// The response type for this class is always ResponseType.Boolean, indicating that the response is a binary choice between two options, 
    /// typically represented as "True" and "False". This property is overridden to return the specific response type associated with this class, 
    /// ensuring that any instance of TrueFalse will be correctly identified as a boolean response when processed or serialized.
    /// </summary>
    [XmlIgnore]
    public override ResponseType ResponseType => ResponseType.TrueFalse; 

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
}

/// <summary>
/// Represents a response type that is defined by a minimum and maximum length constraint. This class is used to specify responses that must adhere to a certain length range,
/// </summary>
public class BoundedText : Response
{
    /// <summary>
    /// Gets the response type for the current instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.BoundedText;

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
}

/// <summary>
/// Represents a numeric response with defined minimum and maximum bounds.
/// </summary>
/// <remarks>Use this class to specify a response that must fall within a specific numeric range. The minimum and
/// maximum values define the inclusive bounds for valid responses. This type is typically used in scenarios where input
/// validation or range enforcement is required.</remarks>
public class BoundedNumber : Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.BoundedNumber;

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
}

/// <summary>
/// A class that defines a response type for fixed digit values, typically used in scenarios where a specific number of digits 
/// is required for input or output. This class inherits from the Response base class and specifies the ResponseType as FixedDig. 
/// The NumDigs property allows you to set the exact number of digits that should be used in the response, ensuring that the input 
/// or output adheres to a defined format. This is particularly useful in contexts such as PIN codes, verification numbers, or any 
/// scenario where a fixed-length numeric response is necessary.
/// </summary>
public class FixedDigit : Response
{
    /// <summary>
    /// Gets the type of response represented by this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.FixedDigit;

    /// <summary>
    /// Gets or sets the number of digits to use in the operation.
    /// </summary>
    [XmlElement(ElementName = "NumDigs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public int NumDigs { get; set; }
}

/// <summary>
/// Represents a response type for Likert-scale items, supporting reverse scoring and a customizable set of choices.
/// </summary>
public class Likert : Response
{
    /// <summary>
    /// Gets the response type for this instance.
    /// </summary>
    [XmlIgnore]
    [XmlAttribute("ResponseType", Form = XmlSchemaForm.Unqualified, Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.Likert;

    [XmlAttribute("NumChoices", Form = XmlSchemaForm.Unqualified)]
    public int NumChoices { get; set; }

    [XmlAttribute("ReverseScored", Form = XmlSchemaForm.Unqualified)]
    public bool ReverseScored { get; set; }

    [XmlElement("Choice", Form = XmlSchemaForm.Unqualified)]
    public List<string> Choices { get; set; } = new List<string>();

    /// <summary>
    /// Initializes a new instance of the Likert class.
    /// </summary>
    public Likert() { }

}

/// <summary>
/// Represents a response that allows selection of multiple options from a predefined set of choices.
/// </summary>
/// <remarks>Use this class to define questions or prompts where the user can select more than one option, with
/// configurable minimum and maximum selection limits. The available choices are specified as an array of strings. The
/// selection constraints are enforced by the MinSelections and MaxSelections properties.</remarks>
public class MultiSelect: Response
{
    /// <summary>
    /// Gets the response type for this operation.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.MultiSelect;

    [XmlElement("NumValues", Form = XmlSchemaForm.Unqualified)]
    public int NumValues { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of selections required.
    /// </summary>
    [XmlElement("MinSelections", Form = XmlSchemaForm.Unqualified)]
    public int MinSelections { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of selections allowed.
    /// </summary>
    [XmlElement("MaxSelections", Form = XmlSchemaForm.Unqualified)]
    public int MaxSelections { get; set; }

    [XmlElement("Label", Form = XmlSchemaForm.Unqualified)]
    public List<string> Choices { get; set; } = new List<string>();
}

/// <summary>
/// Represents a response that contains multiple selectable text choices.
/// </summary>
/// <remarks>Use this class to model responses where a user or system can select from a predefined set of text
/// options. This type is commonly used in scenarios such as multiple-choice questions or selection-based
/// prompts.</remarks>
public class MultiChoice : Response
{
    /// <summary>
    /// Gets the response type for this instance.
    /// </summary>
    [XmlAttribute("ResponseType", Type = typeof(ResponseType))]
    public override ResponseType ResponseType => ResponseType.MultiChoice;

    [XmlElement("NumChoices", Form = XmlSchemaForm.Unqualified)]
    public int NumChoices { get; set; }

    [XmlElement("Choice", Form = XmlSchemaForm.Unqualified)]
    public List<string> Choices { get; set; } = new List<string>();
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

    [XmlElement("Expression", Form = XmlSchemaForm.Unqualified)]
    public string RegularExpression { get; set; } = string.Empty;
}

/// <summary>
/// Represents a date with year, month, and day components.
/// </summary>
/// <remarks>This class provides a simple structure for storing date information without time or timezone details.
/// It is commonly used for serialization scenarios where only the date is required.</remarks>
/// <summary>Date bound used by SurveyPage.xslt (<c>StartDate[@HasValue eq 'True']</c>).</summary>
public class YearMonthDay
{
    [XmlAttribute("HasValue")]
    public string HasValue { get; set; } = "False";

    [XmlElement("Year")]
    public string Year { get; set; } = "0";

    [XmlElement("Month")]
    public string Month { get; set; } = "0";

    [XmlElement("Day")]
    public string Day { get; set; } = "0";

    public static YearMonthDay None() => new();

    public static YearMonthDay From(DateOnly date) => new()
    {
        HasValue = "True",
        Year = date.Year.ToString(),
        Month = date.Month.ToString(),
        Day = date.Day.ToString()
    };
}

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

    [XmlAttribute("HasStartDate", Form = XmlSchemaForm.Unqualified)]
    public bool HasStartDate
    {
        get { return field; }
        set { if (field == value) return; field = value; if (value == false) StartDate = DateOnly.MinValue; }
    } = false;

    [XmlAttribute("HasEndDate", Form = XmlSchemaForm.Unqualified)]
    public bool HasEndDate
    {
        get { return field; }
        set { if (field == value) return; field = value; if (value == false) EndDate = DateOnly.MaxValue; }
    } = false;

    [XmlIgnore]
    public DateOnly StartDate 
    { 
        get { return field; } 
        set { field = value; HasStartDate = value == DateOnly.MinValue ? false : true; } 
    } = DateOnly.MinValue;

    [XmlIgnore]
    public DateOnly EndDate
    {
        get { return field; }
        set { field = value; HasEndDate = value == DateOnly.MaxValue ? false : true; }
    } = DateOnly.MaxValue;


    [XmlElement("StartMonth", Form = XmlSchemaForm.Unqualified)]
    public int StartMonth
    {
        get { return StartDate.Month; }
        set { 
            var year = StartDate.Year;
            StartDate = StartDate.AddMonths(value - StartDate.Month); 
            StartDate = StartDate.AddYears(year - StartDate.Year);
        }
    }

    [XmlElement("StartDay", Form = XmlSchemaForm.Unqualified)]
    public int StartDay
    {
        get { return StartDate.Day; }
        set
        {
            var month = StartDate.Month;
            StartDate = StartDate.AddDays(value - StartDate.Day);
            StartDate = StartDate.AddMonths(month - StartDate.Month);
        }
    }

    [XmlElement("StartYear", Form = XmlSchemaForm.Unqualified)]
    public int StartYear
    {
        get { return StartDate.Year; }
        set { StartDate = StartDate.AddYears(value - StartDate.Year); }
    }

    [XmlElement("EndMonth", Form = XmlSchemaForm.Unqualified)]
    public int EndMonth
    {
        get { return EndDate.Month; }
        set
        {
            var year = EndDate.Year;
            EndDate = EndDate.AddMonths(value - EndDate.Month);
            EndDate = EndDate.AddYears(year - EndDate.Year);
        }
    }

    [XmlElement("EndDay", Form = XmlSchemaForm.Unqualified)]
    public int EndDay
    {
        get { return EndDate.Day; }
        set { 
            var month = EndDate.Month;
            EndDate = EndDate.AddDays(value - EndDate.Day);
            EndDate = EndDate.AddMonths(month - EndDate.Month);
        }
    }

    [XmlElement("EndYear", Form = XmlSchemaForm.Unqualified)]
    public int EndYear
    {
        get { return EndDate.Year; }
        set { EndDate = EndDate.AddYears(value - EndDate.Year); }
    }
}

