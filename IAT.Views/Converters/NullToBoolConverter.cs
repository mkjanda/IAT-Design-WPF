using System.Globalization;
using System.Windows.Data;

namespace IAT.Views.Converters;

/// <summary>
/// Converts null → false, non-null → true. Useful for IsEnabled bindings.
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts null → false, non-null → true. Useful for IsEnabled bindings.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not null;

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
