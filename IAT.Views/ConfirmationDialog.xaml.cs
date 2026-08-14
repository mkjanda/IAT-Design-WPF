using System.Windows;

namespace IAT.Views
{
    /// <summary>
    /// Dark-themed Yes/No confirmation dialog consistent with the main designer chrome.
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        /// <summary>
        /// True if Yes was clicked, false if No was clicked, null if dismissed without a choice.
        /// </summary>
        public bool? Result { get; private set; }

        /// <summary>
        /// Creates a confirmation dialog.
        /// </summary>
        /// <param name="message">Body text shown to the user.</param>
        /// <param name="title">Window / header title.</param>
        public ConfirmationDialog(string message, string title = "Confirm")
        {
            InitializeComponent();
            Title = string.IsNullOrWhiteSpace(title) ? "Confirm" : title;
            DataContext = new { Message = message ?? string.Empty };
        }

        private void OnYes(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
        }

        private void OnNo(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
        }
    }
}
