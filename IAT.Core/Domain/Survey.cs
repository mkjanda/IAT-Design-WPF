using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;

namespace IAT.Core.Domain;

/// <summary>
/// A survey (questionnaire) that can be administered before or after the IAT.
/// Owned by <see cref="IatTest"/>; order of <see cref="Items"/> is authoritative.
/// </summary>
public partial class Survey : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the survey.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>0 = no timeout. Otherwise the questionnaire auto-submits after this many seconds.</summary>
    [ObservableProperty]
    private int _timeoutSeconds;

    /// <summary>When true every question is optional unless the individual question overrides it.</summary>
    [ObservableProperty]
    private bool _allQuestionsOptional;

    /// <summary>Ordered list of headers, instructions, images and questions.</summary>
    public ObservableCollection<SurveyItem> Items { get; } = new();

    /// <summary>
    /// JSON-friendly alias for <see cref="Items"/>. Get-only observable collections do not
    /// round-trip through System.Text.Json on their own.`
    /// </summary>
    public List<SurveyItem> AllItems
    {
        get => Items.ToList();
        set
        {
            if (Items.Count > 0 || value is null) return;
            Items.Clear();
            foreach (var item in value)
                Items.Add(item);
        }
    }
}

/// <summary>
/// Base for all survey content items. Polymorphic for JSON packaging.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "ItemType")]
[JsonDerivedType(typeof(SurveyHeader), "Header")]
[JsonDerivedType(typeof(SurveyInstruction), "Instruction")]
[JsonDerivedType(typeof(SurveyImage), "Image")]
[JsonDerivedType(typeof(SurveyQuestion), "Question")]
public abstract partial class SurveyItem : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the survey item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}

// ── Non-question items ──────────────────────────────────────────────

public partial class SurveyHeader : SurveyItem
{
    [ObservableProperty]
    private string _text = string.Empty;

    /// <summary>
    /// Caption style consumed by the SVG header effect (font, colors, separator).
    /// Always present so package load of older tests still gets the factory defaults.
    /// </summary>
    [ObservableProperty]
    private SurveyHeaderStyle _style = new();
}

public partial class SurveyInstruction : SurveyItem
{
    [ObservableProperty]
    private string _text = string.Empty;
}

public partial class SurveyImage : SurveyItem
{
    /// <summary>Reference to an existing image asset already managed by the test.</summary>
    [ObservableProperty]
    private Guid _imageId;
}

// ── Question ────────────────────────────────────────────────────────

public partial class SurveyQuestion : SurveyItem
{
    [ObservableProperty]
    private string _text = string.Empty;

    /// <summary>null = inherit from <see cref="Survey.AllQuestionsOptional"/>.</summary>
    [ObservableProperty]
    private bool? _isOptional;

    [ObservableProperty]
    private ResponseDefinition? _response;
}
