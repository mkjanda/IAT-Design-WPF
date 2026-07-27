// IAT.ViewModels/Converters/BoolToVisibilityConverter.cs
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IAT.Views.Converters;

/// <summary>
/// Converts a boolean to Visibility.
/// true → Visible, false → Collapsed.
/// Pass ConverterParameter="Invert" to reverse the mapping.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to Visibility.
    /// true → Visible, false → Collapsed.
    /// Pass ConverterParameter="Invert" to reverse the mapping.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var flag = value is true;
        if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a Visibility back to boolean.
    /// Visible → true, Collapsed → false.
    /// Pass ConverterParameter="Invert" to reverse the mapping.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visible = value is Visibility.Visible;
        if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            visible = !visible;
        return visible;
    }
}