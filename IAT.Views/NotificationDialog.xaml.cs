using System.Windows;

namespace IAT.Views
{
    /// <summary>
    /// Dark-themed OK notification dialog consistent with the main designer chrome.
    /// </summary>
    public partial class NotificationDialog : Window
    {
        /// <summary>
        /// Creates a notification dialog.
        /// </summary>
        /// <param name="message">Body text shown to the user.</param>
        /// <param name="title">Window / header title.</param>
        public NotificationDialog(string message, string title = "Notification")
        {
            InitializeComponent();
            Title = string.IsNullOrWhiteSpace(title) ? "Notification" : title;
            DataContext = new { Message = message ?? string.Empty };
        }

        private void OnOkay(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
