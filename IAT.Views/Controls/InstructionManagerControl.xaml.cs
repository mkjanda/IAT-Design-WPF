using System.Windows;
using System.Windows.Controls;
using IAT.ViewModels.Controls;

namespace IAT.Views.Controls;

/// <summary>
/// Interaction logic for InstructionManagerControl.xaml.
/// Provides placeholder behavior for the Instruction Text TextBox that mirrors
/// the pattern used by TextStimulusEditControl:
/// - Clears the type-specific "New … instructions" default when the user focuses the box.
/// - Restores an appropriate default if the user leaves the box empty.
/// All other behaviour lives in <see cref="InstructionManagerViewModel"/>.
/// </summary>
public partial class InstructionManagerControl : UserControl
{
    private static readonly string[] PlaceholderTexts =
    {
        "New text instructions",
        "New keyed instructions",
        "New mock-item instructions"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionManagerControl"/> class.
    /// </summary>
    public InstructionManagerControl()
    {
        InitializeComponent();
    }

    private void InstructionTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        // Clear only when the current value is still one of the creation defaults.
        foreach (var placeholder in PlaceholderTexts)
        {
            if (string.Equals(tb.Text, placeholder, StringComparison.Ordinal))
            {
                tb.Text = string.Empty;
                return;
            }
        }
    }

    private void InstructionTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        if (!string.IsNullOrWhiteSpace(tb.Text))
            return;

        // Restore a sensible default based on the current screen type so the
        // designer always has a starting point and the preview is never blank.
        var defaultText = "New text instructions";
        if (DataContext is InstructionManagerViewModel vm)
        {
            defaultText = vm.SelectedType switch
            {
                "Keyed Response" => "New keyed instructions",
                "Mock Item" => "New mock-item instructions",
                _ => "New text instructions"
            };
        }

        tb.Text = defaultText;
    }
}
