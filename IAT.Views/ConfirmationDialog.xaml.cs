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
        public bool? Result { get; private set; }
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
