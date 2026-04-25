using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using com.sun.org.apache.bcel.@internal.generic;

namespace IAT.Core.Domain;

/// <summary>
/// The abstract base class for all response types in the survey system. This class defines the common interface and properties 
/// for different types of responses that can be associated with survey items. Each specific response type (e.g., BooleanResponse, 
/// LikertResponse, DateResponse) will inherit from this base class and implement its own unique properties and behavior while 
/// adhering to the common structure defined here. The ResponseType property is used to identify the specific type of response being 
/// represented, allowing for appropriate handling and processing of survey responses based on their type.
/// </summary>
public abstract partial class Response : ObservableObject
{
    /// <summary>
    /// Gets the response type associated with the current instance.
    /// </summary>
    public ResponseType ResponseType => ResponseType.FromType(this.GetType());

}

/// <summary>
/// Represents a response that encapsulates a Boolean value and provides associated statements for the true and false
/// outcomes.
/// </summary>
/// <remarks>Use this type when a response must convey a Boolean result along with descriptive statements for each
/// possible outcome. This is commonly used in scenarios where both the result and its explanation are required for
/// display or logging purposes.</remarks>
public partial class BooleanResponse : Response
{
    [ObservableProperty]
    private string _trueStatement = string.Empty;
    [ObservableProperty]
    private string _falseStatement = string.Empty;
}

/// <summary>
/// Represents a response that enforces minimum and maximum length constraints on its content.
/// </summary>
/// <remarks>Use this class when a response must adhere to specific length boundaries. The minimum and maximum
/// length values define the allowed range for the response content. This type is typically used in scenarios where
/// input or output length must be validated or restricted.</remarks>
public partial class BoundedLengthResponse : Response
{
    [ObservableProperty]
    private int _minLength = 0;
    [ObservableProperty]
    private int _maxLength = 0;
}

/// <summary>
/// Represents a response that contains a bounded numeric value, including its minimum and maximum limits.
/// </summary>
/// <remarks>Use this type when a response must convey a numeric value constrained within specific bounds. The
/// minimum and maximum values define the valid range for the response.</remarks>
public partial class BoundedNumberResponse : Response
{
    [ObservableProperty]
    private double _minValue = 0;
    [ObservableProperty]
    private double _maxValue = 0;
}

/// <summary>
/// Represents a response that contains date range information, including optional minimum and maximum date constraints.
/// </summary>
/// <remarks>Use this class to handle responses where a date or a range of dates is required, such as in date
/// picker dialogs or date validation scenarios. The presence of minimum and maximum date constraints can be determined
/// using the corresponding properties.</remarks>
public partial class DateResponse : Response
{
    [ObservableProperty]
    private bool _hasMinDate = false;
    [ObservableProperty]
    private bool _hasMaxDate = false;
    [ObservableProperty]
    private DateTime _minDate = DateTime.MinValue;
    [ObservableProperty]
    private DateTime _maxDate = DateTime.MaxValue;
}

/// <summary>
/// Represents a response that specifies a fixed number of digits required for input.
/// </summary>
public partial class FixedDigitResponse : Response
{
    [ObservableProperty]
    private int _numDigits = 0;
}

/// <summary>
/// Represents a response that contains an instruction, as identified by the response type.
/// </summary>
public partial class InstructionResponse : Response
{
}

/// <summary>
/// Represents a response to a Likert-scale question, capturing the selected choice and whether the scoring is reversed.
/// </summary>
/// <remarks>Use this class to model user responses for survey questions that utilize a Likert scale, such as
/// rating agreement or satisfaction. The response includes the available choices and supports reverse scoring for
/// negatively worded items.</remarks>
public partial class LikertResponse : Response
{
    [ObservableProperty]
    private bool _isReverseScored = false;
    [ObservableProperty]
    private List<string> _choices = new();
}

/// <summary>
/// Represents a response that contains image data or references an image resource.
/// </summary>
/// <remarks>Use this class when handling responses that specifically relate to images, such as retrieving or
/// processing image content in a response pipeline. This type distinguishes image responses from other response types
/// within the system.</remarks>
public partial class ImageResponse : Response
{
    [ObservableProperty]
    private Guid _imageId;
}

