using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Behaviors;

/// <summary>
/// Attached property that provides classic placeholder / watermark behaviour for a TextBox
/// without any code-behind in the consuming control.
/// 
/// Usage:
///   xmlns:behaviors="clr-namespace:IAT.Views.Behaviors"
///   <TextBox behaviors:PlaceholderAttached.Text="New Text Stimulus"
///            Text="{Binding ...}" />
/// 
/// When the TextBox receives focus and its current text equals the placeholder,
/// the text is cleared. When it loses focus and the text is empty/whitespace,
/// the placeholder is restored. The two-way binding still works normally once
/// the user has typed real content.
/// </summary>
public static class PlaceholderAttached
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(PlaceholderAttached),
            new PropertyMetadata(null, OnPlaceholderChanged));

    public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
    public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox) return;

        // Avoid double-subscription if the property is set more than once
        textBox.GotFocus -= OnGotFocus;
        textBox.LostFocus -= OnLostFocus;

        if (e.NewValue is string placeholder && !string.IsNullOrEmpty(placeholder))
        {
            textBox.GotFocus += OnGotFocus;
            textBox.LostFocus += OnLostFocus;

            // Seed the initial value if the box is still empty
            if (string.IsNullOrWhiteSpace(textBox.Text))
                textBox.Text = placeholder;
        }
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var placeholder = GetText(tb);
        if (tb.Text == placeholder)
            tb.Text = string.Empty;
    }

    private static void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var placeholder = GetText(tb);
        if (string.IsNullOrWhiteSpace(tb.Text))
            tb.Text = placeholder;
    }
}
