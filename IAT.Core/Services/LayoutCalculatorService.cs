using System.Windows;
using IAT.Core.Domain;
using IAT.Core.Enumerations;

namespace IAT.Core.Services;

/// <summary>
/// Service responsible for calculating the final layout rectangles for various UI elements based on the provided layout configuration and 
/// any user overrides. This service applies default layout settings, allows for user-defined size overrides for specific regions, and computes 
/// the final rectangles that should be used for rendering the test interface. The service ensures that all layout adjustments are applied 
/// consistently and that the resulting rectangles are valid for use in the test interface.
/// </summary>
public interface ILayoutCalculatorService
{
    /// <summary>
    /// Applies default values to the specified layout configuration.
    /// </summary>
    /// <param name="layout">The layout configuration to which default values will be applied. Cannot be null.</param>
    void ApplyDefaults(Layout layout);

    /// <summary>
    /// Applies user-defined size overrides to the specified region within the given layout configuration.
    /// </summary>
    /// <remarks>If the specified region does not exist in the layout, no changes are made. This method
    /// updates only the size of the region, preserving other layout settings.</remarks>
    /// <param name="layout">The layout configuration to which the user overrides will be applied. Cannot be null.</param>
    /// <param name="layoutItem">The layout item within the layout to update. Cannot be null.</param>
    /// <param name="newSize">The new size to apply to the specified layout item.</param>
    void ApplyUserOverrides(Layout layout, LayoutItem layoutItem, Size newSize);

    /// <summary>
    /// Calculates the final bounding rectangle for the specified layout item within the given layout configuration.
    /// </summary>
    /// <param name="layout">The layout configuration that defines the arrangement and sizing rules for layout items.</param>
    /// <param name="li">The layout item for which to calculate the final bounding rectangle.</param>
    /// <returns>A Rect structure representing the final position and size of the layout item as determined by the layout
    /// configuration.</returns>
    Rect GetFinalRect(Layout layout, LayoutItem li);

    /// <summary>
    /// Calculates the final layout rectangles based on the specified layout configuration.
    /// </summary>
    /// <param name="layout">The layout configuration that defines the arrangement and sizing rules to apply when computing the final
    /// rectangles. Cannot be null.</param>
    /// <returns>A LayoutRects object containing the computed rectangles for the layout. The returned object reflects the final
    /// positions and sizes as determined by the provided configuration.</returns>
    LayoutRects GetFinalRects(Layout layout);
}

/// <summary>
/// Provides services for calculating and applying layout configurations, including default settings, user overrides,
/// and final layout rectangles.
/// </summary>
/// <remarks>This service is intended for use in scenarios where dynamic layout adjustments are required, such as
/// user-driven resizing or restoring default arrangements. It coordinates the application of user size overrides and
/// computes the resulting layout rectangles based on the current configuration.</remarks>
public class LayoutCalculatorService : ILayoutCalculatorService
{
    /// <summary>
    /// Restores the specified layout configuration to its default settings and clears any user-defined size overrides.
    /// </summary>
    /// <remarks>After calling this method, all customizations made to the layout's size settings are removed,
    /// and the layout reverts to its original default state.</remarks>
    /// <param name="layout">The layout configuration to reset. Cannot be null.</param>
    public void ApplyDefaults(Layout layout)
    {
        layout.RestoreDefaults();
        layout.UserSizeOverrides.Clear();
    }

    /// <summary>
    /// Expands the specified rectangle by the difference between its current size and the target size, centering the
    /// expansion equally on all sides.
    /// </summary>
    /// <remarks>If the target size is smaller than the rectangle's current size in either dimension, the
    /// rectangle will be deflated accordingly. The expansion or contraction is distributed equally on all sides to
    /// maintain the rectangle's center.</remarks>
    /// <param name="rect">The rectangle to be inflated. The rectangle's size and position are modified by this method.</param>
    /// <param name="sz">The target size to which the rectangle should be inflated. Both width and height must be greater than or equal
    /// to the rectangle's current size.</param>
    private void Inflate(Rect rect, Size sz)
    {
        var inflateSz = new Size((sz.Width - rect.Size.Width) / 2, (sz.Height - rect.Size.Height) / 2);
        rect.Inflate(inflateSz);
    }

