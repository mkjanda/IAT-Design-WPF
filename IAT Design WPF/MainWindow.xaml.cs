using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Services;
using IAT.ViewModels.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;

namespace IAT_Design_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Guard against null Services (can happen during design-time or Velopack updates)
            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<TestDesignerViewModel>();
            }

            // Listen for errors and show the banner (non-blocking)
            WeakReferenceMessenger.Default.Register<ErrorNotificationMessage>(this,
                (r, msg) =>
                {
                    ErrorBanner.Show(msg.Title ?? "Error", msg.Message, 8);
                    if (msg.Exception != null)
                        System.Diagnostics.Debug.WriteLine(msg.Exception);
                });
        }

        public void OnStimuliTabSelected(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as TestDesignerViewModel;
            viewModel?.StimuliTabSelectedCommand.Execute(null);
        }

        public void OnBlocksTabSelected(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as TestDesignerViewModel;
            viewModel?.BlocksTabSelectedCommand.Execute(null);
        }

        /// <summary>
        /// Prompt to discard unsaved changes before the window closes.
        /// Uses a synchronous wait because Closing does not support async handlers cleanly.
        /// </summary>
        private async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (DataContext is not TestDesignerViewModel vm)
                return;

            if (!vm.IsDirty)
                return;

            // Cancel first; re-close only if the user confirms.
            e.Cancel = true;
            var discard = await vm.ConfirmDiscardIfDirtyAsync();
            if (discard)
            {
                vm.IsDirty = false; // prevent re-entry
                Close();
            }
        }
    }
}
