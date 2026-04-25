using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;


namespace IAT.Core.Domain
{
    /// <summary>
    /// Enumeration representing the display type for a key item, indicating whether the item should be rendered as text or as an image.
    /// </summary>
    public enum KeyItemDisplayType
    {
        /// <summary>
        /// Gets or sets the text content associated with this instance.
        /// </summary>
        Text,

        /// <summary>
        /// Represents an image resource, such as a bitmap or icon, that can be displayed or manipulated within an
        /// application.
        /// </summary>
        /// <remarks>Use this type to load, display, or process image data in supported formats. The
        /// specific capabilities and supported formats may vary depending on the implementation.</remarks>
        Image
    }

    /// <summary>
    /// Represents a key definition with customizable display properties for both left and right sides, including text,
    /// font, color, and image data.
    /// </summary>
    /// <remarks>The Key class is typically used to model a keyboard key or similar UI element where each side
    /// of the key can display text or an image with configurable appearance. Property changes raise notifications for
    /// data binding scenarios. This class is suitable for use in MVVM architectures and UI frameworks that support
    /// property change notification.</remarks>
    public partial class Key : ObservableObject
    {
        [ObservableProperty]
        private Guid _id = Guid.NewGuid();

        [ObservableProperty]
        private KeyItemDisplayType _leftDisplayType;

        [ObservableProperty]
        private string _leftText = string.Empty;

        [ObservableProperty]
        private string _leftFontFamily = string.Empty;

        [ObservableProperty]
        private double _leftFontSize;
        
        [ObservableProperty]
        private Color _leftFontColor;

        [ObservableProperty]
        private byte[] _leftImageBytes = [];

        [ObservableProperty]
        private KeyItemDisplayType _rightDisplayType;

        [ObservableProperty]
        private string _rightText = string.Empty;

        [ObservableProperty]
        private string _rightFontFamily = string.Empty;

        [ObservableProperty]
        private double _rightFontSize;

        [ObservableProperty]
        private Color _rightFontColor;

        [ObservableProperty]
        private byte[] _rightImageBytes = [];
    }
}
