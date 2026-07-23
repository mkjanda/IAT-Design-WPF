using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System.Windows;

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
    /// <param name="rect">The new rectangle to apply to the specified layout item.</param>
    void ApplyUserOverrides(Layout layout, LayoutItem layoutItem, Rect rect);

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
    /// Applies a user-defined size override to a specified region within the given layout configuration.
    /// </summary>
    /// <remarks>If the specified size is smaller than the minimum allowed dimensions, the size is adjusted to
    /// a minimum width and height of 50 units. Subsequent calls with the same region name will overwrite previous
    /// overrides.</remarks>
    /// <param name="layout">The layout configuration to which the size override will be applied. Cannot be null.</param>
    /// <param name="layoutItem">The layout item within the layout to override. Cannot be null.</param>
    /// <param name="rect">The new rectangle to apply to the specified layout item. Width and height values less than 50 are automatically set to 50.</param>
    public void ApplyUserOverrides(Layout layout, LayoutItem layoutItem, Rect rect)
    {
        layout.UserSizeOverrides[layoutItem] = rect;
    }

    /// <summary>
    /// Calculates the final bounding rectangle for a specified layout item within the given layout configuration, 
    /// taking into account any user-defined size overrides.
    /// </summary>
    /// <param name="layout">The layout configuration containing the layout items and any user-defined size overrides. Cannot be null.</param>
    /// <param name="li">The layout item for which to calculate the final bounding rectangle. Cannot be null.</param>
    /// <returns>The final bounding rectangle for the specified layout item, considering any user-defined size overrides.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified layout item is not recognized.</exception>
    public Rect GetFinalRect(Layout layout, LayoutItem li)
    {
        if (layout.UserSizeOverrides.TryGetValue(li, out var sz))
        {
            switch (li)
            {
                case LayoutItem.Interior:
                    return new Rect(0, 0, sz.Width, sz.Height);
                case LayoutItem.Stimulus:
                    return new Rect(layout.StimulusRect.Location.X - (sz.Width - layout.StimulusRect.Size.Width) / 2,
                        layout.StimulusRect.Location.Y - (sz.Height - layout.StimulusRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.LeftKey:
                    return new Rect(layout.LeftKeyRect.Location.X - (sz.Width - layout.LeftKeyRect.Size.Width) / 2,
                        layout.LeftKeyRect.Location.Y - (sz.Height - layout.LeftKeyRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.RightKey:
                    return new Rect(layout.RightKeyRect.Location.X - (sz.Width - layout.RightKeyRect.Size.Width) / 2,
                        layout.RightKeyRect.Location.Y - (sz.Height - layout.RightKeyRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.ErrorMark:
                    return new Rect(layout.ErrorMarkRect.Location.X - (sz.Width - layout.ErrorMarkRect.Size.Width) / 2,
                        layout.ErrorMarkRect.Location.Y - (sz.Height - layout.ErrorMarkRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.BlockInstructions:
                    return new Rect(layout.BlockInstructionsRect.Location.X - (sz.Width - layout.BlockInstructionsRect.Size.Width) / 2,
                        layout.BlockInstructionsRect.Location.Y - (sz.Height - layout.BlockInstructionsRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.MockItemInstructions:
                    return new Rect(layout.MockItemInstructionsRect.Location.X - (sz.Width - layout.MockItemInstructionsRect.Size.Width) / 2,
                        layout.MockItemInstructionsRect.Location.Y - (sz.Height - layout.MockItemInstructionsRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.KeyedInstructions:
                    return new Rect(layout.KeyedInstructionsRect.Location.X - (sz.Width - layout.KeyedInstructionsRect.Size.Width) / 2,
                        layout.KeyedInstructionsRect.Location.Y - (sz.Height - layout.KeyedInstructionsRect.Size.Height) / 2, sz.Width, sz.Height);
                case LayoutItem.TextInstructions:
                    return new Rect(layout.TextInstructionsRect.Location.X - (sz.Width - layout.TextInstructionsRect.Size.Width) / 2,
                        layout.TextInstructionsRect.Location.Y - (sz.Height - layout.TextInstructionsRect.Size.Height) / 2, sz.Width, sz.Height);
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

        foreach (var (layoutItem, rect) in layout.UserSizeOverrides)
            switch (layoutItem)
            {
                case LayoutItem.Interior:
                    rects = rects with { Interior = rect };
                    break;
                case LayoutItem.Stimulus:
                    rects = rects with { Stimulus = rect };
                    break;
                case LayoutItem.LeftKey:
                    rects = rects with { LeftKey = rect };
                    break;
                case LayoutItem.RightKey:
                    rects = rects with { RightKey = rect };
                    break;
                case LayoutItem.ErrorMark:
                    rects = rects with { ErrorMark = rect };
                    break;
                case LayoutItem.BlockInstructions:
                    rects = rects with { BlockInstructions = rect };
                    break;
                case LayoutItem.MockItemInstructions:
                    rects = rects with { MockItemInstructions = rect };
                    break;
                case LayoutItem.KeyedInstructions:
                    rects = rects with { KeyedInstructions = rect };
                    break;
                case LayoutItem.TextInstructions:
                    rects = rects with { TextInstructions = rect };
                    break;
                case LayoutItem.ContinueInstructions:
                    rects = rects with { ContinueInstructions = rect };
                    break;
            }
        return rects;
    }
}


/// <summary>
/// Represents the final layout rectangles for various UI elements after applying the layout 
/// configuration and any user-defined size overrides.
/// </summary>
public record LayoutRects
{
    /// <summary>
    /// Gets the rectangle representing the interior area of the layout, which serves as the main container 
    /// for other UI elements.
    /// </summary>
    public Rect Interior { get; init; }
    /// <summary>
    /// Gets the rectangle representing the stimulus area of the layout.
    /// </summary>
    public Rect Stimulus { get; init; }
    /// <summary>
    /// Gets the rectangle representing the left key area of the layout.
    /// </summary>
    public Rect LeftKey { get; init; }
    /// <summary>
    /// Gets the rectangle representing the right key area of the layout.
    /// </summary>      
    public Rect RightKey { get; init; }
    /// <summary>
    /// Gets the rectangle representing the error mark area of the layout.
    /// </summary>
    public Rect ErrorMark { get; init; }
    /// <summary>
    /// Gets the rectangle representing the block instructions area of the layout.
    /// </summary>
    public Rect BlockInstructions { get; init; }
    /// <summary>
    /// Gets the rectangle representing the mock item instructions area of the layout.
    /// </summary>
    public Rect MockItemInstructions { get; init; }
    /// <summary>
    /// Gets the rectangle representing the keyed instructions area of the layout.
    /// </summary>
    public Rect KeyedInstructions { get; init; }
    /// <summary>
    /// Gets the rectangle representing the text instructions area of the layout.
    /// </summary>  
    public Rect TextInstructions { get; init; }
    /// <summary>
    /// Gets the rectangle representing the continue instructions area of the layout.
    /// </summary>
    public Rect ContinueInstructions { get; init; }

    /// <summary>
    /// Returns the rectangle corresponding to the specified layout item. This method provides a convenient way to access
    /// the rectangle for a given layout item without directly referencing the properties.
    /// </summary>
    /// <param name="layoutItem">The layout item for which to retrieve the rectangle.</param>
    /// <returns>The rectangle corresponding to the specified layout item.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Rect GetRectByLayoutItem(LayoutItem layoutItem) => layoutItem switch
    {
        LayoutItem.Interior => Interior,
        LayoutItem.Stimulus => Stimulus,
        LayoutItem.LeftKey => LeftKey,
        LayoutItem.RightKey => RightKey,
        LayoutItem.ErrorMark => ErrorMark,
        LayoutItem.BlockInstructions => BlockInstructions,
        LayoutItem.MockItemInstructions => MockItemInstructions,
        LayoutItem.KeyedInstructions => KeyedInstructions,
        LayoutItem.TextInstructions => TextInstructions,
        LayoutItem.ContinueInstructions => ContinueInstructions,
        _ => throw new ArgumentOutOfRangeException(nameof(layoutItem), $"Unsupported layout item: {layoutItem}")
    };

}