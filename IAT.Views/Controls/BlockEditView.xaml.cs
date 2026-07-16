using System;
using System.Windows;
using System.Windows.Controls;
using IAT.ViewModels.Controls;

namespace IAT.Views.Controls
{
    public partial class BlockEditView : UserControl
    {
        public BlockEditView()
        {
            InitializeComponent();
            Loaded += (_, _) => TryFitToHost();
            DataContextChanged += (_, _) => TryFitToHost();
        }

        private void OnPreviewHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is BlockEditViewModel vm && vm.LayoutViewModel != null
                && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(e.NewSize);
            }
        }

        private void TryFitToHost()
        {
            if (DataContext is not BlockEditViewModel vm || vm.LayoutViewModel is null)
                return;
            if (ActualWidth > 1 && ActualHeight > 1)
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(
                    new Size(Math.Max(100, ActualWidth - 32), Math.Max(100, ActualHeight - 32)));
            }
        }
    }
}
