using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using System.Windows.Media;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a response key with associated text, unique identifier, and formatting options.
    /// Shared across blocks (Trials tab) and instruction screens (Instructions tab).
    /// Text raises PropertyChanged so ComboBox DisplayMemberPath and previews stay in sync.
    /// </summary>
    public partial class Key : ObservableObject, IFormattedText
    {
        /// <summary>
        /// Unique identifier used to reference this key from blocks and instruction screens.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The key label / character shown to the participant (e.g. "E", "I", "A").
        /// </summary>
        [ObservableProperty]
        private string text = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the current instance represents a combined state.
        /// </summary>
        public bool IsCombined { get; set; }

        /// <summary>
        /// Layout slot this key is intended for (LeftKey / RightKey). Set at creation.
        /// </summary>
        public LayoutItem LayoutItem { get; init; } = LayoutItem.LeftKey;

        /// <summary>
        /// Component key ids when this is a combined key.
        /// </summary>
        public List<Guid> ComponentIds { get; set; } = new();

        /// <summary>
        /// Separator used when rendering a combined key.
        /// </summary>
        public string Separator { get; set; } = " or ";

        /// <summary>
        /// Visual arrangement of component keys.
        /// </summary>
        public KeyLayoutMode LayoutMode { get; set; } = KeyLayoutMode.VerticalStack;

        /// <summary>
        /// Text style applied when rendering the key.
        /// </summary>
        public required TextStyle Style { get; set; }

        /// <summary>
        /// Font family name used for text rendering.
        /// </summary>
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Font size used to display text.
        /// </summary>
        public double FontSize { get; set; } = 24.0;

        /// <summary>
        /// Color used to display text.
        /// </summary>
        public Color FontColor { get; set; } = Colors.Black;

        /// <summary>
        /// Converts author-facing key text into the three-row stacked form used in the
        /// layout preview and on participant slides.
        /// <para>
        /// Trials-tab editors store a single line such as <c>"Good or Flower"</c>.
        /// Rendering splits on the word <c>or</c> (case-insensitive) so the participant
        /// sees:
        /// </para>
        /// <code>
        /// Good
        /// or
        /// Flower
        /// </code>
        /// Already-multiline text is normalized but not re-split.
        /// </summary>
        public static string FormatStackedDisplay(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text.Replace("\r\n", "\n").Trim();

            // Already stacked (author or generate path wrote explicit newlines) — keep structure.
            if (normalized.Contains('\n'))
                return normalized;

            // Split on the word "or" as a whole token so "Good or Flower" → three rows.
            var parts = System.Text.RegularExpressions.Regex
                .Split(normalized, @"\s+or\s+", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToArray();

            if (parts.Length >= 2)
                return string.Join("\nor\n", parts);

            return normalized;
        }

        /// <summary>
        /// Default style for the separator row ("or") on a combined key.
        /// Color is always black; family and size stay on the product defaults
        /// so a 48pt pink term cannot paint the conjunction.
        /// </summary>
        public static TextStyle DefaultSeparatorStyle() => new()
        {
            FontFamily = "Segoe UI",
            FontSize = 24.0,
            FontColor = Colors.Black
        };

        /// <summary>
        /// Rows the preview and slide renderer should paint for this key.
        /// Combined keys with two <see cref="ComponentIds"/> resolve each
        /// component's live style; the "or" row is <see cref="DefaultSeparatorStyle"/>.
        /// Plain keys return a single row from this key's own style.
        /// </summary>
        public static IReadOnlyList<KeyDisplayLine> ResolveDisplayLines(Key? key, IatTest? test)
        {
            if (key is null)
                return Array.Empty<KeyDisplayLine>();

            if (key.IsCombined && test is not null && key.ComponentIds is { Count: >= 2 })
            {
                var first = test.GetKeyById(key.ComponentIds[0]);
                var second = test.GetKeyById(key.ComponentIds[1]);
                if (first is not null && second is not null)
                {
                    var t1 = FormatAuthoringDisplay(first.Text);
                    var t2 = FormatAuthoringDisplay(second.Text);
                    var or = DefaultSeparatorStyle();
                    return new[]
                    {
                        LineFrom(t1, first),
                        new KeyDisplayLine("or", or.FontFamily, or.FontSize, or.FontColor),
                        LineFrom(string.IsNullOrEmpty(t2) ? string.Empty : t2, second)
                    };
                }
            }

            var stacked = FormatStackedDisplay(key.Text);
            if (string.IsNullOrWhiteSpace(stacked))
                return Array.Empty<KeyDisplayLine>();

            var rows = stacked.Split('\n', StringSplitOptions.None);
            if (rows.Length >= 3 && rows[1].Trim().Equals("or", StringComparison.OrdinalIgnoreCase))
            {
                var or = DefaultSeparatorStyle();
                return new[]
                {
                    LineFrom(rows[0], key),
                    new KeyDisplayLine("or", or.FontFamily, or.FontSize, or.FontColor),
                    LineFrom(rows[2], key)
                };
            }

            return new[] { LineFrom(stacked, key) };
        }

        private static KeyDisplayLine LineFrom(string text, Key source)
        {
            var family = source.Style?.FontFamily ?? source.FontFamily ?? "Segoe UI";
            var size = source.Style?.FontSize > 0 ? source.Style.FontSize
                : source.FontSize > 0 ? source.FontSize : 24.0;
            var color = source.Style?.FontColor ?? source.FontColor;
            return new KeyDisplayLine(text, family, size, color);
        }

        /// <summary>
        /// Single-line authoring form used on the Trials tab (e.g. <c>"Good or Flower"</c>).
        /// Collapses an already-stacked multiline value back to one line.
        /// </summary>
        public static string FormatAuthoringDisplay(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text.Replace("\r\n", "\n").Trim();
            if (!normalized.Contains('\n'))
                return normalized;

            // "Good\nor\nFlower" → "Good or Flower"
            var lines = normalized
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return string.Join(" ", lines);
        }
    }

    /// <summary>
    /// One painted row of a response key (term, "or", or a plain label).
    /// </summary>
    public readonly record struct KeyDisplayLine(string Text, string FontFamily, double FontSize, Color FontColor);

    /// <summary>
    /// Specifies the layout mode for arranging keys in a user interface.
    /// </summary>
    public enum KeyLayoutMode
    {
        /// <summary>Horizontal orientation.</summary>
        Horizontal,

        /// <summary>Vertical stack orientation.</summary>
        VerticalStack,

        /// <summary>Vertical layout that combines elements with a logical OR separator.</summary>
        VerticalWithOr
    }
}
