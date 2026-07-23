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
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        /// <summary>
        /// Gets the result of the confirmation dialog. True if "Yes" was clicked, false if "No" was clicked, and null 
        /// if the dialog was closed without a selection.
        /// </summary>
        public bool? Result { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationDialog"/> class.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        public ConfirmationDialog(string message, string title = "Confirm Overwrite")
        {
            InitializeComponent();
            DataContext = new { Message = message }; // or bind to a proper VM
            Title = title;
        }

        private void OnYes(object sender, RoutedEventArgs e) { Result = true; DialogResult = true; }
        private void OnNo(object sender, RoutedEventArgs e) { Result = false; DialogResult = false; }
    }
}
