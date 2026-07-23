using System.Globalization;
using System.Windows.Data;

namespace IAT.Views.Converters;

/// <summary>
/// Two-way converter between <see cref="DateOnly"/>? and a yyyy-MM-dd string for TextBox binding.
/// Empty / invalid input maps to null.
/// </summary>
public sealed class DateOnlyToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateOnly d)
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = (value as string)?.Trim();
        if (string.IsNullOrEmpty(s))
            return null!;

        if (DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;

        // Allow flexible parse as fallback
        if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return d;

        return null!;
    }
}
