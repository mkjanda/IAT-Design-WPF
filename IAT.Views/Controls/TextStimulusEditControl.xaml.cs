using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Interaction logic for TextStimulusEditControl.xaml
    /// Provides placeholder behavior for the stimulus text TextBox:
    /// - Clears the default "New Text Stimulus" text when the user focuses the box.
    /// - Restores the placeholder if the user leaves the box empty.
    /// </summary>
    public partial class TextStimulusEditControl : UserControl
    {
        private const string PlaceholderText = "New Text Stimulus";

        public TextStimulusEditControl()
        {
            InitializeComponent();
        }

        private void StimulusTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text == PlaceholderText)
            {
                tb.Text = string.Empty;
            }
        }

        private void StimulusTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = PlaceholderText;
            }
        }
    }
}
