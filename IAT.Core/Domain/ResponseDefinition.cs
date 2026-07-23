using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IAT.Core.Domain;

/// <summary>
/// Abstract base for all survey response definitions. Discriminated by JSON type for polymorphic serialization.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "ResponseType")]
[JsonDerivedType(typeof(LikertResponse), "Likert")]
[JsonDerivedType(typeof(MultipleChoiceResponse), "MultipleChoice")]
[JsonDerivedType(typeof(MultiSelectResponse), "MultiSelect")]
[JsonDerivedType(typeof(DateResponse), "Date")]
[JsonDerivedType(typeof(FixedDigitsResponse), "FixedDigits")]
[JsonDerivedType(typeof(BoundedTextResponse), "BoundedText")]
[JsonDerivedType(typeof(BoundedNumberResponse), "BoundedNumber")]
[JsonDerivedType(typeof(RegexResponse), "Regex")]
public abstract partial class ResponseDefinition : ObservableObject
{
}

/// <summary>
/// Likert-scale response (numeric range with optional labels and reverse scoring).
/// </summary>
public partial class LikertResponse : ResponseDefinition
{
    [ObservableProperty] private int _min = 1;
    [ObservableProperty] private int _max = 5;
    [ObservableProperty] private bool _reverseScored;

    /// <summary>Optional labels for each scale point (index 0 = Min). May be empty.</summary>
    public ObservableCollection<string> Labels { get; } = new();
}

/// <summary>
/// Single-select multiple choice.
/// </summary>
public partial class MultipleChoiceResponse : ResponseDefinition
{
    /// <summary>
    /// Gets the collection of choices for the multiple choice response. Each choice is represented as a string.
    /// </summary>
    public ObservableCollection<string> Choices { get; } = new();
}

/// <summary>
/// Multi-select with optional min/max selection constraints.
/// </summary>
public partial class MultiSelectResponse : ResponseDefinition
{
    /// <summary>
    /// Gets the collection of choices for the multi-select response. Each choice is represented as a string.
    /// </summary>
    public ObservableCollection<string> Choices { get; } = new();
    [ObservableProperty] private int? _minSelections;
    [ObservableProperty] private int? _maxSelections;
}

/// <summary>
/// Date response with optional bounds (ISO yyyy-MM-dd stored as DateOnly).
/// </summary>
public partial class DateResponse : ResponseDefinition
{
    [ObservableProperty] private DateOnly? _minDate;
    [ObservableProperty] private DateOnly? _maxDate;
}

/// <summary>
/// Fixed number of digits (e.g. PIN / code entry).
/// </summary>
public partial class FixedDigitsResponse : ResponseDefinition
{
    [ObservableProperty] private int _digitCount = 4;
}

/// <summary>
/// Free text with length bounds.
/// </summary>
public partial class BoundedTextResponse : ResponseDefinition
{
    [ObservableProperty] private int _minLength;
    [ObservableProperty] private int _maxLength = 500;
}

/// <summary>
/// Numeric value with optional bounds and decimal places.
/// </summary>
public partial class BoundedNumberResponse : ResponseDefinition
{
    [ObservableProperty] private double? _min;
    [ObservableProperty] private double? _max;
    [ObservableProperty] private int? _decimalPlaces;
}

/// <summary>
/// Text validated against a regular expression.
/// </summary>
public partial class RegexResponse : ResponseDefinition
{
    [ObservableProperty] private string _pattern = @".+";
    [ObservableProperty] private string? _validationMessage;
}
