using IAT.Core.Domain;
using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Interaction logic for StimulusEdit.xaml
    /// </summary>
    public partial class StimulusEdit : UserControl
    {
        private IatTest _currentTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="StimulusEdit"/> class.
        /// </summary>
        /// <param name="currentTest">The current IAT test instance.</param>
        public StimulusEdit(IatTest currentTest)
        {
            InitializeComponent();
            _currentTest = currentTest;
            DataContext = new StimulusEditViewModel(_currentTest);
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