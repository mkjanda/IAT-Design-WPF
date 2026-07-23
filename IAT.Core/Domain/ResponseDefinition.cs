using System.Text.Json.Serialization;

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
public abstract class ResponseDefinition
{
}

/// <summary>
/// Likert-scale response (numeric range with optional labels and reverse scoring).
/// </summary>
public sealed class LikertResponse : ResponseDefinition
{
    public int Min { get; set; } = 1;
    public int Max { get; set; } = 5;
    public List<string> Labels { get; set; } = new();
    public bool ReverseScored { get; set; }
}

/// <summary>
/// Single-select multiple choice.
/// </summary>
public sealed class MultipleChoiceResponse : ResponseDefinition
{
    public List<string> Choices { get; set; } = new();
}

/// <summary>
/// Multi-select with optional min/max selection constraints.
/// </summary>
public sealed class MultiSelectResponse : ResponseDefinition
{
    public List<string> Choices { get; set; } = new();
    public int? MinSelections { get; set; }
    public int? MaxSelections { get; set; }
}

/// <summary>
/// Date response with optional bounds.
/// </summary>
public sealed class DateResponse : ResponseDefinition
{
    public DateOnly? MinDate { get; set; }
    public DateOnly? MaxDate { get; set; }
}

/// <summary>
/// Fixed number of digits (e.g. PIN / code entry).
/// </summary>
public sealed class FixedDigitsResponse : ResponseDefinition
{
    public int DigitCount { get; set; } = 4;
}

/// <summary>
/// Free text with length bounds.
/// </summary>
public sealed class BoundedTextResponse : ResponseDefinition
{
    public int MinLength { get; set; }
    public int MaxLength { get; set; } = 500;
}

/// <summary>
/// Numeric value with optional bounds and decimal places.
/// </summary>
public sealed class BoundedNumberResponse : ResponseDefinition
{
    public double? Min { get; set; }
    public double? Max { get; set; }
    public int? DecimalPlaces { get; set; }
}

/// <summary>
/// Text validated against a regular expression.
/// </summary>
public sealed class RegexResponse : ResponseDefinition
{
    public string Pattern { get; set; } = @".+";
    public string? ValidationMessage { get; set; }
}