    /// <summary>
    /// Applies a user-defined size override to a specified region within the given layout configuration.
    /// </summary>
    /// <remarks>If the specified size is smaller than the minimum allowed dimensions, the size is adjusted to
    /// a minimum width and height of 50 units. Subsequent calls with the same region name will overwrite previous
    /// overrides.</remarks>
    /// <param name="layout">The layout configuration to which the size override will be applied. Cannot be null.</param>
    /// <param name="layoutItem">The layout item within the layout to override. Cannot be null.</param>
    /// <param name="newSize">The new size to apply to the specified layout item. Width and height values less than 50 are automatically set to 50.</param>
    public void ApplyUserOverrides(Layout layout, LayoutItem layoutItem, Size newSize)
    {
        var size = new Size(Math.Max(50, newSize.Width), Math.Max(50, newSize.Height));
        layout.UserSizeOverrides[layoutItem] = size;
    }

    public Rect GetFinalRect(Layout layout, LayoutItem li)
    {
        if (layout.UserSizeOverrides.TryGetValue(li, out var sz))
        {
            Rect r;
            switch (li)
            {
                case LayoutItem.Interior:
                    return new Rect(0, 0, sz.Width, sz.Height);
                case LayoutItem.Stimulus:
                    r = layout.StimulusRect;
                    Inflate(r, sz);
                    return r;
                case LayoutItem.LeftKey:
                    return new Rect(layout.LeftKeyRect.Location, sz);
                case LayoutItem.RightKey:
                    return new Rect(layout.RightKeyRect.Size.Width - sz.Width, 0, sz.Width, sz.Height);
                case LayoutItem.ErrorMark:
                    Rect errorRect = layout.ErrorMarkRect;
                    errorRect.Inflate(sz);
                    return errorRect;
                case LayoutItem.BlockInstructions:
                    return new Rect((layout.InteriorRect.Size.Width - sz.Width) / 2, layout.InteriorRect.Size.Height - sz.Height,
                        sz.Width, sz.Height);
                case LayoutItem.MockItemInstructions:
                    r = layout.MockItemInstructionsRect;
                    Inflate(r, sz);
                    return r;
                case LayoutItem.KeyedInstructions:
                    r = layout.KeyedInstructionsRect;
                    Inflate(r, sz);
                    return r;
                case LayoutItem.TextInstructions:
                    r = layout.TextInstructionsRect;
                    Inflate(r, sz);
                    return r;
                case LayoutItem.ContinueInstructions:
                    return new Rect((layout.InteriorRect.Size.Width - sz.Width) / 2, layout.InteriorRect.Size.Height - sz.Height,
                        sz.Width, sz.Height);
            }
        }
        // If no user override exists for the specified layout item, return the default rectangle from the layout configuration.
        return li switch
        {
            LayoutItem.Interior => layout.InteriorRect,
            LayoutItem.Stimulus => layout.StimulusRect,
            LayoutItem.LeftKey => layout.LeftKeyRect,
            LayoutItem.RightKey => layout.RightKeyRect,
            LayoutItem.ErrorMark => layout.ErrorMarkRect,
            LayoutItem.BlockInstructions => layout.BlockInstructionsRect,
            LayoutItem.MockItemInstructions => layout.MockItemInstructionsRect,
            LayoutItem.KeyedInstructions => layout.KeyedInstructionsRect,
            LayoutItem.TextInstructions => layout.TextInstructionsRect,
            LayoutItem.ContinueInstructions => layout.ContinueInstructionsRect,
            _ => throw new ArgumentOutOfRangeException(nameof(li), $"Unsupported layout item: {li}")
        };
    }
    

