using CommunityToolkit.Mvvm.ComponentModel;

namespace IAT.Core.Domain;

public abstract partial class Stimulus : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private int _displayOrder;

    [ObservableProperty]
    private int _originatingBlock;

    // Pure domain behavior only
    public abstract ValidationResult Validate();
    public abstract string GetDisplayPreview();
}