/// <summary>
/// Represents a response that allows selection of multiple boolean options from a predefined set of choices.
/// </summary>
/// <remarks>Use this type when a question or prompt requires the user to select one or more options from a list,
/// with constraints on the minimum and maximum number of selections. The available choices and selection limits are
/// defined by the corresponding properties.</remarks>
public partial class MultiBooleanResponse : Response
{
    [ObservableProperty]
    private int _minSelections = 0;
    [ObservableProperty]
    private int _maxSelections = 0;
    [ObservableProperty]
    private List<string> _choices = new();
}

/// <summary>
/// Represents a response that contains multiple selectable choices.
/// </summary>
/// <remarks>Use this class when a response may include more than one possible answer or option. This type extends
/// the base Response class to support scenarios where multiple selections are required.</remarks>
public partial class MultipleResponse : Response
{
    [ObservableProperty]
    private List<string> _choices = new();
}

/// <summary>
/// Represents a response that contains a regular expression pattern.
/// </summary>
/// <remarks>Use this type to encapsulate responses where a regular expression is required, such as pattern
/// matching or validation scenarios. Inherits common response functionality from the base Response class.</remarks>
public partial class RegExResponse : Response
{
    [ObservableProperty]
    private string _pattern = string.Empty;
}

/// <summary>
/// Represents a response that allows selection of multiple choices, each associated with a specific weight.
/// </summary>
/// <remarks>Use this type when a response requires users to select multiple options and assign a weight to each
/// selection. The weights can be used to indicate preference, importance, or relevance among the selected choices. This
/// class extends the base Response type and is identified by the WeightedMultiple response type.</remarks>
public partial class WeightedMultipleResponse : Response
{
    [ObservableProperty]
    private List<(string Choice, int Weight)> _choices = new();
}

/// <summary>
/// Represents the caption text and associated visual properties for a survey element.
/// </summary>
/// <remarks>Use this class to configure the display attributes of a survey caption, including its text, font, and
/// color settings. This type is typically used to customize the appearance of captions in survey interfaces.</remarks>
public partial class SurveyCaption : ObservableObject
{
    [ObservableProperty]
    private string _caption = string.Empty;
    [ObservableProperty]
    private string _fontFamilyName = string.Empty;
    [ObservableProperty]
    private double _fontSize = 0;
    [ObservableProperty]
    private Color _color = Colors.White;
    [ObservableProperty]    
    private Color _backgroundColor = Colors.Black;
    [ObservableProperty]
    private Color _borderColor = Colors.White;
}

/// <summary>
/// Represents a single item in a survey, including the question text, whether the item is optional, and the associated
/// response.
/// </summary>
/// <remarks>Use this class to model individual questions within a survey. Each instance holds the question's
/// text, indicates if a response is required, and stores the user's response. This type is typically used as part of a
/// collection to represent an entire survey.</remarks>
public partial class SurveyItem : ObservableObject
{
    [ObservableProperty]
    private string _questionText = string.Empty;
    [ObservableProperty]
    private bool _optional = false;
    [ObservableProperty]
    private Response _response = null!;
}

/// <summary>
/// Represents a survey that contains a collection of survey items, a caption, a survey type, and a configurable
/// timeout.
/// </summary>
/// <remarks>The Survey class provides the core structure for defining and managing a survey, including its
/// metadata and the items it contains. It supports property change notification for data binding scenarios by
/// inheriting from ObservableObject. The class is typically used in applications that require dynamic survey creation,
/// display, or editing.</remarks>
public partial class Survey : ObservableObject
{
    [ObservableProperty]
    private SurveyType _surveyType = SurveyType.Before;
    [ObservableProperty]
    private int _timeout = -1;
    [ObservableProperty]
    private SurveyCaption _caption = new();
    [ObservableProperty]
    private ObservableCollection<SurveyItem> _items = new();
}
