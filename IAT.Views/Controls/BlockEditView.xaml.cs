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

        private void OnPreviewHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is BlockEditViewModel vm && vm.LayoutViewModel != null)
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(e.NewSize); 
            }
        }
    }
}
