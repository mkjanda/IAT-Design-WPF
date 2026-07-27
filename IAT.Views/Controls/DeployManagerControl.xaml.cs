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
}
