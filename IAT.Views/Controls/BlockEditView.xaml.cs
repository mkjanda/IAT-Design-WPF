using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IAT.ViewModels.Controls;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Interaction logic for BlockEditView.xaml
    /// </summary>
    public partial class BlockEditView : UserControl
    {
        public BlockEditView()
        {
            InitializeComponent();
        }

        public void CustomizeLayoutClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as BlockEditViewModel;
           
            // Placeholder for future layout customization logic
            MessageBox.Show("Customize Layout clicked! (Functionality to be implemented)");
        }
    }
}
