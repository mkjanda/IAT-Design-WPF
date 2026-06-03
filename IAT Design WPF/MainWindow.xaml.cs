using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Services;
using IAT.ViewModels.Controls;
using IAT.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    // Optional: also log to console for devs
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
    }
}
