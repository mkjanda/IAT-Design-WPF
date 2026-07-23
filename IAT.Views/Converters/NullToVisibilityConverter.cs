using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IAT.Views.Converters;

/// <summary>
/// Converts null → Collapsed, non-null → Visible.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts null → Collapsed, non-null → Visible.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
