using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;

namespace IAT.Core.Domain;

/// <summary>
/// Defines a base class for stimuli used in an IAT experiment, providing common 
/// properties and abstract methods for validation and display preview.
/// </summary>
public abstract partial class Stimulus : ObservableObject
{
    /// <summary>
    /// The guid the stimulus will be identifiedby
    /// </summary>
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display order for the item.
    /// </summary>
    [ObservableProperty]
    private int _displayOrder;

    /// <summary>
    /// Gets or sets the originating block for the stimulus.
    /// </summary>
    [ObservableProperty]
    private int _originatingBlock;
    
    /// <summary>
    /// Gets or sets the keyed direction for the stimulus.
    /// </summary>
    [ObservableProperty]
    private KeyedDirection _keyedDirection = KeyedDirection.None;

    // Pure domain behavior only
    public abstract ValidationResult Validate();
    public abstract string GetDisplayPreview();
}