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
    }

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
