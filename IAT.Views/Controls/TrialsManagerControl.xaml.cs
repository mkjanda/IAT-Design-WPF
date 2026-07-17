using IAT.Core.Domain;
using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls
{
    public partial class TrialsManagerControl : UserControl
    {
        public TrialsManagerControl()
        {
            InitializeComponent();
        }

        private void OnAssignLeftClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Stimulus stim && DataContext is TrialsManagerViewModel vm)
            {
                vm.AssignStimulusCommand.Execute(new object[] { stim, "Left" });
            }
        }

        private void OnAssignRightClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Stimulus stim && DataContext is TrialsManagerViewModel vm)
            {
                vm.AssignStimulusCommand.Execute(new object[] { stim, "Right" });
            }
        }
    }
}