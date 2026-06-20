using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IAT.Views.Converters
{
    /// <summary>
    /// Converts a boolean value to a BorderBrush color.    
    /// </summary>
    public class BoolToBorderBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean edit mode value to a brush color.
        /// </summary>
        /// <param name="value">The value to convert, expected to be a boolean indicating edit mode status.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional parameter to be used in the conversion.</param>
        /// <param name="culture">The culture to use in the conversion.</param>
        /// <returns>A <see cref="SolidColorBrush"/> with accent blue color (#007ACC) if in edit mode; otherwise, a transparent
        /// brush.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(0, 122, 204)); // #007ACC   
            return Brushes.Transparent;
        }

        /// <summary>
        /// Converts a value from the binding target back to the binding source.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Not applicable.</returns>
        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException(); 
    }
}