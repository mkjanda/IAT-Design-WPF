using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls
{
    public partial class StimulusEdit : UserControl
    {
        public StimulusEdit()
        {
            InitializeComponent();
            DataContext = new StimulusEditViewModel();
        }

        private void OnPaletteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string palette)
            {
                var vm = DataContext as StimulusEditViewModel;
                vm?.ApplyPaletteCommand.Execute(palette);
            }
        }

        private void CloseEditPanel(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as StimulusEditViewModel;
            vm?.CloseCommand.Execute(null); 
        }
    }
}