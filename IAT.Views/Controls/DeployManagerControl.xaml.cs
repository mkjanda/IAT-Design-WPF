using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls;

/// <summary>
/// Interaction logic for DeployManagerControl.xaml.
/// Pure view — all logic lives in <see cref="IAT.ViewModels.Controls.DeployManagerViewModel"/>.
/// </summary>
public partial class DeployManagerControl : UserControl
{
    public DeployManagerControl()
    {
        InitializeComponent();
    }

    private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true && DataContext is DeployManagerViewModel vm)
        {
            await vm.OnActivatedAsync();
        }
    }
}
