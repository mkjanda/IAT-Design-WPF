using IAT.Core.Domain;
using System.Globalization;
using System.Windows.Data;

namespace IAT.Views.Converters;

/// <summary>
/// Maps an <see cref="InstructionScreen"/> instance to a short type label for list display.
/// </summary>
public sealed class InstructionScreenTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            TextInstructionScreen => "Text",
            KeyedInstructionScreen => "Keyed Response",
            MockItemInstructionScreen => "Mock Item",
            _ => "Instruction"
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
