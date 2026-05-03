using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IAT.Views
{
    /// <summary>
    /// Interaction logic for NotificationDialog.xaml
    /// </summary>
    public partial class NotificationDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the NotificationDialog class with the specified message and an optional title.
        /// </summary>
        /// <param name="message">The message to display in the notification dialog. Cannot be null or empty.</param>
        /// <param name="title">The title of the notification dialog. Defaults to "Notification" if not specified.</param>
        public NotificationDialog(string message, string title = "Notification")
        {
            InitializeComponent();
            DataContext = new { Message = message }; // or bind to a proper VM
            Title = title;
        }
        private void OnOkay(object sender, RoutedEventArgs e) { DialogResult = true; }
    }
}
