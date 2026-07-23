using System;
using System.Windows;
using System.Windows.Controls;
using IAT.ViewModels.Controls;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Interaction logic for BlockEditView.xaml
    /// Ensures the shared LayoutViewModel is fitted as soon as the Blocks tab is visible,
    /// so the preview is not blank until the user visits the Layout tab.
    /// </summary>
    public partial class BlockEditView : UserControl
    {
        /// <summary>
        /// Prevents re-entrant SizeChanged → FitToWindow → layout → SizeChanged loops.
        /// </summary>
        private bool _isHandlingSizeChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockEditView"/> class.
        /// </summary>
        public BlockEditView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                RefreshPreviewContent();
                TryFitToHost();
            };
            IsVisibleChanged += (_, e) =>
            {
                if (e.NewValue is true)
                {
                    RefreshPreviewContent();
                    // Defer fit one layout pass so ActualWidth/Height are valid after the tab switch.
                    Dispatcher.BeginInvoke(new Action(TryFitToHost),
                        System.Windows.Threading.DispatcherPriority.Loaded);
                }
            };
            DataContextChanged += (_, _) =>
            {
                RefreshPreviewContent();
                TryFitToHost();
            };
        }

        private void OnPreviewHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isHandlingSizeChanged)
                return;

            if (DataContext is not BlockEditViewModel vm || vm.LayoutViewModel is null)
                return;

            // Ignore zero-size events (tab collapsed / not yet measured).
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
                return;

            _isHandlingSizeChanged = true;
            try
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(e.NewSize);
            }
            finally
            {
                _isHandlingSizeChanged = false;
            }
        }

        /// <summary>
        /// Fit the layout preview to the host using measured size.
        /// Mirrors LayoutEditView so Blocks does not depend on visiting Layout first.
        /// </summary>
        private void TryFitToHost()
        {
            if (DataContext is not BlockEditViewModel vm || vm.LayoutViewModel is null)
                return;

            // Prefer the named preview host when it has a real size.
            if (PreviewHost is FrameworkElement host && host.ActualWidth > 1 && host.ActualHeight > 1)
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(
                    new Size(host.ActualWidth, host.ActualHeight));
                return;
            }

            if (ActualWidth > 1 && ActualHeight > 1)
            {
                vm.LayoutViewModel.FitToWindowCommand.Execute(
                    new Size(Math.Max(100, ActualWidth - 32), Math.Max(100, ActualHeight - 280)));
            }
        }

        /// <summary>
        /// Re-push block keys, instructions, and selected trial into the layout preview.
        /// Needed when returning from the Trials tab after editing keys.
        /// </summary>
        private void RefreshPreviewContent()
        {
            if (DataContext is BlockEditViewModel vm)
                vm.RefreshLayoutPreview();
        }
    }
}
