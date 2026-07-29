using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System.Windows;

namespace IAT.Core.Services;

/// <summary>
/// Service responsible for calculating the final layout rectangles for various UI elements based on the provided layout configuration and
/// any user overrides. Derived regions (text / keyed / mock / continue instructions) are computed from the interior and related elements
/// unless the designer has explicitly overridden them.
/// </summary>
public interface ILayoutCalculatorService
{
    /// <summary>
    /// Applies default values to the specified layout configuration.
    /// </summary>
    void ApplyDefaults(Layout layout);

    /// <summary>
    /// Applies user-defined size overrides to the specified region within the given layout configuration.
    /// </summary>
    void ApplyUserOverrides(Layout layout, LayoutItem layoutItem, Rect rect);

    /// <summary>
    /// Calculates the final bounding rectangle for the specified layout item within the given layout configuration.
    /// </summary>
    Rect GetFinalRect(Layout layout, LayoutItem li);

    /// <summary>
    /// Calculates the final layout rectangles based on the specified layout configuration.
    /// </summary>
    LayoutRects GetFinalRects(Layout layout);
}

/// <summary>
/// Provides services for calculating and applying layout configurations, including default settings, user overrides,
/// and final layout rectangles.
/// </summary>
public class LayoutCalculatorService : ILayoutCalculatorService
{
    /// <summary>Padding on every side when deriving text instructions from the interior.</summary>
    public const double TextInstructionsPadding = 15.0;

    /// <summary>Padding around the keyed-instructions region (below keys, and against left/right/bottom edges).</summary>
    public const double KeyedInstructionsPadding = 15.0;

    /// <summary>Height of the continue-instructions strip — enough for a single line of text.</summary>
    public const double ContinueInstructionsHeight = 28.0;

    /// <summary>Gap between the bottom of the continue strip and the bottom of the interior.</summary>
    public const double ContinueInstructionsBottomOffset = 8.0;

    // ── Derived-region helpers ──────────────────────────────────────────────

    /// <summary>
    /// Text instructions: fill most of the interior with uniform padding.
    /// </summary>
    public static Rect ComputeTextInstructionsRect(Rect interior)
    {
        var pad = TextInstructionsPadding;
        var width = Math.Max(0, interior.Width - 2 * pad);
        var height = Math.Max(0, interior.Height - 2 * pad);
        return new Rect(interior.X + pad, interior.Y + pad, width, height);
    }

    /// <summary>
    /// Continue instructions: full interior width, one line tall, slightly above the bottom edge.
    /// Left = 0, Right = interior.Width, Bottom ≈ interior.Height − offset.
    /// </summary>
    public static Rect ComputeContinueInstructionsRect(Rect interior)
    {
        var height = ContinueInstructionsHeight;
        var offset = ContinueInstructionsBottomOffset;
        var y = Math.Max(0, interior.Height - offset - height);
        var width = Math.Max(0, interior.Width);
        return new Rect(0, y, width, height);
    }

    /// <summary>
    /// Keyed instructions: below both response keys, spanning the interior with light padding.
    /// Top = max(leftKey.Bottom, rightKey.Bottom) + pad;
    /// Left/Right/Bottom = interior edges inset by pad.
    /// </summary>
    public static Rect ComputeKeyedInstructionsRect(Rect interior, Rect leftKey, Rect rightKey)
    {
        var pad = KeyedInstructionsPadding;
        var top = Math.Max(leftKey.Bottom, rightKey.Bottom) + pad;
        var left = pad;
        var right = Math.Max(left, interior.Width - pad);
        var bottom = Math.Max(top, interior.Height - pad);
        return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
    }

    /// <summary>
    /// Mock-item instructions: band between the error mark and the continue strip.
    /// Top = errorMark.Bottom, Left = 0, Right = interior.Width, Bottom = continueInstructions.Top.
    /// </summary>
    public static Rect ComputeMockItemInstructionsRect(Rect interior, Rect errorMark, Rect continueInstructions)
    {
        var top = errorMark.Bottom;
        var bottom = Math.Max(top, continueInstructions.Top);
        var width = Math.Max(0, interior.Width);
        return new Rect(0, top, width, Math.Max(0, bottom - top));
    }

