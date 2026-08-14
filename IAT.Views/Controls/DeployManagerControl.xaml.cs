using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls;

/// <summary>
/// Interaction logic for DeployManagerControl.xaml.
/// Pure view — all logic lives in <see cref="DeployManagerViewModel"/>.
/// Visibility drives WebSocket lifetime: open while shown, closed when hidden.
/// </summary>
public partial class DeployManagerControl : UserControl
{
    public DeployManagerControl()
    {
        InitializeComponent();
    }

    private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is not DeployManagerViewModel vm)
            return;

        if (e.NewValue is true)
            await vm.OnActivatedAsync();
        else
            await vm.OnDeactivatedAsync();
    }
}
