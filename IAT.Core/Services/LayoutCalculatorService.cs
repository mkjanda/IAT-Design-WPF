using System.Windows;
using IAT.Core.Domain;

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
    void ApplyDefaults(LayoutConfiguration layout);

    /// <summary>
    /// Applies user-defined size overrides to the specified region within the given layout configuration.
    /// </summary>
    /// <remarks>If the specified region does not exist in the layout, no changes are made. This method
    /// updates only the size of the region, preserving other layout settings.</remarks>
    /// <param name="layout">The layout configuration to which the user overrides will be applied. Cannot be null.</param>
    /// <param name="regionName">The name of the region within the layout to update. Cannot be null or empty.</param>
    /// <param name="newSize">The new size to apply to the specified region.</param>
    void ApplyUserOverrides(LayoutConfiguration layout, string regionName, Size newSize);

    /// <summary>
    /// Calculates the final layout rectangles based on the specified layout configuration.
    /// </summary>
    /// <param name="layout">The layout configuration that defines the arrangement and sizing rules to apply when computing the final
    /// rectangles. Cannot be null.</param>
    /// <returns>A LayoutRects object containing the computed rectangles for the layout. The returned object reflects the final
    /// positions and sizes as determined by the provided configuration.</returns>
    LayoutRects GetFinalRects(LayoutConfiguration layout);
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
    public void ApplyDefaults(LayoutConfiguration layout)
    {
        layout.RestoreDefaults();
        layout.UserSizeOverrides.Clear();
    }

    private void Inflate(Rect rect, Size sz)
    {
        var inflateSz = new Size((sz.Width - rect.Size.Width) / 2, (sz.Height - rect.Size.Height) / 2);
        rect.Inflate(inflateSz);
    }

    public void ApplyUserOverrides(LayoutConfiguration layout, string regionName, Size newSize)
    {
        var size = new Size(Math.Max(50, newSize.Width), Math.Max(50, newSize.Height));
        layout.UserSizeOverrides[regionName] = size;
    }

    public LayoutRects GetFinalRects(LayoutConfiguration layout)
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

        foreach (var (str, sz) in layout.UserSizeOverrides)
        {
            switch (str)
            {
                case "Interior":
                    rects = rects with { Interior = new Rect(0, 0, sz.Width, sz.Height) };
                    break;
                case "Stimulus":
                    Inflate(rects.Stimulus, sz);
                    break;
                case "LeftKey":
                    rects = rects with { LeftKey = new Rect(rects.LeftKey.Location, sz) };
                    break;
                case "RightKey":
                    rects = rects with { RightKey = new Rect(rects.Interior.Size.Width - sz.Width, 0, sz.Width, sz.Height) };
                    break;
                case "ErrorMark":
                    Inflate(rects.ErrorMark, sz);
                    break;
                case "BlockInstructions":
                    rects = rects with { BlockInstructions = new Rect((rects.Interior.Size.Width - sz.Width) / 2, rects.Interior.Size.Height - sz.Height,
                        sz.Width, sz.Height) };
                    break;
                case "MockItemInstructions":
                    Inflate(rects.MockItemInstructions, sz);
                    break;
                case "KeyedInstructions":
                    Inflate(rects.KeyedInstructions, sz);
                    break;
                case "TextInstructions":
                    Inflate(rects.TextInstructions, sz);
                    break;
                case "ContinueInstructions":
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