    /// <summary>
    /// Recomputes every derived instruction region on <paramref name="layout"/> from its current
    /// interior / key / error geometry. Useful after <see cref="Layout.RestoreDefaults"/>.
    /// </summary>
    public static void ApplyDerivedInstructionRects(Layout layout)
    {
        var interior = layout.InteriorRect;
        layout.TextInstructionsRect = ComputeTextInstructionsRect(interior);
        layout.ContinueInstructionsRect = ComputeContinueInstructionsRect(interior);
        layout.KeyedInstructionsRect = ComputeKeyedInstructionsRect(
            interior, layout.LeftKeyRect, layout.RightKeyRect);
        layout.MockItemInstructionsRect = ComputeMockItemInstructionsRect(
            interior, layout.ErrorMarkRect, layout.ContinueInstructionsRect);
    }

    // ── ILayoutCalculatorService ────────────────────────────────────────────

    /// <inheritdoc />
    public void ApplyDefaults(Layout layout)
    {
        layout.RestoreDefaults();
        layout.UserSizeOverrides.Clear();
        ApplyDerivedInstructionRects(layout);
    }

    /// <inheritdoc />
    public void ApplyUserOverrides(Layout layout, LayoutItem layoutItem, Rect rect)
    {
        layout.UserSizeOverrides[layoutItem] = rect;
    }

    /// <inheritdoc />
    public Rect GetFinalRect(Layout layout, LayoutItem li)
    {
        // Prefer the batch path so derived regions stay consistent with each other.
        return GetFinalRects(layout).GetRectByLayoutItem(li);
    }

    /// <inheritdoc />
    public LayoutRects GetFinalRects(Layout layout)
    {
        // 1. Start from stored geometry.
        var interior = layout.InteriorRect;
        var stimulus = layout.StimulusRect;
        var leftKey = layout.LeftKeyRect;
        var rightKey = layout.RightKeyRect;
        var errorMark = layout.ErrorMarkRect;
        var blockInstructions = layout.BlockInstructionsRect;

        var textOverridden = false;
        var continueOverridden = false;
        var keyedOverridden = false;
        var mockOverridden = false;
        Rect? textOverride = null;
        Rect? continueOverride = null;
        Rect? keyedOverride = null;
        Rect? mockOverride = null;

        // 2. Apply user overrides for base (non-derived) and flag derived overrides.
        foreach (var (layoutItem, rect) in layout.UserSizeOverrides)
        {
            switch (layoutItem)
            {
                case LayoutItem.Interior:
                    interior = new Rect(0, 0, rect.Width, rect.Height);
                    break;
                case LayoutItem.Stimulus:
                    stimulus = rect;
                    break;
                case LayoutItem.LeftKey:
                    leftKey = rect;
                    break;
                case LayoutItem.RightKey:
                    rightKey = rect;
                    break;
                case LayoutItem.ErrorMark:
                    errorMark = rect;
                    break;
                case LayoutItem.BlockInstructions:
                    blockInstructions = rect;
                    break;
                case LayoutItem.TextInstructions:
                    textOverridden = true;
                    textOverride = rect;
                    break;
                case LayoutItem.ContinueInstructions:
                    continueOverridden = true;
                    continueOverride = rect;
                    break;
                case LayoutItem.KeyedInstructions:
                    keyedOverridden = true;
                    keyedOverride = rect;
                    break;
                case LayoutItem.MockItemInstructions:
                    mockOverridden = true;
                    mockOverride = rect;
                    break;
            }
        }

        // 3. Derive instruction regions from the final base geometry (order matters).
        var continueInstructions = continueOverridden
            ? continueOverride!.Value
            : ComputeContinueInstructionsRect(interior);

        var keyedInstructions = keyedOverridden
            ? keyedOverride!.Value
            : ComputeKeyedInstructionsRect(interior, leftKey, rightKey);

        var mockItemInstructions = mockOverridden
            ? mockOverride!.Value
            : ComputeMockItemInstructionsRect(interior, errorMark, continueInstructions);

        var textInstructions = textOverridden
            ? textOverride!.Value
            : ComputeTextInstructionsRect(interior);

        return new LayoutRects
        {
            Interior = interior,
            Stimulus = stimulus,
            LeftKey = leftKey,
            RightKey = rightKey,
            ErrorMark = errorMark,
            BlockInstructions = blockInstructions,
            MockItemInstructions = mockItemInstructions,
            KeyedInstructions = keyedInstructions,
            TextInstructions = textInstructions,
            ContinueInstructions = continueInstructions
        };
    }
}

/// <summary>
/// Final layout rectangles after applying configuration and user-defined size overrides.
/// </summary>
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

    /// <summary>
    /// Returns the rectangle corresponding to the specified layout item.
    /// </summary>
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
