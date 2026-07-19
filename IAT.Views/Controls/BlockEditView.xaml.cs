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
            Loaded += (_, _) => ScheduleFitToHost();
            DataContextChanged += (_, _) => ScheduleFitToHost();
        }

        private void OnPreviewHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is BlockEditViewModel vm && vm.LayoutViewModel != null
                && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(e.NewSize);
            }
        }

        private void ScheduleFitToHost()
        {
            // Use BeginInvoke so we run after the layout pass has completed.
            // This fixes the common issue where ActualWidth/Height are still 0 on Loaded.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is not BlockEditViewModel vm || vm.LayoutViewModel is null)
                    return;

                double availableWidth = PreviewHost.ActualWidth > 50 ? PreviewHost.ActualWidth - 32 : 600;
                double availableHeight = PreviewHost.ActualHeight > 50 ? PreviewHost.ActualHeight - 32 : 500;

                vm.LayoutViewModel.FitToWindowCommand.Execute(
                    new Size(Math.Max(100, availableWidth), Math.Max(100, availableHeight)));
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }
}