    /// <summary>
    /// Calculates the final layout rectangles for all UI elements based on the specified layout configuration and any
    /// user-defined size overrides.
    /// </summary>
    /// <remarks>User-defined size overrides in the layout parameter take precedence over the default
    /// rectangle sizes. The returned rectangles reflect all adjustments specified in the configuration.</remarks>
    /// <param name="layout">The layout configuration containing the initial rectangles and any user size overrides to apply.</param>
    /// <returns>A LayoutRects object containing the computed rectangles for each UI element after applying all relevant
    /// overrides.</returns>
    public LayoutRects GetFinalRects(Layout layout)
    {
        LayoutRects rects = new LayoutRects()
        {
            Interior = layout.InteriorRect,
            Stimulus = layout.StimulusRect,
            LeftKey = layout.LeftKeyRect,
            RightKey = layout.RightKeyRect,
            ErrorMark = layout.ErrorMarkRect,
            BlockInstructions = layout.BlockInstructionsRect,
            MockItemInstructions = layout.MockItemInstructionsRect,
            KeyedInstructions = layout.KeyedInstructionsRect,
            TextInstructions = layout.TextInstructionsRect,
            ContinueInstructions = layout.ContinueInstructionsRect
        };

        foreach (var (layoutItem, sz) in layout.UserSizeOverrides)
        {
            Rect r;
            switch (layoutItem) 
            {
                case LayoutItem.Interior:
                    rects = rects with { Interior = new Rect(0, 0, sz.Width, sz.Height) };
                    break;
                case LayoutItem.Stimulus:
                    r = layout.StimulusRect;
                    Inflate(r, sz);
                    rects = rects with { Stimulus = r };
                    break;
                case LayoutItem.LeftKey:
                    rects = rects with { LeftKey = new Rect(rects.LeftKey.Location, sz) };
                    break;
                case LayoutItem.RightKey:
                    rects = rects with { RightKey = new Rect(rects.Interior.Size.Width - sz.Width, 0, sz.Width, sz.Height) };
                    break;
                case LayoutItem.ErrorMark:
                    Rect errorRect = layout.ErrorMarkRect;
                    errorRect.Inflate(sz);
                    rects = rects with { ErrorMark = errorRect };
                    break;
                case LayoutItem.BlockInstructions:
                    rects = rects with { BlockInstructions = new Rect((rects.Interior.Size.Width - sz.Width) / 2, rects.Interior.Size.Height - sz.Height,
                        sz.Width, sz.Height) };
                    break;
                case LayoutItem.MockItemInstructions:
                    r = layout.MockItemInstructionsRect;
                    Inflate(r, sz);
                    rects = rects with { MockItemInstructions = r };
                    break;
                case LayoutItem.KeyedInstructions:   
                    r = layout.KeyedInstructionsRect;
                    Inflate(r, sz);
                    rects = rects with { KeyedInstructions = r };
                    break;
                case LayoutItem.TextInstructions:
                    r = layout.TextInstructionsRect;
                    Inflate(r, sz);
                    rects = rects with { TextInstructions = r };
                    break;
                case LayoutItem.ContinueInstructions:
                    rects = rects with { ContinueInstructions = new Rect((rects.Interior.Size.Width - sz.Width) / 2, rects.Interior.Size.Height - sz.Height,
                        sz.Width, sz.Height) };
                    break;
            }
        }
        return rects;
    }
}



public record LayoutRects
{
    public Rect Interior { get; init; }
    public Rect Stimulus { get; init; }
    public Rect LeftKey { get; init; }
    public Rect RightKey { get; init; }
    public Rect ErrorMark { get; init; }
    public Rect BlockInstructions { get; init; }
    public Rect MockItemInstructions { get; init; }
    public Rect KeyedInstructions { get; init; }
    public Rect TextInstructions { get; init; }
    public Rect ContinueInstructions { get; init; }
}